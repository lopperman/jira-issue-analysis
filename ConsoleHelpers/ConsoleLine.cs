using System.Reflection.Metadata;
using System;
using static JiraCon.ConsoleUtil;

namespace JiraCon
{
    public class ConsoleLine
    {
        public ConsoleColor Foreground { get; set; }
        public ConsoleColor Background { get; set; }
        public StdLine LineType {get;set;}
        public bool UseColors { get; set; }
        public string Text { get; set; }
        public bool WritePartialLine { get; set; }


        public ConsoleLine(string text) : this(text, false)
        {
        }

        public ConsoleLine(string text, bool writePartialLine)
        {
            WritePartialLine = writePartialLine;
            UseColors = false;
            Text = text;
        }

        public ConsoleLine(string text, StdLine lnType): this(text,StdForecolor(lnType),StdBackcolor(lnType))
        {
            
        }
        public ConsoleLine(string text, StdLine lnType, bool writePartialLine): this(text,StdForecolor(lnType),StdBackcolor(lnType), writePartialLine)
        {
            
        }

        public ConsoleLine(string text, ConsoleColor foreground, ConsoleColor background) : this(text, foreground, background, false)
        {
        }

        public ConsoleLine(string text, ConsoleColor foreground, ConsoleColor background, bool writePartialLine)
        {
            Foreground = foreground;
            Background = background;
            Text = text;
            UseColors = true;
            WritePartialLine = writePartialLine;
        }

    }

}
