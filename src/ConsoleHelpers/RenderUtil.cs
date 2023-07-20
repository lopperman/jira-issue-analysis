using JTIS.Config;
using JTIS.Console;
using JTIS.Data;
using Spectre.Console;

namespace JTIS.Analysis;

public static class RenderUtil 
{

    public static void  WriteIssueHeaderStyle1(jtisIssue jtisIss, int itemIdx, int itemCount, bool clearScreen = true)
    {
        if (clearScreen){AnsiConsole.Clear();}
        var currentlyBlocked = jtisIss.jIssue.IsBlocked;
        string formattedStartDt = string.Empty;
        jtisStatus? firstActive = jtisIss.StatusItems.FirstActive;                
        if (firstActive == null){
            formattedStartDt = "[dim] ACTIVE WORK HAS NOT STARTED[/]";
        }
        else {
            formattedStartDt = string.Format("[dim] ACTIVE WORK STARTED:[/][bold] {0} [/]",firstActive.FirstEntryDate.CheckTimeZone());
        }
        // FIRST SUMMARY 'RULE' LINE
        if (JTISTimeZone.DefaultTimeZone==false)
        {
            AnsiConsole.Write(new Rule(ConsoleUtil.TimeZoneAlert));
        }
        AnsiConsole.Write(new Rule($"[dim]({itemIdx+1:000} of {itemCount:#000} results)[/]"){Style=new Style(Color.Blue,Color.Cornsilk1), Justification=Justify.Center});
        if (currentlyBlocked)
        {
            var blockedOnDesc = string.Empty;
            if (jtisIss.Blockers.Blockers.Count() > 0){
                blockedOnDesc = $" - BLOCKED ON {jtisIss.Blockers.Blockers.Max(x=>x.StartDt).ToString()}";
            }
            AnsiConsole.Write(new Rule($"[bold](THIS ISSUE IS CURRENTLY BLOCKED{blockedOnDesc})[/]").NoBorder().LeftJustified().RuleStyle(new Style(Color.DarkRed_1,Color.Cornsilk1)));
        }
        AnsiConsole.Write(new Rule($"[dim]({jtisIss.jIssue.IssueType.ToUpper()}) [/][bold]{jtisIss.jIssue.Key}[/][dim], DESC:[/] {Markup.Escape(ConsoleUtil.Scrub(jtisIss.jIssue.Summary))}").NoBorder().LeftJustified().RuleStyle(new Style(Color.Blue,Color.Cornsilk1)));

        AnsiConsole.Write(new Rule($"[dim]Current Status:[/][bold] ({jtisIss.jIssue.StatusName.ToUpper()})[/]{formattedStartDt}").NoBorder().LeftJustified().RuleStyle(new Style(Color.Blue,Color.Cornsilk1)));

        // LAST SUMMARY 'RULE' LINE
        AnsiConsole.Write(new Rule(){Style=new Style(Color.Blue,Color.Cornsilk1), Justification=Justify.Center});          
        if (CfgManager.config.issueNotes.HasNote(jtisIss.jIssue.Key))
        {
            AnsiConsole.MarkupLine($"[bold darkred_1 on cornsilk1]ISSUE NOTE: [/]{CfgManager.config.issueNotes.GetNote(jtisIss.jIssue.Key)}");
        }
      

    }



}