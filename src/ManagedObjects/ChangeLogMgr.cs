using System.Net.Http;
using Atlassian.Jira;
using JTIS.Analysis;
using JTIS.Config;
using JTIS.Console;
using JTIS.Data;
using JTIS.Extensions;
using JTIS.Menu;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace JTIS
{
    public class ChangeLogsMgr
    {
        private jtisFilterItems<string> _issueTypeFilter = new jtisFilterItems<string>();
        private jtisFilterItems<string> _issueFieldFilter = new jtisFilterItems<string>();
        private List<jtisIssue> _filteredIssues = new List<jtisIssue>();
        private jtisIssueData? _jtisIssueData = null;
        private string? _exportPath = null;
        private FetchOptions fetchOptions = FetchOptions.DefaultFetchOptions.CacheResults().AllowCachedSelection().IncludeChangeLogs();

        public ChangeLogsMgr(AnalysisType analysisType)
        {
            if (analysisType == AnalysisType.atEpics){fetchOptions.FetchEpicChildren=true;}
            _jtisIssueData = IssueFetcher.FetchIssues(fetchOptions);
            if (fetchOptions.Cancelled) {return;}

            bool doExport = false;
            if (_jtisIssueData != null && _jtisIssueData.jtisIssueCount > 0)
            {
                CheckIssueTypeFilter();
                CheckIssueFieldFilter();
                BuildFilteredList();

                ConsoleUtil.StartAutoRecording();
                Render();
                ConsoleUtil.StopAutoRecording("ChangeLog");

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

        private void BuildFilteredList()
        {
            _filteredIssues = _jtisIssueData.jtisIssuesList.Where(x=>_issueTypeFilter.IsFiltered(x.issue.Type.Name)).ToList();
            _filteredIssues = _filteredIssues.Where(x=>x.ChangeLogs.Any(y=>y.Items.Any(i=>_issueFieldFilter.IsFiltered(i.FieldName)))).ToList();
            _filteredIssues = _filteredIssues.OrderBy(x=>x.jIssue.Key).ToList();
        }
        private void CheckIssueFieldFilter()
        {
            _issueFieldFilter.Clear();
            foreach (var kvp in _jtisIssueData.IssueChangeLogFieldsCount)
            {
                _issueFieldFilter.AddFilterItem(kvp.Key,$"Count: {kvp.Value}");

            }
            if (_issueFieldFilter.Count > 1)
            {
                if (ConsoleUtil.Confirm($"Filter which of the {_issueFieldFilter.Count} fields get displayed?",true))
                {
                    var response = MenuManager.MultiSelect<jtisFilterItem<string>>($"Choose items to include. [dim](To select all items, press ENTER[/])",_issueFieldFilter.Items.ToList(),pageSize:MenuManager.MenuPageSize);
                    if (response != null && response.Count() > 0)
                    {
                        _issueFieldFilter.Clear();
                        _issueFieldFilter.AddFilterItems(response); 
                    }
                }
            }

        }
        private void CheckIssueTypeFilter()
        {
            _issueTypeFilter.Clear();
            foreach (var kvp in _jtisIssueData.IssueTypesCount)
            {
                _issueTypeFilter.AddFilterItem(kvp.Key,$"Count: {kvp.Value}");

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


        public void Render(bool writeAll = false, int startAt = 0)
        {
            var filtered = _filteredIssues;
            var filteredCount = filtered.Count;

            if (startAt < 0 || startAt >= filtered.Count) {startAt = 0;}
            for (int i = startAt; i < filtered.Count; i ++)
            // foreach (var iss in filtered)
            {
                jtisIssue iss = filtered[i];
                if (writeAll==false)
                {
                    AnsiConsole.Clear();
                }
                WriteIssueHeader(iss.jIssue, i+1, filteredCount);
                WriteIssueDetail(iss);
                AnsiConsole.WriteLine();
                if (writeAll == false)
                {
                    var resp = ConsoleUtil.GetInput<string>($"ENTER= View Next, P=Show Previous, 1-{filteredCount}=Go To Item, A=Show All, X=Return to Menu",allowEmpty:true);
                    if (resp.StringsMatch("X")) 
                    {
                        return;
                    } 
                    else if (resp.StringsMatch("A"))
                    {
                        Render(true);
                        return;
                    }
                    else if (resp.StringsMatch("P"))
                    {
                        Render(writeAll,i-1);
                        return;
                    }
                    else if (resp.Length > 0)
                    {
                        int tempIdx = -1;
                        if (int.TryParse(resp, out tempIdx))
                        {
                            tempIdx = tempIdx - 1;
                            if (tempIdx >=0 && tempIdx < filteredCount)
                            {
                                Render(writeAll,tempIdx);
                                return;
                            }
                        }
                    }
                }
            }
            if (writeAll)
            {
                ConsoleUtil.PressAnyKeyToContinue();
            }

        }
        private void WriteIssueHeader(JIssue ji, int itemIndex, int totalResults)
        {
            if (JTISTimeZone.DefaultTimeZone==false)
            {
                AnsiConsole.Write(new Rule(ConsoleUtil.TimeZoneAlert));
            }
            var escSummary = Markup.Escape(ConsoleUtil.Scrub(ji.Summary));
            AnsiConsole.Write(new Rule($"[dim]({itemIndex:000} of {totalResults:#000} results)[/]"){Style=new Style(Color.Blue,Color.Cornsilk1), Justification=Justify.Center});

            var p = new Panel($"[bold]Change Logs For {ji.Key}[/], ([dim]Issue Type: [/][bold]{ji.IssueType}[/][dim] Status:[/][bold] {ji.StatusName})[/]{Environment.NewLine}[dim]{escSummary}[/]");
            p.Border = BoxBorder.Rounded;
            p.Expand();
            p.BorderColor(Color.Blue);
            p.HeavyBorder();
            p.Padding(2,1,1,2);
            AnsiConsole.Write(p);
        }
        private void WriteIssueDetail(jtisIssue ji)
        {
            var tbl = new Table();
            tbl.AddColumn("[dim]CHANGELOG ID[/]");
            tbl.AddColumn("KEY");
            tbl.AddColumn("FIELD");
            tbl.AddColumn("CHANGED DT");
            tbl.AddColumn("OLD VALUE");
            tbl.AddColumn("NEW VALUE");

            //order change logs by create date


            for (int i = 0; i < ji.ChangeLogs.Count; i++)
            {
                IssueChangeLog changeLog = ji.ChangeLogs[i];
                foreach (IssueChangeLogItem cli in changeLog.Items)
                {                    
                    if (!cli.FieldName.ToLower().StartsWith("desc") && !cli.FieldName.ToLower().StartsWith("comment"))
                    {
                        if (_issueFieldFilter.Items.Any(x=>x.Value.StringsMatch(cli.FieldName)))
                        {
                            Markup? toVal;
                            Markup? frVal;
                            Markup? changeDt;
                            if ((cli.FieldName.ToLower()=="status"))
                            {
                                toVal = Markup.FromInterpolated($"[bold blue on white] {cli.ToValue.CheckTimeZone()} [/]");
                                frVal = Markup.FromInterpolated($"[dim blue on white] {cli.FromValue.CheckTimeZone()} [/]");
                                changeDt = Markup.FromInterpolated($"[blue on white] {changeLog.CreatedDate.CheckTimeZone().ToString()} [/]");
                            }
                            else 
                            {
                                if (!string.IsNullOrWhiteSpace(cli.ToValue) && (cli.ToValue.StringsMatch("block",StringCompareType.scContains) || cli.ToValue.StringsMatch("impediment")))
                                {
                                    toVal = Markup.FromInterpolated($"[bold maroon on cornsilk1]{ConsoleUtil.Scrub(cli.ToValue)}[/]");
                                }
                                else 
                                {
                                    toVal = Markup.FromInterpolated($"{ConsoleUtil.Scrub(cli.ToValue.CheckTimeZone())}");
                                }
                                frVal = Markup.FromInterpolated($"{ConsoleUtil.Scrub(cli.FromValue.CheckTimeZone())}");
                                changeDt = Markup.FromInterpolated($"{changeLog.CreatedDate.CheckTimeZone().ToString()}");
                                //new Text(changeLog.CreatedDate.ToString())
                            }
                            var clID = new Markup($"[dim]{changeLog.Id}[/]");
                            tbl.AddRow(new IRenderable[]{clID,  new Text(ji.jIssue.Key.ToString()),new Text(cli.FieldName), changeDt,frVal,toVal});
                        }
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

                        for (int j = 0; j < _jtisIssueData.jtisIssueCount; j++)
                        {
                            var jIss = _jtisIssueData.jtisIssuesList[j].jIssue;
                            for (int i = 0; i < jIss.ChangeLogs.Count; i++)
                            {
                                foreach (JIssueChangeLogItem cli in jIss.ChangeLogs[i].Items)
                                {
                                    if (!cli.FieldName.ToLower().StartsWith("desc") && !cli.FieldName.ToLower().StartsWith("comment"))
                                    {
                                        writer.WriteLine(string.Format("{0},{1},{2},{3},{4},{5},{6}",jIss.Key, jIss.IssueType,jIss.Summary.Replace(',',';') ,cli.ChangeLog.CreatedDate.CheckTimeZone().ToString(),cli.FieldName,cli.FromValue,cli.ToValue ));
                                    }   
                                }
                            }
                        }
                    }

                });
                ConsoleUtil.PressAnyKeyToContinue($"File Saved to [bold]{Environment.NewLine}{ExportPath}[/]");
        }

       
    }
}