using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
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


    public async Task<DbConnection> GetConnectionByRegion(Continent region)
    {
        return region switch
        {
            Continent.africa => await GetConnectionByServerName(AFRICA_DB),
            Continent.asia => await GetConnectionByServerName(ASIA_DB),
            Continent.antarctica => await GetConnectionByServerName(ANTARCTICA_DB),
            Continent.europe => await GetConnectionByServerName(EUROPE_DB),
            Continent.north_america => await GetConnectionByServerName(NORTH_AMERICA_DB),
            Continent.south_america => await GetConnectionByServerName(SOUTH_AMERICA_DB),
            Continent.oceania => await GetConnectionByServerName(OCEANIA_DB),
            Continent.global => await GetConnectionByServerName(GLOBAL_DB),
            _ => throw new ArgumentException($"Unknown region: {region}")
        };
    }

        public async IAsyncEnumerable<DbConnection> GetAllConnections()
        {
            yield return await GetConnectionByServerName(AFRICA_DB);
            yield return await GetConnectionByServerName(ASIA_DB);
            yield return await GetConnectionByServerName(ANTARCTICA_DB);
            yield return await GetConnectionByServerName(EUROPE_DB);
            yield return await GetConnectionByServerName(NORTH_AMERICA_DB);
            yield return await GetConnectionByServerName(SOUTH_AMERICA_DB);
            yield return await GetConnectionByServerName(OCEANIA_DB);
            yield return await GetConnectionByServerName(GLOBAL_DB);
        }

    private async Task<DbConnection> GetConnectionByServerName(string serverName)
    {
        if (ConnectionCache.TryGetValue(serverName, out var connection))
        {
            return connection;
        }

        connection = new SqlConnection($"Server={serverName};User Id=sa;Password=SuperSecret7!;Encrypt=false;");
        await connection.OpenAsync();
        ConnectionCache.Add(serverName, connection);
        return connection;
    }
}