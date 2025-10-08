using Microsoft.Data.SqlClient;
using System.Data.Common;

namespace CommentService
{
    public class Database
    {

        private static Database instance = new Database();
        private readonly static String connectionString = $"Server={"comment-db"};User Id=sa;Password=SuperSecret7!;Encrypt=false;";
        public static Database GetInstance()
        {
            return instance;
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

        public async Task DeleteDatabase()
        {
            var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            try
            {
                Execute(connection, "DROP TABLE IF EXISTS Comments");
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        public async Task RecreateDatabase()
        {
            Console.WriteLine("Creating Comments table in database.");
            var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            try 
            {
                Execute(connection, """
                CREATE TABLE Comments (
                    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
                    ArticleId BIGINT NOT NULL,
                    Username NVARCHAR(100) NOT NULL,
                    Content NVARCHAR(MAX) NOT NULL,
                    CreatedAt DATETIME2 DEFAULT SYSUTCDATETIME(),
                    Continent NVARCHAR(50) NOT NULL
                    )
                """);
            }
            finally
            {
                await connection.CloseAsync();
            }

        }

        public async Task<Comment?> InsertCommentAsync(CreateCommentRequest request)
        {
            // Returns the Id of the newly inserted comment
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            try
            {
                var sql = @"
                    INSERT INTO Comments (ArticleId, Username, Content, Continent)
                    OUTPUT INSERTED.Id, INSERTED.ArticleId, INSERTED.Username, INSERTED.Content, INSERTED.CreatedAt, INSERTED.Continent
                    VALUES (@ArticleId, @Username, @Content, @Continent);
                ";

                using var cmd = new SqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@ArticleId", request.ArticleId);
                cmd.Parameters.AddWithValue("@Username", request.Username);
                cmd.Parameters.AddWithValue("@Content", request.TextContent);
                cmd.Parameters.AddWithValue("@Continent", request.ArticleContinent);

                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new Comment
                    {
                        Id = reader.GetInt64(0),
                        ArticleId = reader.GetInt64(1),
                        Username = reader.GetString(2),
                        TextContent = reader.GetString(3),
                        CreatedAt = reader.GetDateTime(4),
                        ArticleContinent = reader.GetString(5)
                    };
                }
            }
            finally
            {
                await connection.CloseAsync();
            }
            return null;
        }

        public async Task<int> DeleteCommentAsync(long id, long articleId, string continent)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            try
            {
                var sql = @"
                    DELETE FROM Comments 
                    WHERE Id = @Id AND ArticleId = @ArticleId AND Continent = @Continent;
                    ";
                using var cmd = new SqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@ArticleId", articleId);
                cmd.Parameters.AddWithValue("@Continent", continent);
                return await cmd.ExecuteNonQueryAsync();
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        public async Task<int> DeleteArticleAsync(long articleId, string continent)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            try
            {
                var sql = @"
                    DELETE FROM Comments 
                    WHERE ArticleId = @ArticleId AND Continent = @Continent;
                    ";
                using var cmd = new SqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@ArticleId", articleId);
                cmd.Parameters.AddWithValue("@Continent", continent);
                return await cmd.ExecuteNonQueryAsync();
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        public static async Task<List<Comment>> FetchCommentsAsync(long articleId, string continent)
        {
            var comments = new List<Comment>();
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            try
            {
                using SqlCommand cmd = new SqlCommand($"SELECT * FROM Comments Where ArticleId = @ArticleId and Continent = @Continent;", connection);

                cmd.Parameters.AddWithValue("@ArticleId", articleId);
                cmd.Parameters.AddWithValue("@Continent", continent);
                using SqlDataReader reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var comment = new Comment
                    {
                        Id = reader.GetInt64(reader.GetOrdinal("Id")),
                        ArticleId = reader.GetInt64(reader.GetOrdinal("ArticleId")),
                        Username = reader.GetString(reader.GetOrdinal("Username")),
                        TextContent = reader.GetString(reader.GetOrdinal("Content")),
                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                        ArticleContinent = reader.GetString(reader.GetOrdinal("Continent"))
                    };

                    comments.Add(comment);
                }
            }
            finally
            {
                await connection.CloseAsync();
            }

            return comments;
        }
    }
}
