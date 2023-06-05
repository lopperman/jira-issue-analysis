using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JConsole.ConsoleHelpers.ConsoleTables;
using Terminal.Gui;

namespace JiraCon
{
    public static class ConfigHelper
    {
        const string configFileName = "JiraConConfig.txt";
        const string configIssueStatus = "JiraConIssueStatus.txt";

        private static string personalFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/Library/Application Support/JiraCon";


        public static List<JItemStatus> GetItemStatusConfig()
        {
            var ret = new List<JItemStatus>();

            string path = Path.Combine(personalFolder, configIssueStatus);

            if (!File.Exists(path))
            {
                ConsoleUtil.WriteLine("Missing config file: " + path, ConsoleColor.Red, ConsoleColor.Gray, false);
                ConsoleUtil.WriteLine("File will be generated with defaults but you will need to verify 'CalendarWork' and 'ActiveWork' in file: " + path,ConsoleColor.Red, ConsoleColor.Gray, false);
                ConsoleUtil.WriteLine("Press any key to continue");
                var ok = Console.ReadKey(true);

                var jItemStatuses = JiraUtil.JiraRepo.GetJItemStatusDefaults();
                ret = jItemStatuses;
                using (StreamWriter writer = new StreamWriter(path))
                {
                    writer.WriteLine("##statusName, statusId, categoryKey, categoryName, calendarWork, activeWork");
                    foreach (JItemStatus jis in jItemStatuses)
                    {
                        writer.WriteLine(string.Format("{0}|{1}|{2}|{3}|{4}|{5}", jis.StatusName, jis.StatusId, jis.CategoryKey, jis.CategoryName, jis.CalendarWork, jis.ActiveWork));
                    }
                }
            }
            else
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.StartsWith("#"))
                        {
                            continue;
                        }
                        string[] arr = line.Split('|');
                        JItemStatus newJIS = new JItemStatus();
                        newJIS.StatusName = arr[0];
                        newJIS.StatusId = arr[1];
                        newJIS.CategoryKey = arr[2];
                        newJIS.CategoryName = arr[3];
                        newJIS.CalendarWork = (arr[4].ToLower() == "true") ? true : false;
                        newJIS.ActiveWork = (arr[5].ToLower() == "true") ? true : false;
                        ret.Add(newJIS);
                    }
                }
            }


            return ret;
        }

        internal static void ViewAll()
        {

            ConsoleTable table = null;

            ConsoleUtil.WriteLine("");
            ConsoleUtil.WriteLine("********** LOGIN CONFIG **********", ConsoleColor.Yellow, ConsoleColor.Black, false);
            ConsoleUtil.WriteLine("");
            ConsoleUtil.WriteLine(string.Format("Path = {0}",Path.Combine(personalFolder,configFileName)));

            var loginArr = GetConfig();
            table = new ConsoleTable("loginName", "apiKey", "Jira Base Url");
            table.AddRow(loginArr[0], loginArr[1], loginArr[2]);
            table.Write();
            ConsoleUtil.WriteLine("");
            ConsoleUtil.WriteLine("********** END LOGIN CONFIG ******", ConsoleColor.Yellow, ConsoleColor.Black, false);
            ConsoleUtil.WriteLine("");
            ConsoleUtil.WriteLine("PRESS ANY KEY TO CONTINUE");
            var ok = Console.ReadKey(true);


            ConsoleUtil.WriteLine("********** ISSUE STATUS TIME METRICS CONFIG *********", ConsoleColor.Yellow, ConsoleColor.Black, false);
            ConsoleUtil.WriteLine("");
            ConsoleUtil.WriteLine("All issue statuses. These are determined automatically via the Issue Status Cagetory from Jira, unless otherwise annotated. To add or remove overrides, use the Config menu option for 'Override Issue Status Time Metrics'.");
            ConsoleUtil.WriteLine("");
            ConsoleUtil.WriteLine(string.Format("Path = {0}", Path.Combine(personalFolder, configIssueStatus)));

            table = new ConsoleTable("Name","Category Key/Name","CalendarWork","ActiveWork");
            foreach (var jis in JiraUtil.JiraRepo.JItemStatuses)
            {
                table.AddRow(jis.StatusName, string.Format("{0}/{1}",jis.CategoryKey, jis.CategoryName), jis.CalendarWork, jis.ActiveWork);
            }
            table.Write();
            ConsoleUtil.WriteLine("");
            ConsoleUtil.WriteLine("********** END ISSUE STATUS TIME METRICS CONFIG *****", ConsoleColor.Yellow, ConsoleColor.Black, false);
            ConsoleUtil.WriteLine("");

            //ConsoleUtil.WriteLine("statusName,statusId,categoryKey,categoryName,calendartWork,activeWork");
            //foreach (var jis in JiraUtil.JiraRepo.JItemStatuses.OrderBy(xx=>xx.StatusName))
            //{
            //     ConsoleUtil.WriteLine(string.Format("{0},{1},{2},{3},{4},{5}",jis.StatusName, jis.StatusId, jis.CategoryKey, jis.CategoryName, jis.CalendarWork, jis.ActiveWork));
            //} 


            ConsoleUtil.WriteLine("PRESS ANY KEY TO CONTINUE");
            ok = Console.ReadKey(true);
        }

        internal static JiraConfiguration BuildConfig(string[] args)
        {
            JiraConfiguration config = null;

            if (args.Length == 1)
            {
                return LoadConfigFile(args[0]);
            }

            if (args.Length == 1)
            {
                config = LoadConfigFile(args[0]);
            }
            else if (args.Length == 3)
            {
                config = new JiraConfiguration(args[0], args[1], args[2]);
            }

            return config;
        }

        private static JiraConfiguration LoadConfigFile(string configFilePath)
        {
            JiraConfiguration configuration = null;

            StreamReader reader = null;

            try
            {
                reader = new StreamReader(configFilePath);
                string line1 = reader.ReadLine();
                if (!string.IsNullOrWhiteSpace(line1))
                {
                    string[] arr = line1.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    configuration = BuildConfig(arr);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Loading Config File: {0}", ex.Message);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }

            return configuration;
        }

        public static void KillConfig()
        {
            if (Directory.Exists(personalFolder))
            {
                string configFile = Path.Combine(personalFolder, configFileName);
                if (File.Exists(configFile))
                {
                    File.Delete(configFile);
                }
            }

            ConsoleUtil.WriteLine("Config file has been deleted. Run program again to create new config file. Press any key to exit.", ConsoleColor.White, ConsoleColor.DarkMagenta, true);
            Console.ReadKey(true);
        }

        public static string[] GetConfig()
        {
            string[] ret = null;

            var personalFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/Library/Application Support/JiraCon";
            if (!Directory.Exists(personalFolder))
            {
                Directory.CreateDirectory(personalFolder);
            }
            string configFile = Path.Combine(personalFolder, configFileName);

            if (File.Exists(configFile))
            {
                //check to confirm file has 3 arguments
                using (StreamReader reader = new StreamReader(configFile))
                {
                    var text = reader.ReadLine();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        var arr = text.Split(' ');
                        if (arr.Length == 3)
                        {
                            ret = arr;
                        }
                    }
                }
            }

            if (ret == null)
            {
                string userName = "";
                string apiToken = "";
                string jiraBaseUrl = "";

                userName = GetConsoleInput("Missing config -- please enter username (email address) for Jira login:");
                apiToken = GetConsoleInput("Missing config -- please enter API token for Jira login:");
                jiraBaseUrl = GetConsoleInput("Missing config -- please enter base url for Jira instance:");

                bool validCredentials = false;
                //test connection
                try
                {
                    ConsoleUtil.WriteLine("testing Jira connection ...");
                    var testConn = new JiraRepo(jiraBaseUrl, userName, apiToken);

                    if (testConn != null)
                    {
                        var test = testConn.GetJira().IssueTypes.GetIssueTypesAsync().Result.ToList();
                        if (test != null && test.Count > 0)
                        {
                            validCredentials = true;
                            ConsoleUtil.WriteLine("testing Jira connection ... successful");
                        }
                    }
                }
                catch (Exception ex)
                {
                    ConsoleUtil.WriteLine("testing Jira connection ... failed");
                    ConsoleUtil.WriteLine(ex.Message);
                }

                if (!validCredentials)
                {
                    return GetConfig();
                }
                else
                {
                    using (StreamWriter writer = new StreamWriter(configFile))
                    {
                        writer.WriteLine(string.Format("{0} {1} {2}", userName, apiToken, jiraBaseUrl));
                    }
                    return GetConfig();
                }
            }

            return ret;

        }

        private static string GetConsoleInput(string message)
        {
            Console.WriteLine("...");
            ConsoleUtil.WriteLine(message);
            var ret = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(ret))
            {
                return GetConsoleInput(message);
            }
            ConsoleUtil.WriteLine("");
            ConsoleUtil.WriteLine(string.Format("Enter 'Y' to Use '{0}', otherwise enter 'E' to exit or another key to enter new value", ret));
            var key = Console.ReadKey(true);

            if (key.Key == ConsoleKey.E)
            {
                Environment.Exit(0);
            }
            if (key.Key != ConsoleKey.Y)
            {
                return GetConsoleInput(message);
            }

            return ret;

        }

    }

    public class JiraConfiguration
    {

        public JiraConfiguration(string jiraUserName, string jiraAPIToken, string jiraBaseUrl)
        {
            this.jiraUserName = jiraUserName;
            this.jiraAPIToken = jiraAPIToken;
            this.jiraBaseUrl = jiraBaseUrl;
        }

        public bool IsValid { get; set; }
        public string jiraUserName { get; set; }
        public string jiraAPIToken { get; set; }
        public string jiraBaseUrl { get; set; }

    }
}
