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
                using (StreamReader sourceStream = new StreamReader(localFile))
                {
                    fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
                }

                _ftpRequest.ContentLength = fileContents.Length;

                using (Stream requestStream = _ftpRequest.GetRequestStream())
                {
                    requestStream.Write(fileContents, 0, fileContents.Length);
                }

                FtpWebResponse response = (FtpWebResponse)_ftpRequest.GetResponse();


                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Upload Succeed:{localFile}");
                Console.ResetColor();
                response.Dispose();
                return true;
            }
            catch (WebException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Upload Failed:{ex.HResult} {localFile}");
                Console.ResetColor();
                return false;
            }
        }

        ///// <summary>
        ///// Delete File
        ///// </summary>
        ///// <param name="deleteFile"></param>
        //public string Delete(string deleteFile)
        //{
        //    try
        //    {
        //        // Create an FTP Request
        //        _ftpRequest = (FtpWebRequest)WebRequest.Create(_hostAddress + "/" + deleteFile);
        //        // Log in to the FTP Server with the User Name and Password Provided
        //        _ftpRequest.Credentials = new NetworkCredential(_username, _password);
        //        // Specify the Type of FTP Request
        //        _ftpRequest.Method = WebRequestMethods.Ftp.DeleteFile;
        //        // Establish Return Communication with the FTP Server
        //        using (_ftpResponse = (FtpWebResponse)_ftpRequest.GetResponse())
        //        {
        //            return _ftpResponse.StatusDescription;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.ToString());
        //        throw ex;
        //    }
        //}

        ///// <summary>
        ///// Delete Dir
        ///// </summary>
        ///// <param name="directoryPath"></param>
        //public string DeleteDir(string directoryPath)
        //{
        //    try
        //    {
        //        // Create an FTP Request
        //        _ftpRequest = (FtpWebRequest)WebRequest.Create(_hostAddress + "/" + directoryPath);
        //        // Log in to the FTP Server with the User Name and Password Provided
        //        _ftpRequest.Credentials = new NetworkCredential(_username, _password);
        //        // Specify the Type of FTP Request
        //        _ftpRequest.Method = WebRequestMethods.Ftp.RemoveDirectory;
        //        // Establish Return Communication with the FTP Server
        //        using (_ftpResponse = (FtpWebResponse)_ftpRequest.GetResponse())
        //        {
        //            return _ftpResponse.StatusDescription;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.ToString());
        //        throw ex;
        //    }
        //}

        ///// <summary>
        ///// Rename File
        ///// </summary>
        ///// <param name="currentFileNameAndPath"></param>
        ///// <param name="newFileName"></param>
        //public string Rename(string currentFileNameAndPath, string newFileName)
        //{
        //    try
        //    {
        //        // Create an FTP Request
        //        _ftpRequest = (FtpWebRequest)WebRequest.Create(_hostAddress + "/" + currentFileNameAndPath);
        //        // Log in to the FTP Server with the User Name and Password Provided
        //        _ftpRequest.Credentials = new NetworkCredential(_username, _password);
        //        // Specify the Type of FTP Request
        //        _ftpRequest.Method = WebRequestMethods.Ftp.Rename;
        //        // Rename the File
        //        _ftpRequest.RenameTo = newFileName;
        //        // Establish Return Communication with the FTP Server
        //        using (_ftpResponse = (FtpWebResponse)_ftpRequest.GetResponse())
        //        {
        //            return _ftpResponse.StatusDescription;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.ToString());
        //        throw ex;
        //    }
        //}

        /// <summary>
        /// Create a New Directory on the FTP Server
        /// </summary>
        /// <param name="newDirectory"></param>
        public bool CreateDirectory(string directoryUri)
        {
            try
            {
                // Create an FTP Request
                _ftpRequest = (FtpWebRequest)WebRequest.Create(directoryUri);
                // Log in to the FTP Server with the User Name and Password Provided
                _ftpRequest.Credentials = new NetworkCredential(_username.Normalize(), _password.Normalize());
                // Specify the Type of FTP Request
                _ftpRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
                // Establish Return Communication with the FTP Server
                var _ftpResponse = (FtpWebResponse)_ftpRequest.GetResponse();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Create Folder Succeed:{directoryUri}");
                Console.ResetColor();
                _ftpResponse.Dispose();
                return true;
            }
            catch (WebException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Create Folder Failed:{ex.HResult} {directoryUri}");
                Console.ResetColor();
                return false;
                //throw ex;
            }
        }

        ///// <summary>
        ///// Get the Date/Time a File was Created
        ///// </summary>
        ///// <param name="fileName"></param>
        ///// <returns></returns>
        //public DateTime GetFileCreatedDateTime(string fileName)
        //{
        //    try
        //    {
        //        // Create an FTP Request
        //        _ftpRequest = (FtpWebRequest)WebRequest.Create(_hostAddress + "/" + fileName);

        //        // Log in to the FTP Server with the User Name and Password Provided
        //        _ftpRequest.Credentials = new NetworkCredential(_username, _password);

        //        // Specify the Type of FTP Request
        //        _ftpRequest.Method = WebRequestMethods.Ftp.GetDateTimestamp;

        //        // Establish Return Communication with the FTP Server
        //        using (_ftpResponse = (FtpWebResponse)_ftpRequest.GetResponse())
        //        {
        //            return _ftpResponse.LastModified;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.ToString());
        //        throw ex;
        //    }
        //}

        ///// <summary>
        ///// Get the Size of a File
        ///// </summary>
        ///// <param name="fileName"></param>
        ///// <returns></returns>
        //public long GetFileSize(string fileName)
        //{
        //    try
        //    {
        //        // Create an FTP Request
        //        _ftpRequest = (FtpWebRequest)WebRequest.Create(_hostAddress + "/" + fileName);

        //        // Log in to the FTP Server with the User Name and Password Provided
        //        _ftpRequest.Credentials = new NetworkCredential(_username, _password);

        //        // Specify the Type of FTP Request
        //        _ftpRequest.Method = WebRequestMethods.Ftp.GetFileSize;

        //        // Establish Return Communication with the FTP Server
        //        using (_ftpResponse = (FtpWebResponse)_ftpRequest.GetResponse())
        //        {
        //            return _ftpResponse.ContentLength;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.ToString());
        //        throw ex;
        //    }
        //}

        ///// <summary>
        ///// List Directory Contents File/Folder Name Only
        ///// </summary>
        ///// <param name="directory"></param>
        ///// <returns></returns>
        //public IList<string> DirectoryListSimple(string directory)
        //{
        //    try
        //    {
        //        // Create an FTP Request
        //        _ftpRequest = (FtpWebRequest)WebRequest.Create(_hostAddress + "/" + directory);

        //        // Log in to the FTP Server with the User Name and Password Provided
        //        _ftpRequest.Credentials = new NetworkCredential(_username, _password);

        //        // Specify the Type of FTP Request
        //        _ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;

        //        // Establish Return Communication with the FTP Server
        //        using (_ftpResponse = (FtpWebResponse)_ftpRequest.GetResponse())
        //        {
        //            StreamReader streamReader = new StreamReader(_ftpResponse.GetResponseStream());

        //            IList<string> directories = new List<string>();

        //            string line = streamReader.ReadLine();
        //            while (!string.IsNullOrEmpty(line))
        //            {
        //                directories.Add(line);
        //                line = streamReader.ReadLine();
        //            }
        //            return directories;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.ToString());
        //        throw ex;
        //    }
        //}

        ///// <summary>
        ///// List Directory Contents in Detail (Name, Size, Created, etc.)
        ///// </summary>
        ///// <param name="directory"></param>
        ///// <returns></returns>
        //public IList<string> DirectoryListDetailed(string directory)
        //{
        //    try
        //    {
        //        // Create an FTP Request
        //        _ftpRequest = (FtpWebRequest)WebRequest.Create(_hostAddress + "/" + directory);

        //        // Log in to the FTP Server with the User Name and Password Provided
        //        _ftpRequest.Credentials = new NetworkCredential(_username, _password);

        //        // Specify the Type of FTP Request
        //        _ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

        //        // Establish Return Communication with the FTP Server
        //        using (_ftpResponse = (FtpWebResponse)_ftpRequest.GetResponse())
        //        {
        //            StreamReader streamReader = new StreamReader(_ftpResponse.GetResponseStream());

        //            IList<string> directories = new List<string>();

        //            string line = streamReader.ReadLine();
        //            while (!string.IsNullOrEmpty(line))
        //            {
        //                directories.Add(line);
        //                line = streamReader.ReadLine();
        //            }
        //            return directories;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.ToString());
        //        throw ex;
        //    }
        //}

    }
}
