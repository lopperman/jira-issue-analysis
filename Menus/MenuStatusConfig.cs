using System.Xml.Linq;
using System.Linq;
using JConsole.ConsoleHelpers.ConsoleTables;
using Spectre.Console;

namespace JiraCon
{
    public class MenuStatusConfig : IMenuConsole
    {
        public JTISConfig ActiveConfig {get;set;}
        public MenuStatusConfig(JTISConfig cfg)
        {
            ActiveConfig = cfg;                        
        }

        public void BuildMenu()
        {

            var cfgName = string.Format("Connected: {0} ",JTISConfigHelper.config.configName);
            string padd = new string('-',cfgName.Length + 1 );
            ConsoleLines lines = new ConsoleLines();

            lines.AddConsoleLine(" ---------------------------- " + padd, StdLine.slMenuName);
            lines.AddConsoleLine("|  ISSUE STATUS CONFIG Menu  |" + " " + cfgName, StdLine.slMenuName);
            lines.AddConsoleLine(" ---------------------------- " + padd, StdLine.slMenuName);

            lines.AddConsoleLine("(V) View All", StdLine.slMenuDetail);            
            lines.AddConsoleLine("(R) Reset Status Configs to Match Jira", StdLine.slMenuDetail);
            lines.AddConsoleLine("(E) Edit Local State", StdLine.slMenuDetail);


            lines.AddConsoleLine("");
            lines.AddConsoleLine("(B) Back to Previous Menu", StdLine.slMenuDetail);
            lines.AddConsoleLine("Enter selection or (X) to exit.", StdLine.slResponse);            

            lines.WriteQueuedLines(true,true);
            lines = null;
        }

        public bool DoMenu()
        {
            BuildMenu();
            var resp = Console.ReadKey(true);
            return ProcessKey(resp.Key);
        }

        public bool ProcessKey(ConsoleKey key)
        {
            //ConsoleKeyInfo resp = default(ConsoleKeyInfo);

           if (key == ConsoleKey.V)
            {
                ConsoleUtil.WriteStdLine("PLEASE WAIT -- COMPARING STATUS CONFIGS WITH DEFAULT LIST FROM JIRA ...",StdLine.slResponse,false);
                JTISConfigHelper.UpdateDefaultStatusConfigs();

                WriteJiraStatuses();
                ConsoleUtil.PressAnyKeyToContinue();
                return true;                                
            }
            else if (key == ConsoleKey.E)
            {
                ConsoleUtil.WriteStdLine("ENTER PART OF STATUS NAME TO CHANGE OR LEAVE BLANK TO SHOW ALL (E.G. 'Progress' would find 'Progress', 'In Progress', etc.)",StdLine.slResponse,false);
                var statusSearch = Console.ReadLine();
                WriteJiraStatuses(statusSearch);
                var editJiraId = ConsoleUtil.GetConsoleInput<int>("ENTER JiraId TO CHANGE HOW THAT STATUS IS CATEGORIZED",false);
                if (editJiraId > 0)
                {
                    var changeCfg = ActiveConfig.StatusConfigs.SingleOrDefault(x=>x.StatusId == editJiraId);
                    if (changeCfg == null)
                    {
                        ConsoleUtil.WriteStdLine(string.Format("'{0}' IS NOT A VALID JIRA STATUS ID - PRESS ANY KEY TO CONTINUE",editJiraId),StdLine.slError ,false);
                        Console.ReadKey(true);
                    }
                    else 
                    {
                        ConsoleUtil.WriteStdLine(string.Format("Jira Status: '{0}' is currently set to '{1}' for Analysis.{2}{3}Enter 'A' to change to Active{2}{3}Enter 'P' to change to Passive{2}{3}Enter 'I' to change to Ignore{2}{3}Press ENTER to cancel",changeCfg.StatusName,Enum.GetName(typeof(StatusType),changeCfg.Type),Environment.NewLine,"    "),StdLine.slResponse,false);
                        var changeTo = Console.ReadKey(true);
                        bool reSave = false;
                        switch(changeTo.Key)
                        {
                            case ConsoleKey.A:
                                changeCfg.CategoryName = "in progress";
                                reSave = true;
                                break;
                            case ConsoleKey.P:
                                changeCfg.CategoryName = "to do";
                                reSave = true; 
                                break;
                            case ConsoleKey.I:
                                changeCfg.CategoryName = "ignore";
                                reSave = true;
                                break;                        
                        }
                        if (reSave)
                        {
                            ConsoleUtil.WriteStdLine(string.Format("Saving changes to '{0}' ...",changeCfg.StatusName ),StdLine.slOutput  ,false);
                            JTISConfigHelper.SaveConfigList();
                            ConsoleUtil.WriteStdLine("PRESS ANY KEY TO CONTINUE",StdLine.slResponse ,false);
                            Console.ReadKey(true);
                        }
                    }
                }

                return true;
            }
            else if (key == ConsoleKey.R)
            {
                ConsoleUtil.WriteStdLine("PRESS 'Y' TO RESET LOCAL STATUS CONFIGS TO MATCH CURRENT JIRA PROPERTIES?",StdLine.slResponse,false);
                if (Console.ReadKey(true).Key == ConsoleKey.Y)
                {
                    ActiveConfig.DefaultStatusConfigs.Clear();
                    ActiveConfig.StatusConfigs.Clear();
                    ConsoleUtil.WriteStdLine("PLEASE WAIT -- COMPARING STATUS CONFIGS WITH DEFAULT LIST FROM JIRA ...",StdLine.slResponse,false);
                    JTISConfigHelper.UpdateDefaultStatusConfigs(true);
                }
                return true;
            }

            else if (key == ConsoleKey.B)
            {
                return false;
            }
            else if (key == ConsoleKey.X)
            {                
                if(ConsoleUtil.ByeBye())
                {
                    Environment.Exit(0);
                }
            }
 

            return true;
        }

        private void WriteJiraStatuses(string? searchTerm = null)
        {

            var usedInCol = string.Format("UsedIn: {0}",ActiveConfig.defaultProject);
            Table table = new Table();
            table.AddColumns("JiraId","Name","LocalState","DefaultState",usedInCol,"Override");

            foreach (var jStatus in ActiveConfig.StatusConfigs.OrderByDescending(d=>d.DefaultInUse).ThenBy(x=>x.Type).ThenBy(y=>y.StatusName).ToList())
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
                    JiraStatus  defStat = ActiveConfig.DefaultStatusConfigs.Single(x=>x.StatusId == jStatus.StatusId );
                    string usedIn = string.Empty;   
                    string overridden = string.Empty;      
                    string locState = Enum.GetName(typeof(StatusType),jStatus.Type);     
                    if (jStatus.DefaultInUse)
                    {
                        usedIn = "YES";
                    }
                    if (jStatus.Type != defStat.Type)
                    {
                        overridden = "[red on yellow]***[/]";
                        locState = string.Format("[default on yellow]{0}[/]",locState);
                    }
                    table.AddRow(new string[]{jStatus.StatusId.ToString(), jStatus.StatusName,locState,Enum.GetName(typeof(StatusType),defStat.Type),usedIn, overridden});
                }
            }
            AnsiConsole.Write(table);


            // var table = new ConsoleTable("JiraId","Name","LocalState","DefaultState","UsedIn: " + ActiveConfig.defaultProject,"Override");
            // foreach (var jStatus in ActiveConfig.StatusConfigs.OrderByDescending(d=>d.DefaultInUse).ThenBy(x=>x.Type).ThenBy(y=>y.StatusName).ToList())
            // {
            //     bool includeStatus = false;
            //     if (searchTerm == null || searchTerm.Length == 0)
            //     {
            //         includeStatus = true;
            //     }
            //     else 
            //     {
            //         if (jStatus.StatusName.ToLower().Contains(searchTerm.ToLower()))
            //         {
            //             includeStatus = true;
            //         }
            //     }
            //     if (includeStatus)
            //     {
            //         JiraStatus  defStat = ActiveConfig.DefaultStatusConfigs.Single(x=>x.StatusId == jStatus.StatusId );
            //         string usedIn = string.Empty;   
            //         string overridden = string.Empty;                 
            //         if (jStatus.DefaultInUse)
            //         {
            //             usedIn = "YES";
            //         }
            //         if (jStatus.Type != defStat.Type)
            //         {
            //             overridden = "***";
            //         }                    
            //         table.AddRow(jStatus.StatusId, jStatus.StatusName,Enum.GetName(typeof(StatusType),jStatus.Type),Enum.GetName(typeof(StatusType),defStat.Type),usedIn, overridden);
            //     }
            // }
            // table.Options.NumberAlignment = Alignment.Right;                
            // table.Write();                

        }
    }
}