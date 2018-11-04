using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Andoromeda.Kyubey.TokenSyncWorker
{
    class Program
    {
        static string ftpAddress = @"ftp://cnws-prod-sha20-001.ftp.chinacloudsites.chinacloudapi.cn/site/wwwroot";
        static string ftpUserName = @"kyubey-stage\$kyubey-stage";
        static string ftpPassword = "D0fFaniS5Fxbigqkj4jRdosXXi5Dd1XviSBix0dNbpuNAM9pYrKwcDtsTHem";
        static FtpHelper ftpClient = null;
        static void Main(string[] args)
        {
            ftpClient = new FtpHelper(ftpAddress, ftpUserName, ftpPassword);
            UploadFolder("master");


            //// Setup session options
            //SessionOptions sessionOptions = new SessionOptions
            //{
            //    Protocol = Protocol.Sftp,
            //    HostName = "example.com",
            //    UserName = "user",
            //    Password = "mypassword",
            //    SshHostKeyFingerprint = "ssh-rsa 2048 xx:xx:xx:xx:xx:xx:xx:xx:..."
            //};

            //using (Session session = new Session())
            //{
            //    // Connect
            //    session.Open(sessionOptions);

            //    // Upload files
            //    TransferOptions transferOptions = new TransferOptions();
            //    transferOptions.TransferMode = TransferMode.Binary;

            //    TransferOperationResult transferResult;
            //    transferResult =
            //        session.PutFiles(@"d:\toupload\*", "/home/user/", false, transferOptions);

            //    // Throw on any error
            //    transferResult.Check();

            //    // Print results
            //    foreach (TransferEventArgs transfer in transferResult.Transfers)
            //    {
            //        Console.WriteLine("Upload of {0} succeeded", transfer.FileName);
            //    }
            //}

            //return 0;



            //Upload("testfile.txt").Wait();



            //var branchName = "master";
            //var downloadFile = $"{branchName}.zip";
            //string targetUrl = "https://github.com/kyubey-network/token-list/archive/master.zip";
            //DownloadFile(targetUrl, downloadFile);
            //string zipPath = downloadFile;
            //string extractPath = branchName;

            //ZipFile.ExtractToDirectory(zipPath, extractPath, true);
            Console.WriteLine("Done.");
            Console.ReadKey();
        }

        public static void UploadFolder(string path)
        {
            ftpClient.CreateDirectory(path);
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
                //ftpClient.Upload(currentPath, currentPath);
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
        //public static void DownloadFile(string uri, string fileName)
        //{
        //    using (var client = new System.Net.WebClient())
        //    {
        //        client.DownloadFile(uri, fileName);
        //    }
        //}


        //private static FtpWebRequest GetFtpWebRequest(string remoteFileName)
        //{
        //    var request = (FtpWebRequest)WebRequest.Create(ftpAddress + "/" + remoteFileName);
        //    request.Credentials = new NetworkCredential(ftpUserName ?? "anonymous", ftpPassword);
        //    return request;
        //}
    }
}

