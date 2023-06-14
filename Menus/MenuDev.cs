


namespace JiraCon
{
    public class MenuDev : IMenuConsole
    {

        public JTISConfig ActiveConfig {get;set;}
        public MenuDev(JTISConfig cfg)
        {
            ActiveConfig = cfg;                        
        }

        public void BuildMenu()
        {
            var cfgName = string.Format("Connected: {0} ",JTISConfigHelper.config.configName);
            string padd = new string('-',cfgName.Length + 1 );
            ConsoleLines lines = new ConsoleLines();
            lines.AddConsoleLine(" ------------ " + padd, StdLine.slMenuName);
            lines.AddConsoleLine("|  DEV Menu  |" + " " + cfgName, StdLine.slMenuName);
            lines.AddConsoleLine(" ------------ " + padd, StdLine.slMenuName);
            lines.AddConsoleLine("(C) View Console Fore/Back Colors", StdLine.slMenuDetail);
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

            if (key == ConsoleKey.C)
            {
                for (int iBack = 0; iBack <=15; iBack ++)
                {
                    for (int iFore = 0; iFore <= 15; iFore ++)
                    {
                        if (iBack != iFore) 
                        {
                            ConsoleColor ccFore = (ConsoleColor)iFore;
                            ConsoleColor ccBack = (ConsoleColor)iBack;
                            
                            string clrTest = string.Format("BackColor: {0}, ForeColor: {1}, Testing Standing Console Colors",ccBack,ccFore);
                            ConsoleUtil.WriteLine(clrTest,ccFore, ccBack, false);
                        }
                    }
                    Console.WriteLine("** PRESS ANY KEY TO SEE NEXT BACKCOLOR **");
                    Console.ReadKey(true);
                }
                Console.WriteLine("** PRESS ANY KEY TO RETURN TO CONFIG MENU **");
                Console.ReadKey(true);
                return true;                    
            }
            else if (resp.Key == ConsoleKey.E)
            {
                if (ConsoleUtil.Lines.ByeBye())
                {
                    Environment.Exit(0);
                }
                return true;
            }
            else if (resp.Key == ConsoleKey.M)
            {
                return false;
            }            
            return true;
        }
    }
}