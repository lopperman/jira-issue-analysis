



using System;
using System.Globalization;
using Spectre.Console;

namespace JiraCon
{

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
        miExit, 
        miShowChangeHistoryCards, 
        miShowJSONCards, 
        miChangeConnection, 
        miTISIssueSummary, 
        miTISIssues, 
        miTISEpic, 
        miTISJQL
    }

    public static class MenuFunctions
    {

        

        //returns string that can be used with IRenderer for Font Color


        // public static void Start(JTISConfig cfg)
        // {
        //     while (DoMenu(new MenuMain(cfg)))
        //     {

        //     }
        // }
        // public static bool DoMenu(IMenuConsole menu)
        // {
        //     if (menu.ActiveConfig != JTISConfigHelper.config )
        //     {
        //         menu.ActiveConfig = JTISConfigHelper.config;
        //     }
        //     while (menu.DoMenu())
        //     {

        //     }
        //     return false;
        // }
    }

    public class MenuFunction 
    {
        private Func<object>? theFunc;

        public string MenuName {get;private set;}
        public string? MenuNameMarkup {get; private set;}
        public string? MenuDescription {get; private set;}
        public MenuItemEnum MenuItem {get; private set;}

        public MenuFunction(MenuItemEnum menuItem, string menuTitle, string? menuTitleMarkup = null, string? menuDesc = null)
        {
            MenuName = menuTitle;
            MenuNameMarkup = menuTitleMarkup;
            MenuDescription = menuDesc;
            MenuItem = menuItem;
        }

        // public void AssignFunc<T>(Func<object> onExecute)
        // {
        //     theFunc = onExecute;            
        // }

        public override string ToString()
        {
            return MenuNameMarkup ?? MenuName;
        }
    }




}