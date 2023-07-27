using Microsoft.VisualBasic.CompilerServices;
using System.Drawing;
using System.Collections;
namespace JTIS;

using JTIS.Console;
using Spectre.Console;

public enum CfgStyleEnum
{
    csManualKey = 0, 
    csErrorMessage = 1, 
    
}

public class CfgColor
{
    public CfgStyleEnum cfgStyle {get;set;}
    public string ColorKey {get;set;} = string.Empty;
    
    private Style? _style = null;
    public Style? style {
        get {
            if (_style == null)
            {
                _style = new Style();
            }
            return _style;
        } set{
            _style=value;
        }
    }
}

public static class ColorUtil
        {            
            private static Color ColorByIdx(int idx)
            {
                return ColorDictionary[idx];
            }
            public static Color? PickColor(string msg)
            {
                // int colorNameWidth = (System.Console.WindowWidth - 16)/4;
                int colorNameWidth = ColorDictionary.Max(x=>x.Value.ToString().Length) + 5;
                ConsoleUtil.WriteAppTitle();
                ConsoleUtil.WriteBanner(msg);
                Color? picked = null;

                var tbl = new Table();
                List<TableColumn> cols = new List<TableColumn>();
                cols.Add(new TableColumn("[bold underline]COLOR[/]").Centered().Width(colorNameWidth));
                cols.Add(new TableColumn("[bold underline]COLOR[/]").Centered().Width(colorNameWidth));
                cols.Add(new TableColumn("[bold underline]COLOR[/]").Centered().Width(colorNameWidth));
                cols.Add(new TableColumn("[bold underline]COLOR[/]").Centered().Width(colorNameWidth));
                tbl.AddColumns(cols.ToArray());
                for (int i = 0; i < ColorDictionary.Count(); i ++)
                {
                    var bColor1 = ColorByIdx(i);
                    var bColor2 = ColorByIdx(i+1);
                    var bColor3 = ColorByIdx(i+2);
                    var bColor4 = ColorByIdx(i+3);

                    var fColor1 = InverseColor(bColor1);
                    var fColor2 = InverseColor(bColor2);
                    var fColor3 = InverseColor(bColor3);
                    var fColor4 = InverseColor(bColor4);
                    string idx1 = $"{i}";
                    string idx2 = $"{i+1}";
                    string idx3 = $"{i+2}";
                    string idx4 = $"{i+3}";
                    string desc1 = $"{idx1}: {bColor1.ToString()}";
                    string desc2 = $"{idx2}: {bColor2.ToString()}";
                    string desc3 = $"{idx3}: {bColor3.ToString()}";
                    string desc4 = $"{idx4}: {bColor4.ToString()}";
                    if (desc1.Length < colorNameWidth)
                    {
                        desc1 = $"{desc1}{new string(' ',colorNameWidth-desc1.Length)}";
                    }
                    if (desc2.Length < colorNameWidth)
                    {
                        desc2 = $"{desc2}{new string(' ',colorNameWidth-desc2.Length)}";
                    }
                    if (desc3.Length < colorNameWidth)
                    {
                        desc3 = $"{desc3}{new string(' ',colorNameWidth-desc3.Length)}";
                    }
                    if (desc4.Length < colorNameWidth)
                    {
                        desc4 = $"{desc4}{new string(' ',colorNameWidth-desc4.Length)}";
                    }
                    desc1 = $"[bold {fColor1.ToString()} on {bColor1.ToString()}]{desc1}[/]";
                    desc2 = $"[bold {fColor2.ToString()} on {bColor2.ToString()}]{desc2}[/]";
                    desc3 = $"[bold {fColor3.ToString()} on {bColor3.ToString()}]{desc3}[/]";
                    desc4 = $"[bold {fColor4.ToString()} on {bColor4.ToString()}]{desc4}[/]";
                    tbl.AddRow(new Markup[]{
                        new Markup(desc1).LeftJustified(), 
                        new Markup(desc2).LeftJustified(),
                        new Markup(desc3).LeftJustified(), 
                        new Markup(desc4).LeftJustified() 
                        });
                    i +=3;
                }

                AnsiConsole.Write(tbl);
                int tmpColor = ConsoleUtil.GetInput<int>("Enter color index between 0-255, enter any other value to cancel");
                if (tmpColor >= 0 &&  tmpColor <= 255)
                {
                    picked = ColorByIdx(tmpColor);
                }
                else 
                {
                    picked = null;
                }
                return picked;
            }


            private static float CalculateLuminance(int[] rgb){
                return (float) (0.2126*rgb[0] + 0.7152*rgb[1] + 0.0722*rgb[2]);    
            }

            private static int[] HexToRBG(String colorStr) {
                int[] rbg = new int[]{
                    int.Parse(colorStr.Substring(0,3)),
                    int.Parse(colorStr.Substring(3,3)),
                    int.Parse(colorStr.Substring(6,3))};
                return rbg;
            }
            public static String getInverseBW(String hex_color) {
                float luminance = CalculateLuminance(HexToRBG(hex_color));
                String inverse = (luminance < 140) ? "#fff" : "#000";
                return inverse;
            }

            public static Color InverseColor(Color c)
            {
                Byte r = c.R;
                Byte g = c.G;
                Byte b = c.B;
                var clc = (r*0.299 + g*0.587 + b*0.114);
                return (clc < 160) ? Color.Grey100 : Color.Black;
            }

            public static double InverseCalcVal(Color c)
            {
                return (c.R*0.299 + c.G*0.587 + c.B*0.114);
            }

            public static IEnumerable<T> ReverseEnumerable<T>(this IEnumerable<T> source)
            {
                if (source is null)
                {
                    throw new ArgumentNullException(nameof(source));
                }

                return source.Reverse();
            }

            public static Spectre.Console.Style? CStyle(this StdLine line)
            {
                return ConsoleUtil.StdStyle(line);
            }
            public static string FontMkp(this StdLine line)
            {            
                return line.CStyle().Foreground.ToString();
            }
            public static Spectre.Console.Color FontColor(this StdLine line)
            {            
                return line.CStyle().Foreground;
            }
            public static string BackMkp(this StdLine line)
            {            
                return line.CStyle().Background.ToString();
            }
            public static Spectre.Console.Color BackColor(this StdLine line)
            {            
                return line.CStyle().Background;
            }

            public static SortedDictionary<int,Color> ColorDictionary
            {
                get {
                    return new SortedDictionary<int, Color>()
                    {
                        { 0, Color.Black },
                        { 1, Color.Maroon },
                        { 2, Color.Green },
                        { 3, Color.Olive },
                        { 4, Color.Navy },
                        { 5, Color.Purple },
                        { 6, Color.Teal },
                        { 7, Color.Silver },
                        { 8, Color.Grey },
                        { 9, Color.Red },
                        { 10, Color.Lime },
                        { 11, Color.Yellow },
                        { 12, Color.Blue },
                        { 13, Color.Fuchsia },
                        { 14, Color.Aqua },
                        { 15, Color.White },
                        { 16, Color.Grey0 },
                        { 17, Color.NavyBlue },
                        { 18, Color.DarkBlue },
                        { 19, Color.Blue3 },
                        { 20, Color.Blue3_1 },
                        { 21, Color.Blue1 },
                        { 22, Color.DarkGreen },
                        { 23, Color.DeepSkyBlue4 },
                        { 24, Color.DeepSkyBlue4_1 },
                        { 25, Color.DeepSkyBlue4_2 },
                        { 26, Color.DodgerBlue3 },
                        { 27, Color.DodgerBlue2 },
                        { 28, Color.Green4 },
                        { 29, Color.SpringGreen4 },
                        { 30, Color.Turquoise4 },
                        { 31, Color.DeepSkyBlue3 },
                        { 32, Color.DeepSkyBlue3_1 },
                        { 33, Color.DodgerBlue1 },
                        { 34, Color.Green3 },
                        { 35, Color.SpringGreen3 },
                        { 36, Color.DarkCyan },
                        { 37, Color.LightSeaGreen },
                        { 38, Color.DeepSkyBlue2 },
                        { 39, Color.DeepSkyBlue1 },
                        { 40, Color.Green3_1 },
                        { 41, Color.SpringGreen3_1 },
                        { 42, Color.SpringGreen2 },
                        { 43, Color.Cyan3 },
                        { 44, Color.DarkTurquoise },
                        { 45, Color.Turquoise2 },
                        { 46, Color.Green1 },
                        { 47, Color.SpringGreen2_1 },
                        { 48, Color.SpringGreen1 },
                        { 49, Color.MediumSpringGreen },
                        { 50, Color.Cyan2 },
                        { 51, Color.Cyan1 },
                        { 52, Color.DarkRed },
                        { 53, Color.DeepPink4 },
                        { 54, Color.Purple4 },
                        { 55, Color.Purple4_1 },
                        { 56, Color.Purple3 },
                        { 57, Color.BlueViolet },
                        { 58, Color.Orange4 },
                        { 59, Color.Grey37 },
                        { 60, Color.MediumPurple4 },
                        { 61, Color.SlateBlue3 },
                        { 62, Color.SlateBlue3_1 },
                        { 63, Color.RoyalBlue1 },
                        { 64, Color.Chartreuse4 },
                        { 65, Color.DarkSeaGreen4 },
                        { 66, Color.PaleTurquoise4 },
                        { 67, Color.SteelBlue },
                        { 68, Color.SteelBlue3 },
                        { 69, Color.CornflowerBlue },
                        { 70, Color.Chartreuse3 },
                        { 71, Color.DarkSeaGreen4_1 },
                        { 72, Color.CadetBlue },
                        { 73, Color.CadetBlue_1 },
                        { 74, Color.SkyBlue3 },
                        { 75, Color.SteelBlue1 },
                        { 76, Color.Chartreuse3_1 },
                        { 77, Color.PaleGreen3 },
                        { 78, Color.SeaGreen3 },
                        { 79, Color.Aquamarine3 },
                        { 80, Color.MediumTurquoise },
                        { 81, Color.SteelBlue1_1 },
                        { 82, Color.Chartreuse2 },
                        { 83, Color.SeaGreen2 },
                        { 84, Color.SeaGreen1 },
                        { 85, Color.SeaGreen1_1 },
                        { 86, Color.Aquamarine1 },
                        { 87, Color.DarkSlateGray2 },
                        { 88, Color.DarkRed_1 },
                        { 89, Color.DeepPink4_1 },
                        { 90, Color.DarkMagenta },
                        { 91, Color.DarkMagenta_1 },
                        { 92, Color.DarkViolet },
                        { 93, Color.Purple_1 },
                        { 94, Color.Orange4_1 },
                        { 95, Color.LightPink4 },
                        { 96, Color.Plum4 },
                        { 97, Color.MediumPurple3 },
                        { 98, Color.MediumPurple3_1 },
                        { 99, Color.SlateBlue1 },
                        { 100, Color.Yellow4 },
                        { 101, Color.Wheat4 },
                        { 102, Color.Grey53 },
                        { 103, Color.LightSlateGrey },
                        { 104, Color.MediumPurple },
                        { 105, Color.LightSlateBlue },
                        { 106, Color.Yellow4_1 },
                        { 107, Color.DarkOliveGreen3 },
                        { 108, Color.DarkSeaGreen },
                        { 109, Color.LightSkyBlue3 },
                        { 110, Color.LightSkyBlue3_1 },
                        { 111, Color.SkyBlue2 },
                        { 112, Color.Chartreuse2_1 },
                        { 113, Color.DarkOliveGreen3_1 },
                        { 114, Color.PaleGreen3_1 },
                        { 115, Color.DarkSeaGreen3 },
                        { 116, Color.DarkSlateGray3 },
                        { 117, Color.SkyBlue1 },
                        { 118, Color.Chartreuse1 },
                        { 119, Color.LightGreen },
                        { 120, Color.LightGreen_1 },
                        { 121, Color.PaleGreen1 },
                        { 122, Color.Aquamarine1_1 },
                        { 123, Color.DarkSlateGray1 },
                        { 124, Color.Red3 },
                        { 125, Color.DeepPink4_2 },
                        { 126, Color.MediumVioletRed },
                        { 127, Color.Magenta3 },
                        { 128, Color.DarkViolet_1 },
                        { 129, Color.Purple_2 },
                        { 130, Color.DarkOrange3 },
                        { 131, Color.IndianRed },
                        { 132, Color.HotPink3 },
                        { 133, Color.MediumOrchid3 },
                        { 134, Color.MediumOrchid },
                        { 135, Color.MediumPurple2 },
                        { 136, Color.DarkGoldenrod },
                        { 137, Color.LightSalmon3 },
                        { 138, Color.RosyBrown },
                        { 139, Color.Grey63 },
                        { 140, Color.MediumPurple2_1 },
                        { 141, Color.MediumPurple1 },
                        { 142, Color.Gold3 },
                        { 143, Color.DarkKhaki },
                        { 144, Color.NavajoWhite3 },
                        { 145, Color.Grey69 },
                        { 146, Color.LightSteelBlue3 },
                        { 147, Color.LightSteelBlue },
                        { 148, Color.Yellow3 },
                        { 149, Color.DarkOliveGreen3_2 },
                        { 150, Color.DarkSeaGreen3_1 },
                        { 151, Color.DarkSeaGreen2 },
                        { 152, Color.LightCyan3 },
                        { 153, Color.LightSkyBlue1 },
                        { 154, Color.GreenYellow },
                        { 155, Color.DarkOliveGreen2 },
                        { 156, Color.PaleGreen1_1 },
                        { 157, Color.DarkSeaGreen2_1 },
                        { 158, Color.DarkSeaGreen1 },
                        { 159, Color.PaleTurquoise1 },
                        { 160, Color.Red3_1 },
                        { 161, Color.DeepPink3 },
                        { 162, Color.DeepPink3_1 },
                        { 163, Color.Magenta3_1 },
                        { 164, Color.Magenta3_2 },
                        { 165, Color.Magenta2 },
                        { 166, Color.DarkOrange3_1 },
                        { 167, Color.IndianRed_1 },
                        { 168, Color.HotPink3_1 },
                        { 169, Color.HotPink2 },
                        { 170, Color.Orchid },
                        { 171, Color.MediumOrchid1 },
                        { 172, Color.Orange3 },
                        { 173, Color.LightSalmon3_1 },
                        { 174, Color.LightPink3 },
                        { 175, Color.Pink3 },
                        { 176, Color.Plum3 },
                        { 177, Color.Violet },
                        { 178, Color.Gold3_1 },
                        { 179, Color.LightGoldenrod3 },
                        { 180, Color.Tan },
                        { 181, Color.MistyRose3 },
                        { 182, Color.Thistle3 },
                        { 183, Color.Plum2 },
                        { 184, Color.Yellow3_1 },
                        { 185, Color.Khaki3 },
                        { 186, Color.LightGoldenrod2 },
                        { 187, Color.LightYellow3 },
                        { 188, Color.Grey84 },
                        { 189, Color.LightSteelBlue1 },
                        { 190, Color.Yellow2 },
                        { 191, Color.DarkOliveGreen1 },
                        { 192, Color.DarkOliveGreen1_1 },
                        { 193, Color.DarkSeaGreen1_1 },
                        { 194, Color.Honeydew2 },
                        { 195, Color.LightCyan1 },
                        { 196, Color.Red1 },
                        { 197, Color.DeepPink2 },
                        { 198, Color.DeepPink1 },
                        { 199, Color.DeepPink1_1 },
                        { 200, Color.Magenta2_1 },
                        { 201, Color.Magenta1 },
                        { 202, Color.OrangeRed1 },
                        { 203, Color.IndianRed1 },
                        { 204, Color.IndianRed1_1 },
                        { 205, Color.HotPink },
                        { 206, Color.HotPink_1 },
                        { 207, Color.MediumOrchid1_1 },
                        { 208, Color.DarkOrange },
                        { 209, Color.Salmon1 },
                        { 210, Color.LightCoral },
                        { 211, Color.PaleVioletRed1 },
                        { 212, Color.Orchid2 },
                        { 213, Color.Orchid1 },
                        { 214, Color.Orange1 },
                        { 215, Color.SandyBrown },
                        { 216, Color.LightSalmon1 },
                        { 217, Color.LightPink1 },
                        { 218, Color.Pink1 },
                        { 219, Color.Plum1 },
                        { 220, Color.Gold1 },
                        { 221, Color.LightGoldenrod2_1 },
                        { 222, Color.LightGoldenrod2_2 },
                        { 223, Color.NavajoWhite1 },
                        { 224, Color.MistyRose1 },
                        { 225, Color.Thistle1 },
                        { 226, Color.Yellow1 },
                        { 227, Color.LightGoldenrod1 },
                        { 228, Color.Khaki1 },
                        { 229, Color.Wheat1 },
                        { 230, Color.Cornsilk1 },
                        { 231, Color.Grey100 },
                        { 232, Color.Grey3 },
                        { 233, Color.Grey7 },
                        { 234, Color.Grey11 },
                        { 235, Color.Grey15 },
                        { 236, Color.Grey19 },
                        { 237, Color.Grey23 },
                        { 238, Color.Grey27 },
                        { 239, Color.Grey30 },
                        { 240, Color.Grey35 },
                        { 241, Color.Grey39 },
                        { 242, Color.Grey42 },
                        { 243, Color.Grey46 },
                        { 244, Color.Grey50 },
                        { 245, Color.Grey54 },
                        { 246, Color.Grey58 },
                        { 247, Color.Grey62 },
                        { 248, Color.Grey66 },
                        { 249, Color.Grey70 },
                        { 250, Color.Grey74 },
                        { 251, Color.Grey78 },
                        { 252, Color.Grey82 },
                        { 253, Color.Grey85 },
                        { 254, Color.Grey89 },
                        { 255, Color.Grey93 }
                    };
                }
            }
            public static List<Color> ColorsAll
            {
                get{
                    return new List<Color>()
                    {
                        Color.Grey0,
                        Color.NavyBlue,
                        Color.DarkBlue,
                        Color.Blue3,
                        Color.Blue3_1,
                        Color.Blue1,
                        Color.DarkGreen,
                        Color.DeepSkyBlue4,
                        Color.DeepSkyBlue4_1,
                        Color.DeepSkyBlue4_2,
                        Color.DodgerBlue3,
                        Color.DodgerBlue2,
                        Color.Green4,
                        Color.SpringGreen4,
                        Color.Turquoise4,
                        Color.DeepSkyBlue3,
                        Color.DeepSkyBlue3_1,
                        Color.DodgerBlue1,
                        Color.Green3,
                        Color.SpringGreen3,
                        Color.DarkCyan,
                        Color.LightSeaGreen,
                        Color.DeepSkyBlue2,
                        Color.DeepSkyBlue1,
                        Color.Green3_1,
                        Color.SpringGreen3_1,
                        Color.SpringGreen2,
                        Color.Cyan3,
                        Color.DarkTurquoise,
                        Color.Turquoise2,
                        Color.Green1,
                        Color.SpringGreen2_1,
                        Color.SpringGreen1,
                        Color.MediumSpringGreen,
                        Color.Cyan2,
                        Color.Cyan1,
                        Color.DarkRed,
                        Color.DeepPink4,
                        Color.Purple4,
                        Color.Purple4_1,
                        Color.Purple3,
                        Color.BlueViolet,
                        Color.Orange4,
                        Color.Grey37,
                        Color.MediumPurple4,
                        Color.SlateBlue3,
                        Color.SlateBlue3_1,
                        Color.RoyalBlue1,
                        Color.Chartreuse4,
                        Color.DarkSeaGreen4,
                        Color.PaleTurquoise4,
                        Color.SteelBlue,
                        Color.SteelBlue3,
                        Color.CornflowerBlue,
                        Color.Chartreuse3,
                        Color.DarkSeaGreen4_1,
                        Color.CadetBlue,
                        Color.CadetBlue_1,
                        Color.SkyBlue3,
                        Color.SteelBlue1,
                        Color.Chartreuse3_1,
                        Color.PaleGreen3,
                        Color.SeaGreen3,
                        Color.Aquamarine3,
                        Color.MediumTurquoise,
                        Color.SteelBlue1_1,
                        Color.Chartreuse2,
                        Color.SeaGreen2,
                        Color.SeaGreen1,
                        Color.SeaGreen1_1,
                        Color.Aquamarine1,
                        Color.DarkSlateGray2,
                        Color.DarkRed_1,
                        Color.DeepPink4_1,
                        Color.DarkMagenta,
                        Color.DarkMagenta_1,
                        Color.DarkViolet,
                        Color.Purple_1,
                        Color.Orange4_1,
                        Color.LightPink4,
                        Color.Plum4,
                        Color.MediumPurple3,
                        Color.MediumPurple3_1,
                        Color.SlateBlue1,
                        Color.Yellow4,
                        Color.Wheat4,
                        Color.Grey53,
                        Color.LightSlateGrey,
                        Color.MediumPurple,
                        Color.LightSlateBlue,
                        Color.Yellow4_1,
                        Color.DarkOliveGreen3,
                        Color.DarkSeaGreen,
                        Color.LightSkyBlue3,
                        Color.LightSkyBlue3_1,
                        Color.SkyBlue2,
                        Color.Chartreuse2_1,
                        Color.DarkOliveGreen3_1,
                        Color.PaleGreen3_1,
                        Color.DarkSeaGreen3,
                        Color.DarkSlateGray3,
                        Color.SkyBlue1,
                        Color.Chartreuse1,
                        Color.LightGreen,
                        Color.LightGreen_1,
                        Color.PaleGreen1,
                        Color.Aquamarine1_1,
                        Color.DarkSlateGray1,
                        Color.Red3,
                        Color.DeepPink4_2,
                        Color.MediumVioletRed,
                        Color.Magenta3,
                        Color.DarkViolet_1,
                        Color.Purple_2,
                        Color.DarkOrange3,
                        Color.IndianRed,
                        Color.HotPink3,
                        Color.MediumOrchid3,
                        Color.MediumOrchid,
                        Color.MediumPurple2,
                        Color.DarkGoldenrod,
                        Color.LightSalmon3,
                        Color.RosyBrown,
                        Color.Grey63,
                        Color.MediumPurple2_1,
                        Color.MediumPurple1,
                        Color.Gold3,
                        Color.DarkKhaki,
                        Color.NavajoWhite3,
                        Color.Grey69,
                        Color.LightSteelBlue3,
                        Color.LightSteelBlue,
                        Color.Yellow3,
                        Color.DarkOliveGreen3_2,
                        Color.DarkSeaGreen3_1,
                        Color.DarkSeaGreen2,
                        Color.LightCyan3,
                        Color.LightSkyBlue1,
                        Color.GreenYellow,
                        Color.DarkOliveGreen2,
                        Color.PaleGreen1_1,
                        Color.DarkSeaGreen2_1,
                        Color.DarkSeaGreen1,
                        Color.PaleTurquoise1,
                        Color.Red3_1,
                        Color.DeepPink3,
                        Color.DeepPink3_1,
                        Color.Magenta3_1,
                        Color.Magenta3_2,
                        Color.Magenta2,
                        Color.DarkOrange3_1,
                        Color.IndianRed_1,
                        Color.HotPink3_1,
                        Color.HotPink2,
                        Color.Orchid,
                        Color.MediumOrchid1,
                        Color.Orange3,
                        Color.LightSalmon3_1,
                        Color.LightPink3,
                        Color.Pink3,
                        Color.Plum3,
                        Color.Violet,
                        Color.Gold3_1,
                        Color.LightGoldenrod3,
                        Color.Tan,
                        Color.MistyRose3,
                        Color.Thistle3,
                        Color.Plum2,
                        Color.Yellow3_1,
                        Color.Khaki3,
                        Color.LightGoldenrod2,
                        Color.LightYellow3,
                        Color.Grey84,
                        Color.LightSteelBlue1,
                        Color.Yellow2,
                        Color.DarkOliveGreen1,
                        Color.DarkOliveGreen1_1,
                        Color.DarkSeaGreen1_1,
                        Color.Honeydew2,
                        Color.LightCyan1,
                        Color.Red1,
                        Color.DeepPink2,
                        Color.DeepPink1,
                        Color.DeepPink1_1,
                        Color.Magenta2_1,
                        Color.Magenta1,
                        Color.OrangeRed1,
                        Color.IndianRed1,
                        Color.IndianRed1_1,
                        Color.HotPink,
                        Color.HotPink_1,
                        Color.MediumOrchid1_1,
                        Color.DarkOrange,
                        Color.Salmon1,
                        Color.LightCoral,
                        Color.PaleVioletRed1,
                        Color.Orchid2,
                        Color.Orchid1,
                        Color.Orange1,
                        Color.SandyBrown,
                        Color.LightSalmon1,
                        Color.LightPink1,
                        Color.Pink1,
                        Color.Plum1,
                        Color.Gold1,
                        Color.LightGoldenrod2_1,
                        Color.LightGoldenrod2_2,
                        Color.NavajoWhite1,
                        Color.MistyRose1,
                        Color.Thistle1,
                        Color.Yellow1,
                        Color.LightGoldenrod1,
                        Color.Khaki1,
                        Color.Wheat1,
                        Color.Cornsilk1,
                        Color.Grey100,
                        Color.Grey3,
                        Color.Grey7,
                        Color.Grey11,
                        Color.Grey15,
                        Color.Grey19,
                        Color.Grey23,
                        Color.Grey27,
                        Color.Grey30,
                        Color.Grey35,
                        Color.Grey39,
                        Color.Grey42,
                        Color.Grey46,
                        Color.Grey50,
                        Color.Grey54,
                        Color.Grey58,
                        Color.Grey62,
                        Color.Grey66,
                        Color.Grey70,
                        Color.Grey74,
                        Color.Grey78,
                        Color.Grey82,
                        Color.Grey85,
                        Color.Grey89,
                        Color.Grey93,
                    };                    
                }
            }

        }
