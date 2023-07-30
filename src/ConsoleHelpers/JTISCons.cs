using System.Diagnostics.Tracing;

using Spectre.Console;
using Spectre.Console.Json;


namespace JTIS;

    
    public static class JC
    { 
        private static void test()
        {
            // AnsiConsole
        }
        private static IAnsiConsole? _cons = null;

        public static IAnsiConsole Console
        {
            get{
                if (_cons == null)
                {
                    _cons = AnsiConsole.Create(ConsoleSettings);
                }
                return _cons;
            }
        }
        private static AnsiConsoleSettings ConsoleSettings{
            get{
                var stg = new AnsiConsoleSettings();
                // stg.ColorSystem = ColorSystemSupport.Standard;
                // var pe = new ProfileEnrichment();
                


                //stg.EnvironmentVariables.

                return stg;
            }
        }

        public static void ShowJson(string title, string jsonData)
        {
            var json = new JsonText(jsonData);
            
                    
            AnsiConsole.Write(
                new Panel(json)
                    .Header($"{title}")
                    .Padding(2,1,2,1)                    
                    .Collapse()
                    .RoundedBorder()
                    .BorderColor(Color.Blue));            
        }


        public static void Info()
        {
            var grid = new Grid()
                .AddColumn(new GridColumn().NoWrap().PadRight(4))
                .AddColumn()
                .AddRow("[b]Enrichers[/]", string.Join(", ", AnsiConsole.Profile.Enrichers))
                .AddRow("[b]Color system[/]", $"{AnsiConsole.Profile.Capabilities.ColorSystem}")
                .AddRow("[b]Unicode?[/]", $"{YesNo(AnsiConsole.Profile.Capabilities.Unicode)}")
                .AddRow("[b]Supports ansi?[/]", $"{YesNo(AnsiConsole.Profile.Capabilities.Ansi)}")
                .AddRow("[b]Supports links?[/]", $"{YesNo(AnsiConsole.Profile.Capabilities.Links)}")
                .AddRow("[b]Legacy console?[/]", $"{YesNo(AnsiConsole.Profile.Capabilities.Legacy)}")
                .AddRow("[b]Interactive?[/]", $"{YesNo(AnsiConsole.Profile.Capabilities.Interactive)}")
                .AddRow("[b]Terminal?[/]", $"{YesNo(AnsiConsole.Profile.Out.IsTerminal)}")
                .AddRow("[b]Buffer width[/]", $"{AnsiConsole.Console.Profile.Width}")
                .AddRow("[b]Buffer height[/]", $"{AnsiConsole.Console.Profile.Height}")
                .AddRow("[b]Encoding[/]", $"{AnsiConsole.Console.Profile.Encoding.EncodingName}");

            AnsiConsole.Write(
                new Panel(grid)
                    .Header("Information"));            
        }
        private static string YesNo(bool value)
        {
            return value ? "Yes" : "No";
        }



    }



