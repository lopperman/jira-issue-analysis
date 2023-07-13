using System.Data.SqlTypes;
using System;
using JTIS.Config;
using Spectre.Console;


namespace JTIS.Console
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
        public static bool? ScrubData = null;                    
        public static string Scrub(string data)
        {
            if (ScrubData == null) 
            {
                if (!Info.IsDev)
                {
                    ScrubData = false;
                }
                else 
                {
                    ScrubData = Confirm("DEV: SCRUB DATA?",false);
                }
            }
            if (ScrubData == true)
            {
                foreach (var scrubItem in CfgManager.config.ScrubList())
                {
                    data = data.Replace(scrubItem,new string('*',scrubItem.Length),StringComparison.OrdinalIgnoreCase);
                }
            }
            return data;
        }
        public static bool IsConsoleRecording {get;set;}
        
        public static Style StdStyle(StdLine input)
        {
            AnsiConsole.Reset();
            

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
                    // return new Style(Color.White,Color.DarkBlue,Decoration.Bold);
                    return new Style(Color.White,Color.Blue3_1,Decoration.Bold);
                case StdLine.slError:
                    return new Style(Color.Red1,Color.LightCyan1,Decoration.Bold);
                case StdLine.slOutput:
                    return new Style(Color.Blue3,Color.White);
                case StdLine.slOutputTitle:
                    return new Style(Color.Blue3,Color.Grey89,Decoration.Bold & Decoration.Underline);
                case StdLine.slCode:
                    return new Style(Color.Black,Color.Grey82);
                case StdLine.slInfo:
                    return new Style(Color.Blue,Color.Cornsilk1);
                default:
                    return new Style(Color.Blue3,Color.White);
            }            
        }

        static ConsoleLines consoleLines = new ConsoleLines();
        public static ConsoleColor defBackground = System.Console.BackgroundColor;
        public static ConsoleColor defForeground = System.Console.ForegroundColor;

        public static ConsoleLines Lines
        {
            get
            {
                return consoleLines;
            }
        }

        public static void PressAnyKeyToContinue(string? msg = null)
        {
            var startLine = System.Console.CursorTop + 1;
            // ClearLinesBackTo(startLine);

            AnsiConsole.WriteLine();
            var mk = new List<Markup>();
            var finalMsg = new Markup($"{Emoji.Known.BlackSquareButton}  [{StdLine.slResponse.FontMkp()} on {StdLine.slResponse.BackMkp()}]  PRESS ANY KEY TO CONTINUE  [/]");            
            Rows? rws = null;
            if (msg!=null)
            {
                msg = Markup.Remove(msg);
                msg = $"{Emoji.Known.BlackSquareButton}  [{StdLine.slInfo.FontMkp()} on {StdLine.slInfo.BackMkp()}]{msg}[/]{Environment.NewLine}";
                rws = new Rows(new Markup(msg),finalMsg);
                // mk.Add(new Markup(msg));
                //AnsiConsole.MarkupLine(msg);
            }
            else 
            {
                rws = new Rows(finalMsg);
            }
            
            var p = new Panel(rws);
            p.BorderColor(Style.Parse("dim blue").Foreground);
            p.Border(BoxBorder.Heavy);            
            p.Expand();
            AnsiConsole.Write(p);
            System.Console.ReadKey(true);
            ClearLinesBackTo(startLine);            
        }

        public static void WriteStdLine(string text, ConsoleColor fontColor, ConsoleColor backColor,bool clearScreen = false)
        {
            ////System.Console.ResetColor();
            if (clearScreen)
            {
                System.Console.Clear();
            }            
            System.Console.ForegroundColor = fontColor;
            System.Console.BackgroundColor = backColor;
            System.Console.Write(text);
            ////System.Console.ResetColor();
            System.Console.WriteLine();

        }

        public static void WriteMarkup(string text,Style style, bool clearScreen = false)
        {
            try 
            {
                if (clearScreen){System.Console.Clear();}
                if (string.IsNullOrEmpty(text))
                {
                    System.Console.Write("");
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

        public static string RecordingInfo
        {
            get
            {
                if (IsConsoleRecording == false) 
                {
                    return string.Empty;
                }
                else 
                {
                    return " ** [bold red on lightyellow3]RECORDING ON[/] ** ";
                }
            }
        }
        public static string TimeZoneAlert
        {
            get
            {
                if (CfgManager.config != null)
                {                    
                    if (CfgManager.config.DefaultTimeZoneDisplay() == false)
                    {   
                        var tzi = JTISTimeZone.DisplayTimeZone.StandardName;
                        return $" [bold blue on lightyellow3]USING TIME ZONE: {tzi}[/] ";
                    }
                }
                return string.Empty;
            }
        }

        public static void WriteAppHello()
        {
            ConsoleUtil.WriteAppTitle();
            AnsiConsole.Background = Color.LightSteelBlue;
            AnsiConsole.Foreground = Color.Blue3_1;
            var tbl = new Table();
            tbl.Border(TableBorder.None);
            var llamas = $"[white on deepskyblue4_2]       :llama::llama:        [/]";
            var fill2  = $"[white on deepskyblue4_2]                   [/]";
            var tblcol = new TableColumn("").Alignment(Justify.Left);
            var title = $"[white on deepskyblue4_2]  WELCOME FRIEND!  [/]";
            tbl.AddColumn(tblcol).Centered();
            tbl.AddRow(fill2);
            tbl.AddRow(llamas);
            tbl.AddRow(fill2);
            tbl.AddRow(title);
            tbl.AddRow(fill2);
            tbl.AddEmptyRow();            
            tbl.Columns[0].Centered();
            var panel = new Panel(tbl).Border(BoxBorder.Rounded).BorderColor(AnsiConsole.Foreground);

            // AnsiConsole.Write(new Rule());
            // AnsiConsole.MarkupLine(title);
            // var tr = new Rule().DoubleBorder();

            AnsiConsole.Write(panel);
//            ConsoleUtil.PressAnyKeyToContinue();
            var pbar = new ProgressBarColumn();
            // pbar.CompletedStyle = new Style(Color.Green,Color.LightSlateGrey);
            // pbar.IndeterminateStyle = new Style(Color.Yellow,Color.LightSlateGrey);
            pbar.RemainingStyle = new Style(foreground:Color.DarkSlateGray1).Decoration(Decoration.RapidBlink);
            pbar.CompletedStyle = new Style(foreground:Color.SlateBlue3_1).Decoration(Decoration.Bold);
            pbar.Width = 25;

            AnsiConsole.Progress()
                .AutoClear(true)
                .HideCompleted(false)
                .Columns(
                    new ProgressColumn[]{
                        pbar,
                    })
                .Start(ctx=> {
                    var task = ctx.AddTask($"[green]Thank you for trying this app...[/]");
                    task.MaxValue(100);
                    task.StartTask();
                    while (task.Value < task.MaxValue)
                    {
                        task.Increment(1);
                        Thread.Sleep(25);
                    }
                });            
        }
        public static void WriteAppTitle()
        {
            // var panel = new Panel(title);
            // panel.Border = BoxBorder.
            // panel.BorderColor(Color.Grey15);
            // panel.Expand = true;
            // AnsiConsole.Write(panel);
//            panel.HeaderAlignment(Justify.Center );
            AnsiConsole.Clear();
            var title = $"  JIRA Time In Status :llama: [dim]by[/] [dim link=https://github.com/lopperman/jira-issue-analysis]Paul Brower[/]{ConsoleUtil.RecordingInfo}{ConsoleUtil.TimeZoneAlert}{Environment.NewLine}  [dim italic][link]https://github.com/lopperman/jira-issue-analysis[/][/]";     
            AnsiConsole.Write(new Rule());
            AnsiConsole.MarkupLine(title);
            var tr = new Rule().DoubleBorder();
            AnsiConsole.Write(tr);
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
            var ogText = Markup.Remove(text);
            if (msgType == StdLine.slCode)
            {
                text = string.Format("  {0}",text);
            }
            if (clearScreen){System.Console.Clear();}

            try 
            {
                AnsiConsole.MarkupLine(text,StdStyle(msgType));
            }
            catch (Exception ex)
            {
                WriteError("Write Std Line Error - retrying ",ex:ex);
                ////AnsiConsole.ResetColors();
                AnsiConsole.WriteLine(ogText);
            }

            // WriteStdLine(text,StdForecolor(msgType),StdBackcolor(msgType),clearScreen);
        }

        public static void WriteError(string text, bool clearScreen = false, Exception? ex =  null, bool pause = false)
        {
            text = Markup.Escape(text);
            if (clearScreen){System.Console.Clear();}

            AnsiConsole.MarkupLine(text,StdStyle(StdLine.slError));
            if (ex != null)
            {
                WriteError(ex.Message);
                WriteError(ex.StackTrace);
            }
            if (pause)
            {
                PressAnyKeyToContinue();
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
                System.Console.Clear();
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
            if (ConsoleUtil.Confirm("QUIT APPLICATION?",true))
            {
                ByeByeForced();
            }
            return false;
        }

        public static void WritePerfData(SortedList<string,TimeSpan> perf, int sleepMilliseconds=1000)
        {
            foreach (var prf in perf)
            {
                AnsiConsole.MarkupLine($"[bold]{prf.Value.TotalSeconds:##0.000} seconds - [/] {prf.Key}");
            }
            if (sleepMilliseconds <=0){
                PressAnyKeyToContinue();
            }
            else {
                Thread.Sleep(1000);
            }

        }

        public static void ByeByeForced()
        {
            if (CfgManager.config != null && CfgManager.config.IsDirty)
            {
                CfgManager.SaveConfigList();
            }

            if (IsConsoleRecording)
            {
                if (Confirm("Save recorded session to file?",true))
                {
                    var fName = SaveSessionFile();
                    ConsoleUtil.PressAnyKeyToContinue($"Saved to: {fName}");
                }
            }

            ConsoleUtil.WriteAppTitle();
            AnsiConsole.Background = Color.LightSteelBlue;
            AnsiConsole.Foreground = Color.Blue3_1;
            var tbl = new Table();
            tbl.Border(TableBorder.None);
            var llamas = $"[white on deepskyblue4_2]        :llama::llama:        [/]";
            var fill2  = $"[white on deepskyblue4_2]                    [/]";
            var tblcol = new TableColumn("").Alignment(Justify.Left);
            var title = $"[white on deepskyblue4_2]  HAVE A GREAT DAY  [/]";
            tbl.AddColumn(tblcol).Centered();
            tbl.AddRow(fill2);
            tbl.AddRow(llamas);
            tbl.AddRow(fill2);
            tbl.AddRow(title);
            tbl.AddRow(fill2);
            tbl.AddEmptyRow();            
            tbl.Columns[0].Centered();
            var panel = new Panel(tbl).Border(BoxBorder.Rounded).BorderColor(AnsiConsole.Foreground);
            AnsiConsole.Write(panel);
            Environment.Exit(0);
        }    

        public static T GetInput<T>(string msg,T defVal=default(T), bool allowEmpty = false) where T:IComparable<T>
        {
            T retVal = default(T);

            AnsiConsole.WriteLine();
            var r = new Rule();
            r.Style=Style.Parse("dim red");
            AnsiConsole.Write(r);
            msg = Markup.Remove(msg);
            if (msg.EndsWith(Environment.NewLine))
            {
                int place = msg.LastIndexOf(Environment.NewLine);
                if (place >-1)
                {
                    msg = msg.Remove(place,Environment.NewLine.Length);
                }
            }
            if (allowEmpty)
            {
                msg = $"[{StdLine.slResponse.FontMkp()} on {StdLine.slResponse.BackMkp()}] [dim](Optional)[/] {msg}[/]";
            }
            else 
            {
                msg = $"[{StdLine.slResponse.FontMkp()} on {StdLine.slResponse.BackMkp()}] {msg}[/]";
            }
            msg = $"{msg}[{AnsiConsole.Foreground} on {AnsiConsole.Background}]{Environment.NewLine} : [/]";
            var showDefVal = false;
            if (default(T) != null &&  default(T).CompareTo(defVal)!=0)
            {
                showDefVal = true;
            }
            
            var tp = new TextPrompt<T>(msg);
            if (allowEmpty)
            {
                tp.AllowEmpty();
            }
            if (showDefVal)
            {
                tp.DefaultValue<T>(defVal);
            }

            retVal = AnsiConsole.Prompt<T>(tp);

            if (allowEmpty == false && retVal == null )
            {
                PressAnyKeyToContinue("[[Empty] is not allowed, please try again");
                return GetInput<T>(msg,defVal,allowEmpty);
            }
            return retVal;
        }
        public static bool Confirm(string msg, bool defResp, bool keepMarkup = false )
        {
            var startLine = System.Console.CursorTop + 1;
            // ClearLinesBackTo(startLine);

            AnsiConsole.WriteLine();
            var r = new Rule();
            r.Style=Style.Parse("dim red");
            AnsiConsole.Write(r);
            if (keepMarkup == false)
            {
                msg = Markup.Remove(msg);
            }
            msg = $"[{StdLine.slResponse.FontMkp()} on {StdLine.slResponse.BackMkp()}]{Emoji.Known.WhiteQuestionMark} {msg}[/]";
            var finalMsg = new Markup($"{Emoji.Known.BlackSquareButton}  [{StdLine.slResponse.FontMkp()} on {StdLine.slResponse.BackMkp()}] {msg} [/]{Environment.NewLine}");      
            var result = AnsiConsole.Confirm(msg,defResp);
            ClearLinesBackTo(startLine);
            return result;
        }    

        public static void ClearLinesBackTo(int toLine)
        {
            var totalLines = System.Console.GetCursorPosition().Top - toLine + 1;
            if (totalLines > 0)
            {
                ClearLines(totalLines);
            }
        }

        public static void FillLines(Color fillColor, int startRowOffset = 0, int fillRowCount = 1, int resumeOffset = 0)
        {
            var currentPos = System.Console.GetCursorPosition();
            if (startRowOffset != 0)
            {
                System.Console.SetCursorPosition(0,currentPos.Top + startRowOffset);
            }
            var spaces = new string(c:' ', System.Console.WindowWidth);
            for (int i = 0; i < fillRowCount; i++)
            {
                System.Console.SetCursorPosition(0,(currentPos.Top + i));
                AnsiConsole.Markup($"[yellow on navy]{spaces}[/]");
            }
            System.Console.SetCursorPosition(0,(currentPos.Top + resumeOffset));
        }

        public static void ClearLines(int lineCount)
        {
            var currentPos = System.Console.GetCursorPosition();
            for (int i = 0; i < lineCount; i ++)
            {
                System.Console.SetCursorPosition(0,currentPos.Top - i);
                System.Console.Write(new string(' ',System.Console.WindowWidth));
            }
            System.Console.SetCursorPosition(0,currentPos.Top-lineCount);
        }


        public static string SaveSessionFile()
        {
            string sessFile = Path.Combine(CfgManager.JTISRootPath,"SessionFiles");
            if (!Directory.Exists(sessFile)){Directory.CreateDirectory(sessFile);}
            string fName = string.Format("SessionFile_{0}.html",DateTime.Now.ToString("yyyyMMMddHHmmss"));
            sessFile = Path.Combine(sessFile,fName);
            using (StreamWriter writer = new StreamWriter(sessFile,false))
            {
                writer.Write(AnsiConsole.ExportHtml());
            }
            IsConsoleRecording = false;
            return sessFile ;
        }

        internal static void StartRecording()
        {
            if (IsConsoleRecording)
            {
                ConsoleUtil.PressAnyKeyToContinue("Recording is already in progress");
            }
            else 
            {
                IsConsoleRecording = true;
                AnsiConsole.Record();
                ConsoleUtil.PressAnyKeyToContinue("Recording has started");
            }
        }

        public static void WaitWhileSimple(string msg, Action action)
        {
            var st = new Status(AnsiConsole.Console);            
            st.Spinner(Spinner.Known.BouncingBar);
            // st.Spinner(Spinner.Known.Dots);

            st.SpinnerStyle(new Style(Color.Blue3_1,Color.LightSkyBlue1));
            st.Start(msg, ctx => action.Invoke());
        }

        public static void StatusWait2(List<KeyValuePair<string,Action>> actions)
        {
                List<ProgressTask> tasks = new List<ProgressTask>();
                List<Action> actionsList = new List<Action>();
                List<string> msgList = new List<string>();
                foreach (var kvp in actions)
                {
                    msgList.Add(kvp.Key);
                    actionsList.Add(kvp.Value);
                }

                var pr = new Progress(AnsiConsole.Console);
                pr.AutoClear(true);
                pr.AutoRefresh(true);
                pr.HideCompleted(false);
                pr.Columns(new ProgressColumn[]
                {
                    new TaskDescriptionColumn(), 
                    new ProgressBarColumn(), 
                    new ElapsedTimeColumn(), 
                    new SpinnerColumn(Spinner.Known.BouncingBar).Style(new Style(Color.Blue3_1,Color.LightSkyBlue1)), 
                })
                .Start(ctx => 
                {
                    var actionArr = actionsList.ToArray();
                    var msgArr = msgList.ToArray();
                    for (int i = 0; i < actions.Count; i ++)
                    {
                        var tStg = new ProgressTaskSettings();
                        tStg.AutoStart = false;
                        tStg.MaxValue = 2;

                        tasks.Add(ctx.AddTask($"[dim blue on white] {msgArr[i]} [/]",tStg));                            
                    }
                    var taskArr = tasks.ToArray();
                    for (int i = 0; i < actions.Count; i ++)
                    {
                        var tmpTask = taskArr[i];
                        tmpTask.Description = $"[blue on white] {msgArr[i]} [/]";
                        Thread.Sleep(500);
                        tmpTask.StartTask();
                        tmpTask.Increment(1);
                        actionArr[i].Invoke();
                        tmpTask.Description = $"[dim blue on white] {msgArr[i]} [/]";
                        tmpTask.Increment(1);
                        tmpTask.StopTask();
                    }

                });            
        }

    }

}
