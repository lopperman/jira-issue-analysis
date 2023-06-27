using System.Reflection.Emit;




using System;
using System.Globalization;
using Spectre.Console;

namespace JiraCon
{

    public enum VisualSnapshotType
    {
        vsProject = 1, 
        vsEpic = 2, 
        vsCustom = 3
    }
    public enum MenuEnum
    {
        meMain = 1, 
        meConfig, 
        meDev, 
        meIssue_States, 
        meStatus_Config, 
        meJQL
    }
    public enum MenuItemEnum
    {
        miSeparator = 0, 
        miMenu_Main = 1, 
        miMenu_Config, 
        miMenu_Dev, 
        miMenu_IssueStates, 
        miMenu_StatusConfig, 
        miMenu_JQL, 
        miChangeTimeZoneDisplay, 
        miVisualSnapshotAll, 
        miExit, 
        miShowChangeHistoryCards, 
        miShowChangeHistoryJQL, 
        miShowJSONCards, 
        miChangeConnection, 
        miTISIssues, 
        miTISEpic, 
        miTISJQL, 
        miJiraConfigAdd, 
        miJiraConfigView, 
        miJiraConfigRemove, 
        miJiraServerInfo, 
        miDev1, 
        miDev2, 
        miSavedJQLView, 
        miSavedJQLAdd,         
        miSavedJQLFind, 
        miSavedJQLRemove,
        miIssCfgView,
        miIssCfgEdit,
        miIssCfgReset, 
        miSaveSessionToFile, 
        miStartRecordingSession
    }


    public class MenuFunction 
    {
        // private Func<object>? theFunc;

        public string MenuName {get;private set;}
        public string? MenuNameMarkup {get; private set;}
        public MenuItemEnum MenuItem {get; private set;}

        public MenuFunction()
        {
            MenuName = string.Empty;
            MenuNameMarkup = string.Empty;
            
        }

        public MenuFunction(MenuItemEnum menuItem, string menuTitle, string menuTitleMarkup, bool dimItem = false, string? emoji = null)
        {
            MenuName = menuTitle;
            if (dimItem == true)
            {
                MenuNameMarkup = $"[dim]{menuTitleMarkup}[/]";
            }
            if (emoji == null){emoji = Emoji.Known.SmallBlueDiamond;}
            if (emoji != null){emoji = $"{emoji}  ";}
            MenuNameMarkup = $"{emoji ?? string.Empty}{menuTitleMarkup}";
            if (MenuNameMarkup.ToLower().Contains("menu:"))
            {
                var nwMain = "Menu:";
                MenuNameMarkup = MenuNameMarkup.Replace("Menu:",$"[bold]{nwMain}[/]",ignoreCase:true,CultureInfo.InvariantCulture);
            }
            MenuItem = menuItem;
        }

        public override string ToString()
        {
            return MenuNameMarkup ?? MenuName;
        }
    }




}