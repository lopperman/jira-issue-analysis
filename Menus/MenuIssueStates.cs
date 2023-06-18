using System.Linq;
 


namespace JiraCon
{
    public class MenuIssueStates : IMenuConsole
    {
        public JTISConfig ActiveConfig {get;set;}
        public MenuIssueStates(JTISConfig cfg)
        {
            ActiveConfig = cfg;                        
        }

        public void BuildMenu()
        {

            var cfgName = string.Format("Connected: {0} ",JTISConfigHelper.config.configName);
            string padd = new string('-',cfgName.Length + 1 );
            ConsoleLines lines = new ConsoleLines();

            lines.AddConsoleLine(" ----------------------- " + padd, StdLine.slMenuName);
            lines.AddConsoleLine("|  Time in Status Menu  |" + " " + cfgName, StdLine.slMenuName);
            lines.AddConsoleLine(" ----------------------- " + padd, StdLine.slMenuName);

//            lines.AddConsoleLine("-- [SHOW STATUS SELECTION & CLASSIFICATION] -- ", StdLine.slOutputTitle );
            lines.AddConsoleLine("(A) Edit Analysis Configuration", StdLine.slMenuDetail);
            lines.AddConsoleLine(string.Format("-- You have {0:00} Saved JQL Searches -- ",ActiveConfig.SavedJQLCount  ), StdLine.slOutputTitle );
            lines.AddConsoleLine("(J) Manage Saved JQL", StdLine.slMenuDetail);
            lines.AddConsoleLine("(S) Manage Issue Status Classification", StdLine.slMenuDetail);
            lines.AddConsoleLine("(I) Analyze: Enter Issue(s)", StdLine.slMenuDetail);
            lines.AddConsoleLine("(P) Analyze: Issues in an Epic", StdLine.slMenuDetail);
            lines.AddConsoleLine("(Q) Analyze: Issues from JQL", StdLine.slMenuDetail);

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
//            ConsoleKeyInfo resp = default(ConsoleKeyInfo);


            if (key == ConsoleKey.B)
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
            else if (key == ConsoleKey.S)
            {                
                while (MenuManager.DoMenu(new MenuStatusConfig(ActiveConfig)))
                {

                }
                return true;                
            }
            else if (key == ConsoleKey.J)
            {
                while (MenuManager.DoMenu(new MenuJQL(ActiveConfig)))
                {

                }
                return true;                
            }
            else if (key == ConsoleKey.I)
            {
                NewAnalysis(AnalysisType.atIssues);
                return true;
            }
            else if (key == ConsoleKey.P)
            {
                NewAnalysis(AnalysisType.atEpics);
                return true;

            }
            else if (key == ConsoleKey.Q)
            {
                NewAnalysis(AnalysisType.atJQL);
                return true;
            }
 

            return true;
        }

        private void NewAnalysis(AnalysisType anType)
        {
            AnalyzeIssues analyze = new AnalyzeIssues(anType);
            int issueCount = 0;
            if (analyze.HasSearchData)
            {
                issueCount = analyze.GetData();
            } 
            if (issueCount > 0)
            {
                analyze.ClassifyStates();
            }
        }
    }
}