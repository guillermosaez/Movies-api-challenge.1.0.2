using System.Threading.Tasks;
using ProtoDefinitions;

namespace ApiApplication.Infrastructure.Grpc.MoviesApi;

public interface IApiClientGrpc
{
    Task<showListResponse> GetAllAsync();
}