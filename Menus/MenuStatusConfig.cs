using System.Xml.Linq;
using System.Linq;
using JConsole.ConsoleHelpers.ConsoleTables;

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

                var table = new ConsoleTable("JiraId","Name","LocalState","DefaultState","UsedIn: " + ActiveConfig.defaultProject);
                foreach (var jStatus in ActiveConfig.StatusConfigs.OrderByDescending(d=>d.DefaultInUse).ThenBy(x=>x.Type).ThenBy(y=>y.StatusName).ToList())
                {
                    JiraStatus  defStat = ActiveConfig.DefaultStatusConfigs.Single(x=>x.StatusId == jStatus.StatusId );
                    string usedIn = string.Empty;                    
                    if (jStatus.DefaultInUse)
                    {
                        usedIn = "YES";
                    }
                    table.AddRow(jStatus.StatusId, jStatus.StatusName,Enum.GetName(typeof(StatusType),jStatus.Type),Enum.GetName(typeof(StatusType),defStat.Type),usedIn);
                }
                table.Write();                
                ConsoleUtil.WriteStdLine(" -- PRESS ANY KEY -- ",StdLine.slResponse,false);
                Console.ReadKey(true);


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
    }
}