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
    public static class ConsoleUtil
    {
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

        public static void WriteLine(string text)
        {
            WriteLine(text, false);
        }

        public static void WriteError(string text)
        {
            WriteLine(text,StdForecolor(StdLine.slError),StdBackcolor(StdLine.slError),false);
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

        public static bool ByeBye()
        {
            var lines = new ConsoleLines();
            lines.AddConsoleLine("Press 'Y' to exit, otherwise press any key to continue",StdLine.slResponse);
            lines.WriteQueuedLines(false);
            if (Console.ReadKey(true).Key !=ConsoleKey.Y)
            {
                return false;
            }
            else 
            {
                lines.AddConsoleLine("   HAVE A GREAT DAY!!   ", ConsoleColor.DarkBlue, ConsoleColor.Yellow);
                lines.WriteQueuedLines(true);
                return true;
            }
        }

        public static void ByeByeForced()
        {
            var lines = new ConsoleLines();
            lines.AddConsoleLine("   HAVE A GREAT DAY!!   ", ConsoleColor.DarkBlue, ConsoleColor.Yellow);
            lines.WriteQueuedLines(false);
            Environment.Exit(0);
        }        


    }

}
