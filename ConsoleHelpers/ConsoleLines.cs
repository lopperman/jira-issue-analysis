using System;
using System.Collections.Generic;

namespace JiraCon
{
    public class ConsoleLines
    {
        private SortedDictionary<int, ConsoleLine> _lines = new SortedDictionary<int, ConsoleLine>();
        public string configInfo = string.Empty;

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
            WriteQueuedLines(false);
        }

        public void WriteQueuedLines(bool clearScreen)
        {
            if (clearScreen)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.White;

                string title = string.Format("JiraConsole (Not Trademarked) - written by Paul Brower");

                Console.WriteLine(title);
                if (!string.IsNullOrEmpty(configInfo))
                {
                    Console.WriteLine(configInfo);
                }
            }
            for (int i = 0; i < _lines.Count; i++)
            {
                ConsoleLine l = _lines[i];
                if (l.UseColors)
                {
                    Console.ForegroundColor = l.Foreground;
                    Console.BackgroundColor = l.Background;
                }
                if (l.WritePartialLine)
                {
                    Console.Write(l.Text);
                }
                else
                {
                    Console.WriteLine(l.Text);
                }
            }
            _lines.Clear();

        }

        public void ByeBye()
        {
            AddConsoleLine("   HAVE A GREAT DAY!!   ", ConsoleColor.DarkBlue, ConsoleColor.Yellow);
            WriteQueuedLines(true);
        }
    }

}
