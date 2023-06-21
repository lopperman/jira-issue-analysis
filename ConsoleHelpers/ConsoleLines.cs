using System;
using System.Collections.Generic;
using Spectre.Console;
using static JiraCon.ConsoleUtil;

namespace JiraCon
{
    public class ConsoleLines
    {
        private SortedDictionary<int, ConsoleLine> _lines = new SortedDictionary<int, ConsoleLine>();

        public SortedDictionary<int, ConsoleLine> Lines
        {
            get
            {
                return _lines;
            }
        }

        public ConsoleLines()
        {
        }

        public bool HasQueuedLines
        {
            get
            {
                return _lines.Count > 0;
            }
        }

        // public void AddConsoleLine(string text, Style lineStyle)
        // {
        //     AddConsoleLine(text,lineStyle);
        // }
        public void AddConsoleLine(string text, Style lineStyle, bool writePartial = false)
        {
            var ln = new ConsoleLine(text,lineStyle,(!writePartial));
            Lines.Add(Lines.Count,ln);
            // AddConsoleLine(text,StdForecolor(lineType),StdBackcolor(lineType), writePartial);
        }



//         private void AddLine(ConsoleLine ln)
//         {
//             var kvp = new KeyValuePair<int,ConsoleLine>({1,ln};)
// v
//             _lines.Add(kvp);
//         }

        // public void AddConsoleLine(string text, ConsoleColor foreground, ConsoleColor background)
        // {
        //     AddConsoleLine(text, foreground, background, false);
        // }
        // public void AddConsoleLine(string text, ConsoleColor foreground, ConsoleColor background, bool writePartial)
        // {
        //     AddConsoleLine(new ConsoleLine(text, foreground, background));
        // }


        // public void AddConsoleLine(string text)
        // {
        //     AddConsoleLine(text, false);
        // }
        // public void AddConsoleLine(string text, bool writePartial)
        // {
        //     AddConsoleLine(new ConsoleLine(text, writePartial));
        // }


        public void AddConsoleLine(ConsoleLine cl)
        {
            _lines.Add(_lines.Count, cl);
        }

        public void WriteQueuedLines()
        {
            WriteQueuedLines(false,true);
        }

        public void WriteQueuedLines(bool clearScreen, bool addTitle)
        {
            Console.ResetColor();
            if (clearScreen)
            {
                AnsiConsole.Clear();
            }
            if (addTitle)
            {
                AnsiConsole.Clear();
                WriteAppTitle();
            }
            for (int i = 0; i < _lines.Count; i++)
            {
                _lines[i].Write();
            //     Console.ResetColor();
            //     ConsoleLine l = _lines[i];
            //     if (l.UseColors)
            //     {
            //         Console.ForegroundColor = l.Foreground;
            //         Console.BackgroundColor = l.Background;
            //     }
            //     else 
            //     {
            //         Console.ForegroundColor = defForeground;
            //         Console.BackgroundColor = defBackground;
            //     }
            //     if (l.WritePartialLine)
            //     {
            //         if (l.UseColors)
            //         {
            //             ConsoleUtil.WriteAppend(l.Text,l.Foreground,l.Background);
            //         }
            //         else 
            //         {
            //             ConsoleUtil.WriteAppend(l.Text,defForeground,defBackground);
            //         }
            //         // Console.Write(l.Text);
            //     }
            //     else
            //     {
            //         if (l.UseColors)
            //         {
            //             ConsoleUtil.WriteStdLine(l.Text,l.Foreground,l.Background);
            //         }
            //         else 
            //         {
            //             ConsoleUtil.WriteStdLine(l.Text,defForeground,defBackground);
            //         }
            //     }
            }
            _lines.Clear();
        }
        public void WriteQueuedLines(bool clearScreen)
        {
            WriteQueuedLines(clearScreen,true);
        }


    }

}
