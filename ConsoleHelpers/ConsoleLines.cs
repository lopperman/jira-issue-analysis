using System;
using System.Collections.Generic;
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

        public void AddConsoleLine(string text, StdLine lineType)
        {
            AddConsoleLine(text,StdForecolor(lineType),StdBackcolor(lineType));
        }
        public void AddConsoleLine(string text, StdLine lineType, bool writePartial)
        {
            AddConsoleLine(text,StdForecolor(lineType),StdBackcolor(lineType), writePartial);
        }

        public void AddConsoleLine(string text, ConsoleColor foreground, ConsoleColor background)
        {
            AddConsoleLine(text, foreground, background, false);
        }
        public void AddConsoleLine(string text, ConsoleColor foreground, ConsoleColor background, bool writePartial)
        {
            AddConsoleLine(new ConsoleLine(text, foreground, background));
        }


        public void AddConsoleLine(string text)
        {
            AddConsoleLine(text, false);
        }
        public void AddConsoleLine(string text, bool writePartial)
        {
            AddConsoleLine(new ConsoleLine(text, writePartial));
        }


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
                Console.Clear();

            }
            if (addTitle)
            {
                string title = "JiraTIS - (https://github.com/lopperman/jiraTimeInStatus";
                ConsoleUtil.WriteLine(title,ConsoleColor.White,ConsoleColor.DarkGray,false);
            }
            for (int i = 0; i < _lines.Count; i++)
            {
                Console.ResetColor();
                ConsoleLine l = _lines[i];
                if (l.UseColors)
                {
                    Console.ForegroundColor = l.Foreground;
                    Console.BackgroundColor = l.Background;

                }
                if (l.WritePartialLine)
                {
                    if (l.UseColors)
                    {
                        ConsoleUtil.WriteAppend(l.Text,l.Foreground,l.Background);
                    }
                    else 
                    {
                        ConsoleUtil.WriteAppend(l.Text);
                    }
                    // Console.Write(l.Text);
                }
                else
                {
                    if (l.UseColors)
                    {
                        ConsoleUtil.WriteLine(l.Text,l.Foreground,l.Background,false);
                    }
                    else 
                    {
                        ConsoleUtil.WriteLine(l.Text);
                    }
                }
            }
            _lines.Clear();
        }
        public void WriteQueuedLines(bool clearScreen)
        {
            WriteQueuedLines(clearScreen,true);
        }

        public void ByeBye()
        {
            AddConsoleLine("   HAVE A GREAT DAY!!   ", ConsoleColor.DarkBlue, ConsoleColor.Yellow);
            WriteQueuedLines(true);
        }
    }

}
