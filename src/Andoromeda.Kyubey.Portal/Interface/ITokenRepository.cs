using Andoromeda.Kyubey.Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Andoromeda.Kyubey.Portal.Interface
{
    public interface ITokenRepository
    {
        TokenManifestJObject GetOne(string tokenId);
        IList<TokenManifestJObject> GetAll();
        string[] GetTokenIncubationBannerPaths(string tokenId, string cultureStr);
        string GetTokenIconPath(string tokenId);
        string GetTokenIncubationDescription(string tokenId, string cultureStr);
        string GetTokenIncubationDetail(string tokenId, string cultureStr);
        List<TokenIncubatorUpdateModel> GetTokenIncubatorUpdates(string tokenId, string cultureStr);
        string GetPriceJsText(string tokenId);
    }
}
