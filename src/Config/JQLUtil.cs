using JTIS.Config;
using JTIS.Console;
using JTIS.Extensions;
using Spectre.Console;

namespace JTIS
{
    public static class JQLUtil
    {
        public static string[] JQLOperators
        {
            get 
            {
                return new string[]{
                    "Equals","=","Not equals ","!=","Greater than",">","Greater than equals", "(>=)","Less than","<",
                    "Less than equals","<=","IN","NOT IN","CONTAINS","~","DOES NOT CONTAIN","!~","IS","IS NOT","WAS", "WAS IN","WAS NOT IN",
                    "WAS NOT","CHANGED"};
            }
        }
        public static bool JQLSyntax(string jqlText)
        {
            bool isjql = false;                
            if (jqlText.Contains('='))
            {
                isjql = true;
            }
            else 
            {
                string[] tmp = jqlText.Split(' ',StringSplitOptions.RemoveEmptyEntries );
                if (tmp.Length > 0)
                {
                    string[] jOper = JQLUtil.JQLOperators;
                    for (int i = 0 ;i < jOper.Length; i ++)
                    {
                        var tmpO = jOper[i].ToLower();
                        if (jqlText.ToLower().Contains(tmpO,StringComparison.OrdinalIgnoreCase))
                        {
                            isjql = true;
                            break;
                        }
                    }
                }
            }
            return isjql;
        }
        public static void RemoveJQL(JTISConfig cfg)
        {
            if (cfg.SavedJQLCount == 0)
            {
                ConsoleUtil.WriteError("You don't have any saved JQL",pause:true);
                return;
            }
            ViewSavedJQL(cfg,false);
            var tbl = new Table();
            tbl.AddColumns("jqlId","jqlName","jql");
            foreach (var jqlCfg in cfg.SavedJQL)
            {
                tbl.AddRow(new Text[]{
                    new Text($"[bold blue on lightyellow3]{jqlCfg.jqlId}[/]"), 
                    new Text($"[bold]{jqlCfg.jqlName}[/]"), 
                    new Text($"[dim]{jqlCfg.jql}[/]")                    
                    });
            }
            var delId = ConsoleUtil.GetInput<int>("Enter the jqlId you wish to delete");
            if (delId > 0)
            {
                var delItem = cfg.SavedJQL.FirstOrDefault(x=>x.jqlId == delId);
                if (ConsoleUtil.Confirm($"[bold]Delete[/] saved JQL: {delItem.jqlId:00} - {delItem.jqlName}?",true))
                {
                    cfg.DeleteJQL(delItem);
                    ConsoleUtil.WaitWhileSimple("Saving config file",CfgManager.SaveConfigList);
                }
            }
        }
        public static void ViewSavedJQL(JTISConfig cfg, bool pause = true)
        {
            if (cfg.SavedJQLCount == 0)
            {                
                ConsoleUtil.WriteMarkupLine("You don't have any saved JQL",StdLine.slError.CStyle());
            }
            else 
            {
                var codeStyle = ConsoleUtil.StdStyle(StdLine.slCode);
                var colNameStyle = ConsoleUtil.StdStyle(StdLine.slOutputTitle);

                var tbl = new Table();
                tbl.AddColumns(
                    new TableColumn($"[bold]jqlId[/]").Centered(), 
                    new TableColumn($"[bold]jqlName[/]").Width(25).Centered(), 
                    new TableColumn($"jql")).LeftAligned();
                tbl.Border(TableBorder.Rounded);
                tbl.BorderColor(AnsiConsole.Foreground);
                tbl.Expand();
                foreach (var jql in cfg.SavedJQL)
                {   
                    var tRow = new TableRow(
                        new Markup[]{
                            new Markup($"[bold]{jql.jqlId:00}[/]"),
                            new Markup($"{jql.jqlName}"), 
                            new Markup($"[dim]{jql.jql}[/]")}
                            );
                    tbl.AddRow(tRow);
                    tbl.AddEmptyRow();
                }
                AnsiConsole.Write(tbl);
                if (pause)
                {
                    ConsoleUtil.PressAnyKeyToContinue($"{cfg.SavedJQLCount} results");
                }
            }
        }

        public static void AddJQL()
        {
            var jql = ConsoleInput.GetJQLOrIssueKeys(false);
            if (!string.IsNullOrEmpty(jql))
            {
                if (JQLUtil.ValidJQL(jql))
                {
                    string sName = ConsoleUtil.GetInput<string>("Enter short name to describe the entry (leave blank to cancel)",allowEmpty:true);
                    if (!string.IsNullOrWhiteSpace(sName))
                    {
                        CfgManager.config.AddJQL(sName,jql);
                        CfgManager.SaveConfigList();
                    }
                }

            }

        }

        public static bool ValidJQL(string jql)
        {
            return JiraUtil.JiraRepo.GetJQLResultsCount(jql, ignoreError:true) >= 0;
        }

        internal static void CheckManualJQL()
        {
            var jql = ConsoleInput.GetJQLOrIssueKeys(false,manualJQLValidation:true);
            if (jql.Length == 0){
                return;
            }
            AnsiConsole.Write(new Rule());
            if (JQLUtil.ValidJQL(jql) == false)
            {
                AnsiConsole.MarkupLine($"[bold]JQL is invalid and could not be parsed![/]{Environment.NewLine}[dim]({jql})[/]");
            }
            else 
            {
                var totRows = JiraUtil.JiraRepo.GetJQLResultsCount(jql);
                AnsiConsole.MarkupLine($"[bold]JQL is valid, and would return {totRows} Jira items[/]");

            }

            AnsiConsole.Write(new Rule());

            ConsoleUtil.PressAnyKeyToContinue();
        }

        public static void CheckDefaultJQL(JTISConfig cfg)
        {             

            var blockedName = "(def) Blocked Work";
            var editedName = "(def) Recent Updates";
            var allInProgressName = "(def) Status Category In Progress";
            var blockedJql = $"project={cfg.defaultProject} and status not in (backlog, done) and (priority = Blocked OR Flagged in (Impediment))";
            var editedJql = $"project={cfg.defaultProject} and updated >= -7d";
            var allInProgressJql = $"project={cfg.defaultProject} and statusCategory=\"in progress\"";

            var list = new SortedList<string,string>();
            list.Add(blockedName,blockedJql);
            list.Add(editedName,editedJql);
            list.Add(allInProgressName,allInProgressJql);

            foreach (var kvp in list)
            {
                var existJql = cfg.SavedJQL.FirstOrDefault(x=>x.jqlName == kvp.Key);
                if (existJql == null || existJql.jql.StringsMatch(kvp.Value)==false)
                {
                    if (existJql != null)
                    {
                        cfg.DeleteJQL(existJql);
                    }
                    cfg.AddJQL(kvp.Key,kvp.Value);
                }
            }
            CfgManager.SaveConfigList();
        }        
    }
}