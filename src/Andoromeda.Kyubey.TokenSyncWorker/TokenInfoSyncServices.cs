using Andoromeda.Kyubey.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
namespace Andoromeda.Kyubey.TokenSyncWorker
{
    public class TokenInfoSyncServices
    {
        private string _tokenInfoPath;
        private const string manifestFileName = "manifest.json";
        private KyubeyContext _dbContext;
        public TokenInfoSyncServices(string tokenInfoPath, KyubeyContext dbContext)
        {
            _dbContext = dbContext;
            _tokenInfoPath = tokenInfoPath;
        }
        private TokenInfoSyncServices() { }
        private List<string> GetTokenFolders()
        {
            var rootDir = new DirectoryInfo(_tokenInfoPath);
            var tokenfolders = new List<string>();
            foreach (var f in rootDir.GetDirectories())
            {
                if (f.Name.ToUpper() == f.Name)
                    tokenfolders.Add(f.Name);
            }
            return tokenfolders;
        }

        public void SyncTokenInfo()
        {
            var tokenIds = GetTokenFolders();
            var existed = _dbContext.Tokens.ToList();
            var tokensToRemove = existed.Where(x => !tokenIds.Contains(x.Id));
            _dbContext.RemoveRange(tokensToRemove);
            _dbContext.SaveChanges();
            tokenIds.ForEach(x => SyncOneToken(GetTokenObj(x)));
        }
        private TokenManifestJObject GetTokenObj(string name)
        {
            var filePath = Path.Combine(_tokenInfoPath, name, manifestFileName);
            if (File.Exists(filePath))
            {
                var ordinalText = File.ReadAllText(filePath);
                try
                {
                    var rObj = JsonConvert.DeserializeObject<TokenManifestJObject>(ordinalText);
                    return rObj;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            return null;
        }
        public void SyncOneToken(TokenManifestJObject obj)
        {
            Console.WriteLine("Inserting " + obj.Id);
            var token = _dbContext.Tokens.FirstOrDefault(x => x.Id == obj.Id);
            if (token != null)
            {
                token.Priority = obj.Priority;
                token.HasIncubation = obj.Incubation != null;
                token.HasDex = obj.Dex;
                token.HasContractExchange = obj.Contract_Exchange;
                _dbContext.SaveChanges();
            }
            else
            {
                token = new Token
                {
                    Id = obj.Id,
                    Priority = obj.Priority,
                    HasIncubation = obj.Incubation != null,
                    HasDex = obj.Dex,
                    HasContractExchange = obj.Contract_Exchange
                };
                _dbContext.Tokens.Add(token);
                _dbContext.SaveChanges();
            }
        }
    }
}
