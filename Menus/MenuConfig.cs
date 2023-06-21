


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
            if (ActiveConfig.configId != JTISConfigHelper.config.configId)
            {
                ActiveConfig = JTISConfigHelper.config;
            }

            var cfgName = string.Format("Connected: {0} ",ActiveConfig.configName);
            string padd = new string('-',cfgName.Length + 1 );
            ConsoleLines lines = new ConsoleLines();
            lines.AddConsoleLine(" --------------- " + padd, ConsoleUtil.StdStyle(StdLine.slMenuDetail));
            lines.AddConsoleLine("|  Config Menu  |" + " " + cfgName, ConsoleUtil.StdStyle(StdLine.slMenuDetail));
            lines.AddConsoleLine(" --------------- " + padd, ConsoleUtil.StdStyle(StdLine.slMenuDetail));
            lines.AddConsoleLine(string.Format("INFO - Config File: {0}",JTISConfigHelper.ConfigFilePath), ConsoleUtil.StdStyle(StdLine.slMenuDetail));
            lines.AddConsoleLine(string.Format("INFO - Output Files: {0}",JTISConfigHelper.JTISRootPath), ConsoleUtil.StdStyle(StdLine.slMenuDetail));
            lines.AddConsoleLine("(J) Manage Saved JQL", ConsoleUtil.StdStyle(StdLine.slMenuDetail));
            lines.AddConsoleLine("(N) Add New Jira Config", ConsoleUtil.StdStyle(StdLine.slMenuDetail));
            lines.AddConsoleLine("(C) Change Current Jira Config", ConsoleUtil.StdStyle(StdLine.slMenuDetail));
            lines.AddConsoleLine("(V) View JiraConsole (this app) config", ConsoleUtil.StdStyle(StdLine.slMenuDetail));
            lines.AddConsoleLine("(R) Remove Login Configuation", ConsoleUtil.StdStyle(StdLine.slMenuDetail));
            lines.AddConsoleLine(string.Format("(I) View Jira Info for {0}",JiraUtil.JiraRepo.ServerInfo.BaseUrl), ConsoleUtil.StdStyle(StdLine.slMenuDetail));
            lines.AddConsoleLine("",ConsoleUtil.StdStyle(StdLine.slMenuDetail));
            lines.AddConsoleLine("(B) Back to Main Menu", ConsoleUtil.StdStyle(StdLine.slMenuDetail));
            lines.AddConsoleLine("Enter selection or (X) to exit.", ConsoleUtil.StdStyle(StdLine.slResponse));
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
                if (changeCfg != null && changeCfg.ValidConfig)
                {
                    JTISConfigHelper.config = changeCfg;                    
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