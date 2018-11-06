using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Andoromeda.Kyubey.Portal.Services
{
    public static class FileHelper
    {
        public static void RemoveDangerousChar(this string fileName)
        {
            fileName = fileName.Replace(".", "").Replace("\\", "").Replace("/", "");
        }

        public static string[] GetAllFileNameFromFolder(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return Directory.GetFiles(path, searchPattern, searchOption);
        }





        public static string ReadAllText(string path)
        {
            return System.IO.File.ReadAllText(path);
        }

        public static string[] ReadAllLines(string path)
        {
            return System.IO.File.ReadAllLines(path);
        }


    }
}
