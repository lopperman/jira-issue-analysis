using System.Data.SqlTypes;
using System;
using JTIS.Config;
using Spectre.Console;
using JTIS.Extensions;

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
            if (string.IsNullOrWhiteSpace(data))
            {
                return data;
            }
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
                List<string> scList = new List<string>();
                if (CfgManager.config != null)
                {
                    scList = CfgManager.config.ScrubData;
                }
                else 
                {
                    scList = CfgManager.LoadDevScrubList();
                }
                foreach (var scrubItem in scList)
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

            AnsiConsole.WriteLine();
            var mk = new List<Markup>();
            var finalMsg = new Markup($"{Emoji.Known.BlackSquareButton}  [{StdLine.slResponse.FontMkp()} on {StdLine.slResponse.BackMkp()}]  PRESS ANY KEY TO CONTINUE  [/]");            
            Rows? rws = null;
            if (msg!=null)
            {
                msg = Markup.Remove(msg);
                msg = $"{Emoji.Known.BlackSquareButton}  [{StdLine.slInfo.FontMkp()} on {StdLine.slInfo.BackMkp()}]{msg}[/]{Environment.NewLine}";
                rws = new Rows(new Markup(msg),finalMsg);
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
                        return $" [bold maroon on cornsilk1]USING TIME ZONE: {tzi}[/] ";
                    }
                }
                return string.Empty;
            }
        }

        public static void WriteAppHello()
        {
            WriteAppTitle();
            WriteBanner("FEATURE SUGGESTIONS? DROP A NOTE HERE: [link]https://github.com/lopperman/jira-issue-analysis/discussions[/] ");
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

            AnsiConsole.Write(panel);
            var pbar = new ProgressBarColumn();
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

        public static string scrubMode 
        {
            get 
            {
                if (ScrubData != null && ScrubData==true)
                {
                    return $"[bold maroon on cornsilk1] * SCRUB DATA MODE * [/]";
                }
                return string.Empty;
            }

        }
        public static void WriteAppTitle(bool underDev = false)
        {
            // var panel = new Panel(title);
            // panel.Border = BoxBorder.
            // panel.BorderColor(Color.Grey15);
            // panel.Expand = true;
            // AnsiConsole.Write(panel);
//            panel.HeaderAlignment(Justify.Center );
            AnsiConsole.Clear();
            var title = $"  JIRA Time In Status :llama: [dim]by[/] [dim link=https://github.com/lopperman/jira-issue-analysis]Paul Brower[/]{scrubMode}{ConsoleUtil.RecordingInfo}{ConsoleUtil.TimeZoneAlert}{Environment.NewLine}  [dim italic][link]https://github.com/lopperman/jira-issue-analysis[/][/]";     
            AnsiConsole.Write(new Rule());
            AnsiConsole.MarkupLine(title);
            var tr = new Rule().DoubleBorder();
            AnsiConsole.Write(tr);
            if (underDev)
            {
                WriteBanner("THIS AREA IS UNDER DEVELOPMENT");
            }
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

//            AnsiConsole.MarkupLine(text,StdStyle(StdLine.slError));
            WriteBanner(text,Color.Red);

            if (ex != null)
            {                
                WriteBanner(ex.Message,Color.Red);
                WriteBanner(ex.StackTrace,Color.Red);
                // WriteError(ex.StackTrace);
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
        
        public static void  WriteBanner(string msg, Color? foreColor = null, Color? backColor = null)
        {
            if (foreColor == null){foreColor = Color.Maroon;}
            if (backColor == null){backColor = Color.Cornsilk1;}
            var r = new Rule();
            r.Style = new Style(foreColor,backColor).Decoration(Decoration.Dim);
            r.Border(BoxBorder.Heavy);            
            AnsiConsole.Write(r);

            msg = $"  {msg.Trim()}  ";
            int lines = 1;
            lines = (int)(msg.Length/System.Console.WindowWidth);
            if (lines * System.Console.WindowWidth < msg.Length){lines+=1;}
            msg = $"[bold {foreColor} on {backColor}]{msg}[/]";

            FillNextXLines(Color.Cornsilk1,lines);
            FillLines(Color.Cornsilk1,0,lines,0);
            AnsiConsole.MarkupLine(msg);
            AnsiConsole.Write(r);
        }

        public static T GetInput<T>(string msg,T defVal=default(T), bool allowEmpty = false, bool concealed = false) where T:IComparable<T>
        {
            T retVal = default(T);

            AnsiConsole.WriteLine();
            var noMarkup = Markup.Remove(msg);
            if (msg.EndsWith(Environment.NewLine))
            {
                int place = msg.LastIndexOf(Environment.NewLine);
                if (place >-1)
                {
                    msg = msg.Remove(place,Environment.NewLine.Length);
                }                
            }
            msg = $"  {msg.Trim()}";
            noMarkup = Markup.Remove(msg);
            int lines = 1;
            // if (msg.Length <= System.Console.WindowWidth)
            // {
            //     msg = $"[bold blue on cornsilk1]{msg}[/]";            
            // }
            // else 
            // {
                lines = (int)(msg.Length/System.Console.WindowWidth);
                if (lines * System.Console.WindowWidth < msg.Length){lines+=1;}
//                msg = $"{msg}" + new string(' ',(lines*System.Console.WindowWidth)-msg.Length);

                msg = $"[bold blue on cornsilk1]{msg}[/]";
            // }
            // msg = $"{msg}{Environment.NewLine}:: ";
            var showDefVal = false;
            if (default(T) != null &&  default(T).CompareTo(defVal)!=0)
            {
                showDefVal = true;
            }
            
            var r = new Rule();
            r.Style = new Style(Color.Blue,Color.Cornsilk1).Decoration(Decoration.Dim);
            r.Border(BoxBorder.Heavy);            
            AnsiConsole.Write(r);
            FillNextXLines(Color.Cornsilk1,lines);
            AnsiConsole.MarkupLine(msg);
            AnsiConsole.Write(r);

            var tp = new TextPrompt<T>(" : ");
            if (concealed)
            {
                tp.Secret('*');
            }
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
                PressAnyKeyToContinue("(Empty) is not allowed, please try again");
                return GetInput<T>(msg,defVal,allowEmpty,concealed);
            }
            return retVal;
        }
        public static bool Confirm(string msg, bool defResp)
        {
            var startLine = System.Console.CursorTop;
            var r = new Rule();
            r.Style = new Style(Color.Blue,Color.Cornsilk1).Decoration(Decoration.Dim);
            r.Border(BoxBorder.Heavy);
            AnsiConsole.Write(r);

            msg = Markup.Remove(msg);
            msg = $"  {msg}  ";
            int lines = System.Console.WindowWidth/msg.Length;
            if (System.Console.WindowWidth * lines < msg.Length){lines +=1;}
//            FillNextXLines(Color.Cornsilk1,lines);
            msg = $"[bold blue on cornsilk1]{msg}[/]";
            AnsiConsole.MarkupLine(msg);
            var result = AnsiConsole.Confirm($"{Environment.NewLine}:",defResp);
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

        public static void FillNextXLines(Color fillColor, int lines)
        {
            var currentPos = System.Console.GetCursorPosition();
            var spaces = new string(c:' ', System.Console.WindowWidth);
//            var startBackColor = System.Console.BackgroundColor;
            for (int i = 0; i < lines; i++)
            {
                System.Console.SetCursorPosition(0,(currentPos.Top + i));
                System.Console.BackgroundColor=fillColor;
                AnsiConsole.Write(new Markup(spaces,new Style(AnsiConsole.Foreground,fillColor)));
                // System.Console.Write(spaces);
                // AnsiConsole.Markup($"[yellow on navy]{spaces}[/]");
            }
            // System.Console.BackgroundColor=startBackColor;
            System.Console.SetCursorPosition(currentPos.Left,currentPos.Top);

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
                AnsiConsole.Write(new Markup($"{spaces}",new Style(AnsiConsole.Foreground,fillColor)));
            }
            System.Console.SetCursorPosition(currentPos.Left,(currentPos.Top + resumeOffset));
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
