


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

            lines.AddConsoleLine("");
            lines.AddConsoleLine("(C) Back to Config Menu", StdLine.slMenuDetail);
            lines.AddConsoleLine("Enter selection or (E) to exit.", StdLine.slResponse);            

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
                        ConsoleUtil.Lines.AddConsoleLine(string.Format("JQL: {0}",tJql.jql) ,StdLine.slOutput);
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
            else if (key == ConsoleKey.C)
            {
                return false;
            }
            else if (key == ConsoleKey.E)
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