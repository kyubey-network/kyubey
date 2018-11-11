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
        public const string ZHCN = ".zh";
        public const string ZHTW = ".zh-Hant";
        public const string EN = ".en";
        public const string JP = ".ja";
    }
    public class TokenFileInfoRepository : ITokenRepository
    {
        private const string tokenFolderRelativePath = @"Tokens";
        private string tokenFolderAbsolutePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, tokenFolderRelativePath);
        private const string manifestFileName = "manifest.json";
        private const string iconFileName = "icon.png";

        private string GetFileNameSuffixByCulture(string cultureStr)
        {
            if (new string[] { "en" }.Contains(cultureStr))
                return TokenCultureFileSuffix.EN;
            if (new string[] { "zh" }.Contains(cultureStr))
                return TokenCultureFileSuffix.ZHCN;
            if (new string[] { "zh-Hant" }.Contains(cultureStr))
                return TokenCultureFileSuffix.ZHTW;
            if (new string[] { "ja" }.Contains(cultureStr))
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
        private string[] GetAvailableFileNames(string[] fileNames, string cultureStr)
        {
            var cultureSuffix = GetFileNameSuffixByCulture(cultureStr);
            string[] availuableFilenames = null;
            if (!string.IsNullOrEmpty(cultureSuffix))
            {
                //current culture 
                availuableFilenames = fileNames.Where(x => x.Contains(cultureSuffix + ".")).ToArray();
                if (availuableFilenames.Count() > 0)
                    return availuableFilenames;

                //zh-cn no file, -->zh-tw
                if (cultureSuffix == TokenCultureFileSuffix.ZHCN)
                    availuableFilenames = fileNames.Where(x => x.Contains(TokenCultureFileSuffix.ZHTW + ".")).ToArray();
                if (availuableFilenames.Count() > 0)
                    return availuableFilenames;

                //zh-tw no file, -->zh-cn
                if (cultureSuffix == TokenCultureFileSuffix.ZHTW)
                    availuableFilenames = fileNames.Where(x => x.Contains(TokenCultureFileSuffix.ZHCN + ".")).ToArray();
                if (availuableFilenames.Count() > 0)
                    return availuableFilenames;
            }
            //en
            availuableFilenames = fileNames.Where(x => x.Contains(TokenCultureFileSuffix.EN + ".")).ToArray();
            if (availuableFilenames.Count() > 0)
                return availuableFilenames;

            //default
            availuableFilenames = fileNames.Where(x => !AvailuableCulcureFileSuffix.Any(c => x.Contains(c))).ToArray();
            return availuableFilenames;
        }

        public string[] GetTokenIncubationBannerPaths(string tokenId, string cultureStr)
        {
            var folderPath = Path.Combine(tokenFolderAbsolutePath, tokenId, "slides");
            if (!Directory.Exists(folderPath))
            {
                return null;
            }
            var files = FileHelper.GetAllFileNameFromFolder(folderPath, "*.png");
            var availableFiles = GetAvailableFileNames(files, cultureStr);
            var availablePaths = availableFiles.Select(x => Path.Combine(tokenFolderRelativePath, x)).ToArray();
            return availablePaths;
        }

        public string GetTokenIncubationDescription(string tokenId, string cultureStr)
        {
            var folderPath = Path.Combine(tokenFolderAbsolutePath, tokenId, "incubator");
            if (!Directory.Exists(folderPath))
            {
                return null;
            }
            var files = FileHelper.GetAllFileNameFromFolder(folderPath, "description.*.txt");
            var availableFiles = GetAvailableFileNames(files, cultureStr);
            var availablePath = availableFiles.Select(x => Path.Combine(folderPath, x)).FirstOrDefault();
            if (availablePath != null)
                return FileHelper.ReadAllText(availablePath);
            return null;
        }

        public string GetTokenIncubationDetail(string tokenId, string cultureStr)
        {
            var folderPath = Path.Combine(tokenFolderAbsolutePath, tokenId, "incubator");
            if (!Directory.Exists(folderPath))
            {
                return null;
            }
            var files = FileHelper.GetAllFileNameFromFolder(folderPath, "detail.*.md");
            var availableFiles = GetAvailableFileNames(files, cultureStr);
            var availablePath = availableFiles.Select(x => Path.Combine(folderPath, x)).FirstOrDefault();
            if (availablePath != null)
                return FileHelper.ReadAllText(availablePath);
            return null;
        }

        public Models.TokenManifestJObject GetOne(string tokenId)
        {
            var filePath = Path.Combine(tokenFolderAbsolutePath, tokenId, manifestFileName);
            if (File.Exists(filePath))
            {
                var fileContent = System.IO.File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<TokenManifestJObject>(fileContent);
            }
            return null;
        }

        public IList<TokenManifestJObject> GetAll()
        {
            var tokenFolderDirectories = Directory.GetDirectories(tokenFolderAbsolutePath);
            var result = new List<TokenManifestJObject>();
            foreach (var t in tokenFolderDirectories)
            {
                result.Add(GetOne(t));
            }
            return result;
        }

        public string GetTokenIconPath(string tokenId)
        {
            var absolutePath = Path.Combine(tokenFolderAbsolutePath, tokenId, iconFileName);
            if (File.Exists(absolutePath))
            {
                return absolutePath;
            }
            return null;
        }

        public string GetPriceJsText(string tokenId)
        {
            var filePath = Path.Combine(tokenFolderAbsolutePath, tokenId, "contract_exchange", "price.js");
            if (File.Exists(filePath))
            {
                return FileHelper.ReadAllText(filePath);
            }
            return null;
        }

        public List<TokenIncubatorUpdateModel> GetTokenIncubatorUpdates(string tokenId, string cultureStr)
        {
            var folderPath = Path.Combine(tokenFolderAbsolutePath, tokenId, "updates");
            if (!Directory.Exists(folderPath))
            {
                return null;
            }
            var files = FileHelper.GetAllFileNameFromFolder(folderPath);
            var availableFiles = GetAvailableFileNames(files, cultureStr);
            var mainFile = availableFiles.FirstOrDefault(x => x.EndsWith(".json"));
            if (!string.IsNullOrWhiteSpace(mainFile))
            {
                var updateList = JsonConvert.DeserializeObject<List<TokenIncubatorUpdateModel>>(System.IO.File.ReadAllText(mainFile));
                if (updateList != null)
                {
                    foreach (var u in updateList)
                    {
                        var contentPath = Path.Combine(folderPath, u.Content);
                        if (!string.IsNullOrWhiteSpace(u.Content) && File.Exists(contentPath))
                        {
                            u.Content = System.IO.File.ReadAllText(contentPath);
                        }
                    }
                }
                return updateList;
            }
            return null;
        }
    }
}
