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
        static string ftpAddress = null;
        static string ftpUserName = null;
        static string ftpPassword = null;
        static void Main(string[] args)
        {
            Console.WriteLine($"{DateTime.Now}:loading...");

            var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        ;
            var configuration = builder.Build();


            ftpAddress = configuration["FtpAddress"];
            ftpUserName = configuration["FtpUserName"];
            ftpPassword = configuration["FtpPassword"];

            var branchName = "master";
            var downloadFile = $"{branchName}.zip";

            Console.WriteLine($"{DateTime.Now}:delete exist files");
            if (File.Exists(downloadFile))
            {
                File.Delete(downloadFile);
            }
            if (Directory.Exists(branchName))
            {
                Directory.Delete(branchName, true);
            }

            //download
            Console.WriteLine($"{DateTime.Now}:start download");
            string targetUrl = "https://github.com/kyubey-network/token-list/archive/master.zip";
            DownloadFile(targetUrl, downloadFile);
            Console.WriteLine($"{DateTime.Now}:download complete");


            //unzip
            Console.WriteLine($"{DateTime.Now}:start unzip");
            string zipPath = downloadFile;
            string extractPath = branchName;
            ZipFile.ExtractToDirectory(zipPath, extractPath, true);
            Console.WriteLine($"{DateTime.Now}:unzip complete");


            //upload
            Console.WriteLine($"{DateTime.Now}:start upload");
            ftpClient = new FtpHelper(ftpAddress, ftpUserName, ftpPassword);
            ftpClient.CreateDirectory(Path.Combine(ftpAddress, "Tokens"));
            UploadFolder(Path.Combine(branchName, "token-list-master"), "Tokens");
            Console.WriteLine($"{DateTime.Now}:upload complete");


            //Sync Data
            var optionsBuilder = new DbContextOptionsBuilder<KyubeyContext>();
            optionsBuilder.UseMySql(configuration["MySQL"]);
            var dbContext = new KyubeyContext(optionsBuilder.Options);

            var tokensFolder = Path.Combine(branchName, "token-list-master");
            var dbSyncService = new TokenInfoSyncServices(tokensFolder, dbContext);
            dbSyncService.SyncTokenInfo();


            Console.WriteLine("Done.");
            Thread.Sleep(10 * 1000);
            //Console.ReadKey();
        }


        public static void UploadFolder(string path, string ftpPath)
        {
            int repeatIndex = 1;
            var rootDir = new DirectoryInfo(path);
            var dirs = rootDir.GetDirectories();
            foreach (var d in dirs)
            {
                var currentFtpPath = Path.Combine(ftpAddress, ftpPath, d.Name);
                while (!ftpClient.CreateDirectory(currentFtpPath) && (repeatIndex < 4))
                {
                    Thread.Sleep(300 * RecUrsive(repeatIndex++));
                }
                repeatIndex = 1;
                UploadFolder(Path.Combine(path, d.Name), Path.Combine(ftpPath, d.Name));
            }

            var files = rootDir.GetFiles();
            foreach (var f in files)
            {
                var currentPhysicalPath = Path.Combine(path, f.Name);
                var currentFtpPath = Path.Combine(ftpAddress, ftpPath, f.Name);
                while (!ftpClient.Upload(currentPhysicalPath, currentFtpPath) && (repeatIndex < 8))
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
        public static void DownloadFile(string uri, string fileName)
        {
            using (var client = new System.Net.WebClient())
            {
                client.DownloadFile(uri, fileName);
            }
        }
    }
}

