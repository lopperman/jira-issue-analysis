using System.Collections;
using System.Runtime.ExceptionServices;
using System;
using System.Text;


// GOOD COLOR COMBINATIONS
// ** BLACK BACKGROUND ** DarkGreen, DarkCyan (bluish), DarkMagenta, Green, Cyan (bluish), Red, Magenta, Yellow, White
// ** DARKBLUE BACKGROUND ** Gray, Green, Cyan, Red, Magenta, Yellow, White
// ** DARKGREEN BACKGROUND ** Black, DarkBlue, Gray, Blue, Yellow, White
// ** DARKCYAN BACKGROUND ** Black, DarkBlue, GRay, Blue, Yellow, White
// ** DARKRED BACKGROUND ** Black, Gray, Cyan, Red, Magenta, Yellow, White
// ** DARKMAGENTA BACKGROUND ** Black, DarkBlue, Gray, Yellow, White
// ** DARKYELLOW BACKGROUND ** Black, DarkBlue, Blue, Yellow, White
// ** GRAY BACKGROUND ** Black, DarkBlue, DarkGreen, DarkCyan, DarkRed, DarkMagenta, DarkGray, Blue, Red
// ** DARKGRAY BACKGROUND ** Gray, Green, Cyan, Red, Magenta, Yellow, White
// ** BLUE BACKGROUND ** Gray, Cyan, Red, Magenta, Yellow, White
// ** GREEN BACKGROUND ** Black, DarkBlue, DarkRed, DarkMagenta, Blue, Red, Yellow, White
// ** CYAN BACKGROUND ** Black, DarkBlue, DarkRed, DarkMagenta, DarkGray, Blue, Red, Magenta
// ** RED BACKGROUND ** Black, DarkBlue, Blue, Yellow, White
// ** MAGENTA BACKGROUND ** Black, DarkBlue, Blue, Yellow, White
// ** YELLOW BACKGROUND ** Black, DarkBlue, DarkGreen, DarkCyan, DarkRed, DarkMagenta, DarkGray, Blue, Red, Magenta
// ** WHITE BACKGROUND ** Black, DarkBlue, DarkGreen, DarkCyan, DarkRed, DarkMagenta, DarkYellow, DarkGray, Blue, Red, Magenta



namespace JiraCon
{
    public static class ConsoleUtil
    {
        public enum StdLine
        {
            slTitle = 1, 
            slMenuName, 
            slMenuDetail, 
            slResponse, 
            slError,
            slOutputTitle, 
            slOutput
        }
        public static ConsoleColor StdForecolor(StdLine lineType )
        {
            switch(lineType)
            {
                case StdLine.slTitle:
                    return ConsoleColor.White;
                case StdLine.slMenuName:
                    return ConsoleColor.Blue;
                case StdLine.slMenuDetail:
                    return ConsoleColor.Black ;
                case StdLine.slResponse:
                    return ConsoleColor.DarkMagenta;
                case StdLine.slError:
                    return ConsoleColor.Red;
                case StdLine.slOutput:
                    return ConsoleColor.DarkBlue;
                case StdLine.slOutputTitle:
                    return ConsoleColor.White;
                default:
                    return ConsoleColor.Black;
            }
        } 
        public static ConsoleColor StdBackcolor(StdLine lineType )
        {
            switch(lineType)
            {
                case StdLine.slTitle:
                    return ConsoleColor.DarkGray;
                case StdLine.slMenuName:
                    return ConsoleColor.Cyan;
                case StdLine.slMenuDetail:
                    return ConsoleColor.White ;
                case StdLine.slResponse:
                    return ConsoleColor.White;
                case StdLine.slError:
                    return ConsoleColor.Yellow;
                case StdLine.slOutput:
                    return ConsoleColor.White;
                case StdLine.slOutputTitle:
                    return ConsoleColor.DarkBlue;
                default:
                    return ConsoleColor.White;
            }
        } 
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
            // Console.BackgroundColor = defBackground;
            // Console.ForegroundColor = defForeground;
        }

        public static void InitializeConsole(ConsoleColor defForeground, ConsoleColor defBackground)
        {
            Console.Clear();
            ResetConsoleColors();
            Console.Clear();

        }

        public static void BuildInitializedMenu()
        {

            var cfgName = string.Format("Connected: {0} ",JTISConfigHelper.config.configName);
            string padd = new string('-',cfgName.Length + 1 );

            consoleLines.AddConsoleLine(" ------------- " + padd, StdLine.slMenuName);
            consoleLines.AddConsoleLine("|  Main Menu  |" + " " + cfgName, StdLine.slMenuName);
            consoleLines.AddConsoleLine(" ------------- " + padd, StdLine.slMenuName);
            consoleLines.AddConsoleLine("(M) Show Change History for 1 or (M)ore Cards", StdLine.slMenuDetail);
            consoleLines.AddConsoleLine("(J) Show (J)SON for 1 or more Cards", StdLine.slMenuDetail);
            consoleLines.AddConsoleLine("(X) Create E(X)tract files", StdLine.slMenuDetail);
            consoleLines.AddConsoleLine("(W) Create (W)ork Metrics Analysis from JQL Query", StdLine.slMenuDetail);
            consoleLines.AddConsoleLine("(A) Epic (A)nalysis - Find and Analyze - Yep, this exists", StdLine.slMenuDetail);
            consoleLines.AddConsoleLine("");
            consoleLines.AddConsoleLine("(I) View (I)tem Status values for work metrics", StdLine.slMenuDetail);
            consoleLines.AddConsoleLine("(C) Config Menu", StdLine.slMenuDetail);
            consoleLines.AddConsoleLine("(D) Dev/Misc Menu", StdLine.slMenuDetail);
            consoleLines.AddConsoleLine("Enter selection or E to exit.", StdLine.slResponse );
        }

        public static void BuildJQLMenu()
        {
            var cfgName = string.Format("Connected: {0} ",JTISConfigHelper.config.configName);
            string padd = new string('-',cfgName.Length + 1 );
            consoleLines.AddConsoleLine(" ------------ " + padd, StdLine.slMenuName);
            consoleLines.AddConsoleLine("|  JQL Menu  |" + " " + cfgName, StdLine.slMenuName);
            consoleLines.AddConsoleLine(" ------------ " + padd, StdLine.slMenuName);

            consoleLines.AddConsoleLine("(V) View All Saved JQL", StdLine.slMenuDetail);
            consoleLines.AddConsoleLine("(A) Add JQL", StdLine.slMenuDetail);
            consoleLines.AddConsoleLine("(F) Find Saved JQL", StdLine.slMenuDetail);

            consoleLines.AddConsoleLine("");
            consoleLines.AddConsoleLine("(C) Back to Config Menu", StdLine.slMenuDetail);
            consoleLines.AddConsoleLine("Enter selection or (E) to exit.", StdLine.slResponse);            
        }

        public static void BuildDevMenu()
        {
            var cfgName = string.Format("Connected: {0} ",JTISConfigHelper.config.configName);
            string padd = new string('-',cfgName.Length + 1 );
            consoleLines.AddConsoleLine(" ------------ " + padd, StdLine.slMenuName);
            consoleLines.AddConsoleLine("|  DEV Menu  |" + " " + cfgName, StdLine.slMenuName);
            consoleLines.AddConsoleLine(" ------------ " + padd, StdLine.slMenuName);

            consoleLines.AddConsoleLine("(C) View Console Fore/Back Colors", StdLine.slMenuDetail);
            consoleLines.AddConsoleLine("");
            consoleLines.AddConsoleLine("(M) Main Menu", StdLine.slMenuDetail);
            consoleLines.AddConsoleLine("Enter selection or (E) to exit.", StdLine.slResponse);            
        }
        public static void BuildConfigMenu()
        {

            var cfgName = string.Format("Connected: {0} ",JTISConfigHelper.config.configName);
            string padd = new string('-',cfgName.Length + 1 );
            consoleLines.AddConsoleLine(" --------------- " + padd, StdLine.slMenuName);
            consoleLines.AddConsoleLine("|  Config Menu  |" + " " + cfgName, StdLine.slMenuName);
            consoleLines.AddConsoleLine(" --------------- " + padd, StdLine.slMenuName);
            consoleLines.AddConsoleLine(string.Format("INFO - Config File: {0}",JTISConfigHelper.ConfigFilePath), StdLine.slMenuName);
            consoleLines.AddConsoleLine(string.Format("INFO - Output Files: {0}",JTISConfigHelper.JTISRootPath), StdLine.slMenuName);

            consoleLines.AddConsoleLine("(J) Manage Saved JQL", StdLine.slMenuDetail);
            consoleLines.AddConsoleLine("(N) Add New Jira Config", StdLine.slMenuDetail);
            consoleLines.AddConsoleLine("(C) Change Current Jira Config", StdLine.slMenuDetail);
            consoleLines.AddConsoleLine("(V) View JiraConsole (this app) config", StdLine.slMenuDetail);
            consoleLines.AddConsoleLine("(R) Remove Login Configuation", StdLine.slMenuDetail);
            consoleLines.AddConsoleLine(string.Format("(I) View Jira Info for {0}",JiraUtil.JiraRepo.ServerInfo.BaseUrl), StdLine.slMenuDetail);
            consoleLines.AddConsoleLine("");
            consoleLines.AddConsoleLine("(M) Main Menu", StdLine.slMenuDetail);
            consoleLines.AddConsoleLine("Enter selection or (E) to exit.", StdLine.slResponse);
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

        public static void WriteLine(string text, ConsoleColor foregroundColor, ConsoleColor backgroundColor)
        {
            WriteLine(text,foregroundColor,backgroundColor,false);
        }

        public static void Writeline(string text, StdLine lineType)
        {
            WriteLine(text, StdForecolor(lineType), StdBackcolor(lineType),false);
        }
        public static void Writeline(string text, StdLine lineType, bool clearScreen)
        {
            WriteLine(text, StdForecolor(lineType), StdBackcolor(lineType),clearScreen);            
        }

        public static void WriteLine(string text, ConsoleColor foregroundColor, ConsoleColor backgroundColor, bool clearScreen)
        {
            Console.ResetColor();
            if (clearScreen)
            {
                Console.Clear();
            }            
            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;
            Console.Write(text);
            Console.ResetColor();
            Console.WriteLine();
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

            Console.ResetColor();
            
            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;
            Console.Write(text);
            if (endOfLine)
            {
                Console.ResetColor();
                Console.WriteLine();
            }
        }

        public static T GetConsoleInput<T>(string message) where T:IConvertible 
        {
            WriteLine("...");
            WriteLine(message);
            var ret = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(ret))
            {
                return GetConsoleInput<T>(message);
            }
            WriteLine("");
            WriteLine(string.Format("Enter 'Y' to Use '{0}', otherwise enter 'E' to exit or another key to enter new value", ret));
            var key = Console.ReadKey(true);

            if (key.Key == ConsoleKey.E)
            {
                Environment.Exit(0);
            }
            if (key.Key != ConsoleKey.Y)
            {
                return GetConsoleInput<T>(message);
            }
            try 
            {
                T retVal = (T)Convert.ChangeType(ret,typeof(T));
                return retVal;
            }
            catch
            {
                WriteLine(ret + " is not valid, try again",ConsoleColor.Red,ConsoleColor.DarkYellow ,false);
                return GetConsoleInput<T>(message);
            }

        }


    }

}
