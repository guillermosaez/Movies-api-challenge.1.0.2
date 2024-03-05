using System.Net.Http;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using ProtoDefinitions;

namespace ApiApplication
{
    public class ApiClientGrpc : IApiClientGrpc
    {
        private readonly IConfiguration _configuration;

        public ApiClientGrpc(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public async Task<showListResponse> GetAll()
        {
            var httpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            var channel =
                GrpcChannel.ForAddress("https://localhost:7443", new GrpcChannelOptions()
                {
                    HttpHandler = httpHandler
                });
            var client = new MoviesApi.MoviesApiClient(channel);

            var headers = new Metadata
            {
                { "X-Apikey", _configuration[ConfigurationKeyNames.MoviesApi.Key] }
            };
            var all = await client.GetAllAsync(new Empty(), headers);
            all.Data.TryUnpack<showListResponse>(out var data);
            return data;
        }
    }
}