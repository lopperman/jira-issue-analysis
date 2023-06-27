using Spectre.Console;

namespace JiraCon
{
    public static class JQLUtil
    {
        
        public static void RemoveJQL(JTISConfig cfg)
        {
            if (cfg.SavedJQLCount == 0)
            {
                ConsoleUtil.WriteError("You don't have any saved JQL",pause:true);
                return;
            }
            ViewSavedJQL(cfg,false);
            var tbl = new Table();
            tbl.AddColumns("jqlId","jqlName","jql");
            foreach (var jqlCfg in cfg.SavedJQL)
            {
                tbl.AddRow(new Text[]{
                    new Text($"[bold blue on lightyellow3]{jqlCfg.jqlId}[/]"), 
                    new Text($"[bold]{jqlCfg.jqlName}[/]"), 
                    new Text($"[dim]{jqlCfg.jql}[/]")                    
                    });
            }
            var delId = ConsoleUtil.GetInput<int>("Enter the jqlId you wish to delete");
            if (delId > 0)
            {
                var delItem = cfg.SavedJQL.FirstOrDefault(x=>x.jqlId == delId);
                if (ConsoleUtil.Confirm($"[bold]Delete[/] saved JQL: {delItem.jqlId:00} - {delItem.jqlName}?",true))
                {
                    cfg.DeleteJQL(delItem);
                    ConsoleUtil.WaitWhileSimple("Saving config file",JTISConfigHelper.SaveConfigList);
                }
            }
        }
        public static void ViewSavedJQL(JTISConfig cfg, bool pause = true)
        {
            if (cfg.SavedJQLCount == 0)
            {
                ConsoleUtil.WriteMarkupLine("You don't have any saved JQL",StdLine.slError.CStyle());
            }
            else 
            {
                var codeStyle = ConsoleUtil.StdStyle(StdLine.slCode);
                var colNameStyle = ConsoleUtil.StdStyle(StdLine.slOutputTitle);

                var tbl = new Table();
                tbl.AddColumns(
                    new TableColumn($"[bold blue on white]jqlId[/]").Centered(), 
                    new TableColumn($"[bold]jqlName[/]").Width(25).Centered(), 
                    new TableColumn($"jql")).LeftAligned();
                tbl.Border(TableBorder.Rounded);
                tbl.BorderColor(AnsiConsole.Foreground);
                tbl.Expand();
                // tbl.Columns[0].Padding(1,1,1,1);
                // tbl.Columns[1].Padding(1,1,1,1);
                // tbl.Columns[2].Padding(1,1,1,1);

                

                foreach (var jql in cfg.SavedJQL)
                {   
                    var tRow = new TableRow(
                        new Markup[]{
                            new Markup($"[blue on white]{jql.jqlId}[/]"),
                            new Markup($"{jql.jqlName}"), 
                            new Markup($"[dim]{jql.jql}[/]")}
                            );
                    tbl.AddRow(tRow);
                    tbl.AddEmptyRow();
                    

                    // ).BorderColor(AnsiConsole.Foreground);

                    // var _jql = new Panel(Markup.FromInterpolated($"[dim]JQL/Issue List: [/][italic]{jql.jql}[/]"));
                    // _jql.Header($"|  {jql.jqlId:00} - Saved as: {jql.jqlName}  |",Justify.Left);
                    // _jql.Border(BoxBorder.None);
                    // // _jql.Border = BoxBorder.Rounded;
                    // // _jql.BorderColor(Style.Parse("dim blue").Foreground);
                    // _jql.Padding(2,2,2,0);
                    // _jql.Expand();
                    // _jql.SafeBorder();
                    // AnsiConsole.Write(_jql);
                }
                AnsiConsole.Write(tbl);
                if (pause)
                {
                    ConsoleUtil.PressAnyKeyToContinue($"{cfg.SavedJQLCount} results");
                }
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