using System;
using System.IO;
using System.Linq;
using Microsoft.Win32;

namespace SharedDLLCleanup
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                var specialPath = args.Length > 0 ? string.Join(" ", args).Trim('\"') : "";
                var sharedDlls =
                    Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\SharedDlls", RegistryKeyPermissionCheck.ReadWriteSubTree);

                foreach (var dllPath in sharedDlls?.GetValueNames().ToArray() ?? new string[0])
                {
                    try
                    {
                        if (!File.Exists(dllPath))
                        {
                            Console.WriteLine("File is not present on system: {0}", dllPath);
                            Console.WriteLine("-- Deleting registry entry");
                            sharedDlls?.DeleteValue(dllPath, false);
                        }
                        else if (!string.IsNullOrWhiteSpace(specialPath) &&
                                 Path.GetFullPath(dllPath).ToLower()
                                     .StartsWith(Path.GetFullPath(specialPath).ToLower()))
                        {
                            Console.WriteLine("File is marked for cleanup by path: {0}", dllPath);
                            Console.WriteLine("-- Deleting registry entry");
                            sharedDlls?.DeleteValue(dllPath, false);
                            Console.WriteLine("-- Deleting file from disk");
                            File.Delete(dllPath);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("-- Error: {0}", e.Message);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
#if DEBUG
                Console.ReadLine();
#endif
                Environment.Exit(-1);
            }
#if DEBUG
            Console.ReadLine();
#endif
            Environment.Exit(0);
        }
    }
}