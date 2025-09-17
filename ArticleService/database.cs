
using System.Data;
using System.Data.Common;
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


        public void DeleteDatabase()
        {
            foreach (var connection in coordinator.GetAllConnections())
            {
                Execute(connection, "DROP TABLE IF EXISTS Articles");
            }
        }

        public void RecreateDatabase()
        {
            foreach (var connection in coordinator.GetAllConnections())
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

        private void Execute(DbConnection connection, string sql)
        {
            using var trans = connection.BeginTransaction();
            var cmd = connection.CreateCommand();
            cmd.Transaction = trans;
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
            trans.Commit();
        }

        public void InsertArticle(Article article)
        {   
            var connection = coordinator.GetConnectionByRegion(article.Continent);
            Execute(connection, $"""
            INSERT INTO Articles (Title, Content, Author, Continent)
            VALUES ('{article.Title}', '{article.Content}', '{article.Author}', '{article.Continent}')
            """);
        }
    }
}