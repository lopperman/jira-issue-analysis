using JTIS.Analysis;
using JTIS.Config;
using JTIS.Console;
using JTIS.Extensions;
using Spectre.Console;

namespace JTIS
{
    public static class IssueStatesUtil
    {

        public static void EditIssueStatus()
        {
            ConsoleUtil.WriteStdLine("ENTER PART OF STATUS NAME TO CHANGE OR LEAVE BLANK TO SHOW ALL (E.G. 'Progress' would find 'Progress', 'In Progress', etc.)",StdLine.slResponse,false);
            string statusSearch = ConsoleUtil.GetInput<string>("ENTER PART OF STATUS NAME TO CHANGE OR LEAVE BLANK TO SHOW ALL",allowEmpty:true);
            WriteJiraStatuses(statusSearch);
            var editJiraId = ConsoleUtil.GetInput<int>("ENTER JiraId TO CHANGE HOW THAT STATUS IS CATEGORIZED");
            if (editJiraId > 0)
            {
                var changeCfg = CfgManager.config.StatusConfigs.SingleOrDefault(x=>x.StatusId == editJiraId);
                if (changeCfg == null)
                {
                    ConsoleUtil.WriteError($"'{editJiraId}' IS NOT A VALID JIRA STATUS ID",pause:true);
                }
                else 
                {
                    ConsoleUtil.WriteStdLine(string.Format("Jira Status: '{0}' is currently set to '{1}' for Analysis.{2}{3}Enter 'A' to change to Active{2}{3}Enter 'P' to change to Passive{2}{3}Enter 'I' to change to Ignore{2}{3}Press ENTER to cancel",changeCfg.StatusName,Enum.GetName(typeof(StatusType),changeCfg.Type),Environment.NewLine,"    "),StdLine.slResponse,false);
                    var changeTo = ConsoleUtil.GetInput<string>(">: ",allowEmpty:true);
                    bool reSave = false;
                    switch(changeTo.ToUpper())
                    {
                        case "A":
                            changeCfg.CategoryName = "in progress";
                            reSave = true;
                            break;
                        case "P":
                            changeCfg.CategoryName = "to do";
                            reSave = true; 
                            break;
                        case "I":
                            changeCfg.CategoryName = "ignore";
                            reSave = true;
                            break;                        
                    }
                    if (reSave)
                    {
                        ConsoleUtil.WriteStdLine(string.Format("Saving changes to '{0}' ...",changeCfg.StatusName ),StdLine.slOutput  ,false);
                        CfgManager.SaveConfigList();
                        ConsoleUtil.PressAnyKeyToContinue();
                    }
                }
            }            
        }



        public static string ExportPath
        {
            get
            {
                var _exportPath = Path.Combine(CfgManager.JTISRootPath,"Exports");
                if (Directory.Exists(_exportPath)==false)
                {
                    Directory.CreateDirectory(_exportPath);
                }
                _exportPath = Path.Combine(_exportPath,ExportFileName);
                return _exportPath;

            }
        }
        private static string ExportFileName
        {
            get
            {
                var dtInfo = DateTime.Now.ToString("yyyyMMMdd_HHmmss");
                var tmpFileName = string.Format($"{CfgManager.config.defaultProject}_IssueStatusConfig_{dtInfo}.csv");
                return tmpFileName;
            }
        }

        public static void WriteJiraStatusesToCSV()
        {
            var savePath = ExportPath;
                
            using( var writer = new StreamWriter(savePath,false))
            {
                writer.WriteLine($"JiraId,statusName,localState,serverState,statusCategory,usedInProj_{CfgManager.config.defaultProject},overriden");

                foreach (var jStatus in CfgManager.config.StatusConfigs.OrderByDescending(d=>d.DefaultInUse).ThenBy(x=>x.Type).ThenBy(y=>y.StatusName).ToList())
                {
                    JiraStatus  defStat = CfgManager.config.DefaultStatusConfigs.Single(x=>x.StatusId == jStatus.StatusId );
                    string usedIn = string.Empty;   
                    string overridden = string.Empty;      
                    string locState = Enum.GetName(typeof(StatusType),jStatus.Type);     
                    if (jStatus.DefaultInUse)
                    {
                        usedIn = "YES";
                    }
                    if (jStatus.Type != defStat.Type)
                    {
                        overridden = "YES";                                
                    }
                    var _jiraId = jStatus.StatusId.ToString();
                    var _name = jStatus.StatusName.ToString();
                    var _localState = locState;
                    var _defState = Enum.GetName(typeof(StatusType),defStat.Type);
                    var _jiraStatusCat = jStatus.CategoryName.ToString();
                    writer.WriteLine($"{_jiraId},{_name},{_localState},{_defState},{_jiraStatusCat},{usedIn},{overridden}");
                }
            }
            ConsoleUtil.PressAnyKeyToContinue($"File Saved to [bold]{Environment.NewLine}{savePath}[/]");                        
        }

        public static void WriteJiraStatuses(string? searchTerm = null, bool defProjectOnly = false)
        {
            ConsoleUtil.WriteAppTitle();
            AnsiConsole.Write(new Rule());
            AnsiConsole.MarkupLine($"[bold italic]Note: [/][italic]stActiveState, stPassiveState, stEnd[/] map respectively to Jira Status Categories: [italic]IN PROGRESS, TO DO, and DONE[/]{Environment.NewLine}[dim]([italic]stStart[/] also maps to Jira [italic]IN PROGRESS[/] status category, and dynamically displays for the [underline]first[/] 'Active/IN PROGRESS' status for any given Jira issue.)[/]");
            AnsiConsole.Write(new Rule());

            var usedInCol = string.Format("UsedIn: {0}",CfgManager.config.defaultProject);
            Table table = new Table();
            table.AddColumns("JiraId","Name",$"Local{Environment.NewLine}State",$"Default{Environment.NewLine}State",$"Jira Status{Environment.NewLine}Category",usedInCol,"Override");
            table.Alignment(Justify.Left);
            table.Columns[1].Alignment(Justify.Center);
            table.Columns[2].Alignment(Justify.Center);
            table.Columns[3].Alignment(Justify.Center);
            table.Columns[4].Alignment(Justify.Center);
            table.Columns[5].Alignment(Justify.Center);
            table.Columns[6].Alignment(Justify.Center);

            foreach (var jStatus in CfgManager.config.StatusConfigs.OrderByDescending(d=>d.DefaultInUse).ThenBy(x=>x.Type).ThenBy(y=>y.StatusName).ToList())
            {
                bool includeStatus = false;
                if (searchTerm == null || searchTerm.Length == 0 )
                {
                    if (defProjectOnly==false)
                    {    
                        includeStatus = true;
                    }
                    else if (jStatus.DefaultInUse == true)
                    {
                        includeStatus = true;
                    }

                }
                else 
                {
                    if (jStatus.StatusName.ToLower().Contains(searchTerm.ToLower()))
                    {
                        includeStatus = true;
                    }
                }
                if (includeStatus)
                {
                    JiraStatus  defStat = CfgManager.config.DefaultStatusConfigs.Single(x=>x.StatusId == jStatus.StatusId );
                    string usedIn = string.Empty;   
                    string overridden = string.Empty;      
                    string locState = Enum.GetName(typeof(StatusType),jStatus.Type);     
                    if (jStatus.DefaultInUse)
                    {
                        usedIn = "YES";
                    }
                    if (jStatus.Type != defStat.Type)
                    {
                        
                        overridden = string.Format("[bold red on cornsilk1]{0}{0}{0}[/]",":triangular_Flag:");
                        locState = string.Format("[bold blue on cornsilk1]{0}[/]",locState);
                    }
                    var _jiraId = usedIn.StringsMatch("yes") ? jStatus.StatusId.ToString() : $"[dim]{jStatus.StatusId.ToString()}[/]";
                    var _name = usedIn.StringsMatch("yes") ? jStatus.StatusName.ToString() : $"[dim]{jStatus.StatusName.ToString()}[/]";

                    if (jStatus.ChartColor != null)
                    {
                        Color backClr = ColorUtil.ColorDictionary.Values.First(x=>x.ToString().StringsMatch(jStatus.ChartColor));
                        // Color backClr = ColorUtil.ColorsAll.First(x=>x.ToString().StringsMatch(jStatus.ChartColor));
                        string fontClr = ColorUtil.InverseColor(backClr).ToString();
                        _name =  $"[bold {fontClr} on {backClr.ToString()}]{jStatus.StatusName.ToString()}[/]";
                    }

                    var _localState = usedIn.StringsMatch("yes") ? locState : $"[dim]{locState}[/]";
                    var _defState = usedIn.StringsMatch("yes") ? Enum.GetName(typeof(StatusType),defStat.Type) : $"[dim]{Enum.GetName(typeof(StatusType),defStat.Type)}[/]";
                    var _jiraStatusCat = usedIn.StringsMatch("yes") ? jStatus.CategoryName.ToString() : $"[dim]{jStatus.CategoryName.ToString()}[/]";

                    table.AddRow(new string[]{_jiraId, _name,_localState,_defState,_jiraStatusCat, usedIn, overridden});
                }
            }
            AnsiConsole.Write(table);

        }

        internal static void EditIssueSequence(bool editMode = false)
        {
            List<JiraStatus> _defStatuses = CfgManager.config.LocalProjectDefaultStatuses;
            ConsoleUtil.WriteAppTitle();
            ConsoleUtil.WriteBanner($"ISSUE STATUS SEQUENCE ORDER FOR PROJECT: {CfgManager.config.defaultProject} - ALL STATUSES MUST HAVE A SEQUENCE ORDER");
            var tbl = new Table();
            tbl.AddColumns(
                new TableColumn("SEQUENCE ORDER").Centered(), 
                new TableColumn("STATUS").Centered(), 
                new TableColumn("STATUS ID").Centered());
            foreach (var status in _defStatuses)
            {
                tbl.AddRow(
                    new Text($"{status.ProgressOrder}").Centered(), 
                    new Text($"{status.StatusName}").LeftJustified(), 
                    new Text($"{status.StatusId}").Centered());
            }
            AnsiConsole.Write(new Panel(tbl));
            if (editMode == false)
            {
                if (ConsoleUtil.Confirm("Edit Issue Status Sequence?",false))
                {
                    EditIssueSequence(true);
                }
            }
            else 
            {
                var sp = new SelectionPrompt<JiraStatus>();
                sp.AddChoices(_defStatuses);
                sp.Title = "SELECT STATUS TO CHANGE ORDER SEQUENCE";
                var jiraStat = AnsiConsole.Prompt(sp);
                int newSeq = ConsoleUtil.GetInput<int>($"ENTER SEQUENCE NUMBER BETWEEN 1 AND {CfgManager.config.StatusesCount} for Status: {jiraStat.StatusName}. (ENTER '0' TO CANCEL)");
                if (newSeq >= 1 && newSeq <= CfgManager.config.StatusesCount)
                {
                    CfgManager.config.UpdateStatusProgressOrder(jiraStat.StatusId,newSeq);
                    CfgManager.SaveConfigList();
                }
                EditIssueSequence();
            }
            
        }

        internal static void EditIssueColor()
        {
            WriteJiraStatuses(defProjectOnly:true);

            var editIss = ConsoleUtil.GetInput<string>("Enter JiraId of Issues Status to set chart color.  (Press ENTER to cancel)",allowEmpty:true);
            int editId = 0;
            if (int.TryParse(editIss, out editId))
            {
                var jIss = CfgManager.config.StatusConfigs.SingleOrDefault(x=>x.StatusId==editId);
                if (jIss != null && jIss.DefaultInUse)
                {
                    SetIssueStatusColor(jIss);
                    EditIssueColor();
                }
            }


        }

        private static void SetIssueStatusColor(JiraStatus jIss)
        {
            Color? clr = ColorUtil.PickColor($"Pick a background color for charts for status: {jIss.StatusName}");
            if (clr != null)
            {
                jIss.ChartColor = clr.ToString();
                CfgManager.SaveConfigList();

            }
        }
    }
}