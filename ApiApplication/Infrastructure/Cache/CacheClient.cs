using System.Text.Json;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace ApiApplication.Infrastructure.Cache;

public class CacheClient : ICacheClient
{
    private readonly IDatabase _database;

    public CacheClient(IDatabase database)
    {
        _database = database;
    }

    public async Task StoreAsync<T>(string key, T data)
    {
        var serializedData = JsonSerializer.Serialize(data);
        await _database.StringSetAsync(key, serializedData);
    }

    public async Task<T> GetAsync<T>(string key)
    {
        var cachedElement = await _database.StringGetAsync(key);
        return JsonSerializer.Deserialize<T>(cachedElement);
    }
}