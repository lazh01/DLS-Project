
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace ArticleService
{
    public class Database
    {
        private Coordinator coordinator = new Coordinator();

        private static Database instance = new Database();

        public static Database GetInstance()
        {
            return instance;
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

        public async Task<List<long>> InsertSqlAsync(DbConnection connection, string sql)
        {
            using var trans = connection.BeginTransaction();
            var cmd = connection.CreateCommand();
            cmd.Transaction = trans;
            cmd.CommandText = sql;
            var insertedIds = new List<long>();

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    insertedIds.Add(Convert.ToInt64(reader.GetValue(0)));
                }
            }

            await trans.CommitAsync();

            Console.WriteLine($"Executed insert: {sql}, inserted IDs: {string.Join(", ", insertedIds)}");

            return insertedIds;
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
            Console.WriteLine($"Executed UPDATE: {sql}, rows affected: {affected}");
            return affected;
        }

        private void Execute(DbConnection connection, string sql)
        {
            using var trans = connection.BeginTransaction();
            var cmd = connection.CreateCommand();
            cmd.Transaction = trans;
            cmd.CommandText = sql;
            var result = cmd.ExecuteNonQuery();
            Console.WriteLine($"Executed command: {sql}, affected rows: {result}");
            trans.Commit();
        }

        public async Task<long?> InsertArticle(Article article)
        {
            var connection = await coordinator.GetConnectionByRegion(article.Continent);
            /*Execute(connection, $"""
            INSERT INTO Articles (Title, Content, Author, Continent)
            VALUES ('{article.Title}', '{article.Content}', '{article.Author}', '{article.Continent}')
            """);*/
            var result = await InsertSqlAsync(connection, $"""
            INSERT INTO Articles (Title, Content, Author, Continent)
            OUTPUT INSERTED.Id
            VALUES ('{article.Title}', '{article.Content}', '{article.Author}', '{article.Continent}');
            """);

            Console.WriteLine(result);
            if (result is not List<long> rows || rows.Count == 0)
            {
                return null;
            }

            var id = rows[0];
            Console.WriteLine($"Inserted article: {id}");
            return id;
        }

        public async Task<Article?> FindArticle(long id, string continent)
        {
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
    }
}