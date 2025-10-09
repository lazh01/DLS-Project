
using Azure.Core;
using EasyNetQ;
using Microsoft.Data.SqlClient;
using SharedModels;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Messages.SharedModels;
using Articleservice.Services;
using Monitoring;

namespace ArticleService.database
{
    public class Database
    {
        private Coordinator coordinator = new Coordinator();
        private readonly IBus _bus;
        private readonly RedisCacheService _cacheService;
        
        public Database(IBus bus, RedisCacheService cacheService)
        {
            _bus = bus;
            _cacheService = cacheService;
        }


        public async Task DeleteDatabase()
        {
            await foreach (var connection in coordinator.GetAllConnections())
            {
                Execute(connection, "DROP TABLE IF EXISTS Articles");
            }
        }

        public async Task RecreateDatabase()
        {
            await foreach (var connection in coordinator.GetAllConnections())
            {
                Console.WriteLine("Creating table in database connected to " + connection.DataSource);
                Execute(connection, """
                CREATE TABLE Articles (
                    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
                    Title NVARCHAR(255) NOT NULL,
                    Content NVARCHAR(MAX) NOT NULL,
                    Author NVARCHAR(100) NOT NULL,
                    PublishedAt DATETIME2 DEFAULT SYSUTCDATETIME(),
                    CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME(),
                    Continent NVARCHAR(50) NOT NULL
                    )
                """);
            }
        }


        public async Task<object?> SelectSqlAsync(DbConnection connection, string sql)
        {
            using var trans = connection.BeginTransaction();
            var cmd = connection.CreateCommand();
            cmd.Transaction = trans;
            cmd.CommandText = sql;
            var results = new List<Dictionary<string, object>>();

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.GetValue(i);
                }
                results.Add(row);
            }
            Console.WriteLine($"Executed query: {sql}, returned rows: {results.Count}");
            foreach (var row in results)
            {
                Console.WriteLine(string.Join(", ", row.Select(kv => $"{kv.Key}: {kv.Value}")));
            }
            return results;
        }

        public async Task<List<object[]>> InsertSqlAsync(DbConnection connection, string sql)
        {
            using var trans = await connection.BeginTransactionAsync();
            using var cmd = connection.CreateCommand();
            cmd.Transaction = trans;
            cmd.CommandText = sql;

            var insertedRows = new List<object[]>();

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var row = new object[reader.FieldCount];
                    reader.GetValues(row); // fills the array with all columns
                    insertedRows.Add(row);
                }
            }

            await trans.CommitAsync();

            Console.WriteLine($"Executed insert: {sql}, inserted rows: {insertedRows.Count}");

            return insertedRows;
        }

        public async Task<int> DeleteSqlAsync(DbConnection connection, string sql)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = sql;

            int affected = await cmd.ExecuteNonQueryAsync();
            Console.WriteLine($"Executed DELETE: {sql}, rows affected: {affected}");
            return affected;
        }

        public async Task<int> UpdateSqlAsync(DbConnection connection, string sql)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = sql;

            int affected = await cmd.ExecuteNonQueryAsync();

            return affected;
        }

        private void Execute(DbConnection connection, string sql)
        {
            using var trans = connection.BeginTransaction();
            var cmd = connection.CreateCommand();
            cmd.Transaction = trans;
            cmd.CommandText = sql;
            var result = cmd.ExecuteNonQuery();

            trans.Commit();
        }

        public async Task<long?> InsertArticle(Article article)
        {
            var connection = await coordinator.GetConnectionByRegion(article.Continent);

            var result = await InsertSqlAsync(connection, $"""
            INSERT INTO Articles (Title, Content, Author, Continent)
            OUTPUT INSERTED.Id, INSERTED.Title, INSERTED.Author, INSERTED.PublishedAt, INSERTED.CreatedAt, INSERTED.Continent
            VALUES ('{article.Title}', '{article.Content}', '{article.Author}', '{article.Continent}');
            """);


            if (result is not List<object[]> rows || rows.Count == 0)
            {
                return null;
            }

            var row = result[0];
            var createdEvent = new ArticleCreatedEvent
            {
                Id = Convert.ToInt64(row[0]),
                Title = row[1]?.ToString() ?? "",
                Author = row[2]?.ToString() ?? "",
                PublishedAt = row[3] is DateTime dt ? dt : DateTime.Parse(row[3]?.ToString() ?? DateTime.UtcNow.ToString()),
                Continent = row[5]?.ToString() ?? ""
            };

            await _bus.PubSub.PublishAsync(createdEvent);
            return createdEvent.Id;
        }

        public async Task<Article?> FindArticle(long id, string continent)
        {
            if (continent == "global")
            {
                var cached = await _cacheService.GetArticleByIdAsync(id);
                if (cached != null)
                {
                    Console.WriteLine($"Fetched article from cache: {cached.Title}");
                    return cached;
                }
            }

            var connection = await coordinator.GetConnectionByRegion(continent);
            var result = await SelectSqlAsync(connection, $"""
            select * from Articles where Id = {id}
            """);

            if (result is not List<Dictionary<string, object>> rows || rows.Count == 0)
            {
                return null;
            }

            var row = rows[0];
            var article = new Article
            {
                Id = (long)row["Id"],
                Title = (string)row["Title"],
                Content = (string)row["Content"],
                Author = (string)row["Author"],
                PublishedAt = (DateTime)row["PublishedAt"],
                CreatedAt = (DateTime)row["CreatedAt"],
                Continent = (string)row["Continent"]
            };
            Console.WriteLine($"Fetched article: {article.Title} by {article.Author}");
            return article;
        }

        public async Task<int> DeleteArticle(long id, string continent)
        {
            var connection = await coordinator.GetConnectionByRegion(continent);
            var affected = await DeleteSqlAsync(connection, $"""
            DELETE FROM Articles WHERE Id = {id}
            """);
            return affected;
        }

        public async Task<int> UpdateArticle(Article article)
        {
            var connection = await coordinator.GetConnectionByRegion(article.Continent);
            var affected = await UpdateSqlAsync(connection, $"""
            UPDATE Articles
            SET Title = '{article.Title}',
                Content = '{article.Content}',
                Author = '{article.Author}'
            WHERE Id = {article.Id}
            """);
            return affected;
        }

        public async Task<List<Article>> FetchArticles(FetchArticlesRequest request)
        {
            if (request.Continent == "global")
            {

                var now = DateOnly.FromDateTime(DateTime.UtcNow);
                var earliestCachedDate = now.AddDays(-14);


                var start = request.StartDate ?? earliestCachedDate;
                var end = request.EndDate ?? now;

                if (start >= earliestCachedDate && end <= now)
                {

                    var cached_articles = await _cacheService.GetArticlesInRangeAsync(start, end);


                    if (request.MaxArticles.HasValue)
                        cached_articles = cached_articles.Take(request.MaxArticles.Value).ToList();
                    MonitorService.Log.Information(
                        "Global Article Fetch CACHE HIT: current data: {date_now}, start date: {start_date}, end date {end_date}",
                        now, start, end);
                    Console.WriteLine($"Fetched {cached_articles.Count} articles from cache for continent {request.Continent}");
                    return cached_articles;
                }

                MonitorService.Log.Information(
                        "Global Article Fetch CACHE MISS: current data: {date_now}, start date: {start_date}, end date {end_date}",
                        now, start, end);
            }

            var connection = await coordinator.GetConnectionByRegion(request.Continent);
            var conditions = new List<string>();
            if (request.StartDate.HasValue)
            {
                conditions.Add($"PublishedAt >= '{request.StartDate.Value:yyyy-MM-dd}'");
            }
            if (request.EndDate.HasValue)
            {
                conditions.Add($"PublishedAt < '{request.EndDate.Value.AddDays(1):yyyy-MM-dd}'");
            }
            var whereClause = conditions.Count > 0 ? " AND " + string.Join(" AND ", conditions) : "";
            var limitClause = request.MaxArticles.HasValue ? $"TOP {request.MaxArticles.Value}" : "";
            var result = await SelectSqlAsync(connection, $"""
            SELECT {limitClause} * FROM Articles
            WHERE Continent = '{request.Continent}'{whereClause}
            ORDER BY PublishedAt DESC
            """);
            if (result is not List<Dictionary<string, object>> rows)
            {
                return new List<Article>();
            }
            var articles = rows.Select(row => new Article
            {
                Id = (long)row["Id"],
                Title = (string)row["Title"],
                Content = (string)row["Content"],
                Author = (string)row["Author"],
                PublishedAt = (DateTime)row["PublishedAt"],
                CreatedAt = (DateTime)row["CreatedAt"],
                Continent = (string)row["Continent"]
            }).ToList();
            Console.WriteLine($"Fetched {articles.Count} articles for continent {request.Continent}");
            return articles;
        }
    }
}