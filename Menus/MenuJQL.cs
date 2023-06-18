using System.Xml.Linq;
using System.Linq;



namespace JiraCon
{
    public class MenuJQL : IMenuConsole
    {
        public JTISConfig ActiveConfig {get;set;}
        public MenuJQL(JTISConfig cfg)
        {
            ActiveConfig = cfg;                        
        }

        public void BuildMenu()
        {
            var cfgName = string.Format("Connected: {0} ",JTISConfigHelper.config.configName);
            string padd = new string('-',cfgName.Length + 1 );
            ConsoleLines lines = new ConsoleLines();

            lines.AddConsoleLine(" ------------ " + padd, StdLine.slMenuName);
            lines.AddConsoleLine("|  JQL Menu  |" + " " + cfgName, StdLine.slMenuName);
            lines.AddConsoleLine(" ------------ " + padd, StdLine.slMenuName);

            lines.AddConsoleLine("(V) View All Saved JQL", StdLine.slMenuDetail);
            lines.AddConsoleLine("(A) Add JQL", StdLine.slMenuDetail);
            lines.AddConsoleLine("(F) Find Saved JQL", StdLine.slMenuDetail);
            lines.AddConsoleLine("(D) Delete a Saved JQL", StdLine.slMenuDetail);


            lines.AddConsoleLine("");
            lines.AddConsoleLine("(B) Back to Previous Menu", StdLine.slMenuDetail);
            lines.AddConsoleLine("(X) Exit", StdLine.slResponse);            
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
            ConsoleKeyInfo resp = default(ConsoleKeyInfo);

           if (key == ConsoleKey.V)
            {
                if (JTISConfigHelper.config.SavedJQLCount > 0)
                {
                    for (int i = 0; i < JTISConfigHelper.config.SavedJQLCount; i ++)
                    {
                        JQLConfig tJql = JTISConfigHelper.config.SavedJQL[i];
                        ConsoleUtil.Lines.AddConsoleLine(string.Format("NAME: {0:00} - {1}",tJql.jqlId,tJql.jqlName) ,StdLine.slOutputTitle);
                        ConsoleUtil.Lines.AddConsoleLine(string.Format("JQL: {0}",tJql.jql) ,StdLine.slCode);
                    }
                }
                else 
                {
                    ConsoleUtil.Lines.AddConsoleLine("Saved JQL does not exist for current config",StdLine.slOutput);
                }
                ConsoleUtil.Lines.WriteQueuedLines(false);
                Console.ReadKey(true);
                return true;                                
            }
            else if (key == ConsoleKey.D)
            {
                ConsoleUtil.Lines.AddConsoleLine(" ** SAVED JQL **",StdLine.slCode );
                if (JTISConfigHelper.config.SavedJQLCount > 0)
                {
                    for (int i = 0; i < JTISConfigHelper.config.SavedJQLCount; i ++)
                    {
                        JQLConfig tJql = JTISConfigHelper.config.SavedJQL[i];
                        ConsoleUtil.Lines.AddConsoleLine(string.Format("NAME: {0:00} - {1}",tJql.jqlId,tJql.jqlName) ,StdLine.slOutputTitle);
                        ConsoleUtil.Lines.AddConsoleLine(string.Format("JQL: {0}",tJql.jql) ,StdLine.slCode);
                    }
                }
                else 
                {
                    ConsoleUtil.Lines.AddConsoleLine("Saved JQL does not exist for current config",StdLine.slOutput);
                }
                ConsoleUtil.Lines.WriteQueuedLines(false);
                if (JTISConfigHelper.config.SavedJQLCount > 0)
                {
                    var deleteJqlId = ConsoleUtil.GetConsoleInput<int>("Enter JQL item number (number after 'NAME') to delete. Enter zero ('0') to cancel",false);
                    if (deleteJqlId > 0 && deleteJqlId <= JTISConfigHelper.config.SavedJQLCount)
                    {
                        JQLConfig? delCfg = JTISConfigHelper.config.SavedJQL.Single(x=>x.jqlId == deleteJqlId);
                        if (delCfg != null) 
                        {
                            ConsoleUtil.WriteStdLine(string.Format("PRESS 'Y' TO DELETE SAVED JQL BELOW",delCfg.jqlName),StdLine.slResponse,false);
                            ConsoleUtil.WriteStdLine(string.Format("{0:00} - {1}",delCfg.jqlId, delCfg.jqlName),StdLine.slCode,false);
                            ConsoleUtil.WriteStdLine(string.Format("{0}",delCfg.jql),StdLine.slCode,false);

                            if (Console.ReadKey(true).Key == ConsoleKey.Y)
                            {
                                JTISConfigHelper.config.SavedJQL.Remove(delCfg);
                                if (JTISConfigHelper.config.SavedJQLCount > 0)
                                {
                                    for (int i = 0; i < JTISConfigHelper.config.SavedJQLCount; i ++)
                                    {
                                        JTISConfigHelper.config.SavedJQL[i].jqlId = i + 1;
                                    }
                                }
                                JTISConfigHelper.SaveConfigList();
                                ConsoleUtil.WriteStdLine(string.Format("SAVED JQL: {0} WAS DELETED - PRESS ANY KEY",delCfg.jqlName),StdLine.slResponse,false);
                                Console.ReadKey(true);
                            }
                        }
                    }
                }
                else 
                {
                    ConsoleUtil.WriteStdLine("PRESS ANY KEY TO CONTINUE",StdLine.slResponse,false);
                    Console.ReadKey(true);
                }
                return true;                                

            }
            else if (key == ConsoleKey.F)
            {
                int matchCount = 0;
                bool wcBegin = false;
                bool wcEnd = false;

                if (JTISConfigHelper.config.SavedJQLCount > 0)
                {
                    var searchTerm = ConsoleUtil.GetConsoleInput<string>("Enter search text - use '*' at beginning and/or end to do wildcard search (e.g. '*Story' finds text ending with 'story'; '*story*' find any text containing 'story' - NOTE will search name as well as JQL",false);
                    if (searchTerm.Length > 0)
                    {
                        bool isMatch = false ;
                        if (searchTerm.StartsWith("*") && searchTerm.EndsWith("*"))
                        {
                            wcBegin = true;
                            wcEnd = true;
                        }
                        else if (searchTerm.StartsWith("*"))
                        {
                            wcBegin = true;
                        }
                        else if (searchTerm.EndsWith("*"))
                        {
                            wcEnd = true ;
                        }
                        searchTerm = searchTerm.Replace("*","");

                        for (int i = 0; i < JTISConfigHelper.config.SavedJQLCount; i ++)
                        {
                            isMatch = false; 

                            JQLConfig tJql = JTISConfigHelper.config.SavedJQL[i];
                            if (wcBegin && wcEnd)
                            {
                                if (tJql.jql.Contains(searchTerm,StringComparison.OrdinalIgnoreCase) || tJql.jqlName.Contains(searchTerm,StringComparison.OrdinalIgnoreCase))
                                {
                                    isMatch = true;
                                }
                            }
                            else if (wcBegin)

                            {
                                if (tJql.jql.EndsWith(searchTerm,StringComparison.OrdinalIgnoreCase ) || tJql.jqlName.EndsWith(searchTerm,StringComparison.OrdinalIgnoreCase ))
                                {
                                    isMatch = true;
                                }
                            }
                            else if (wcEnd)
                            {
                                if (tJql.jql.StartsWith(searchTerm,StringComparison.OrdinalIgnoreCase ) || tJql.jqlName.StartsWith(searchTerm,StringComparison.OrdinalIgnoreCase ))
                                {
                                    isMatch = true;
                                }
                            }
                            if (isMatch)
                            {
                                matchCount +=1;
                                ConsoleUtil.Lines.AddConsoleLine(string.Format("NAME: {0:00} - {1}",tJql.jqlId,tJql.jqlName) ,StdLine.slOutputTitle);
                                ConsoleUtil.Lines.AddConsoleLine(string.Format("JQL: {0}",tJql.jql) ,StdLine.slOutput);
                            }
                        }
                    }
                }
                else 
                {
                    ConsoleUtil.Lines.AddConsoleLine("Saved JQL does not exist for current config",StdLine.slOutput);
                }
                if (matchCount == 0 && JTISConfigHelper.config.SavedJQLCount > 0)
                {
                    ConsoleUtil.Lines.AddConsoleLine("no matching jql was found",StdLine.slOutput);
                }
                ConsoleUtil.Lines.WriteQueuedLines(false);
                Console.ReadKey(true);
                return true;                                


            }
            else if (key == ConsoleKey.A)
            {
                string tmpName = string.Empty;
                string tmpJql = string.Empty;
                tmpJql = ConsoleUtil.GetConsoleInput<string>("Enter JQL");                
                tmpName = ConsoleUtil.GetConsoleInput<string>("Enter short name to describe JQL");                
                ConsoleUtil.WriteLine(string.Format("Name: {0}",tmpName));
                ConsoleUtil.WriteLine(string.Format("JQL: {0}",tmpJql));
                ConsoleUtil.WriteLine(string.Format("Press 'Y' to save, otherwise press any key"));
                resp = Console.ReadKey(true);
                if (resp.Key == ConsoleKey.Y)
                {
                    JTISConfigHelper.config.AddJQL(tmpName,tmpJql);
                    JTISConfigHelper.SaveConfigList();
                    return false;
                }
                
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