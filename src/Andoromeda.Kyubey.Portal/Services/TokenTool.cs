using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Andoromeda.Kyubey.Portal.Services
{
    public class TokenTool
    {
        private const string tokenRoutePrefix = "/token_assets";
        public static string GetTokenIncubatorBannerUri(string tokenId, string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return Path.Combine("img", "null.png");
            }
            string fileName = Path.GetFileName(filePath);
            return $"/Token/tokenbanner/{tokenId}/{fileName}";
        }
        public static List<string> GetTokenIncubatorBannerUris(string tokenId, string[] filePaths)
        {
            var result = filePaths.Select(x => GetTokenIncubatorBannerUri(tokenId, x)).ToList();
            return result;
        }
    }
}
