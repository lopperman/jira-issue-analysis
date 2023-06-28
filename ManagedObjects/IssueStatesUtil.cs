using JTIS.Config;
using JTIS.Console;
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
                var changeCfg = JTISConfigHelper.config.StatusConfigs.SingleOrDefault(x=>x.StatusId == editJiraId);
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
                        JTISConfigHelper.SaveConfigList();
                        ConsoleUtil.PressAnyKeyToContinue();
                    }
                }
            }            
        }

        public static void WriteJiraStatuses(string? searchTerm = null)
        {
            var usedInCol = string.Format("UsedIn: {0}",JTISConfigHelper.config.defaultProject);
            Table table = new Table();
            table.AddColumns("JiraId","Name","LocalState","DefaultState",usedInCol,"Override");
            table.Columns[2].Alignment(Justify.Center);
            table.Columns[3].Alignment(Justify.Center);
            table.Columns[4].Alignment(Justify.Center);
            table.Columns[5].Alignment(Justify.Center);

            foreach (var jStatus in JTISConfigHelper.config.StatusConfigs.OrderByDescending(d=>d.DefaultInUse).ThenBy(x=>x.Type).ThenBy(y=>y.StatusName).ToList())
            {
                bool includeStatus = false;
                if (searchTerm == null || searchTerm.Length == 0)
                {
                    includeStatus = true;
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
                    JiraStatus  defStat = JTISConfigHelper.config.DefaultStatusConfigs.Single(x=>x.StatusId == jStatus.StatusId );
                    string usedIn = string.Empty;   
                    string overridden = string.Empty;      
                    string locState = Enum.GetName(typeof(StatusType),jStatus.Type);     
                    if (jStatus.DefaultInUse)
                    {
                        usedIn = "YES";
                    }
                    if (jStatus.Type != defStat.Type)
                    {
                        
                        overridden = string.Format("[bold red on yellow]{0}{0}{0}[/]",":triangular_Flag:");
                        locState = string.Format("[bold blue on lightyellow3]{0}[/]",locState);
                    }
                    table.AddRow(new string[]{jStatus.StatusId.ToString(), jStatus.StatusName,locState,Enum.GetName(typeof(StatusType),defStat.Type),usedIn, overridden});
                }
            }
            AnsiConsole.Write(table);
        }                

    }
}