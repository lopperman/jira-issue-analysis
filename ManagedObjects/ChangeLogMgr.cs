using System.Security.Cryptography;
using Atlassian.Jira;
using JTIS.Analysis;
using JTIS.Config;
using JTIS.Console;
using JTIS.Data;
using JTIS.Extensions;
using JTIS.ManagedObjects;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace JTIS
{
   public class ChangeLogsMgr
    {
        private readonly AnalysisType _analysisType;
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
        public void Render()
        {
            foreach (var iss in _jtisIssues)
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
                        if ((cli.FieldName.ToLower()=="status"))
                        {
                            toVal = Markup.FromInterpolated($"[bold blue on white] {cli.ToValue} [/]");
                            frVal = Markup.FromInterpolated($"[blue on white] {cli.FromValue} [/]");
                        }
                        else 
                        {
                            toVal = Markup.FromInterpolated($"{cli.ToValue}");
                            frVal = Markup.FromInterpolated($"{cli.FromValue}");
                        }
                        // string toVal = (cli.FieldName.ToLower()=="status") ? string.Format($"[bold blue on white] {cli.ToValue} [/]") : cli.ToValue;
                        // string frVal = (cli.FieldName.ToLower()=="status") ? string.Format($"[dim blue on white] {cli.FromValue} [/]") : cli.FromValue;
                        tbl.AddRow(new IRenderable[]{new Text(ji.Key.ToString()),new Text(cli.FieldName), new Text(changeLog.CreatedDate.ToString()),frVal,toVal});
                        
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
            _jtisIssues = jtisIssueData.Create(JiraUtil.JiraRepo).GetIssuesWithChangeLogs(searchJQL);
        }
        private void PopulateEpicLinks()
        {
            List<jtisIssue> epics = _jtisIssues.Where(x=>x.issue.Type.StringsMatch("epic")).ToList();
            if (epics.Count() > 0)
            {
                var epicKeys = epics.Select(x=>x.issue.Key.Value).ToArray();
                var jql = JQLBuilder.BuildJQLForFindEpicIssues(epicKeys);
                var children = jtisIssueData.Create(JiraUtil.JiraRepo).GetIssuesWithChangeLogs(jql);
                if (children.Count > 0)
                {
                    foreach (var child in children)
                    {
                        if (!_jtisIssues.Exists(x=>x.issue.Key.Value == child.issue.Key.Value))
                        {
                            _jtisIssues.Add(child);
                        }
                    }
                }
            }
        }
       
    }
}