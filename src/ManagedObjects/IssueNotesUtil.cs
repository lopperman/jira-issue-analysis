using System.Security.Cryptography;
using JTIS.Config;
using JTIS.Console;
using Spectre.Console;

namespace JTIS
{
    public static class IssueNotesUtil
    {
        public static void View(bool pause = true)
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
                if (pause)
                {
                    ConsoleUtil.PressAnyKeyToContinue();
                }

            }
        }

        internal static void AddEdit(string? issKey=null, bool clearScreen = true)
        {            
            if (clearScreen)
            {
                ConsoleUtil.WriteAppTitle();
            }
            AnsiConsole.Write(new Rule());
            AnsiConsole.MarkupLine($"[italic]A [underline]single note, stored in a local configuration file[/], can be added to any Jira Issue Key, and may be included for reference in various areas of this application.[/]");
            AnsiConsole.MarkupLine($"[italic]Enter the Jira Issue number to add/replace note. If you enter the numeric portion only, the default project ('{CfgManager.config.defaultProject}') will automatically be prepended[/].");
            string tmpKey = string.Empty;
            if (issKey==null)
            {
                tmpKey = ConsoleUtil.GetInput<string>("Issue Key",allowEmpty:true);
                tmpKey=tmpKey.Trim();
                if (tmpKey.Length==0) {return;}
                if (!tmpKey.Contains("-",StringComparison.OrdinalIgnoreCase))
                {
                    tmpKey=$"{CfgManager.config.defaultProject}-{tmpKey.Trim()}";
                }
            }
            else 
            {
                tmpKey = issKey;
            }

            bool hasNote = CfgManager.config.issueNotes.HasNote(tmpKey);
            string tmpNote = hasNote ? CfgManager.config.issueNotes.GetNote(tmpKey) : string.Empty;
            tmpNote=ConsoleUtil.GetInput<string>($"Enter/edit note to be associated with issue '{tmpKey}'",tmpNote,true);
            if (tmpNote.Trim().Length == 0)
            {
                ConsoleUtil.PressAnyKeyToContinue("OPERATION CANCELLED");
            }
            else 
            {
                CfgManager.config.issueNotes.CreateNote(tmpKey,tmpNote);
                CfgManager.SaveConfigList();
                ConsoleUtil.PressAnyKeyToContinue($"Note has been changed on issue '{tmpKey}' to: {tmpNote}");
            }
        }

        internal static void Delete()
        {
            View(false);
            AnsiConsole.Write(new Rule());
            var tmpKey = ConsoleUtil.GetInput<string>("Enter Jira Issue Key to delete note");
            tmpKey=tmpKey.Trim();
            if (tmpKey.Length==0) {return;}
            if (!tmpKey.Contains("-",StringComparison.OrdinalIgnoreCase))
            {
                tmpKey=$"{CfgManager.config.defaultProject}-{tmpKey.Trim()}";
            }
            bool hasNote = CfgManager.config.issueNotes.HasNote(tmpKey);
            if (!hasNote)
            {
                ConsoleUtil.PressAnyKeyToContinue($"There are no notes for '{tmpKey}'");
            }
            else 
            {
                if (ConsoleUtil.Confirm($"Delete note for '{tmpKey}'?",true))
                {
                    CfgManager.config.issueNotes.DeleteNote(tmpKey);
                    CfgManager.SaveConfigList();
                    ConsoleUtil.PressAnyKeyToContinue($"Note has been removed for '{tmpKey}'");
                }
            }


        }
    }
}