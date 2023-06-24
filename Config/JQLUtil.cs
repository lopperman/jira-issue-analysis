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

                // Table table = new Table();
                // table.Border(TableBorder.HeavyHead);

                // table.AddColumns(new TableColumn[]{
                //     new TableColumn(new Text("ID",colNameStyle)).Centered(),
                //     new TableColumn(new Text("NAME",colNameStyle)).Centered().Width(20),
                //     new TableColumn(new Text("JQL",colNameStyle)).Centered()});

                foreach (var jql in cfg.SavedJQL)
                {   
                    // var _id = new Text(jql.jqlId.Value.ToString());
                    // var _name = new Text(jql.jqlName);
                    //var _jql = new Text(jql.jql);
                    var _jql = new Panel(jql.jql);
                    _jql.Header($" {jql.jqlId:00} - Saved as: {jql.jqlName} ",Justify.Left);
                    _jql.Border = BoxBorder.Rounded;
                    _jql.BorderColor(Style.Parse("blue dim").Foreground);
                    _jql.Expand();
                    _jql.SafeBorder();
//                    table.AddRow(_id,_name,_jql);
                    AnsiConsole.Write(_jql);
                }
                // AnsiConsole.Write(table);
                ConsoleUtil.PressAnyKeyToContinue($"{cfg.SavedJQLCount} results");
            }
        }

        public static void AddJQL()
        {
            ConsoleUtil.PressAnyKeyToContinue("not implemented");
        }

    }
}