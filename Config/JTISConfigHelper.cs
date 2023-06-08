using System.Reflection;
using System.Runtime.Versioning;
using System.Diagnostics;
using System.Threading;
using System;
using System.Collections.Generic;

namespace JiraCon
{
    public class JTISConfigHelper
    {
        public bool CreateConfig()
        {

            return false;
        }

        public static void  DeleteConfigFile(string? filePath)
        {
            if (filePath == null && MainClass.config != null)
            {
                filePath = MainClass.config.ConfigFilePath ;
            }
            if (filePath != null && filePath.Length > 0 )
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    ConsoleUtil.WriteLine("Config file has been deleted. Run program again to create new config file. Press any key to exit.", ConsoleColor.White, ConsoleColor.DarkMagenta, true);
                    Console.ReadKey(true);
                }
            }
        }
    }
}
