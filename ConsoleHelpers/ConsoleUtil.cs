using System.Data.SqlTypes;
using System.Runtime.CompilerServices;
using System.Reflection.Metadata;
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
        slOutput, 
        slCode, 
        slInfo
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
                    return ConsoleColor.Yellow;
                case StdLine.slError:
                    return ConsoleColor.Red;
                case StdLine.slOutput:
                    return ConsoleColor.DarkBlue;
                case StdLine.slOutputTitle:
                    return ConsoleColor.White;
                case StdLine.slCode:
                    return ConsoleColor.Gray;
                case StdLine.slInfo:
                    return ConsoleColor.Gray;
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
                    return ConsoleColor.DarkBlue;
                case StdLine.slError:
                    return ConsoleColor.Yellow;
                case StdLine.slOutput:
                    return ConsoleColor.White;
                case StdLine.slOutputTitle:
                    return ConsoleColor.DarkBlue;
                case StdLine.slCode:
                    return ConsoleColor.Black ;
                case StdLine.slInfo:
                    return ConsoleColor.Black ;
                default:
                    return ConsoleColor.White;
            }
        } 
        static ConsoleLines consoleLines = new ConsoleLines();
        public static ConsoleColor defBackground = Console.BackgroundColor;
        public static ConsoleColor defForeground = Console.ForegroundColor;

        public static ConsoleLines Lines
        {
            get
            {
                return consoleLines;
            }
        }

        // public static T? ReadConsoleLine<T>(bool allowNone = false) where T:IConvertible
        // {
        //     var input = Console.ReadLine();
        //     T? ret;
        //     try
        //     {
        //         if (input == null || input.Length==0)
        //         {
        //             if (allowNone==false)
        //             {
        //                 return ReadConsoleLine<T>(allowNone);
        //             }
        //             else 
        //             {
        //                 return default(T);
        //             }
        //         }                
        //         ret = (T)Convert.ChangeType(input,typeof(T));
        //     }
        //     catch 
        //     {
        //         WriteStdLine(string.Format("'{0}' is not a valid response - try again"),StdLine.slError);
        //         return ReadConsoleLine<T>(allowNone);
        //     }


        //     ret = (T)Convert.ChangeType(input,typeof(T));
        //     return ret;

        // }

        public static void PressAnyKeyToContinue()
        {
            WriteStdLine("       --- --- --- ---       ",StdLine.slResponse );
            WriteStdLine("  PRESS ANY KEY TO CONTINUE  ",StdLine.slResponse);
            var key = Console.ReadKey(true);
        }

        public static void WriteStdLine(string text, ConsoleColor fontColor, ConsoleColor backColor,bool clearScreen = false)
        {
            Console.ResetColor();
            if (clearScreen)
            {
                Console.Clear();
            }            
            Console.ForegroundColor = fontColor;
            Console.BackgroundColor = backColor;
            Console.Write(text);
            Console.ResetColor();
            Console.WriteLine();

        }
        public static void WriteStdLine(string text, StdLine msgType, bool clearScreen = false)
        {
            if (msgType == StdLine.slCode)
            {
                text = string.Format("  ||  {0}",text);
            }
            WriteStdLine(text,StdForecolor(msgType),StdBackcolor(msgType),clearScreen);
        }

        public static void WriteError(string text, bool clearScreen = false, Exception? ex =  null)
        {
            WriteStdLine(text,StdLine.slError);
            if (ex != null)
            {
                WriteError(ex.Message);
                WriteError(ex.StackTrace);
            }
        }
        public static void WriteAppend(string text, StdLine lnType, bool endOfLine = false)
        {
            WriteAppend(text,StdForecolor(lnType),StdBackcolor(lnType),endOfLine);
        }


        public static void WriteAppend(string text, ConsoleColor foregroundColor, ConsoleColor backgroundColor, bool endOfLine = false)
        {

            Console.ResetColor();
            
            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;

            if (endOfLine)
            {
                Console.Write(text);
                ConsoleUtil.WriteStdLine("",foregroundColor,backgroundColor);
            }
            else 
            {
                Console.Write(text);
            }
        }
        private static T? ConvertString<T>(string item, out bool isError) where T:IConvertible
        {
            T? ret = default(T);
            isError = false;
            try 
            {
                var testItem = (T)Convert.ChangeType(item,typeof(T));
                ret = testItem;
            }
            catch 
            {
                isError = true;
            }
            finally
            {
                if (isError)
                {
                    ConsoleUtil.WriteError(string.Format("Could not convert '{0}' to {1}",item,typeof(T).Name));
                }
            }
            return ret;
        }
        public static T GetConsoleInput<T>(string? message = null, bool requireConfirmation = false, bool allowNull = false, bool clearScreen = false) where T:IConvertible
        {
            if (clearScreen)
            {
                Console.Clear();
            }
            if (message!=null && message.Length > 0)
            {
                WriteStdLine(message,StdLine.slResponse);
            }
            Console.ResetColor();
            var rslt = Console.ReadLine();
            bool isError = false;
            var ret = ConvertString<T>(rslt, out isError);
            if (isError)
            {
                WriteStdLine(string.Format("'{0}' is not a valid choice, please try again", rslt),StdLine.slResponse);
                return GetConsoleInput<T>(requireConfirmation:requireConfirmation,allowNull:allowNull);
            }
            if (allowNull == false && (ret == null || ret.ToString().Length == 0))
            {
                return GetConsoleInput<T>(message,requireConfirmation,allowNull,clearScreen);
            }
            
            // if (ret.Cast<T> == null)
            // {
            //     WriteStdLine(string.Format("'{0}' is not a valid choice, please try again", ret),StdLine.slResponse);
            //     return GetConsoleInput<T>(requireConfirmation:requireConfirmation,allowNull:allowNull);
            // }
            if (requireConfirmation)
            {
                WriteStdLine(string.Format("ENTER 'Y' TO USE '{0}', OTHERWISE 'X' TO EXIT'", ret),StdLine.slResponse);
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.X)
                {
                    ConsoleUtil.ByeByeForced();
                }
                if (key.Key != ConsoleKey.Y)
                {
                    return GetConsoleInput<T>(message);
                }
            }
            return (T)Convert.ChangeType(ret,typeof(T));
            // try 
            // {
            //     T retVal = (T)Convert.ChangeType(ret,typeof(T));
            //     return retVal;
            // }
            // catch
            // {
            //     WriteError(ret + " is not valid, try again");
            //     return GetConsoleInput<T>(message);
            // }            
        }

        public static T GetConsoleInput<T>(string message) where T:IConvertible 
        {
            return GetConsoleInput<T>(message,true);
            // WriteLine(message,StdForecolor(StdLine.slResponse), StdBackcolor(StdLine.slResponse));
            // var ret = Console.ReadLine();            
            // if (string.IsNullOrWhiteSpace(ret))
            // {
            //     return GetConsoleInput<T>(message);
            // }
            // WriteLine(string.Format("ENTER 'Y' TO USE '{0}', OTHERWISE 'X' TO EXIT'", ret),StdForecolor(StdLine.slResponse),StdBackcolor(StdLine.slResponse),false);
            // var key = Console.ReadKey(true);
            // if (key.Key == ConsoleKey.X)
            // {
            //     ConsoleUtil.ByeByeForced();
            // }
            // if (key.Key != ConsoleKey.Y)
            // {
            //     return GetConsoleInput<T>(message);
            // }
            // try 
            // {
            //     T retVal = (T)Convert.ChangeType(ret,typeof(T));
            //     return retVal;
            // }
            // catch
            // {
            //     WriteError(ret + " is not valid, try again");
            //     return GetConsoleInput<T>(message);
            // }

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
