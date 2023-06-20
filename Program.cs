using System.Data.SqlTypes;
using Atlassian.Jira;
using Newtonsoft.Json;

namespace JiraCon
{
    class MainClass
    {
        // public static JTISConfig? config ;
        //private static string[] _args = null;
        static string projectKey = string.Empty ;

        //Valid Args are either empty, or a single arg which is the filepath to your desired config file
        public static void Main(string[] args) 
        {
            bool requireManualConfig = false ;
            
            if (args == null || args.Length == 0)
            {
                if (File.Exists(JTISConfigHelper.ConfigFilePath))
                {
                    JTISConfigHelper.ReadConfigList();
                }
            }            
            else 
            {
                if (File.Exists(args[0]))
                {
                    JTISConfigHelper.JTISConfigFilePath = args[0];
                    JTISConfigHelper.ReadConfigList();
                }
            }
            if (JTISConfigHelper.ConfigCount == 1)
            {
                JTISConfigHelper.config  = JTISConfigHelper.GetConfigFromList(1);
            }
            else if (JTISConfigHelper.ConfigCount > 1)
            {
                var changeCfg = JTISConfigHelper.ChangeCurrentConfig(null);
                if (changeCfg != null && changeCfg.ValidConfig==true)
                {
                    JTISConfigHelper.config = changeCfg;
                }
            }

            if (JTISConfigHelper.config==null || JTISConfigHelper.config.ValidConfig==false)
            {
                requireManualConfig = true;
            }

            if (requireManualConfig==true)
            {
                JTISConfig? manualConfig = JTISConfigHelper.CreateConfig(); 
                if (manualConfig != null)
                {
                    if (manualConfig.ValidConfig)
                    {
                        JTISConfigHelper.config = manualConfig;
                    }
                }
            }

            MenuManager.Start(JTISConfigHelper.config);

            // bool showMenu = true;

            // while (showMenu)
            // {
            //     showMenu = MainMenu();
            // }
            ConsoleUtil.ByeByeForced();
        }


        public static Dictionary<string,int> GetBusinessHours()
        {

            Dictionary<string, int> ret = new Dictionary<string, int>();
            ret.Add("start", 7);
            ret.Add("end", 18);

            while (true)
            {
                ConsoleUtil.WriteStdLine("",StdLine.slResponse);
                ConsoleUtil.WriteStdLine("Enter (Y) to change the defaults for business hours? (7AM-6PM)",StdLine.slResponse);
                var keys = Console.ReadKey(true);

                try
                {
                    if (keys.Key == ConsoleKey.Y)
                    {
                        ConsoleUtil.WriteStdLine("Enter Hour for Business Start (0-23)",StdLine.slResponse);
                        string read = Console.ReadLine();
                        int start = Convert.ToInt32(read);

                        ConsoleUtil.WriteStdLine("Enter Hour for Business End (0-23)",StdLine.slResponse);
                        read = Console.ReadLine();
                        int end = Convert.ToInt32(read);

                        if (end > start && start >=0 && end <=23)
                        {
                            ConsoleUtil.WriteStdLine(string.Format("Enter (Y) to use {0} to {1} as business hours?", start, end),StdLine.slResponse);
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

        public static string GetJQL()
        {
            ConsoleUtil.WriteStdLine("",StdLine.slResponse);
            ConsoleUtil.WriteStdLine("Enter or paste JQL then press enter to continue.",StdLine.slResponse);
            var jql = Console.ReadLine();
            ConsoleUtil.WriteStdLine("",StdLine.slResponse);
            ConsoleUtil.WriteStdLine(string.Format("Enter (Y) to use this JQL:  {0}", jql),StdLine.slResponse);
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

        // public static void CreateWorkMetricsFile(string jql, int startHour, int endHour)
        // {
        //     CreateWorkMetricsFile(jql, startHour, endHour, string.Empty);
        // }

        // public static void CreateWorkMetricsFile(string jql, int startHour, int endHour, string epicKey)
        // {

        //     try
        //     {
        //         DateTime now = DateTime.Now;
        //         string fileNameSuffix = string.Format("_{0:0000}{1}{2:00}_{3}.csv", now.Year, now.ToString("MMM"), now.Day, now.ToString("hhmmss"));

        //         string workMetricsFile = String.Format("JiraCon_WorkMetrics_{0}", fileNameSuffix);
        //         if (epicKey != null && epicKey.Length > 0)
        //         {
        //             workMetricsFile = String.Format("JiraCon_WorkMetrics_Epic_{0}_{1}", epicKey, fileNameSuffix);
        //         }

        //         ConsoleUtil.WriteLine(string.Format("getting issues from JQL:{0}", Environment.NewLine));
        //         ConsoleUtil.WriteLine(string.Format("{0}", jql));
        //         ConsoleUtil.WriteLine("");

        //         var issues = JiraUtil.JiraRepo.GetIssues(jql);

        //         ConsoleUtil.WriteLine(string.Format("Retrieved {0} issues", issues.Count()));

        //         List<JIssue> jissues = new List<JIssue>();

        //         foreach (var issue in issues)
        //         {
        //             ConsoleUtil.WriteLine(string.Format("getting changelogs for {0}", issue.Key.Value));
        //             JIssue newIssue = new JIssue(issue);
        //             newIssue.AddChangeLogs(JiraUtil.JiraRepo.GetIssueChangeLogs(issue));

        //             jissues.Add(newIssue);
        //         }

        //         var extractFolder = JTISConfigHelper.JTISRootPath;

        //         ConsoleUtil.WriteLine("Calculating work time metrics ...");

        //         var metrics = new WorkMetrics(JiraUtil.JiraRepo,startHour,endHour);

        //         SortedDictionary<string, string> forceIgnoreReasons = new SortedDictionary<string, string>();
        //         //build workMetrics ONLY FOR PARENTS AND SUB-TASKS, then go back and update exclusions for parent/sub-tasks
        //         var tempMetrics = new WorkMetrics(JiraUtil.JiraRepo, startHour, endHour);
        //         foreach (JIssue j in jissues)
        //         {
        //             if (j.SubTasks.Count > 0)
        //             {
        //                 var workMetrics = tempMetrics.AddIssue(j, jissues);
        //                 double parentActiveWorkTotal = workMetrics.Sum(item => item.Total8HourAdjBusinessHours);

        //                 List<WorkMetric> subTaskWorkMetrics = new List<WorkMetric>();
        //                 foreach (var subtask in j.SubTasks)
        //                 {
        //                     if (jissues.Any(x=>x.Key == subtask.Key))
        //                     {
        //                         subTaskWorkMetrics.AddRange(tempMetrics.AddIssue(jissues.Single(x=>x.Key == subtask.Key), jissues));
        //                     }
        //                 }
        //                 var subTasksActiveWorkTotal = subTaskWorkMetrics.Sum(item => item.Total8HourAdjBusinessHours);
        //                 if (parentActiveWorkTotal > subTasksActiveWorkTotal)
        //                 {
        //                     //use parent, ignore subtasks
        //                     foreach (var subtask in j.SubTasks)
        //                     { 
        //                         forceIgnoreReasons.Add(subtask.Key, string.Format("Parent {0} active work time ({1}) is greater than combined sub-task active work time ({2})", j.Key, Math.Round(parentActiveWorkTotal, 2), Math.Round(subTasksActiveWorkTotal, 2)));
        //                     }

        //                 }
        //                 else
        //                 {
        //                     //use subtasks, ignore parent
        //                     var subTaskKeys = string.Join("*", j.SubTasks.Select(x => x.Key).ToList());

        //                     forceIgnoreReasons.Add(j.Key, string.Format("subtasks {0} active work time ({1}) is greater than parent active work time ({2})", subTaskKeys, Math.Round(subTasksActiveWorkTotal, 2), Math.Round(parentActiveWorkTotal, 2)));
        //                 }
        //             }

        //         }

        //         foreach (var kvp in forceIgnoreReasons)
        //         {
        //             metrics.AddForceIgnore(kvp.Key, kvp.Value);
        //         }

        //         using (StreamWriter writer = new StreamWriter(Path.Combine(extractFolder,workMetricsFile)))
        //         {
        //                 writer.WriteLine("key,type,created,featureTeam,summary,epicKey,parentIssueKey,currentStatus,labels,start,end,status,activeWork,calendarWork,totalBusinessDays,totalBusinessHours_8HourDay,transitionAfterHours,exclude,reason");
        //             foreach (JIssue j in jissues)
        //             {
        //                 ConsoleUtil.WriteLine(string.Format("analyzing {0} - {1}", j.Key, j.IssueType));
        //                 var workMetrics = metrics.AddIssue(j, jissues);
        //                 ConsoleUtil.WriteLine("key,type,created,featureTeam,summary,epicKey,parentIssueKey,currentStatus,labels,start,end,status,activeWork,calendarWork,totalBusinessDays,totalBusinessHours_8HourDay,transitionAfterHours,exclude,reason");
        //                 foreach (var wm in workMetrics)
        //                 {
        //                     string text = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18}",
        //                         j.Key, j.IssueType, j.CreateDate.Value.ToShortDateString(), j.FeatureTeamChoices, JHelper.RemoveCommas(j.Summary),
        //                         j.EpicLinkKey,j.ParentIssueKey, JHelper.RemoveCommas(j.StatusName),
        //                         JHelper.RemoveCommas(j.LabelsToString),wm.Start, wm.End,
        //                         wm.ItemStatus.StatusName, wm.ItemStatus.ActiveWork, wm.ItemStatus.CalendarWork,
        //                         wm.TotalBusinessDays,wm.Total8HourAdjBusinessHours,wm.TransitionAfterHours,
        //                         wm.Exclude,JHelper.RemoveCommas(wm.ExcludeReasons));
        //                     writer.WriteLine(text);
        //                     ConsoleUtil.WriteLine(text) ;
        //                 }
        //             }

        //         }

        //         ConsoleUtil.WriteLine(string.Format("data has been written to {0}/{1}",extractFolder,workMetricsFile));

        //     }
        //     catch (Exception ex)
        //     {
        //         ConsoleUtil.WriteLine("*** An error has occurred ***", ConsoleColor.DarkRed, ConsoleColor.Gray, false);
        //         ConsoleUtil.WriteLine(ex.Message, ConsoleColor.DarkRed, ConsoleColor.Gray, false);
        //         ConsoleUtil.WriteLine(ex.StackTrace, ConsoleColor.DarkRed, ConsoleColor.Gray, false);
        //     }
        // }


        public static void CreateExtractFiles(string jql, bool includeCommentsAndDesc)
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


                ConsoleUtil.WriteStdLine("getting issues from JQL:",StdLine.slCode);
                ConsoleUtil.WriteStdLine(string.Format("{0}", jql),StdLine.slCode);
                ConsoleUtil.WriteStdLine("",StdLine.slCode);

                var issues = JiraUtil.JiraRepo.GetIssues(jql);

                ConsoleUtil.WriteStdLine(string.Format("Retrieved {0} issues", issues.Count()),StdLine.slCode);

                List<JIssue> jissues = new List<JIssue>();

                foreach (var issue in issues)
                {
                    ConsoleUtil.WriteStdLine(string.Format("getting changelogs for {0}", issue.Key.Value),StdLine.slCode);
                    JIssue newIssue = new JIssue(issue);
                    newIssue.AddChangeLogs(JiraUtil.JiraRepo.GetIssueChangeLogs(issue));

                    jissues.Add(newIssue);
                }

                var extractFolder = JTISConfigHelper.JTISRootPath;

                ConsoleUtil.WriteStdLine("Calculating QA Failures ...",StdLine.slCode);
                CreateQAFailExtract(jissues, Path.Combine(extractFolder, qaFailFile));
                ConsoleUtil.WriteStdLine(string.Format("Created qa failures file ({0})", qaFailFile),StdLine.slCode);

                ConsoleUtil.WriteStdLine("Calculating cycle times...",StdLine.slCode);
                CreateCycleTimeExtract(jissues, Path.Combine(extractFolder, cycleTimeFile));
                ConsoleUtil.WriteStdLine(string.Format("Created cycle time file ({0})", cycleTimeFile),StdLine.slCode);

                ConsoleUtil.WriteStdLine("Calculating velocities ...",StdLine.slCode);
                CreateVelocityExtract(jissues, Path.Combine(extractFolder, velocityFile));
                ConsoleUtil.WriteStdLine(string.Format("Created velocity file ({0})", velocityFile),StdLine.slCode);

                ConsoleUtil.WriteStdLine("Organizing change logs ...",StdLine.slCode);
                CreateChangeLogExtract(jissues, Path.Combine(extractFolder, changeHistoryFile),includeCommentsAndDesc);
                ConsoleUtil.WriteStdLine(string.Format("Created change log file ({0})", changeHistoryFile),StdLine.slCode);

                ConsoleUtil.WriteStdLine("writing config for this extract process ...",StdLine.slCode);
                CreateConfigExtract(Path.Combine(extractFolder, extractConfigFile),jql);

                ConsoleUtil.WriteStdLine(string.Format("Created config file ({0})", extractConfigFile),StdLine.slCode);
                ConsoleUtil.WriteStdLine("",StdLine.slCode);
                ConsoleUtil.WriteStdLine(string.Format("Files are located in: {0}", extractFolder),StdLine.slCode);
                ConsoleUtil.WriteStdLine("",StdLine.slCode);

            }
            catch (Exception ex)
            {
                ConsoleUtil.WriteError("*** An error has occurred ***",ex:ex);
            }
        }

        public static void ShowJSON(string jql)
        {
            try
            {
                ConsoleUtil.WriteStdLine("",StdLine.slCode);
                ConsoleUtil.WriteStdLine("getting issues from JQL:", StdLine.slCode);
                ConsoleUtil.WriteStdLine(string.Format("{0}", jql),StdLine.slCode);
                ConsoleUtil.WriteStdLine("",StdLine.slCode);
                ConsoleUtil.WriteStdLine("Querying JIRA ...",StdLine.slCode);
                ConsoleUtil.WriteStdLine("",StdLine.slCode);

                var issues = JiraUtil.JiraRepo.GetIssues(jql);

                ConsoleUtil.WriteStdLine(string.Format("Retrieved {0} issues", issues.Count()),StdLine.slCode);

                List<JIssue> jissues = new List<JIssue>();

                foreach (var issue in issues)
                {
                    ConsoleUtil.WriteStdLine(string.Format("getting changelogs for {0}", issue.Key.Value),StdLine.slCode);
                    JIssue newIssue = new JIssue(issue);
                    newIssue.AddChangeLogs(JiraUtil.JiraRepo.GetIssueChangeLogs(issue));

                    jissues.Add(newIssue);
                }

                var extractFolder = JTISConfigHelper.JTISRootPath;
                if (!Directory.Exists(extractFolder))
                {
                    Directory.CreateDirectory(extractFolder);
                }

                DateTime now = DateTime.Now;
                string fileNameSuffix = string.Format("_{0:0000}{1}{2:00}_{3}.json", now.Year, now.ToString("MMM"), now.Day, now.ToString("hhmmss"));
                string jsonFile = String.Format("JiraCon_JSON_{0}", fileNameSuffix);

                ConsoleUtil.WriteStdLine(string.Format("saving JSON to {0}", Path.Combine(extractFolder,jsonFile)),StdLine.slCode);

                using (StreamWriter w = new StreamWriter(Path.Combine(extractFolder,jsonFile)))
                {
                    foreach (var jiss in jissues)
                    {
                        w.WriteLine(JsonConvert.SerializeObject(jiss, Formatting.Indented));
                    }
                }

                ConsoleUtil.WriteStdLine("file saved successfully",StdLine.slResponse);


            }
            catch (Exception ex)
            {
                ConsoleUtil.WriteError("*** An error has occurred ***");
                ConsoleUtil.WriteError(ex.Message);
                ConsoleUtil.WriteError(ex.StackTrace);
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
            issues = issues.OrderBy(x => x.Key).ToList();

            using (StreamWriter writer = new StreamWriter(file))
            {
               writer.WriteLine("key,type,status,name");
               foreach (var iss in issues)
               {
                   DateTime? devDoneDate = iss.GetDevDoneDate();
                   if (devDoneDate.HasValue)
                   {
                       writer.WriteLine(string.Format("{0},{1},{2}", iss.Key, iss.IssueType,iss.StatusName,devDoneDate.Value.ToShortDateString()));
                   }
               }
            }

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



        public static List<JIssue> AnalyzeIssues(string cardNumbers)
        {
            var retIssues = new List<JIssue>();
            string[] arr = cardNumbers.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < arr.Length; i++)
            {
                JIssue? nIssue = AnalyzeOneIssue(arr[i]);
                if (nIssue != null) 
                {
                    retIssues.Add(nIssue);
                }
            }
            return retIssues;
        }

        public static JIssue? AnalyzeOneIssue(string key) 
        {
            ConsoleUtil.WriteStdLine("",StdLine.slCode);
            ConsoleUtil.WriteStdLine("Jira Card: " + key, StdLine.slCode);

            JIssue? retJIssue = null;
            Issue tmpIssue;

            try 
            {
                tmpIssue = JiraUtil.JiraRepo.GetIssue(key);                
                if (tmpIssue != null)
                {
                    retJIssue = new JIssue(tmpIssue);
                }
            }
            finally 
            {
                if (retJIssue == null) 
                {
                    ConsoleUtil.WriteError("***** Jira Card: " + key + " NOT FOUND!");
                }
            }
            if (retJIssue == null)
            {
                return null;
            }   
            
            ConsoleUtil.WriteStdLine(string.Format("***** loading change logs for {0}-({1}):",key,tmpIssue.Summary),StdLine.slOutputTitle ,false);

            retJIssue.AddChangeLogs(JiraUtil.JiraRepo.GetIssueChangeLogs(tmpIssue));

            ConsoleUtil.WriteStdLine(string.Format("Found {0} change logs for {1}", retJIssue.ChangeLogs.Count, key),StdLine.slOutputTitle);

            for (int i = 0; i < retJIssue.ChangeLogs.Count; i++)
            {

                JIssueChangeLog changeLog = retJIssue.ChangeLogs[i];
                foreach (JIssueChangeLogItem cli in changeLog.Items)
                {
                    if (!cli.FieldName.ToLower().StartsWith("desc") && !cli.FieldName.ToLower().StartsWith("comment"))
                    {
                        if (cli.FieldName.ToLower().StartsWith("status"))
                        {
                            ConsoleUtil.WriteAppend(string.Format("{0} - Changed On {1}, {2} field changed from '{3}' to ", tmpIssue.Key, changeLog.CreatedDate.ToString(), cli.FieldName, cli.FromValue),StdLine.slOutput ,false);
                            ConsoleUtil.WriteAppend(string.Format("{0}", cli.ToValue),ConsoleColor.White,ConsoleColor.Red,true);
                        }
                        else if (cli.FieldName.ToLower().StartsWith("label"))
                        {
                            ConsoleUtil.WriteAppend(string.Format("{0} - Changed On {1}, {2} field changed from '{3}' to ", tmpIssue.Key, changeLog.CreatedDate.ToString(), cli.FieldName, cli.FromValue),StdLine.slOutput ,false);
                            ConsoleUtil.WriteAppend(string.Format("{0}", cli.ToValue), ConsoleColor.White, ConsoleColor.Blue);
                        }
                        else
                        {
                            ConsoleUtil.WriteStdLine($"{tmpIssue.Key} - Changed On {changeLog.CreatedDate}, {cli.FieldName} field changed from '{cli.FromValue}' to '{cli.ToValue}'",StdLine.slOutput,false);
                        }
                    }
                }
            }

            return retJIssue ;

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



        public static string WriteChangeLogCSV(List<JIssue> issues)
        {

            string fName = string.Format("changeLogs_{0}.csv",DateTime.Now.ToString("yyyyMMMdd_HHmmss"));
            string filePath = Path.Combine(JTISConfigHelper.JTISRootPath,fName);
            if (Directory.Exists(filePath)==false)
            {
                Directory.CreateDirectory(filePath);
            }
            filePath = Path.Combine(filePath,fName);
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
                        }
                    }
                }
           }
            writer.Close();

            ConsoleUtil.WriteStdLine(String.Format("file saved to: {0}",filePath),StdLine.slCode);
            ConsoleUtil.PressAnyKeyToContinue();
            return filePath;
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
