using System;
using System.IO;

namespace Filtration.Common.Services
{
    public interface IFileSystemService
    {
        string ReadFileAsString(string filePath);
        void WriteFileFromString(string filePath, string inputString);
        bool DirectoryExists(string directoryPath);
        string GetUserProfilePath();
    }

    internal class FileSystemService : IFileSystemService
    {
        public string ReadFileAsString(string filePath)
        {
            return File.ReadAllText(filePath);
        }

        public void WriteFileFromString(string filePath, string inputString)
        {
            File.WriteAllText(filePath, inputString);
        }

        public bool DirectoryExists(string directoryPath)
        {
            return Directory.Exists(directoryPath);
        }

        public string GetUserProfilePath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }
    }
}
