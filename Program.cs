using System;
using JConsole.Utilities;
using Newtonsoft.Json;

namespace JiraCon
{
    class MainClass
    {
        private static bool _initialized = false;
        static JTISConfig? jConfig ;
        static JiraConfiguration config = null;
        private static string[] _args = null;
        static string projectKey = string.Empty ;

        

        public static void Main(string[] args) 
        {
            ConsoleUtil.InitializeConsole(ConsoleColor.Black, ConsoleColor.White);
            jConfig = new JTISConfig();             

            if (args == null || args.Length == 0)
            {
                args = ConfigHelper.GetConfig();
            }

            _args = args;
            bool showMenu = true;

            while (showMenu)
            {
                showMenu = MainMenu();
            }
            ConsoleUtil.Lines.ByeBye();
            Environment.Exit(0);
        }

        private static bool ConfigMenu()
        {
            ConsoleUtil.BuildConfigMenu();
            ConsoleUtil.Lines.WriteQueuedLines(true);

            var resp = Console.ReadKey(true);
            if (resp.Key == ConsoleKey.R)
            {
                ConfigHelper.KillConfig();
                ConsoleUtil.Lines.ByeBye();
                Environment.Exit(0);
            }
            else if (resp.Key == ConsoleKey.E)
            {
                ConsoleUtil.Lines.ByeBye();
                Environment.Exit(0);
            }
            else if (resp.Key == ConsoleKey.V)
            {
                ConfigHelper.ViewAll();
                return true;
            }
            else if (resp.Key == ConsoleKey.J)
            {
                JEnvironmentConfig.JiraEnvironmentInfo();
                return true;
            }
            else if (resp.Key == ConsoleKey.M)
            {
                return false;
            }

            return true;
        }

        private static bool MainMenu()
        {
            if (!_initialized)
            {
                if (config == null && _args != null)
                {
                    config = ConfigHelper.BuildConfig(_args);
                }
                if (config == null)
                {
                    ConsoleUtil.BuildNotInitializedQueue();
                    ConsoleUtil.Lines.WriteQueuedLines(true);
                    string vs = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(vs)) vs = string.Empty;
                    string[] arr = vs.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    config = ConfigHelper.BuildConfig(arr);

                    ConsoleUtil.Lines.configInfo = string.Format("User: {0}", config.jiraUserName);


                    if (config != null)
                    {
                        if (JiraUtil.CreateRestClient(config))
                        {
                            _initialized = true;
                            return _initialized;
                        }
                        else
                        {
                            _initialized = false;
                            ConsoleUtil.WriteLine("Invalid arguments!", ConsoleColor.Yellow, ConsoleColor.DarkBlue, false);
                            ConsoleUtil.WriteLine("Enter path to config file");
                            ConsoleUtil.WriteLine("Do you want to try again? (Y/N):");
                            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                            if (keyInfo.Key == ConsoleKey.Y)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                }

                else
                {
                    if (JiraUtil.CreateRestClient(config))
                    {
                        ConsoleUtil.WriteLine("Successfully connected to Jira as " + config.jiraUserName);
                        ConsoleUtil.Lines.configInfo = string.Format("User: {0}", config.jiraUserName);

                        _initialized = true;
                        return _initialized;
                    }
                    else
                    {
                        _initialized = false;
                        ConsoleUtil.WriteLine("Invalid arguments!", ConsoleColor.Yellow, ConsoleColor.DarkBlue, false);
                        ConsoleUtil.WriteLine("Enter path to config file");
                        ConsoleUtil.WriteLine("Do you want to try again? (Y/N):");
                        ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                        if (keyInfo.Key == ConsoleKey.Y)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }

                }

            }

            return InitializedMenu();

        }

        private static bool InitializedMenu()
        {
            ConsoleUtil.BuildInitializedMenu();
            ConsoleUtil.Lines.WriteQueuedLines(true);

            var resp = Console.ReadKey(true);
            if (resp.Key == ConsoleKey.T)
            {
                if (jConfig.ValidConfig )
                {
                    ConsoleUtil.WriteLine(string.Format("userName: {0}",jConfig.userName ),ConsoleColor.DarkBlue,ConsoleColor.Yellow,false);
                    ConsoleUtil.WriteLine(string.Format("apiToken: {0}",jConfig.apiToken ),ConsoleColor.DarkBlue,ConsoleColor.Yellow,false);
                    ConsoleUtil.WriteLine(string.Format("jiraURL: {0}",jConfig.baseUrl ),ConsoleColor.DarkBlue,ConsoleColor.Yellow,false);
                    ConsoleUtil.WriteLine(string.Format("defaultProject: {0}",jConfig.defaultProject  ),ConsoleColor.DarkBlue,ConsoleColor.Yellow,false);
                    resp = Console.ReadKey(true);
                }
                return true;
            }
            else if (resp.Key == ConsoleKey.M)
            {
                ConsoleUtil.WriteLine("");
                ConsoleUtil.WriteLine("Enter 1 or more card keys separated by a space (e.g. POS-123 POS-456 BAM-789), or E to exit.", ConsoleColor.Black, ConsoleColor.White, false);
                var keys = Console.ReadLine().ToUpper();
                if (string.IsNullOrWhiteSpace(keys))
                {
                    return true;
                }
                if (keys.ToUpper() == "E")
                {
                    return false;
                }

                string[] arr = keys.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (arr.Length >= 1)
                {
                    ConsoleUtil.WriteLine("Would you like to include changes to card description and comments? (enter Y to include)");
                    resp = Console.ReadKey(true);
                    List<JIssue>? retIssues;
                    if (resp.Key == ConsoleKey.Y)
                    {
                        retIssues = AnalyzeIssues(keys,true);
                    }
                    else
                    {
                        retIssues = AnalyzeIssues(keys,false);                    
                    }
                    if (retIssues != null)
                    {
                        WriteChangeLogCSV(retIssues);
                    }

                }

                ConsoleUtil.WriteLine("");
                ConsoleUtil.WriteLine("Press any key to continue.");
                Console.ReadKey(true);
                return true;
            }
            else if (resp.Key == ConsoleKey.X)
            {
                
                ConsoleUtil.WriteLine("");
                ConsoleUtil.WriteLine("Enter or paste JQL then press enter to continue.");
                var jql = Console.ReadLine();
                ConsoleUtil.WriteLine("");
                ConsoleUtil.WriteLine(string.Format("Enter (Y) to use the following JQL?\r\n***** {0}", jql));
                ConsoleUtil.WriteLine("");
                var keys = Console.ReadKey(true);
                if (keys.Key == ConsoleKey.Y)
                {
                    ConsoleUtil.WriteLine("");
                    ConsoleUtil.WriteLine("Enter (Y)es to include card descriptions and comments in the Change History file, otherwise press any key");
                    ConsoleUtil.WriteLine("");
                    var k = Console.ReadKey(true);
                    bool includeCommentsAndDesc = false;
                    if (k.Key == ConsoleKey.Y)
                    {
                        includeCommentsAndDesc = true;
                    }

                    CreateExtractFiles(jql,includeCommentsAndDesc);
                    ConsoleUtil.WriteLine("");
                    ConsoleUtil.WriteLine("Press any key to continue.");
                    Console.ReadKey(true);
                }
                return true;

            }
            else if (resp.Key == ConsoleKey.W)
            {
                string jql = GetJQL();

                int startHour = 7;
                int endHour = 18;

                if (jql != null)
                {
                    var dic = GetBusinessHours();
                    startHour = dic["start"];
                    endHour = dic["end"];

                    CreateWorkMetricsFile(jql,startHour,endHour);

                    ConsoleUtil.PressAnyKeyToContinue();
                }
                return true;

            }
            else if (resp.Key == ConsoleKey.A)
            {
                var epicAnalysis = new EpicAnalysis();
                epicAnalysis.Analyze();
                return true;
            }
            else if (resp.Key == ConsoleKey.I)
            {
                ShowItemStatusConfig();

                ConsoleUtil.WriteLine("");
                ConsoleUtil.WriteLine("Press any key to continue.");
                Console.ReadKey(true);
                return true;
            }
            else if (resp.Key == ConsoleKey.J)
            {

                ConsoleUtil.WriteLine("");
                ConsoleUtil.WriteLine("Enter or paste JQL then press enter to continue.");
                var jql = Console.ReadLine();
                ConsoleUtil.WriteLine("");
                ConsoleUtil.WriteLine(string.Format("Enter (Y) to use the following JQL?\r\n***** {0}", jql));
                var keys = Console.ReadKey(true);
                if (keys.Key == ConsoleKey.Y)
                {
                    ShowJSON(jql);
                    ConsoleUtil.WriteLine("");
                    ConsoleUtil.WriteLine("Press any key to continue.");
                    Console.ReadKey(true);
                }
                return true;

            }
            else if (resp.Key == ConsoleKey.C)
            {
                while (ConfigMenu())
                {

                }
                return true;
            }
            return false;
        }

        public static Dictionary<string,int> GetBusinessHours()
        {

            Dictionary<string, int> ret = new Dictionary<string, int>();
            ret.Add("start", 7);
            ret.Add("end", 18);

            while (true)
            {
                ConsoleUtil.WriteLine("");
                ConsoleUtil.WriteLine("Enter (Y) to change the defaults for business hours? (7AM-6PM)");
                var keys = Console.ReadKey(true);

                try
                {
                    if (keys.Key == ConsoleKey.Y)
                    {
                        ConsoleUtil.WriteLine("Enter Hour for Business Start (0-23)");
                        string read = Console.ReadLine();
                        int start = Convert.ToInt32(read);

                        ConsoleUtil.WriteLine("Enter Hour for Business End (0-23)");
                        read = Console.ReadLine();
                        int end = Convert.ToInt32(read);

                        if (end > start && start >=0 && end <=23)
                        {
                            ConsoleUtil.WriteLine(string.Format("Enter (Y) to use {0} to {1} as business hours?", start, end));
                            keys = Console.ReadKey(true);
                            if (keys.Key == ConsoleKey.Y)
                            {
                                ret["start"] = start;
                                ret["end"] = end;
                                break;
                            }
                        }
                    }
                    else
                    {
                        break;
                    }

                }
                catch
                {
                }

            }

            return ret;

        }

        private static string GetJQL()
        {
            ConsoleUtil.WriteLine("");
            ConsoleUtil.WriteLine("Enter or paste JQL then press enter to continue.");
            var jql = Console.ReadLine();
            ConsoleUtil.WriteLine("");
            ConsoleUtil.WriteLine(string.Format("Enter (Y) to use this JQL:  {0}", jql));
            ConsoleUtil.WriteLine("");
            var keys = Console.ReadKey(true);
            if (keys.Key == ConsoleKey.Y)
            {
                return jql;
            }
            else
            {
                return null;
            }


        }

        private static void ShowItemStatusConfig()
        {
            List<JItemStatus> ordered = JiraUtil.JiraRepo.JItemStatuses.OrderBy(x => x.StatusName).ToList();
            foreach (JItemStatus item in ordered)
            {
                ConsoleUtil.WriteLine(item.ToString());
            }
        }

        private static void CreateWorkMetricsFile(string jql, int startHour, int endHour)
        {
            CreateWorkMetricsFile(jql, startHour, endHour, null);
        }

        public static void CreateWorkMetricsFile(string jql, int startHour, int endHour, string epicKey)
        {
            try
            {
                DateTime now = DateTime.Now;
                string fileNameSuffix = string.Format("_{0:0000}{1}{2:00}_{3}.txt", now.Year, now.ToString("MMM"), now.Day, now.ToString("hhmmss"));

                string workMetricsFile = String.Format("JiraCon_WorkMetrics_{0}", fileNameSuffix);
                if (epicKey != null && epicKey.Length > 0)
                {
                    workMetricsFile = String.Format("JiraCon_WorkMetrics_Epic_{0}_{1}", epicKey, fileNameSuffix);
                }

                ConsoleUtil.WriteLine(string.Format("getting issues from JQL:{0}", Environment.NewLine));
                ConsoleUtil.WriteLine(string.Format("{0}", jql));
                ConsoleUtil.WriteLine("");

                var issues = JiraUtil.JiraRepo.GetIssues(jql);

                ConsoleUtil.WriteLine(string.Format("Retrieved {0} issues", issues.Count()));

                List<JIssue> jissues = new List<JIssue>();

                foreach (var issue in issues)
                {
                    ConsoleUtil.WriteLine(string.Format("getting changelogs for {0}", issue.Key.Value));
                    JIssue newIssue = new JIssue(issue);
                    newIssue.AddChangeLogs(JiraUtil.JiraRepo.GetIssueChangeLogs(issue));

                    jissues.Add(newIssue);
                }

                var extractFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "JiraCon");
                if (!Directory.Exists(extractFolder))
                {
                    Directory.CreateDirectory(extractFolder);
                }

                ConsoleUtil.WriteLine("Calculating work time metrics ...");

                var metrics = new WorkMetrics(JiraUtil.JiraRepo,startHour,endHour);

                SortedDictionary<string, string> forceIgnoreReasons = new SortedDictionary<string, string>();
                //build workMetrics ONLY FOR PARENTS AND SUB-TASKS, then go back and update exclusions for parent/sub-tasks
                var tempMetrics = new WorkMetrics(JiraUtil.JiraRepo, startHour, endHour);
                foreach (JIssue j in jissues)
                {
                    if (j.SubTasks.Count > 0)
                    {
                        var workMetrics = tempMetrics.AddIssue(j, jissues);
                        double parentActiveWorkTotal = workMetrics.Sum(item => item.Total8HourAdjBusinessHours);

                        List<WorkMetric> subTaskWorkMetrics = new List<WorkMetric>();
                        foreach (var subtask in j.SubTasks)
                        {
                            if (jissues.Any(x=>x.Key == subtask.Key))
                            {
                                subTaskWorkMetrics.AddRange(tempMetrics.AddIssue(jissues.Single(x=>x.Key == subtask.Key), jissues));
                            }
                        }
                        var subTasksActiveWorkTotal = subTaskWorkMetrics.Sum(item => item.Total8HourAdjBusinessHours);
                        if (parentActiveWorkTotal > subTasksActiveWorkTotal)
                        {
                            //use parent, ignore subtasks
                            foreach (var subtask in j.SubTasks)
                            { 
                                forceIgnoreReasons.Add(subtask.Key, string.Format("Parent {0} active work time ({1}) is greater than combined sub-task active work time ({2})", j.Key, Math.Round(parentActiveWorkTotal, 2), Math.Round(subTasksActiveWorkTotal, 2)));
                            }

                        }
                        else
                        {
                            //use subtasks, ignore parent
                            var subTaskKeys = string.Join("*", j.SubTasks.Select(x => x.Key).ToList());

                            forceIgnoreReasons.Add(j.Key, string.Format("subtasks {0} active work time ({1}) is greater than parent active work time ({2})", subTaskKeys, Math.Round(subTasksActiveWorkTotal, 2), Math.Round(parentActiveWorkTotal, 2)));
                        }
                    }

                }

                foreach (var kvp in forceIgnoreReasons)
                {
                    metrics.AddForceIgnore(kvp.Key, kvp.Value);
                }

                using (StreamWriter writer = new StreamWriter(Path.Combine(extractFolder,workMetricsFile)))
                {
                        writer.WriteLine("key,type,created,featureTeam,summary,epicKey,parentIssueKey,currentStatus,labels,start,end,status,activeWork,calendarWork,totalBusinessDays,totalBusinessHours_8HourDay,transitionAfterHours,exclude,reason");
                    foreach (JIssue j in jissues)
                    {
                        ConsoleUtil.WriteLine(string.Format("analyzing {0} - {1}", j.Key, j.IssueType));
                        var workMetrics = metrics.AddIssue(j, jissues);
                        ConsoleUtil.WriteLine("key,type,created,featureTeam,summary,epicKey,parentIssueKey,currentStatus,labels,start,end,status,activeWork,calendarWork,totalBusinessDays,totalBusinessHours_8HourDay,transitionAfterHours,exclude,reason");
                        foreach (var wm in workMetrics)
                        {
                            string text = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18}",
                                j.Key, j.IssueType, j.CreateDate.Value.ToShortDateString(), j.FeatureTeamChoices, JHelper.RemoveCommas(j.Summary),
                                j.EpicLinkKey,j.ParentIssueKey, JHelper.RemoveCommas(j.StatusName),
                                JHelper.RemoveCommas(j.LabelsToString),wm.Start, wm.End,
                                wm.ItemStatus.StatusName, wm.ItemStatus.ActiveWork, wm.ItemStatus.CalendarWork,
                                wm.TotalBusinessDays,wm.Total8HourAdjBusinessHours,wm.TransitionAfterHours,
                                wm.Exclude,wm.ExcludeReasons);
                            writer.WriteLine(text);
                            ConsoleUtil.WriteLine(text) ;
                        }
                    }

                }

                ConsoleUtil.WriteLine(string.Format("data has been written to {0}/{1}",extractFolder,workMetricsFile));

            }
            catch (Exception ex)
            {
                ConsoleUtil.WriteLine("*** An error has occurred ***", ConsoleColor.DarkRed, ConsoleColor.Gray, false);
                ConsoleUtil.WriteLine(ex.Message, ConsoleColor.DarkRed, ConsoleColor.Gray, false);
                ConsoleUtil.WriteLine(ex.StackTrace, ConsoleColor.DarkRed, ConsoleColor.Gray, false);
            }
        }


        private static void CreateExtractFiles(string jql, bool includeCommentsAndDesc)
        {
            try
            {
                DateTime now = DateTime.Now;
                string fileNameSuffix = string.Format("_{0:0000}{1}{2:00}_{3}.txt", now.Year, now.ToString("MMM"), now.Day, now.ToString("hhmmss"));

                string cycleTimeFile = String.Format("JiraCon_CycleTime_{0}", fileNameSuffix);
                string qaFailFile = String.Format("JiraCon_QAFailure_{0}", fileNameSuffix);
                string velocityFile = String.Format("JiraCon_Velocity_{0}", fileNameSuffix);
                string changeHistoryFile = String.Format("JiraCon_ChangeHistory_{0}", fileNameSuffix);
                string extractConfigFile = String.Format("JiraCon_ExtractConfig_{0}", fileNameSuffix);


                ConsoleUtil.WriteLine(string.Format("getting issues from JQL:{0}",Environment.NewLine));
                ConsoleUtil.WriteLine(string.Format("{0}", jql));
                ConsoleUtil.WriteLine("");

                var issues = JiraUtil.JiraRepo.GetIssues(jql);

                ConsoleUtil.WriteLine(string.Format("Retrieved {0} issues", issues.Count()));

                List<JIssue> jissues = new List<JIssue>();

                foreach (var issue in issues)
                {
                    ConsoleUtil.WriteLine(string.Format("getting changelogs for {0}", issue.Key.Value));
                    JIssue newIssue = new JIssue(issue);
                    newIssue.AddChangeLogs(JiraUtil.JiraRepo.GetIssueChangeLogs(issue));

                    jissues.Add(newIssue);
                }

                var extractFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "JiraCon");
                if (!Directory.Exists(extractFolder))
                {
                    Directory.CreateDirectory(extractFolder);
                }

                ConsoleUtil.WriteLine("Calculating QA Failures ...");
                CreateQAFailExtract(jissues, Path.Combine(extractFolder, qaFailFile));
                ConsoleUtil.WriteLine(string.Format("Created qa failures file ({0})", qaFailFile));

                ConsoleUtil.WriteLine("Calculating cycle times...");
                CreateCycleTimeExtract(jissues, Path.Combine(extractFolder, cycleTimeFile));
                ConsoleUtil.WriteLine(string.Format("Created cycle time file ({0})", cycleTimeFile));

                ConsoleUtil.WriteLine("Calculating velocities ...");
                CreateVelocityExtract(jissues, Path.Combine(extractFolder, velocityFile));
                ConsoleUtil.WriteLine(string.Format("Created velocity file ({0})", velocityFile));

                ConsoleUtil.WriteLine("Organizing change logs ...");
                CreateChangeLogExtract(jissues, Path.Combine(extractFolder, changeHistoryFile),includeCommentsAndDesc);
                ConsoleUtil.WriteLine(string.Format("Created change log file ({0})", changeHistoryFile));

                ConsoleUtil.WriteLine("writing config for this extract process ...");
                CreateConfigExtract(Path.Combine(extractFolder, extractConfigFile),jql);

                ConsoleUtil.WriteLine(string.Format("Created config file ({0})", extractConfigFile));
                ConsoleUtil.WriteLine("");
                ConsoleUtil.WriteLine(string.Format("Files are located in: {0}", extractFolder));
                ConsoleUtil.WriteLine("");

            }
            catch (Exception ex)
            {
                ConsoleUtil.WriteLine("*** An error has occurred ***", ConsoleColor.DarkRed, ConsoleColor.Gray, false);
                ConsoleUtil.WriteLine(ex.Message, ConsoleColor.DarkRed, ConsoleColor.Gray, false);
                ConsoleUtil.WriteLine(ex.StackTrace, ConsoleColor.DarkRed, ConsoleColor.Gray, false);
            }
        }

        private static void ShowJSON(string jql)
        {
            try
            {
                ConsoleUtil.WriteLine("");
                ConsoleUtil.WriteLine(string.Format("getting issues from JQL:{0}", Environment.NewLine));
                ConsoleUtil.WriteLine(string.Format("{0}", jql));
                ConsoleUtil.WriteLine("");
                ConsoleUtil.WriteLine("Querying JIRA ...");
                ConsoleUtil.WriteLine("");

                var issues = JiraUtil.JiraRepo.GetIssues(jql);

                ConsoleUtil.WriteLine(string.Format("Retrieved {0} issues", issues.Count()));

                List<JIssue> jissues = new List<JIssue>();

                foreach (var issue in issues)
                {
                    ConsoleUtil.WriteLine(string.Format("getting changelogs for {0}", issue.Key.Value));
                    JIssue newIssue = new JIssue(issue);
                    newIssue.AddChangeLogs(JiraUtil.JiraRepo.GetIssueChangeLogs(issue));

                    jissues.Add(newIssue);
                }

                var extractFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "JiraCon");
                if (!Directory.Exists(extractFolder))
                {
                    Directory.CreateDirectory(extractFolder);
                }

                DateTime now = DateTime.Now;
                string fileNameSuffix = string.Format("_{0:0000}{1}{2:00}_{3}.txt", now.Year, now.ToString("MMM"), now.Day, now.ToString("hhmmss"));
                string jsonFile = String.Format("JiraCon_JSON_{0}", fileNameSuffix);

                ConsoleUtil.WriteLine(string.Format("saving JSON to {0}", Path.Combine(extractFolder,jsonFile)));

                using (StreamWriter w = new StreamWriter(Path.Combine(extractFolder,jsonFile)))
                {
                    foreach (var jiss in jissues)
                    {
                        w.WriteLine(JsonConvert.SerializeObject(jiss, Formatting.Indented));
                    }
                }

                ConsoleUtil.WriteLine("file saved successfully");


            }
            catch (Exception ex)
            {
                ConsoleUtil.WriteLine("*** An error has occurred ***", ConsoleColor.DarkRed, ConsoleColor.Gray, false);
                ConsoleUtil.WriteLine(ex.Message, ConsoleColor.DarkRed, ConsoleColor.Gray, false);
                ConsoleUtil.WriteLine(ex.StackTrace, ConsoleColor.DarkRed, ConsoleColor.Gray, false);
            }
        }

        private static void CreateConfigExtract(string file, string jql)
        {
            using (StreamWriter w = new StreamWriter(file,false))
            {
                w.WriteLine("***** JQL Used for Extract *****");
                w.WriteLine("");

                w.WriteLine(jql);
            }
        }

        private static void CreateChangeLogExtract(List<JIssue> issues, string file,bool includeCommentsAndDesc)
        {
            //issues = issues.OrderBy(x => x.Key).ToList();

            //using (StreamWriter writer = new StreamWriter(file))
            //{

            //    writer.WriteLine("key,type,status,name");
            //    foreach (var iss in issues)
            //    {
            //        DateTime? devDoneDate = iss.GetDevDoneDate();
            //        if (devDoneDate.HasValue)
            //        {
            //            //writer.WriteLine(string.Format("{0},{1},{2}", iss.Key, iss.IssueType,iss.StatusName,iss.devDoneDate.Value.ToShortDateString()));
            //        }

            //    }
            //}

        }

        private static void CreateVelocityExtract(List<JIssue> issues, string file)
        {
            issues = issues.OrderBy(x => x.Key).ToList();

            using (StreamWriter writer = new StreamWriter(file))
            {

                writer.WriteLine("key,type,summary,epicLink,doneDate");
                foreach (var iss in issues)
                {
                    DateTime? devDoneDate = iss.GetDevDoneDate();
                    if (devDoneDate.HasValue)
                    {
                        writer.WriteLine(string.Format("{0},{1},{2},{3}", iss.Key, iss.IssueType, iss.Summary, iss.EpicLinkKey, devDoneDate.Value.ToShortDateString()));
                    }

                }
            }

        }

        private static void CreateCycleTimeExtract(List<JIssue> issues, string file)
        {
            issues = issues.OrderBy(x => x.Key).ToList();

            using (StreamWriter writer = new StreamWriter(file))
            {

                writer.WriteLine("key,type,summary,epicLink,confidence,cycleTime,cycleTimeUom,inDevDate,doneDate,inDevCount");
                foreach (var iss in issues)
                {
                    DateTime? devDoneDate = iss.GetDevDoneDate();
                    if (devDoneDate.HasValue)
                    {
                        string ct = iss.CycleTimeSummary;
                        if (!string.IsNullOrWhiteSpace(ct) && ct.ToLower() != "n/a")
                        {
                            writer.WriteLine(ct);
                        }
                    }
                }
            }
        }

        private static void CreateQAFailExtract(List<JIssue> issues, string file)
        {
            issues = issues.OrderBy(x => x.Key).ToList();

            using (StreamWriter writer = new StreamWriter(file))
            {

                writer.WriteLine("key,type,summary,epicLink," +
                    "failedQADate,determinedBy,comments");
                foreach (var iss in issues)
                {
                    DateTime? devDoneDate = iss.GetDevDoneDate();
                    if (iss.FailedQASummary.Count > 0)
                    {
                        foreach (var s in iss.FailedQASummary)
                        {
                            writer.WriteLine(s);
                        }
                    }

                }
            }

        }



        public static List<JIssue> AnalyzeIssues(string cardNumbers, bool includeDescAndComments)
        {
            var retIssues = new List<JIssue>();
            string[] arr = cardNumbers.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < arr.Length; i++)
            {
                JIssue? nIssue = AnalyzeOneIssue(arr[i], includeDescAndComments);
                if (nIssue != null) 
                {
                    retIssues.Add(nIssue);
                }
            }
            return retIssues;
        }

        public static JIssue? AnalyzeOneIssue(string key, bool includeDescAndComments) 
        {

            ConsoleUtil.WriteLine("");
            ConsoleUtil.WriteLine("***** Jira Card: " + key, ConsoleColor.DarkBlue, ConsoleColor.White, false);

            var issue = JiraUtil.JiraRepo.GetIssue(key);
            //issue.

            if (issue == null)
            {
                ConsoleUtil.WriteLine("***** Jira Card: " + key + " NOT FOUND!", ConsoleColor.DarkBlue, ConsoleColor.White, false);
                return null;
            }

            ConsoleUtil.WriteLine(string.Format("***** loading change logs for {0}-({1}):",key,issue.Summary));

            var jIss = new JIssue(issue);
            jIss.AddChangeLogs(JiraUtil.JiraRepo.GetIssueChangeLogs(issue));

            ConsoleUtil.WriteLine(string.Format("Found {0} change logs for {1}", jIss.ChangeLogs.Count, key));

            for (int i = 0; i < jIss.ChangeLogs.Count; i++)
            {

                JIssueChangeLog changeLog = jIss.ChangeLogs[i];


                foreach (JIssueChangeLogItem cli in changeLog.Items)
                {
                    if (cli.FieldName.ToLower().StartsWith("desc") || cli.FieldName.ToLower().StartsWith("comment"))
                    {
                        if (includeDescAndComments)
                        {
                            ConsoleUtil.WriteAppend(string.Format("{0} - Changed On {1}, {2} field changed ", issue.Key, changeLog.CreatedDate.ToString(), cli.FieldName), true);
                            ConsoleUtil.WriteAppend(string.Format("\t{0} changed from ", cli.FieldName), true);
                            ConsoleUtil.WriteAppend(string.Format("{0}", cli.FromValue), ConsoleColor.DarkGreen, Console.BackgroundColor, true);
                            ConsoleUtil.WriteAppend(string.Format("\t{0} changed to ", cli.FieldName), true);
                            ConsoleUtil.WriteAppend(string.Format("{0}", cli.ToValue), ConsoleColor.Green, Console.BackgroundColor, true);
                        }
                    }
                    else
                    {
                        if (cli.FieldName.ToLower().StartsWith("status"))
                        {
                            ConsoleUtil.WriteAppend(string.Format("{0} - Changed On {1}, {2} field changed from '{3}' to ", issue.Key, changeLog.CreatedDate.ToString(), cli.FieldName, cli.FromValue),false);
                            ConsoleUtil.WriteAppend(string.Format("{0}", cli.ToValue),ConsoleColor.White,ConsoleColor.Red,true);
                        }
                        else if (cli.FieldName.ToLower().StartsWith("label"))
                        {
                            ConsoleUtil.WriteAppend(string.Format("{0} - Changed On {1}, {2} field changed from '{3}' to ", issue.Key, changeLog.CreatedDate.ToString(), cli.FieldName, cli.FromValue),false);
                            ConsoleUtil.WriteAppend(string.Format("{0}", cli.ToValue), ConsoleColor.White, ConsoleColor.Blue);
                        }

                        else
                        {
                            ConsoleUtil.WriteLine($"{issue.Key} - Changed On {changeLog.CreatedDate}, {cli.FieldName} field changed from '{cli.FromValue}' to '{cli.ToValue}'");

                        }
                    }
                }
            }

            return jIss ;

            //ConsoleUtil.WriteLine("***** JSON for  " + key + " *****", ConsoleColor.Black, ConsoleColor.Cyan, false);
            //ConsoleUtil.WriteLine(JsonConvert.SerializeObject(jIss,Formatting.Indented), ConsoleColor.DarkBlue, ConsoleColor.Cyan, false);

            //ConsoleUtil.WriteLine("***** Jira Card: " + key + " END", ConsoleColor.DarkBlue, ConsoleColor.White, false);

        }


        //**********************************************************************************************************************************************************************************************
        //**********************************************************************************************************************************************************************************************
        //**********************************************************************************************************************************************************************************************
        //**********************************************************************************************************************************************************************************************
        //                                  KEEP FINAL CODE ABOVE THIS LINE
        //**********************************************************************************************************************************************************************************************
        //**********************************************************************************************************************************************************************************************
        //**********************************************************************************************************************************************************************************************
        //**********************************************************************************************************************************************************************************************
        //**********************************************************************************************************************************************************************************************



        public static void WriteChangeLogCSV(List<JIssue> issues)
        {
            //        private static string personalFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal),"Library","Application Support","JiraCon");
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal),"Library","Application Support","JiraCon","changeLog.csv");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            var writer = new StreamWriter(filePath,false);

            writer.WriteLine("jiraKEY,changeLogTime,fieldName,fromStatus,toStatus");

            for (int j = 0; j < issues.Count; j++)
            {
                var jIss = issues[j];

                for (int i = 0; i < jIss.ChangeLogs.Count; i++)
                {
                    JIssueChangeLog changeLog = jIss.ChangeLogs[i];
                    foreach (JIssueChangeLogItem cli in changeLog.Items)
                    {
                        if (cli.FieldName.ToLower().StartsWith("status"))
                        {
                            writer.WriteLine(string.Format("{0},{1},{2},{3},{4}",jIss.Key,changeLog.CreatedDate.ToString(),cli.FieldName,cli.FromValue,cli.ToValue ));
                            // writer.WriteLine(string.Format("{0} - Changed On {1}, {2} field changed from '{3}' to ", jIss.Key, changeLog.CreatedDate.ToString(), cli.FieldName, cli.FromValue));
                            // writer.WriteLine(string.Format("{0}", cli.ToValue));
                        }
                        // else if (cli.FieldName.ToLower().StartsWith("label"))
                        // {
                        //     writer.WriteLine(string.Format("{0} - Changed On {1}, {2} field changed from '{3}' to ", jIss.Key, changeLog.CreatedDate.ToString(), cli.FieldName, cli.FromValue));
                        //     writer.WriteLine(string.Format("{0}", cli.ToValue));
                        // }
                    }
                }


//                writer.WriteLine(jIss.Key);
//                writer.WriteLine("Change Log Count: " + jIss.ChangeLogs.Count);
            }

            writer.Close();
        }



        /// <summary>
        /// Build CSV file.
        /// Importing into MS Excel (delimited by ",") works nicely, but I'm also planning on adding a JSON option. 
        /// </summary>
        /// <param name="cards"></param>
        /// <param name="filePath"></param>
        public static void WriteCSVFile(List<JiraCard> cards, string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            StreamWriter writer = new StreamWriter(filePath, false);
            writer.WriteLine("id{0}key{0}cardType{0}status{0}description{0}created{0}updated{0}changeLogId{0}changeLogDt{0}fieldName{0}fieldType{0}fromId{0}fromValue{0}toId{0}toValue{0}DevStart{0}DevDone{0}CycleTimeDays", ",");

            for (int i = 0; i < cards.Count; i ++)
            {
                var card = cards[i];

                for (int j = 0; j < card.ChangeLogs.Count; j++)
                {
                    var cLog = card.ChangeLogs[j];

                    writer.WriteLine("{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}{0}{9}{0}{10}{0}{11}{0}{12}{0}{13}{0}{14}{0}{15}{0}{16}{0}{17}{0}{18}", ",", card._id, card.Key, card.CardType, card.Status, "", card.Created, card.Updated, cLog._id, cLog.ChangeLogDt, cLog.FieldName, cLog.FieldType, cLog.FromId, CleanText(cLog.FromValue), cLog.ToId, CleanText(cLog.ToValue), card.DevStartDt.HasValue ? card.DevStartDt.Value.ToString() : "", card.DevDoneDt.HasValue ? card.DevDoneDt.Value.ToString() : "", card.CycleTime.HasValue ? card.CycleTime.ToString() : "");
                }

            }

            writer.Close();
        }

        /// <summary>
        /// Clean out the mess that Jira includes in their description and comments fields. Was having some issues with that, so description and comments currently
        /// aren't written to the text file. I consider that a nice to have, but I'll work on it soon.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string CleanText(string text)
        {
            string ret = string.Empty;

            if (!String.IsNullOrEmpty(text))
            {
                //Regex rgx = new Regex("[^a-zA-Z0-9 -]");
                //ret = rgx.Replace(text, "");
                ret = text.Replace(",", string.Empty);
            }
            return ret;
        }
    }
}
