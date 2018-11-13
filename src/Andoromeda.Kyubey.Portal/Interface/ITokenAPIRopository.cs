using Andoromeda.Kyubey.Portal.Models;
using System.Threading.Tasks;

namespace Andoromeda.Kyubey.Portal.Interface
{
    public interface ITokenAPIRopository
    {
        Task<TokenContractPriceModel> GetTokenContractPriceAsync(string id);
    }
}
