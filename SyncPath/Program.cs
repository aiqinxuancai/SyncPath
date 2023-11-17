using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;

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
            Console.ReadLine();
            
        }

        public static async Task SyncTimer(string sourcePath, string targetPath)
        {
            string server = new Uri(sourcePath).Host;

            // 创建并添加凭据
            NetworkCredential credentials = new NetworkCredential("1234", "5678");
            CredentialCache cache = new CredentialCache();
            cache.Add(new Uri($"\\\\{server}"), "Basic", credentials);

            // 将凭据添加到默认网络凭据
            // 这样 System.IO 就可以使用这些凭据来访问 UNC 路径
            System.Net.WebRequest.DefaultWebProxy.Credentials = cache;


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

            if (!targetDirectory.Exists)
            {
                targetDirectory.Create();
            }

            foreach (FileInfo sourceFile in sourceDirectory.GetFiles())
            {
                string targetFilePath = Path.Combine(targetPath, sourceFile.Name);

                FileInfo targetFile = new FileInfo(targetFilePath);

                if (!targetFile.Exists || sourceFile.LastWriteTime > targetFile.LastWriteTime || sourceFile.Length != targetFile.Length)
                {
                    Console.WriteLine("同步文件: " + sourceFile.Name);
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

            foreach (FileInfo targetFile in targetDirectory.GetFiles())
            {
                string sourceFilePath = Path.Combine(sourcePath, targetFile.Name);

                if (!File.Exists(sourceFilePath))
                {
                    Console.WriteLine("删除文件: " + targetFile.Name);
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

            foreach (DirectoryInfo sourceSubDirectory in sourceDirectory.GetDirectories())
            {
                string targetSubDirectoryPath = Path.Combine(targetPath, sourceSubDirectory.Name);

                SyncDirectories(sourceSubDirectory.FullName, targetSubDirectoryPath);
            }

            foreach (DirectoryInfo targetSubDirectory in targetDirectory.GetDirectories())
            {
                string sourceSubDirectoryPath = Path.Combine(sourcePath, targetSubDirectory.Name);

                if (!Directory.Exists(sourceSubDirectoryPath))
                {
                    Console.WriteLine("删除目录: " + targetSubDirectory.Name);

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