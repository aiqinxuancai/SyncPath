using System;

namespace SyncPath // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            SyncDirectories("D:\\git\\SyncPath\\SyncPath\\bin\\Debug\\net7.0\\test\\A",
                "D:\\git\\SyncPath\\SyncPath\\bin\\Debug\\net7.0\\test\\B");


        }


        public static void SyncDirectories(string sourcePath, string targetPath)
        {
            DirectoryInfo sourceDirectory = new DirectoryInfo(sourcePath);
            DirectoryInfo targetDirectory = new DirectoryInfo(targetPath);

            // Create the target directory if it doesn't exist
            if (!targetDirectory.Exists)
            {
                targetDirectory.Create();
            }

            // Sync files from source to target directory
            foreach (FileInfo sourceFile in sourceDirectory.GetFiles())
            {
                string targetFilePath = Path.Combine(targetPath, sourceFile.Name);

                FileInfo targetFile = new FileInfo(targetFilePath);

                if (!targetFile.Exists || sourceFile.LastWriteTime > targetFile.LastWriteTime || sourceFile.Length != targetFile.Length)
                {
                    Console.WriteLine("Syncing file: " + sourceFile.Name);

                    // Copy the file from source to target
                    sourceFile.CopyTo(targetFilePath, true);
                }
            }

            // Delete files from target directory that don't exist in source directory
            foreach (FileInfo targetFile in targetDirectory.GetFiles())
            {
                string sourceFilePath = Path.Combine(sourcePath, targetFile.Name);

                if (!File.Exists(sourceFilePath))
                {
                    Console.WriteLine("Deleting file: " + targetFile.Name);

                    // Delete the file from target
                    targetFile.Delete();
                }
            }

            // Sync directories recursively
            foreach (DirectoryInfo sourceSubDirectory in sourceDirectory.GetDirectories())
            {
                string targetSubDirectoryPath = Path.Combine(targetPath, sourceSubDirectory.Name);

                SyncDirectories(sourceSubDirectory.FullName, targetSubDirectoryPath);
            }
        }

    }
}