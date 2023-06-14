


namespace JiraCon
{


    public class MenuConfig : IMenuConsole
    {
        public JTISConfig ActiveConfig {get;set;}        
        public MenuConfig(JTISConfig cfg)
        {
            ActiveConfig = cfg;                        
        }


        private void BuildMenu()
        {
            var cfgName = string.Format("Connected: {0} ",ActiveConfig.configName);
            string padd = new string('-',cfgName.Length + 1 );
            ConsoleLines lines = new ConsoleLines();
            lines.AddConsoleLine(" --------------- " + padd, StdLine.slMenuName);
            lines.AddConsoleLine("|  Config Menu  |" + " " + cfgName, StdLine.slMenuName);
            lines.AddConsoleLine(" --------------- " + padd, StdLine.slMenuName);
            lines.AddConsoleLine(string.Format("INFO - Config File: {0}",JTISConfigHelper.ConfigFilePath), StdLine.slMenuName);
            lines.AddConsoleLine(string.Format("INFO - Output Files: {0}",JTISConfigHelper.JTISRootPath), StdLine.slMenuName);
            lines.AddConsoleLine("(J) Manage Saved JQL", StdLine.slMenuDetail);
            lines.AddConsoleLine("(N) Add New Jira Config", StdLine.slMenuDetail);
            lines.AddConsoleLine("(C) Change Current Jira Config", StdLine.slMenuDetail);
            lines.AddConsoleLine("(V) View JiraConsole (this app) config", StdLine.slMenuDetail);
            lines.AddConsoleLine("(R) Remove Login Configuation", StdLine.slMenuDetail);
            lines.AddConsoleLine(string.Format("(I) View Jira Info for {0}",JiraUtil.JiraRepo.ServerInfo.BaseUrl), StdLine.slMenuDetail);
            lines.AddConsoleLine("");
            lines.AddConsoleLine("(M) Main Menu", StdLine.slMenuDetail);
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
            if (key == ConsoleKey.R)
            {
                if (JTISConfigHelper.config != null)
                {
                    Console.WriteLine("Type 'Y' to DELETE 1 or more saved Jira Configurations");
                    resp = Console.ReadKey(true);
                    if (resp.Key == ConsoleKey.Y)
                    {
                        JTISConfigHelper.DeleteConfig();
                    }
                    if (JTISConfigHelper.config == null )
                    {
                        ConsoleUtil.ByeByeForced();
                    }
                    return true;                    
                }
            }
            else if (key == ConsoleKey.V)
            {                
                JTISConfigHelper.ViewAll();
                return true;
            }
            else if (key == ConsoleKey.I)
            {
                JEnvironmentConfig.JiraEnvironmentInfo();
                return true;
            }
            else if (key == ConsoleKey.J)
            {
                while (MenuManager.DoMenu(new MenuJQL(ActiveConfig)))
                {

                }
                return true;                
            }
            else if (key == ConsoleKey.N)
            {
                var newCfg = JTISConfigHelper.CreateConfig();
                if (newCfg != null && newCfg.ValidConfig==true)
                {
                    JTISConfigHelper.config = newCfg;
                    return true;
                }
                return false;
            }
            else if (key == ConsoleKey.C)
            {
                var changeCfg = JTISConfigHelper.ChangeCurrentConfig(null);
                if (changeCfg != null)
                {
                    JTISConfigHelper.config = changeCfg;                    
                }
                return true;
            }
            else if (key == ConsoleKey.M)
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