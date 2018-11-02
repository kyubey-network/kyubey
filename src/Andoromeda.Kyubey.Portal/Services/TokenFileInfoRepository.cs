using Andoromeda.Kyubey.Portal.Interface;
using Andoromeda.Kyubey.Portal.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Andoromeda.Kyubey.Portal.Services
{
    public class TokenCultureFileSuffix
    {
        public const string ZHCN = ".zhcn";
        public const string ZHTW = ".zhtw";
        public const string EN = ".en";
        public const string JP = ".jajp";
    }
    public class TokenFileInfoRepository : ITokenRepository
    {
        private string tokenFolderAbsolutePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, @"Tokens");
        private string manifestFileName = "manifest.json";

        private string GetFileNameSuffixByCulture(string cultureStr)
        {
            if (new string[] { "en", "en-US", "en-GB" }.Contains(cultureStr))
                return TokenCultureFileSuffix.EN;
            if (new string[] { "zh", "zh-CN", "zh-Hans", "zh-Hans-CN", "zh-cn" }.Contains(cultureStr))
                return TokenCultureFileSuffix.ZHCN;
            if (new string[] { "zh-Hant", "zh-Hant-TW", "zh-TW", "zh-tw" }.Contains(cultureStr))
                return TokenCultureFileSuffix.ZHTW;
            if (new string[] { "ja", "ja-JP" }.Contains(cultureStr))
                return TokenCultureFileSuffix.JP;
            return "";
        }
        private string[] AvailuableCulcureFileSuffix = new string[] {
            TokenCultureFileSuffix.EN,
            TokenCultureFileSuffix.JP,
            TokenCultureFileSuffix.ZHCN,
            TokenCultureFileSuffix.ZHTW
        };
        /// <summary>
        /// get specified folder max available file count in any culture
        /// </summary>
        /// <param name="filePaths"></param>
        /// <returns></returns>
        private int GetMaxCountWithCulture(string[] filePaths)
        {
            var maxCount = 0;
            foreach (var f in filePaths)
            {
                var nameArr = f.Split('.');
                if (nameArr.Length > 1)
                {
                    var currFileNo = 0;
                    if (int.TryParse(nameArr[0], out currFileNo) && currFileNo > maxCount)
                    {
                        maxCount = currFileNo;
                    }
                }
            }
            return maxCount;
        }
        private string[] GetAvailuableFileNames(string[] fileNames, string cultureStr)
        {
            var cultureSuffix = GetFileNameSuffixByCulture(cultureStr);
            string[] availuableFilenames = null;
            if (!string.IsNullOrEmpty(cultureSuffix))
            {
                //current culture 
                availuableFilenames = fileNames.Where(x => x.Contains(cultureSuffix)).ToArray();
                if (availuableFilenames.Count() > 0)
                    return availuableFilenames;

                //zh-cn no file, -->zh-tw
                if (cultureSuffix == TokenCultureFileSuffix.ZHCN)
                    availuableFilenames = fileNames.Where(x => x.Contains(TokenCultureFileSuffix.ZHTW)).ToArray();
                if (availuableFilenames.Count() > 0)
                    return availuableFilenames;

                //zh-tw no file, -->zh-cn
                if (cultureSuffix == TokenCultureFileSuffix.ZHTW)
                    availuableFilenames = fileNames.Where(x => x.Contains(TokenCultureFileSuffix.ZHCN)).ToArray();
                if (availuableFilenames.Count() > 0)
                    return availuableFilenames;
            }
            //en
            availuableFilenames = fileNames.Where(x => x.Contains(TokenCultureFileSuffix.EN)).ToArray();
            if (availuableFilenames.Count() > 0)
                return availuableFilenames;

            //default
            availuableFilenames = fileNames.Where(x => !AvailuableCulcureFileSuffix.Any(c => x.Contains(c))).ToArray();
            return availuableFilenames;
        }

        public string[] GetTokenIncubationBannerPaths(string tokenId, string cultureStr)
        {
            tokenId.RemoveDangerousChar();
            var folderPath = Path.Combine(tokenFolderAbsolutePath, tokenId, "slides");
            var files = FileHelper.GetAllFileNameFromFolder(folderPath, "*.png");
            var availuableFiles = GetAvailuableFileNames(files, cultureStr);
            return availuableFiles;
        }


        public Models.TokenManifestJObject GetTokenInfoByTokenId(string tokenId)
        {
            tokenId.RemoveDangerousChar();
            var filePath = Path.Combine(tokenFolderAbsolutePath, tokenId, manifestFileName);
            if (File.Exists(filePath))
            {
                var fileContent = System.IO.File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<TokenManifestJObject>(fileContent);
            }
            return null;
        }

    }
}
