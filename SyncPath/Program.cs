using System;
using System.Diagnostics;

namespace SyncPath // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }


        public static async Task MainAsync(string[] args) 
        {

            if (args.Length == 2)
            {
                var sourcePath = args[0];
                var targetPath = args[1];

                Console.WriteLine("开始同步...");
                Console.WriteLine($"源{sourcePath} ->");
                Console.WriteLine($"目标{targetPath}");

                await SyncTimer(sourcePath, targetPath);
            }
            else
            {
                Console.WriteLine("参数错误");
            }
            
        }

        public static async Task SyncTimer(string sourcePath, string targetPath)
        {
            while (true)
            {

                await Task.Run(() =>
                {
                    Console.WriteLine("开始一次同步");
                    SyncDirectories(sourcePath, targetPath);
                });
                Console.WriteLine("等待3小时...");
                await Task.Delay(1000 * 60 * 60 * 3); //每3小时一次

            }
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
                    Console.WriteLine("同步文件: " + sourceFile.Name);

                    // Copy the file from source to target

                    try
                    {
                        sourceFile.CopyTo(targetFilePath, true);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("同步文件错误: " + targetFile.Name);

                    }


                }
            }

            // Delete files from target directory that don't exist in source directory
            foreach (FileInfo targetFile in targetDirectory.GetFiles())
            {
                string sourceFilePath = Path.Combine(sourcePath, targetFile.Name);

                if (!File.Exists(sourceFilePath))
                {
                    Console.WriteLine("删除文件: " + targetFile.Name);

                    // Delete the file from target
                    try
                    {
                        targetFile.Delete();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("删除文件错误: " + targetFile.Name);
                    }

                }
            }

            // Sync directories recursively
            foreach (DirectoryInfo sourceSubDirectory in sourceDirectory.GetDirectories())
            {
                string targetSubDirectoryPath = Path.Combine(targetPath, sourceSubDirectory.Name);

                SyncDirectories(sourceSubDirectory.FullName, targetSubDirectoryPath);
            }

            // Delete subdirectories from target directory that don't exist in source directory
            foreach (DirectoryInfo targetSubDirectory in targetDirectory.GetDirectories())
            {
                string sourceSubDirectoryPath = Path.Combine(sourcePath, targetSubDirectory.Name);

                if (!Directory.Exists(sourceSubDirectoryPath))
                {
                    Console.WriteLine("删除目录: " + targetSubDirectory.Name);

                    // Delete the directory from target
                    try
                    {
                        targetSubDirectory.Delete(true);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("删除目录错误: " + targetSubDirectory.Name);
                    }

                }
            }
        }


    }
}