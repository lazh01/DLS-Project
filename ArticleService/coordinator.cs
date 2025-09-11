using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;

public class Coordinator
{
    private IDictionary<string, DbConnection> ConnectionCache = new Dictionary<string, DbConnection>();
    private const string AFRICA_DB = "articles-africa-db";
    private const string ASIA_DB = "articles-asia-db";
    private const string ANTARCTICA_DB = "articles-antarctica-db";
    private const string EUROPE_DB = "articles-europe-db";
    private const string NORTH_AMERICA_DB = "articles-north-america-db";
    private const string SOUTH_AMERICA_DB = "articles-south-america-db";
    private const string OCEANIA_DB = "articles-oceania-db";
    private const string GLOBAL_DB = "articles-global-db";

    private DbConnection GetConnectionByServerName(string serverName)
    {
        if (ConnectionCache.TryGetValue(serverName, out var connection))
        {
            return connection;
        }

        connection = new SqlConnection($"Server={serverName};User Id=sa;Password=SuperSecret7!;Encrypt=false;");
        connection.Open();
        ConnectionCache.Add(serverName, connection);
        return connection;
    }
}