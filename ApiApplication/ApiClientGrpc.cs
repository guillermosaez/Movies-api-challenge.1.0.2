using System;
using System.Threading.Tasks;
using ApiApplication.Infrastructure.Cache;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using ProtoDefinitions;

namespace ApiApplication.Infrastructure.Grpc.MoviesApi;

public class ApiClientGrpc : IApiClientGrpc
{
    private readonly ProtoDefinitions.MoviesApi.MoviesApiClient _moviesApiClient;
    private readonly IConfiguration _configuration;
    private readonly ICacheClient _cacheClient;

    public ApiClientGrpc(
        ProtoDefinitions.MoviesApi.MoviesApiClient moviesApiClient,
        IConfiguration configuration,
        ICacheClient cacheClient
    )
    {
        _moviesApiClient = moviesApiClient;
        _configuration = configuration;
        _cacheClient = cacheClient;
    }
        
    public async Task<showListResponse> GetAllAsync()
    {
        const string cacheKey = "ApiClientGrpc:MoviesApi:GetAll";
        try
        {
            var all = await _moviesApiClient.GetAllAsync(new Empty(), GetDefaultHeaders());
            all.Data.TryUnpack<showListResponse>(out var data);
            return data;
        }
        catch
        {
            return await _cacheClient.GetAsync<showListResponse>(cacheKey);
        }
    }

    private Metadata GetDefaultHeaders()
    {
        return new()
        {
            { "X-Apikey", _configuration[ConfigurationKeyNames.MoviesApi.Key] }
        };
    }
}