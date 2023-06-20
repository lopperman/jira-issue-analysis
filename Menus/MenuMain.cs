


namespace JiraCon
{


    public class MenuMain : IMenuConsole
    {
        public JTISConfig ActiveConfig {get;set;}        
        public MenuMain(JTISConfig cfg)
        {
            ActiveConfig = cfg;                        
        }

        private void BuildMenu()
        {
            var cfgName = string.Format("Connected: {0} ",ActiveConfig.configName);
            string padd = new string('-',cfgName.Length + 1 );
            ConsoleLines lines = new ConsoleLines();
            lines.AddConsoleLine(" ------------- " + padd, StdLine.slMenuName);
            lines.AddConsoleLine("|  Main Menu  |" + " " + cfgName, StdLine.slMenuName);
            lines.AddConsoleLine(" ------------- " + padd, StdLine.slMenuName);
            lines.AddConsoleLine("(A) Analyze Issue(s) Time In Status", StdLine.slMenuDetail);
            lines.AddConsoleLine("(M) Show Change History for 1 or more Cards", StdLine.slMenuDetail);
            lines.AddConsoleLine("(J) Show JSON for 1 or more Cards", StdLine.slMenuDetail);
            // lines.AddConsoleLine("(F) Create Extract Files", StdLine.slMenuDetail);
            // lines.AddConsoleLine("(W) Create Work Metrics Analysis from JQL Query", StdLine.slMenuDetail);
            // lines.AddConsoleLine("(E) Epic Analysis - Find and Analyze - Yep, this exists", StdLine.slMenuDetail);
            lines.AddConsoleLine("");
            lines.AddConsoleLine("(C) Config Menu", StdLine.slMenuDetail);
            lines.AddConsoleLine("(D) Dev/Misc Menu", StdLine.slMenuDetail);
            lines.AddConsoleLine("Enter selection or X to exit.", StdLine.slResponse );
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

            if (key == ConsoleKey.M)
            {
                ConsoleUtil.WriteStdLine("Enter 1 or more card keys separated by a space (e.g. POS-123 POS-456 BAM-789), or ENTER to cancel", StdLine.slResponse, false);
                var keys = Console.ReadLine().ToUpper();
                if (string.IsNullOrWhiteSpace(keys))
                {
                    return true;
                }
                string[] arr = keys.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (arr.Length >= 1)
                {
                    List<JIssue>? retIssues;
                    retIssues = MainClass.AnalyzeIssues(keys);                    
                    if (retIssues != null)
                    {
                        ConsoleUtil.WriteStdLine("Enter 'Y' to save output to csv file, otherwise press any key",StdLine.slResponse,false);
                        resp = Console.ReadKey(true);
                        if (resp.Key == ConsoleKey.Y)
                        {
                            MainClass.WriteChangeLogCSV(retIssues);                                
                            ConsoleUtil.PressAnyKeyToContinue();
                        }
                    }
                }

                return true;
            }
            else if (key == ConsoleKey.F)
            {                
                ConsoleUtil.WriteStdLine("",StdLine.slResponse);
                ConsoleUtil.WriteStdLine("Enter or paste JQL, or ENTER to return",StdLine.slResponse,false);
                var jql = Console.ReadLine();
                if (jql.Length > 0)
                {
                    ConsoleUtil.WriteStdLine(string.Format("Enter (Y) to use the following JQL?\r\n***** {0}", jql),StdLine.slResponse,false);
                    var keys = Console.ReadKey(true);
                    if (keys.Key == ConsoleKey.Y)
                    {
                        ConsoleUtil.WriteStdLine("Enter (Y)es to include card descriptions and comments in the Change History file, otherwise press any key",StdLine.slResponse,false);
                        var k = Console.ReadKey(true);
                        bool includeCommentsAndDesc = false;
                        if (k.Key == ConsoleKey.Y)
                        {
                            includeCommentsAndDesc = true;
                        }
                        MainClass.CreateExtractFiles(jql,includeCommentsAndDesc);
                        ConsoleUtil.PressAnyKeyToContinue();
                    }

                }
                return true;
            }
            else if (key == ConsoleKey.J)
            {
                ConsoleUtil.WriteStdLine("",StdLine.slResponse);
                ConsoleUtil.WriteStdLine("Enter or paste JQL then press enter to continue.",StdLine.slResponse);
                var jql = Console.ReadLine();
                if (jql.Length > 0)
                {
                    ConsoleUtil.WriteStdLine("",StdLine.slResponse);
                    ConsoleUtil.WriteStdLine(string.Format("Enter (Y) to use the following JQL?\r\n***** {0}", jql),StdLine.slResponse);
                    var keys = Console.ReadKey(true);
                    if (keys.Key == ConsoleKey.Y)
                    {
                        MainClass.ShowJSON(jql);
                        ConsoleUtil.PressAnyKeyToContinue();
                    }
                }
                return true;

            }
            else if (key == ConsoleKey.C)
            {
                while (MenuManager.DoMenu(new MenuConfig(ActiveConfig)))
                {

                }
                return true;
            }
            else if (key == ConsoleKey.D)
            {
                while (MenuManager.DoMenu(new MenuDev(ActiveConfig)))
                {

                }
                return true;
            }
            else if (key == ConsoleKey.A)
            {
                while (MenuManager.DoMenu(new MenuIssueStates(ActiveConfig)))
                {

                }
                return true;
            }

            else if (key == ConsoleKey.X)
            {
                return false;
            }
            return true;
        }
    }
}