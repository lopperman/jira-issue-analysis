using System.ComponentModel.Design;
using System.Security.Cryptography;
using Atlassian.Jira;
using JTIS.Analysis;
using JTIS.Config;
using JTIS.Console;
using JTIS.Data;
using JTIS.Extensions;
using JTIS.ManagedObjects;
using JTIS.Menu;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace JTIS
{
   public class ChangeLogsMgr
    {
        private readonly AnalysisType _analysisType;

        private jtisFilterItems<string> _issueTypeFilter = new jtisFilterItems<string>();
        private List<jtisIssue> _jtisIssues = new List<jtisIssue>();
        private string? searchJQL = null;
        private string? _exportPath = null;
        

        public ChangeLogsMgr(AnalysisType analysisType)
        {
            this._analysisType = analysisType;
            bool doExport = false;
            if (!BuildSearch()) return;
            PopulateIssuesAction();
            if (_jtisIssues.Count > 0)
            {
                if (ConsoleUtil.Confirm("Show results on screen? (To export only, enter 'n')",true))
                {
                    Render();                
                }
                else 
                {
                    doExport = true;
                }
                if (ConsoleUtil.Confirm("Export to csv file?",doExport))
                {
                    Export();
                }
            }
        }
        public string ExportPath
        {
            get
            {
                if (_exportPath == null)
                {
                    _exportPath = Path.Combine(CfgManager.JTISRootPath,"Exports");                    
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
                var tmpFileName = string.Format($"{CfgManager.config.defaultProject}_ChangeLogExport_{dtInfo}.csv");
                return tmpFileName;
            }
        }

        private void CheckIssueTypeFilter()
        {
            //        private jtisFilterItems<string> _issueTypeFilter = new jtisFilterItems<string>();
            //var filtered = jtisIssues.Where(x=>_issueTypeFilter.IsFiltered(x.issue.Type.Name)).ToList();

            _issueTypeFilter.Clear();
            foreach (var issType in _jtisIssues.Select(x=>x.issue.Type.Name).Distinct())
            {
                int cnt = _jtisIssues.Count(x=>x.issue.Type.Name.StringsMatch(issType));
                _issueTypeFilter.AddFilterItem(issType,$"Count: {cnt}");
            }
            if (_issueTypeFilter.Count > 1)
            {
                if (ConsoleUtil.Confirm($"Filter which of the {_issueTypeFilter.Count} issue types get displayed?",true))
                {
                    var response = MenuManager.MultiSelect<jtisFilterItem<string>>($"Choose items to include. [dim](To select all items, press ENTER[/])",_issueTypeFilter.Items.ToList());
                    if (response != null && response.Count() > 0)
                    {
                        _issueTypeFilter.Clear();
                        _issueTypeFilter.AddFilterItems(response); 
                    }
                }
            }

        }


        public void Render()
        {
            CheckIssueTypeFilter();

            var filtered = _jtisIssues.Where(x=>_issueTypeFilter.IsFiltered(x.issue.Type.Name)).ToList();

            foreach (var iss in filtered)
            {
                WriteIssueHeader(iss.jIssue);
                WriteIssueDetail(iss.jIssue);
                AnsiConsole.WriteLine();
            }
            ConsoleUtil.PressAnyKeyToContinue();

        }
        private void WriteIssueHeader(JIssue ji)
        {
            var escSummary = Markup.Escape(ji.Summary);
            var p = new Panel($"[bold]Change Logs For {ji.Key}[/], ([dim]Issue Type: [/][bold]{ji.IssueType}[/][dim] Status:[/][bold] {ji.StatusName})[/]{Environment.NewLine}[dim]{escSummary}[/]");
            p.Border = BoxBorder.Rounded;
            p.Expand();
            p.BorderColor(Color.Blue);
            p.HeavyBorder();
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
                        Markup? toVal;
                        Markup? frVal;
                        Markup? changeDt;
                        if ((cli.FieldName.ToLower()=="status"))
                        {
                            toVal = Markup.FromInterpolated($"[bold blue on white] {cli.ToValue} [/]");
                            frVal = Markup.FromInterpolated($"[dim blue on white] {cli.FromValue} [/]");
                            changeDt = Markup.FromInterpolated($"[blue on white] {changeLog.CreatedDate.ToString()} [/]");
                        }
                        else 
                        {
                            toVal = Markup.FromInterpolated($"{cli.ToValue}");
                            frVal = Markup.FromInterpolated($"{cli.FromValue}");
                            changeDt = Markup.FromInterpolated($"{changeLog.CreatedDate.ToString()}");
                            //new Text(changeLog.CreatedDate.ToString())
                        }
                        tbl.AddRow(new IRenderable[]{new Text(ji.Key.ToString()),new Text(cli.FieldName), changeDt,frVal,toVal});
                        
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
                        writer.WriteLine("jiraKEY,issueType,summary,changeLogTime,fieldName,fromStatus,toStatus");

                        for (int j = 0; j < _jtisIssues.Count; j++)
                        {
                            var jIss = _jtisIssues[j].jIssue;
                            for (int i = 0; i < jIss.ChangeLogs.Count; i++)
                            {
                                foreach (JIssueChangeLogItem cli in jIss.ChangeLogs[i].Items)
                                {
                                    if (!cli.FieldName.ToLower().StartsWith("desc") && !cli.FieldName.ToLower().StartsWith("comment"))
                                    {
                                        writer.WriteLine(string.Format("{0},{1},{2},{3},{4},{5},{6}",jIss.Key, jIss.IssueType,jIss.Summary.Replace(',',';') ,cli.ChangeLog.CreatedDate.ToString(),cli.FieldName,cli.FromValue,cli.ToValue ));
                                    }
                                }
                            }
                        }
                    }

                });
                ConsoleUtil.PressAnyKeyToContinue($"File Saved to [bold]{Environment.NewLine}{ExportPath}[/]");
        }

        private bool BuildSearch()
        {
            switch(_analysisType)
            {
                case AnalysisType.atIssues:
                case AnalysisType.atJQL:                    
                    searchJQL = ConsoleInput.GetJQLOrIssueKeys(true);
                    break;
                case AnalysisType.atEpics:
                    searchJQL = ConsoleInput.GetJQLOrIssueKeys(true,true);
                    break;
                default:
                    throw new NotImplementedException($"ChangeLogsMgs does not support AnalysisType: {Enum.GetName(typeof(AnalysisType),_analysisType)}");
            }
            return (searchJQL != null && searchJQL.Length > 0);
        }

        private void PopulateIssuesAction()        
        {   
            var jtisData = jtisIssueData.Create(JiraUtil.JiraRepo);         
            _jtisIssues.Clear();
            _jtisIssues.AddRange(jtisData.GetIssuesWithChangeLogs(searchJQL));
            ConsoleUtil.WritePerfData(jtisData.Performance);
            if (_analysisType == AnalysisType.atEpics)
            {
                PopulateEpicLinks();
            }
        }
        private void PopulateEpicLinks()
        {
            List<jtisIssue> epics = _jtisIssues.Where(x=>x.issue.Type.StringsMatch("epic")).ToList();
            if (epics.Count() > 0)
            {
                AnsiConsole.MarkupLine($"getting linked issues for [bold]{epics.Count} epics[/]");
                var epicKeys = epics.Select(x=>x.issue.Key.Value).ToArray();
                var jql = JQLBuilder.BuildJQLForFindEpicIssues(epicKeys);
                var jtisData = jtisIssueData.Create(JiraUtil.JiraRepo);         
                var children = jtisData.GetIssuesWithChangeLogs(jql);
                if (children.Count > 0)
                {
                    foreach (var child in children)
                    {
                        if (!_jtisIssues.Exists(x=>x.issue.Key.Value == child.issue.Key.Value))
                        {
                            _jtisIssues.Add(child);
                        }
                    }
                    ConsoleUtil.WritePerfData(jtisData.Performance);
                }
            }
        }
       
    }
}