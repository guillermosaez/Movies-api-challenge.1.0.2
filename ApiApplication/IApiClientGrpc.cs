using System.Threading.Tasks;
using ProtoDefinitions;

namespace ApiApplication;

public interface IApiClientGrpc
{
    Task<showListResponse> GetAll();
}