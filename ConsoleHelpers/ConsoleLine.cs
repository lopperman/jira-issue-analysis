using System.Reflection.Metadata;
using System;
using static JiraCon.ConsoleUtil;
using Spectre.Console;

namespace JiraCon
{
    public class ConsoleLine
    {
        public Color Foreground { get; set; }
        public Color Background { get; set; }
        public Style LineStyle {get;set;}
        public bool UseColors { get; set; }
        public string Text { get; set; }
        public bool WritePartialLine { get; set; }


        public ConsoleLine(string msg, Style lnStyle, bool terminateLine = true)
        {
            LineStyle = lnStyle;
            // Foreground = StdStyle(lnType).Foreground;
            // Background = StdStyle(lnType).Background;
            WritePartialLine = !terminateLine;
            UseColors = true;
            Text = msg;
        }

        public void Write()
        {
            if (WritePartialLine)
            {
                ConsoleUtil.WriteMarkup(Text,LineStyle);
            }
            else
            {
                // var m1 = new Markup(Text,LineStyle);

                // AnsiConsole.WriteLine(new Text(Text,LineStyle));

                // ConsoleUtil.WriteMarkupLine(Text,LineStyle);

                var rows = new List<Text>(){
                    new Text(Text, LineStyle)
                };
                // Renders each item with own style
                AnsiConsole.Write(new Rows(rows));

            }
        }

    }

}
