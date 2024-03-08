using System.Threading.Tasks;

namespace ApiApplication.Infrastructure.Cache;

public interface ICacheClient
{
    Task StoreAsync<T>(string key, T data);
    Task<T> GetAsync<T>(string key);
}