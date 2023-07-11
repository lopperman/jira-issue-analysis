using System.Security.Cryptography;
using JTIS.Config;
using JTIS.Console;
using Spectre.Console;

namespace JTIS
{
    public static class IssueNotesUtil
    {
        public static void View()
        {
            if (CfgManager.config.issueNotes.Count == 0)
            {
                ConsoleUtil.PressAnyKeyToContinue("YOU HAVE NOTE CREATED ANY ISSUE NOTES");
            }
            else 
            {
                var tbl = new Table();
                tbl.AddColumns("Issue Key", "Last Edit", "Note");
                foreach (var note in CfgManager.config.issueNotes.Notes)
                {
                    tbl.AddRow($"[bold]{note.Key}[/]",$"[dim]{note.LastEdit.ToString()}[/]",$"{note.Value}");
                }
                tbl.Border(TableBorder.Rounded);
                ConsoleUtil.WriteAppTitle();
                AnsiConsole.Write(new Panel(tbl));
                ConsoleUtil.PressAnyKeyToContinue();

            }
        }
    }
}