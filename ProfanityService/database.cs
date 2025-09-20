using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProfanityService
{
    public class Database
    {
        private static Database instance = new Database();
        private readonly static String connectionString = $"Server={"profanity-db"};User Id=sa;Password=SuperSecret7!;Encrypt=false;";
        public static Database GetInstance()
        {
            return instance;
        }

        public async Task DeleteDatabase()
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            try
            {
                Execute(connection, "DROP TABLE IF EXISTS Profanities");
            }
            finally
            {
                await connection.CloseAsync();
            }
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

        public async Task CreateDatabase()
        {
            Console.WriteLine("Creating Profanities table in database.");
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            try 
            {
                Execute(connection, "CREATE TABLE Profanities (Word NVARCHAR(100) PRIMARY KEY)");
                await LoadJsonToDatabaseAsync("Profanities/words.json", connection);
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        public static async Task<List<string>> GetAllWordsAsync()
        {
            var words = new List<string>();
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            try
            {
                using SqlCommand cmd = new SqlCommand("SELECT Word FROM Profanities;", connection);
                using SqlDataReader reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    words.Add(reader.GetString(0));
                }
            }
            finally
            {
                await connection.CloseAsync();
            }

            return words;
        }

        public static async Task LoadJsonToDatabaseAsync(string jsonFilePath, SqlConnection connection)
        {
            if (!File.Exists(jsonFilePath))
            {
                throw new FileNotFoundException("JSON file not found", jsonFilePath);
            }

            // Read JSON file
            string jsonContent = await File.ReadAllTextAsync(jsonFilePath);

            // Deserialize into a list of strings
            List<string> words = JsonSerializer.Deserialize<List<string>>(jsonContent)
                                    ?? new List<string>();

            if (words.Count == 0)
            {
                Console.WriteLine("No words found in JSON.");
                return;
            }

            foreach (var word in words)
            {
                // Simple INSERT (assumes Profanities table has a column named Word)
                using SqlCommand cmd = new SqlCommand(
                    "INSERT INTO Profanities (Word) VALUES (@word);", connection);
                cmd.Parameters.AddWithValue("@word", word.Trim());
                await cmd.ExecuteNonQueryAsync();
            }

            Console.WriteLine($"Inserted {words.Count} words into Profanities table.");
        }
    }
}