using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using ProtoDefinitions;

namespace ApiApplication.Infrastructure.Grpc.MoviesApi;

public class ApiClientGrpc : IApiClientGrpc
{
    private readonly ProtoDefinitions.MoviesApi.MoviesApiClient _moviesApiClient;
    private readonly IConfiguration _configuration;

    public ApiClientGrpc(ProtoDefinitions.MoviesApi.MoviesApiClient moviesApiClient, IConfiguration configuration)
    {
        _moviesApiClient = moviesApiClient;
        _configuration = configuration;
    }
        
    public async Task<showListResponse> GetAllAsync()
    {
        var headers = new Metadata
        {
            { "X-Apikey", _configuration[ConfigurationKeyNames.MoviesApi.Key] }
        };
        var all = await _moviesApiClient.GetAllAsync(new Empty(), headers);
        all.Data.TryUnpack<showListResponse>(out var data);
        return data;
    }
}