using Atlassian.Jira;
using Spectre.Console;

namespace JiraCon
{


    public class ChangeLogsMgr
    {
        private readonly AnalysisType _analysisType;
        private List<Issue>? _issues;
        private List<JIssue>? _jIssues = new List<JIssue>();
        private string? searchJQL = null;
        private string? _exportPath = null;

        public ChangeLogsMgr(AnalysisType analysisType)
        {
            this._analysisType = analysisType;
            BuildSearch();
            
            ConsoleUtil.PressAnyKeyToContinue($"search jql: {searchJQL}");

            if (!string.IsNullOrWhiteSpace(searchJQL))
            {
                PopulateIssues();
                ConsoleUtil.PressAnyKeyToContinue($"issue count: {_issues.Count}");
            }
            if (_issues.Count > 0)
            {
                PopulateChangeLogs();
            }
            if (_jIssues.Count > 0)
            {
                Render();
            }
            if (ConsoleUtil.Confirm("Write data to csv file?",false))
            {
                Export();
            }
        }
        public string ExportPath
        {
            get
            {
                if (_exportPath == null)
                {
                    _exportPath = Path.Combine(JTISConfigHelper.JTISRootPath,"Exports");                    
                    if (Directory.Exists(_exportPath)==false)
                    {
                        Directory.CreateDirectory(_exportPath);
                    }
                    _exportPath = Path.Combine(_exportPath,ExportFileName);
                }
                return _exportPath;
            }
        }
        private string ExportFileName
        {
            get
            {
                var dtInfo = DateTime.Now.ToString("yyyyMMMdd_HHmmss");
                var tmpFileName = string.Format($"{JTISConfigHelper.config.defaultProject}_ChangeLogExport_{dtInfo}.csv");
                return tmpFileName;
            }
        }
        public void Render()
        {
            foreach (var jIss in _jIssues)
            {
                WriteIssueHeader(jIss);
                WriteIssueDetail(jIss);
                AnsiConsole.WriteLine();
            }
            ConsoleUtil.PressAnyKeyToContinue();

        }
        private void WriteIssueHeader(JIssue ji)
        {
            var p = new Panel($"Change Logs For {ji.Key}, (Status: {ji.StatusName}){Environment.NewLine}[dim]{ji.Summary}[/]");
            p.Border = BoxBorder.Rounded;
            p.Expand();
            p.BorderColor(Color.Blue);
            p.Padding(2,1,1,2);
            AnsiConsole.Write(p);
        }
        private void WriteIssueDetail(JIssue ji)
        {
            var tbl = new Table();
            tbl.AddColumn("KEY");
            tbl.AddColumn("FIELD");
            tbl.AddColumn("CHANGED DT");
            tbl.AddColumn("OLD VALUE");
            tbl.AddColumn("NEW VALUE");

            for (int i = 0; i < ji.ChangeLogs.Count; i++)
            {
                JIssueChangeLog changeLog = ji.ChangeLogs[i];
                foreach (JIssueChangeLogItem cli in changeLog.Items)
                {
                    if (!cli.FieldName.ToLower().StartsWith("desc") && !cli.FieldName.ToLower().StartsWith("comment"))
                    {
                        string toVal = (cli.FieldName.ToLower()=="status") ? string.Format($"[bold blue on white] {cli.ToValue} [/]") : cli.ToValue;
                        string frVal = (cli.FieldName.ToLower()=="status") ? string.Format($"[dim blue on white] {cli.FromValue} [/]") : cli.FromValue;
                        tbl.AddRow(new string[]{ji.Key.ToString(),cli.FieldName, changeLog.CreatedDate.ToString(),frVal,toVal});
                    }
                }
            }
            AnsiConsole.Write(tbl);




        }
        public void Export()
        {
            AnsiConsole.Status()
                .Start($"Creating file", ctx=>
                {
                    ctx.Spinner(Spinner.Known.Dots);
                    ctx.SpinnerStyle(new Style(AnsiConsole.Foreground,AnsiConsole.Background));
                    Thread.Sleep(100);

                    ctx.Status($"[italic]Saving to {ExportPath} [/]");
                
                    using( var writer = new StreamWriter(ExportPath,false))
                    {
                        writer.WriteLine("jiraKEY,changeLogTime,fieldName,fromStatus,toStatus");

                        for (int j = 0; j < _jIssues.Count; j++)
                        {
                            var jIss = _jIssues[j];
                            for (int i = 0; i < jIss.ChangeLogs.Count; i++)
                            {
                                foreach (JIssueChangeLogItem cli in jIss.ChangeLogs[i].Items)
                                {
                                    if (!cli.FieldName.ToLower().StartsWith("desc") && !cli.FieldName.ToLower().StartsWith("comment"))
                                    {
                                        writer.WriteLine(string.Format("{0},{1},{2},{3},{4}",jIss.Key,cli.ChangeLog.CreatedDate.ToString(),cli.FieldName,cli.FromValue,cli.ToValue ));
                                    }
                                }
                            }
                        }
                    }

                });
                ConsoleUtil.PressAnyKeyToContinue($"File Saved to [bold]{Environment.NewLine}{ExportPath}[/]");
        }

        private void PopulateChangeLogs()
        {
            foreach (var tIss in _issues )            
            {
                var jIss = new JIssue(tIss);
                jIss.AddChangeLogs(JiraUtil.JiraRepo.GetIssueChangeLogs(tIss));
                _jIssues.Add(jIss);
            }
        }

        private void BuildSearch()
        {
            switch(_analysisType)
            {
                case AnalysisType.atIssues:
                    searchJQL = ConsoleInput.IssueKeysToJQL();
                    break;
                case AnalysisType.atJQL:
                    searchJQL = ConsoleInput.JQL();
                    break;
                // case AnalysisType.atEpics:

                //     break;
                default:
                    throw new NotImplementedException($"ChangeLogsMgs does not support AnalysisType: {Enum.GetName(typeof(AnalysisType),_analysisType)}");
            }
        }
        private void PopulateIssues()
        {
            _issues = JiraUtil.JiraRepo.GetIssues(searchJQL);
        }
       
    }
}