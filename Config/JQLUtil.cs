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
                    var _jql = new Panel(jql.jql);
                    _jql.Header($" {jql.jqlId:00} - Saved as: {jql.jqlName} ",Justify.Left);
                    _jql.Border = BoxBorder.Rounded;
                    _jql.BorderColor(Style.Parse("blue dim").Foreground);
                    _jql.Expand();
                    _jql.SafeBorder();
                    AnsiConsole.Write(_jql);
                }
                ConsoleUtil.PressAnyKeyToContinue($"{cfg.SavedJQLCount} results");
            }
        }

        public static void AddJQL()
        {
            ConsoleUtil.PressAnyKeyToContinue("not implemented");
        }

    }
}