using Spectre.Console;

namespace JiraCon
{
    public static class JQLUtil
    {
        
        public static void ViewSavedJQL(JTISConfig cfg)
        {
            if (cfg.SavedJQLCount == 0)
            {
                ConsoleUtil.WriteMarkupLine("You don't have any saved JQL",StdLine.slError.CStyle());
            }
            else 
            {
                var codeStyle = ConsoleUtil.StdStyle(StdLine.slCode);
                var colNameStyle = ConsoleUtil.StdStyle(StdLine.slOutputTitle);

                foreach (var jql in cfg.SavedJQL)
                {   
                    var _jql = new Panel(Markup.FromInterpolated($"[dim]JQL/Issue List: [/][italic]{jql.jql}[/]"));
                    _jql.Header($"|  {jql.jqlId:00} - Saved as: {jql.jqlName}  |",Justify.Left);
                    _jql.Border = BoxBorder.Rounded;
                    _jql.BorderColor(Style.Parse("dim blue").Foreground);
                    _jql.Padding(2,2,2,0);
                    _jql.Expand();
                    _jql.SafeBorder();
                    AnsiConsole.Write(_jql);
                }
                ConsoleUtil.PressAnyKeyToContinue($"{cfg.SavedJQLCount} results");
            }
        }

        public static void AddJQL()
        {
            var jql = ConsoleUtil.GetInput<string>($"Enter JQL Statement, or delimited (space or comma) list of issues{Environment.NewLine}",allowEmpty:true);
            if (!string.IsNullOrEmpty(jql))
            {
                string sName = ConsoleUtil.GetInput<string>("Enter short name to describe the entry (leave blank to cancel)",allowEmpty:true);
                if (!string.IsNullOrWhiteSpace(sName))
                {
                    JTISConfigHelper.config.AddJQL(sName,jql);
                    JTISConfigHelper.SaveConfigList();
                }
            }

        }

    }
}