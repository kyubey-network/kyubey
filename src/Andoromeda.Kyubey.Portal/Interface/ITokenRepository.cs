using Andoromeda.Kyubey.Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Andoromeda.Kyubey.Portal.Interface
{
    public interface ITokenRepository : IBaseRepository<TokenManifestJObject>
    {
        TokenManifestJObject GetTokenInfoByTokenId(string tokenId);
        string[] GetTokenIncubationBannerPaths(string tokenId, string cultureStr);
    }
}
