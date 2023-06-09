using System.Reflection;
using System.Runtime.Versioning;
using System.Diagnostics;
using System.Threading;
using System;
using System.Collections.Generic;

namespace JiraCon
{
    public static class JTISConfigHelper
    {
        public static bool CreateConfig()
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

        // public static JTISConfig GetConfig()
        // {            


        //     if (ret == null)
        //     {
        //         string userName = "";
        //         string apiToken = "";
        //         string jiraBaseUrl = "";
        //         string projKey = "";

        //         userName = GetConsoleInput("Missing config -- please enter username (email address) for Jira login:");
        //         apiToken = GetConsoleInput("Missing config -- please enter API token for Jira login:");
        //         jiraBaseUrl = GetConsoleInput("Missing config -- please enter base url for Jira instance:");
        //         projKey = GetConsoleInput("Missing Project Key -- please enter ProjectKey for current Jira instance:");

        //         bool validCredentials = false;
        //         //test connection
        //         try
        //         {
        //             ConsoleUtil.WriteLine("testing Jira connection ...");
        //             var testConn = new JiraRepo(jiraBaseUrl, userName, apiToken);

        //             if (testConn != null)
        //             {
        //                 var test = testConn.GetJira().IssueTypes.GetIssueTypesAsync().Result.ToList();
        //                 if (test != null && test.Count > 0)
        //                 {
        //                     validCredentials = true;
        //                     ConsoleUtil.WriteLine("testing Jira connection ... successful");
        //                 }
        //             }
        //         }
        //         catch (Exception ex)
        //         {
        //             ConsoleUtil.WriteLine("testing Jira connection ... failed");
        //             ConsoleUtil.WriteLine(ex.Message);
        //         }

        //         if (!validCredentials)
        //         {
        //             return GetConfig();
        //         }
        //         else
        //         {
        //             using (StreamWriter writer = new StreamWriter(configFile))
        //             {
        //                 writer.WriteLine(string.Format("{0} {1} {2} {3}", userName, apiToken, jiraBaseUrl, projKey));
        //             }
        //             return GetConfig();
        //         }
        //     }

        //     return ret;

        // }

    }
}
