using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Andoromeda.Kyubey.TokenSyncWorker
{
    public class FtpHelper
    {
        private readonly string _hostAddress = null;
        private readonly string _username = null;
        private readonly string _password = null;
        private FtpWebRequest _ftpRequest = null;
        //private FtpWebResponse _ftpResponse = null;

        // Constructor
        public FtpHelper(string hostAddress, string username, string password)
        {
            _hostAddress = hostAddress;
            _username = username;
            _password = password;
        }

        /// <summary>
        /// Download File
        /// </summary>
        /// <param name="remoteFile"></param>
        /// <param name="localFile"></param>
        public void Download(string remoteFile, string localFile)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Credentials = new NetworkCredential(_username, _password);
                    client.DownloadFile((_hostAddress + "/" + remoteFile).Trim(), localFile);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }


        public bool Upload(string localFile, string uri)
        {
            try
            {
                _ftpRequest = (FtpWebRequest)WebRequest.Create(uri);
                _ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;

                // This example assumes the FTP site uses anonymous logon.
                _ftpRequest.Credentials = new NetworkCredential(_username.Normalize(), _password.Normalize());

                // Copy the contents of the file to the request stream.
                byte[] fileContents;

                fileContents = File.ReadAllBytes(localFile);

                _ftpRequest.UseBinary = true;

                _ftpRequest.ContentLength = fileContents.Length;

                using (Stream requestStream = _ftpRequest.GetRequestStream())
                {
                    requestStream.Write(fileContents, 0, fileContents.Length);
                }

                FtpWebResponse response = (FtpWebResponse)_ftpRequest.GetResponse();


                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{DateTime.Now} Upload File Succeed:{uri}");
                Console.ResetColor();
                response.Dispose();
                return true;
            }
            catch (WebException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{DateTime.Now} Upload File Failed:{uri}");
                Console.ResetColor();
                return false;
            }
        }

        /// <summary>
        /// Create a New Directory on the FTP Server
        /// </summary>
        /// <param name="newDirectory"></param>
        public bool CreateDirectory(string directoryUri)
        {
            try
            {
                //create the directory
                FtpWebRequest requestDir = (FtpWebRequest)FtpWebRequest.Create(new Uri(directoryUri));
                requestDir.Method = WebRequestMethods.Ftp.MakeDirectory;
                requestDir.Credentials = new NetworkCredential(_username.Normalize(), _password.Normalize());
                requestDir.UsePassive = true;
                requestDir.UseBinary = true;
                requestDir.KeepAlive = false;
                FtpWebResponse response = (FtpWebResponse)requestDir.GetResponse();
                Stream ftpStream = response.GetResponseStream();

                ftpStream.Close();
                response.Close();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{DateTime.Now} Create Folder Succeed:{directoryUri}");
                Console.ResetColor();
                return true;
            }
            catch (WebException ex)
            {
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    response.Close();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"{DateTime.Now} Create Folder Succeed:{directoryUri}");
                    Console.ResetColor();
                    return true;
                }
                else
                {
                    response.Close();
                    
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"{DateTime.Now} Create Folder Failed:{directoryUri}");
                    Console.ResetColor();
                    return false;
                }
            }
        }
        
    }
}
