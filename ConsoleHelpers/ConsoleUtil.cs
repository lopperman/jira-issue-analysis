using System;
using System.Text;

namespace JiraCon
{

    public static class ConsoleUtil
    {
        static ConsoleLines consoleLines = new ConsoleLines();
        static ConsoleColor defBackground = Console.BackgroundColor;
        static ConsoleColor defForeground = Console.ForegroundColor;

        public static void test()
        {
            
        }

        public static ConsoleLines Lines
        {
            get
            {
                return consoleLines;
            }
        }

        public static void PressAnyKeyToContinue()
        {
            WriteLine("...");
            WriteLine("PRESS ANY KEY TO CONTINUE");
            var key = Console.ReadKey(true);
        }

        public static void ResetConsoleColors()
        {
            Console.BackgroundColor = defBackground;
            Console.ForegroundColor = defForeground;
        }

        public static void InitializeConsole(ConsoleColor defForeground, ConsoleColor defBackground)
        {
            ResetConsoleColors();
            Console.Clear();

        }

        public static void BuildInitializedMenu()
        {
            consoleLines.AddConsoleLine(" ------------- ", ConsoleColor.Black, ConsoleColor.White);
            consoleLines.AddConsoleLine("|  Main Menu  |");
            consoleLines.AddConsoleLine(" ------------- ");            
            consoleLines.AddConsoleLine("(M) Show Change History for 1 or (M)ore Cards");
            consoleLines.AddConsoleLine("(J) Show (J)SON for 1 or more Cards");
            consoleLines.AddConsoleLine("(X) Create E(X)tract files");
            consoleLines.AddConsoleLine("(W) Create (W)ork Metrics Analysis from JQL Query");
            consoleLines.AddConsoleLine("(A) Epic (A)nalysis - Find and Analyze - Yep, this exists");
            consoleLines.AddConsoleLine("");
            consoleLines.AddConsoleLine("(I) View (I)tem Status values for work metrics");
            consoleLines.AddConsoleLine("(C) Config Menu");
            consoleLines.AddConsoleLine("(T) DEV TESTING");
            consoleLines.AddConsoleLine("Enter selection or E to exit.");
        }

        public static void BuildConfigMenu()
        {
            consoleLines.AddConsoleLine(" --------------- ", ConsoleColor.Black, ConsoleColor.White);
            consoleLines.AddConsoleLine("|  Config Menu  |");
            consoleLines.AddConsoleLine(" --------------- ");
            consoleLines.AddConsoleLine("(R) Rebuild Login Configuation");
            consoleLines.AddConsoleLine("(V) View JiraConsole (this app) config");
            consoleLines.AddConsoleLine(string.Format("(J) View Jira Info for {0}",JiraUtil.JiraRepo.ServerInfo.BaseUrl));
            consoleLines.AddConsoleLine("");
            consoleLines.AddConsoleLine("(M) Main Menu");
            consoleLines.AddConsoleLine("Enter selection or (E) to exit.");
        }


        public static void BuildNotInitializedQueue()
        {
            consoleLines.AddConsoleLine("This application can be initialized with");
            consoleLines.AddConsoleLine("1a. No Arguments, if previous config file has been created in default location");
            consoleLines.AddConsoleLine("1b. No Arguments, if you wish to create new config file in default location");

            consoleLines.AddConsoleLine("2. path to valid config file");
            consoleLines.AddConsoleLine("3. The 4 required valid arguments (see below example)");
            consoleLines.AddConsoleLine("   (arguments must contain argument label, and use a SPACE between args)");
            consoleLines.AddConsoleLine("For Example:  'userName=john.doe@github.com apiToken=VALID_API_TOKEN jiraUrl=https://client.atlassian.net project=JIRA_PROJECT_KEY");
        }



        public static void WriteLine(string text)
        {
            WriteLine(text, false);
        }

        public static void WriteLine(string text, bool clearScreen)
        {
            WriteLine(text, Console.ForegroundColor, Console.BackgroundColor, clearScreen);
        }

        public static void WriteLine(string text, ConsoleColor foregroundColor, ConsoleColor backgroundColor, bool clearScreen)
        {
            if (clearScreen)
            {
                Console.Clear();
            }
            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;
            Console.WriteLine(text);
            ResetConsoleColors();

        }

        public static void WriteAppend(string text)
        {
            WriteAppend(text, false);    
        }

        public static void WriteAppend(string text, bool endOfLine)
        {
            WriteAppend(text, defForeground, defBackground,endOfLine);
        }

        public static void WriteAppend(string text, ConsoleColor foregroundColor, ConsoleColor backgroundColor)
        {
            WriteAppend(text, foregroundColor, backgroundColor, false);
        }

        public static void WriteAppend(string text, ConsoleColor foregroundColor, ConsoleColor backgroundColor, bool endOfLine)
        {
            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;
            Console.Write(text);
            if (endOfLine)
            {
                Console.WriteLine();
            }
            ResetConsoleColors();
        }

    }

}
