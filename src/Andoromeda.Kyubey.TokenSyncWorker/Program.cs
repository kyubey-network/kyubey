using Andoromeda.Kyubey.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace Andoromeda.Kyubey.TokenSyncWorker
{
    class Program
    {
        static FtpHelper ftpClient = null;
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        ;

            var configuration = builder.Build();


            var ftpAddress = configuration["FtpAddress"];
            var ftpUserName = configuration["FtpUserName"];
            var ftpPassword = configuration["FtpPassword"];







            var branchName = "master";
            var downloadFile = $"{branchName}.zip";

            //if (File.Exists(downloadFile))
            //{
            //    File.Delete(downloadFile);
            //}

            ////download
            //Console.WriteLine($"{DateTime.Now}:start download");
            //string targetUrl = "https://github.com/kyubey-network/token-list/archive/master.zip";
            //DownloadFile(targetUrl, downloadFile);
            //Console.WriteLine($"{DateTime.Now}:download complete");


            ////unzip
            //Console.WriteLine($"{DateTime.Now}:start unzip");
            //string zipPath = downloadFile;
            //string extractPath = branchName;
            //ZipFile.ExtractToDirectory(zipPath, extractPath, true);
            //Console.WriteLine($"{DateTime.Now}:unzip complete");


            ////upload
            //Console.WriteLine($"{DateTime.Now}:start upload");
            //ftpClient = new FtpHelper(ftpAddress, ftpUserName, ftpPassword);
            //UploadFolder(Path.Combine(branchName, "token-list-master"));
            //Console.WriteLine($"{DateTime.Now}:upload complete");


            //Sync Data
            var optionsBuilder = new DbContextOptionsBuilder<KyubeyContext>();
            optionsBuilder.UseMySql(configuration["MySQL"]);
            var dbContext = new KyubeyContext(optionsBuilder.Options);

            var tokensFolder = Path.Combine(branchName, "token-list-master");
            var dbSyncService = new TokenInfoSyncServices(tokensFolder, dbContext);
            dbSyncService.SyncTokenInfo();


            Console.WriteLine("Done.");
            Console.ReadKey();
        }


        public static void UploadFolder(string path)
        {
            int repeatIndex = 1;
            while (!ftpClient.CreateDirectory(path) && (repeatIndex < 4))
            {
                Thread.Sleep(300 * RecUrsive(repeatIndex++));
            }
            repeatIndex = 1;

            var rootDir = new DirectoryInfo(path);
            var dirs = rootDir.GetDirectories();
            foreach (var d in dirs)
            {
                var currentPath = Path.Combine(path, d.Name);
                UploadFolder(currentPath);
            }
            var files = rootDir.GetFiles();
            foreach (var f in files)
            {
                var currentPath = Path.Combine(path, f.Name);
                while (!ftpClient.Upload(currentPath, currentPath) && (repeatIndex < 8))
                {
                    Thread.Sleep(300 * RecUrsive(repeatIndex++));
                }
                repeatIndex = 1;
            }
        }
        public static int RecUrsive(int index)
        {
            if (index < 3)//若index的值等于1或2，则返回1
            {
                return 1;
            }
            else
            {
                return RecUrsive(index - 1) + RecUrsive(index - 2);
            }
        }
        //public static async Task<bool> CreateFtpFolderAsync(string ftpFolderPath)
        //{
        //    var request = GetFtpWebRequest(ftpFolderPath);

        //    request.Method = WebRequestMethods.Ftp.MakeDirectory;

        //    using (await request.GetResponseAsync()) { }
        //}
        //public static async Task CreateFtpFolderAsync(string ftpFolderPath)
        //{
        //    var request = GetFtpWebRequest(ftpFolderPath);

        //    request.Method = WebRequestMethods.Ftp.MakeDirectory;

        //    using (await request.GetResponseAsync()) { }
        //}
        //public static async Task UploadFtpFile(string localFileName, string remoteFileName)
        //{
        //    remoteFileName = remoteFileName ?? Path.GetFileName(localFileName);

        //    var request = GetFtpWebRequest(remoteFileName);

        //    request.Method = WebRequestMethods.Ftp.UploadFile;
        //    request.ContentLength = new FileInfo(localFileName).Length;

        //    using (var requestStream = await request.GetRequestStreamAsync())
        //    {
        //        using (var fs = new FileStream(localFileName, FileMode.Open))
        //            await fs.CopyToAsync(requestStream);
        //    }

        //    using (await request.GetResponseAsync()) { }
        //}
        public static void DownloadFile(string uri, string fileName)
        {
            using (var client = new System.Net.WebClient())
            {
                client.DownloadFile(uri, fileName);
            }
        }


        //private static FtpWebRequest GetFtpWebRequest(string remoteFileName)
        //{
        //    var request = (FtpWebRequest)WebRequest.Create(ftpAddress + "/" + remoteFileName);
        //    request.Credentials = new NetworkCredential(ftpUserName ?? "anonymous", ftpPassword);
        //    return request;
        //}
    }
}

