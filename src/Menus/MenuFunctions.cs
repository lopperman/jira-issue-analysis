using Spectre.Console;

namespace JTIS.Menu
{

    public enum VisualSnapshotType
    {
        vsIssueStatus = 1, 
        vsOpenedClosed = 2, 
        vsBlockers = 3
    }
    public enum MenuEnum
    {
        meMain = 1, 
        meConfig, 
        meDev, 
        meChangeLog, 
        meIssue_States, 
        meStatus_Config, 
        meJQL, 
        meAdvanced_Search, 
        meIssue_Summary_Visualization, 
        meIssue_Notes
    }
    public enum MenuItemEnum
    {
        miMenuGroupHeader = -2, 
        miSeparator = -1, 
        miMenu_Main = 0, 
        miMenu_Config, 
        miMenu_Advanced_Search, 
        miMenu_IssueStates, 
        miMenu_Issue_Summary_Visualization, 
        miMenu_Issue_Notes, 
        miIssue_Summary_Visualization, 
        miIssue_Summary_Visualization_Epic, 
        miMenu_StatusConfig, 
        miMenu_JQL, 
        miMenu_Dev, 
        miMenu_Change_Log,
        miIssueNotesView, 
        miIssueNotesAdd, 
        miIssueNotesDelete, 
        miDev1, 
        miDev2, 
        miChangeTimeZoneDisplay, 
        miExit, 
        miShowChangeHistoryCards, 
        miShowChangeHistoryEpics, 
        miChangeConnection, 
        miTISIssues, 
        miTISEpic, 
        miJiraConfigAdd, 
        miJiraConfigView, 
        miJiraConfigRemove, 
        miJiraServerInfo, 
        miSavedJQLView, 
        miSavedJQLAdd,         
        miSavedJQLFind, 
        miSavedJQLRemove,
        miSavedJQLDefault, 
        miCheckJQLStatement, 
        miIssCfgView,
        miIssCfgEdit,
        miIssCfgReset, 
        miSaveSessionToFile, 
        miStartRecordingSession,
        miAdvSearchViewCustomFields, 
        miAdvSearchViewIssueFields
        
    }


    public class MenuFunction 
    {
        // private Func<object>? theFunc;

        public string MenuName {get;private set;}
        public string? MenuNameMarkup {get; private set;}
        public MenuItemEnum MenuItem {get; private set;}
        public bool Disabled {get;set;}

        public MenuFunction()
        {
            MenuName = string.Empty;
            MenuNameMarkup = string.Empty;
            
        }

        public static MenuFunction Separator
        {
            get
            {
                var mf = new MenuFunction();
                mf.MenuName = $"\t\t--- --- ---";
                mf.MenuItem = MenuItemEnum.miSeparator;
                mf.MenuNameMarkup = $"[dim]{mf.MenuName}[/]";
                mf.Disabled = true;
                return mf;
            }
        }

        public static MenuFunction GroupHeader(string menuTitle)
        {
            var mf = new MenuFunction();
            mf.MenuName = Markup.Remove(menuTitle);
            mf.MenuItem = MenuItemEnum.miMenuGroupHeader;
            Color menuForecolor = AnsiConsole.Background;
            Color menuBackcolor = AnsiConsole.Foreground;
            mf.MenuNameMarkup = $"[dim]{mf.MenuName}[/]";
            mf.Disabled = true;
            return mf;
        }
        public MenuFunction(MenuItemEnum menuItem, string menuTitle, string menuTitleMarkup, bool dimItem = false, string? emoji = null)
        {
            if (menuItem == MenuItemEnum.miSeparator)
            {
                MenuName = $"\t\t--- --- ---";
                MenuItem = MenuItemEnum.miSeparator;
                MenuNameMarkup = $"[dim]{MenuName}[/]";
                Disabled = true;                
                return;
            } 
            else if (menuItem == MenuItemEnum.miMenuGroupHeader)
            {
                var gh = MenuFunction.GroupHeader(menuTitle);
                MenuName=gh.MenuName;
                menuItem=gh.MenuItem;
                Disabled=gh.Disabled;
                MenuNameMarkup = gh.MenuNameMarkup;
                return;
            }
            MenuName = menuTitle;
            if (dimItem == true)
            {
                MenuNameMarkup = $"[dim]{menuTitleMarkup}[/]";
            }
            if (MenuManager.IsMenu(menuItem))
            {
                emoji = Emoji.Known.Llama;
                var title = MenuName.Replace("Menu:","").Trim();
                var markupTitle = $"[bold]MENU: [/]{title.Trim().ToUpper()}";                
                menuTitleMarkup = markupTitle;
            }
            if (emoji == null){emoji = Emoji.Known.SmallBlueDiamond;}
            if (emoji != null){emoji = $"{emoji}  ";}
            MenuNameMarkup = $"{emoji ?? string.Empty}{menuTitleMarkup}";
            MenuItem = menuItem;
        }

        public override string ToString()
        {
            return MenuNameMarkup ?? MenuName;
        }
    }




}