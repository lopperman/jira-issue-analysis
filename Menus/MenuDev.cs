using System.Runtime.InteropServices;



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
            lines.AddConsoleLine("(V) View Console Fore/Back Colors", StdLine.slMenuDetail);
            lines.AddConsoleLine("(S) View Configured Console Styles", StdLine.slMenuDetail);

            lines.AddConsoleLine("(D) DEVTEST1()", StdLine.slMenuDetail);
            lines.AddConsoleLine("(T) DEVTEST2()", StdLine.slMenuDetail);

            lines.AddConsoleLine("");
            lines.AddConsoleLine("(B) Back to Main Menu", StdLine.slMenuDetail);
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
            // ConsoleKeyInfo resp = default(ConsoleKeyInfo);

            if (key == ConsoleKey.V)
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
                            ConsoleUtil.WriteStdLine(clrTest,ccFore, ccBack);
                        }
                    }
                    Console.WriteLine("** PRESS ANY KEY TO SEE NEXT BACKCOLOR **");
                    Console.ReadKey(true);
                }
                Console.WriteLine("** PRESS ANY KEY TO RETURN TO CONFIG MENU **");
                Console.ReadKey(true);
                return true;                    
            }
            else if (key == ConsoleKey.S)
            {
                ConsoleUtil.WriteStdLine("CONFIGURED CONSOLE LINE STYLES",StdLine.slOutputTitle,false);
                ConsoleUtil.WriteStdLine("CONFIGURED CONSOLE LINE STYLE FOR: TITLE",StdLine.slTitle,false);
                ConsoleUtil.WriteStdLine("CONFIGURED CONSOLE LINE STYLE FOR: MENU NAME",StdLine.slMenuName,false);
                ConsoleUtil.WriteStdLine("CONFIGURED CONSOLE LINE STYLE FOR: MENU DETAIL",StdLine.slMenuDetail,false);
                ConsoleUtil.WriteStdLine("CONFIGURED CONSOLE LINE STYLE FOR: RESPONSE NEEDED",StdLine.slResponse,false);
                ConsoleUtil.WriteStdLine("CONFIGURED CONSOLE LINE STYLE FOR: ERROR",StdLine.slError,false);
                ConsoleUtil.WriteStdLine("CONFIGURED CONSOLE LINE STYLE FOR: OUTPUT TITLE",StdLine.slOutputTitle,false);
                ConsoleUtil.WriteStdLine("CONFIGURED CONSOLE LINE STYLE FOR: OUTPUT",StdLine.slOutput,false);
                ConsoleUtil.WriteStdLine("CONFIGURED CONSOLE LINE STYLE FOR: CODE",StdLine.slCode,false);
                Console.WriteLine();
                ConsoleUtil.WriteStdLine("PRESS ANY KEY",StdLine.slResponse,false);
                Console.ReadKey(true);
                return true;
            }
            else if (key == ConsoleKey.X)
            {
                if (ConsoleUtil.ByeBye())
                {
                    Environment.Exit(0);
                }
                return true;
            }
            else if (key == ConsoleKey.B)
            {
                return false;
            }
            else if (key == ConsoleKey.D)
            {
                DevTest1();
                Console.WriteLine("** PRESS ANY KEY **");
                Console.ReadKey(true);
                return true;
            }            
            else if (key == ConsoleKey.T)
            {
                DevTest2();
                ConsoleUtil.PressAnyKeyToContinue();
                return true;
            }            
            return true;
        }

        private void DevTest2()
        {
            ConsoleUtil.WriteStdLine("READ LINE",StdLine.slInfo);
            var input = Console.ReadLine();
            ConsoleUtil.WriteStdLine("WRITE AFTER READ LINE",StdLine.slInfo);
            ConsoleUtil.WriteStdLine(string.Format("You entered: '{0}'",input),StdLine.slInfo);
        }

        private void DevTest1()
        {
            var tmpIssue = JiraUtil.JiraRepo.GetIssue("WWT-292"); 
            if (tmpIssue.Labels != null) 
            {
                Console.WriteLine(string.Format("Labels Count {0}",tmpIssue.Labels.Count ));
            }
            var labels = JiraUtil.JiraRepo.GetIssueLabelsAsync("WWT-292").GetAwaiter().GetResult();
                Console.WriteLine(string.Format("Labels Count {0}",tmpIssue.Labels.Count ));

            //return GetSubTasksAsync(issue).GetAwaiter().GetResult().ToList();

        }

    }
}