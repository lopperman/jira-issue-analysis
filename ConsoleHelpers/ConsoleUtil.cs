using System.Net.Mime;
using Microsoft.Extensions.Configuration;
using Spectre.Console;


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
            

        public static Style StdStyle(StdLine input)
        {
            switch(input)
            {
                case StdLine.slTitle:
                    return new Style(Color.White,Color.Grey19,Decoration.Bold);
                case StdLine.slMenuName:
                    return new Style(Color.DarkBlue,Color.PaleTurquoise1 );
                case StdLine.slMenuDetail:
                    return new Style(AnsiConsole.Foreground,AnsiConsole.Background);
                    // return new Style(Color.Blue3,Color.LightYellow3 );
                case StdLine.slResponse:
                    return new Style(Color.White,Color.DarkBlue,Decoration.Bold);
                case StdLine.slError:
                    return new Style(Color.Red1,Color.LightCyan1,Decoration.Bold);
                case StdLine.slOutput:
                    return new Style(Color.Blue3,Color.White);
                case StdLine.slOutputTitle:
                    return new Style(Color.Blue3,Color.Grey89,Decoration.Bold & Decoration.Underline);
                case StdLine.slCode:
                    return new Style(Color.Black,Color.Grey82);
                case StdLine.slInfo:
                    return new Style(Color.Black,Color.Yellow2);
                default:
                    return new Style(Color.Blue3,Color.White);
            }            
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

        public static void PressAnyKeyToContinue(string? msg = null)
        {
            AnsiConsole.WriteLine();
            if (msg!=null)
            {
                msg = Markup.Remove(msg);
                msg = $"[{StdLine.slInfo.FontMkp()} on {StdLine.slInfo.BackMkp()}]  {Emoji.Known.Information}  {msg}[/]";
                AnsiConsole.MarkupLine(msg);
            }
            WriteMarkupLine(" PRESS ANY KEY TO CONTINUE ",StdStyle(StdLine.slResponse));
            // Markup l2 = new Markup(" PRESS ANY KEY TO CONTINUE ",StdStyle(StdLine.slResponse));
            // AnsiConsole.MarkupLine()
            // AnsiConsole.MarkupLine(l2.ToString());

//            AnsiConsole.Write(new Rows(l2));

            Console.ReadKey(true);
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

        public static void WriteMarkup(string text,Style style, bool clearScreen = false)
        {
            try 
            {
                if (clearScreen){Console.Clear();}
                if (string.IsNullOrEmpty(text))
                {
                    Console.Write("");
                }
                else 
                {
                    Markup m = new Markup(text,style);
                    AnsiConsole.Write(new Rows(m));
                }
            }
            catch 
            {
                ConsoleUtil.WriteError($"Error Writing ConsoleUtil.WriteMarkup ({text})");
            }
        }

        public static void WriteAppTitle()
        {
            AnsiConsole.Clear();
            var title = $"JIRA Time In Status[dim] :llama: by Paul Brower[/]{Environment.NewLine}[dim italic][link]https://github.com/lopperman/jiraTimeInStatus[/][/]";
            var panel = new Panel(title);
            panel.Border = BoxBorder.Rounded;
            panel.BorderColor(Color.Grey15);
            panel.Expand = true;
            AnsiConsole.Write(panel);
//            panel.HeaderAlignment(Justify.Center );
        }
        public static void WriteMarkupLine(string text,Style style, bool clearScreen = false)
        {
            if (!string.IsNullOrEmpty(text))
            {
                if (clearScreen){AnsiConsole.Clear();}
                AnsiConsole.MarkupLine(text,StdStyle(StdLine.slError));
                // var m = new Markup(text,style);
                // AnsiConsole.MarkupLine()
                // WriteMarkup(text,style,clearScreen);
            }
//            Console.WriteLine();
        }
        public static void WriteStdLine(string text, StdLine msgType, bool clearScreen = false)
        {
            if (msgType == StdLine.slCode)
            {
                text = string.Format("  {0}",text);
            }
            if (clearScreen){Console.Clear();}
            AnsiConsole.MarkupLine(text,StdStyle(msgType));
            // WriteStdLine(text,StdForecolor(msgType),StdBackcolor(msgType),clearScreen);
        }

        public static void WriteError(string text, bool clearScreen = false, Exception? ex =  null)
        {
            if (clearScreen){Console.Clear();}
            AnsiConsole.MarkupLine(text,StdStyle(StdLine.slError));
            if (ex != null)
            {
                WriteError(ex.Message);
                WriteError(ex.StackTrace);
            }
        }
        public static void WriteAppend(string text, StdLine lnType, bool endOfLine = false)
        {
            if (endOfLine)
            {
                AnsiConsole.MarkupLine(text,StdStyle(lnType));
            }
            else 
            {
                AnsiConsole.Markup(text,StdStyle(lnType));
            }
        }


        // public static void WriteAppend(string text, ConsoleColor foregroundColor, ConsoleColor backgroundColor, bool endOfLine = false)
        // {

        //     Console.ResetColor();
            
        //     Console.ForegroundColor = foregroundColor;
        //     Console.BackgroundColor = backgroundColor;

        //     if (endOfLine)
        //     {
        //         Console.Write(text);
        //         ConsoleUtil.WriteStdLine("",foregroundColor,backgroundColor);
        //     }
        //     else 
        //     {
        //         Console.Write(text);
        //     }
        // }
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
        public static T GetConsoleInput<T>(string? message = null, bool requireConfirmation = false, bool allowNull = false, bool clearScreen = false, T defaultValue = default(T)) where T:IConvertible
        {
            if (clearScreen)
            {
                Console.Clear();
            }
            message = string.Format("[bold white on darkblue]{0}[/]",message);
            // var mk = new Markup(message, new Style(Color.White,Color.DarkBlue,Decoration.Bold));
            return AnsiConsole.Ask<T>(message);

            // var mk = new Markup(message, new Style(Color.White,Color.DarkBlue,Decoration.Bold));
            // return AnsiConsole.Ask<T>(mk.ToString());


            // if (message!=null && message.Length > 0)
            // {
            //     WriteStdLine(message,StdLine.slResponse);
            // }
            // Console.ResetColor();
            // var rslt = Console.ReadLine();
            // bool isError = false;
            // var ret = ConvertString<T>(rslt, out isError);
            // if (isError)
            // {
            //     WriteStdLine(string.Format("'{0}' is not a valid choice, please try again", rslt),StdLine.slResponse);
            //     return GetConsoleInput<T>(requireConfirmation:requireConfirmation,allowNull:allowNull);
            // }
            // if (allowNull == false && (ret == null || ret.ToString().Length == 0))
            // {
            //     return GetConsoleInput<T>(message,requireConfirmation,allowNull,clearScreen);
            // }
            
            // // if (ret.Cast<T> == null)
            // // {
            // //     WriteStdLine(string.Format("'{0}' is not a valid choice, please try again", ret),StdLine.slResponse);
            // //     return GetConsoleInput<T>(requireConfirmation:requireConfirmation,allowNull:allowNull);
            // // }
            // if (requireConfirmation)
            // {
            //     WriteStdLine(string.Format("ENTER 'Y' TO USE '{0}', OTHERWISE 'X' TO EXIT'", ret),StdLine.slResponse);
            //     var key = Console.ReadKey(true);
            //     if (key.Key == ConsoleKey.X)
            //     {
            //         ConsoleUtil.ByeByeForced();
            //     }
            //     if (key.Key != ConsoleKey.Y)
            //     {
            //         return GetConsoleInput<T>(message);
            //     }
            // }
            // return (T)Convert.ChangeType(ret,typeof(T));
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
            message = string.Format("[bold white on darkblue]{0}[/]",message);
            // var mk = new Markup(message, new Style(Color.White,Color.DarkBlue,Decoration.Bold));
            return AnsiConsole.Ask<T>(message);
            
            //return GetConsoleInput<T>(message,true);
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
            lines.AddConsoleLine("Press 'Y' to exit, otherwise press any key to continue",ConsoleUtil.StdStyle(StdLine.slResponse));
            lines.WriteQueuedLines(false);
            if (Console.ReadKey(true).Key !=ConsoleKey.Y)
            {
                return false;
            }
            else 
            {
                lines.AddConsoleLine("   HAVE A GREAT DAY!!   ", ConsoleUtil.StdStyle(StdLine.slResponse));
                lines.WriteQueuedLines(true);
                return true;
            }
        }

        public static void ByeByeForced()
        {
            ConsoleUtil.WriteAppTitle();
            AnsiConsole.MarkupLine("   :llama:  :llama:  :llama:  ");
            var title = $"{Environment.NewLine}[{StdLine.slResponse.FontMkp()} on {StdLine.slResponse.BackMkp()}] :smiling_face_with_sunglasses: HAVE A GREAT DAY! :smiling_face_with_sunglasses:[/]{Environment.NewLine}";
            var panel = new Panel(title).Border(BoxBorder.Rounded).BorderColor(AnsiConsole.Foreground);
            AnsiConsole.Write(panel);
            Environment.Exit(0);
        }    

        public static bool Confirm(string msg, bool defResp )
        {
            msg = Markup.Remove(msg);
            msg = $"[{StdLine.slResponse.FontMkp()} on {StdLine.slResponse.BackMkp()}]{Emoji.Known.WhiteQuestionMark} {msg}[/]";
            return AnsiConsole.Confirm(msg,defResp);
        }    


    }

}
