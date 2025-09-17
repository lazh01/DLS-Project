using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace ArticleService;

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


    public DbConnection GetConnectionByRegion(string region)
    {
        return region.ToLower() switch
        {
            "africa" => GetConnectionByServerName(AFRICA_DB),
            "asia" => GetConnectionByServerName(ASIA_DB),
            "antarctica" => GetConnectionByServerName(ANTARCTICA_DB),
            "europe" => GetConnectionByServerName(EUROPE_DB),
            "north america" => GetConnectionByServerName(NORTH_AMERICA_DB),
            "south america" => GetConnectionByServerName(SOUTH_AMERICA_DB),
            "oceania" => GetConnectionByServerName(OCEANIA_DB),
            "global" => GetConnectionByServerName(GLOBAL_DB),
            _ => throw new ArgumentException($"Unknown region: {region}")
        };
    }

        public IEnumerable<DbConnection> GetAllConnections()
        {
            yield return GetConnectionByServerName(AFRICA_DB);
            yield return GetConnectionByServerName(ASIA_DB);
            yield return GetConnectionByServerName(ANTARCTICA_DB);
            yield return GetConnectionByServerName(EUROPE_DB);
            yield return GetConnectionByServerName(NORTH_AMERICA_DB);
            yield return GetConnectionByServerName(SOUTH_AMERICA_DB);
            yield return GetConnectionByServerName(OCEANIA_DB);
            yield return GetConnectionByServerName(GLOBAL_DB);
        }

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