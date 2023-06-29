using System.Resources;
using System.Net.Http.Headers;
using System.IO.Pipes;




using Spectre.Console;
using JTIS.Config;
using JTIS.Console;
using JTIS.ManagedObjects;

namespace JTIS.Menu
{
    public interface IMenuConsole
    {        
        public JTISConfig ActiveConfig {get; set;}
        public bool DoMenu();
        public bool ProcessKey(ConsoleKey key);        
    }


    public static class MenuManager
    {
        private static MenuEnum? exitMenu;        
        private static MenuEnum lastMenu = MenuEnum.meMain;
        private static MenuFunction menuSeparator = MakeMenuDetail(MenuItemEnum.miSeparator,string.Format("{0}{0}{0}{0}{0}{0}{0}",Emoji.Known.WavyDash),Emoji.Known.WavyDash);
//        private static MenuFunction menuSeparator = MakeMenuDetail(MenuItemEnum.miSeparator,string.Format("Connect to different Jira",Emoji.Known.WavyDash),Emoji.Known.WavyDash);
     
        public static void Execute(MenuFunction item, MenuEnum? returnToMenu = null)
        {
            //MenuEnum finalMenu = returnToMenu ?? MenuEnum.meMain;
            
            exitMenu = null;
            if (returnToMenu != null)
            {
                exitMenu = returnToMenu;
            }

            if (item.MenuItem == MenuItemEnum.miSeparator)
            {
                ShowMenu(lastMenu);
                return;
            }

            
            switch (item.MenuItem)
            {
                case MenuItemEnum.miMenu_Main:
                    exitMenu = MenuEnum.meMain;
                    break;
                case MenuItemEnum.miMenu_Config:
                    exitMenu = MenuEnum.meConfig;
                    break;
                // case MenuItemEnum.miMenu_Dev:
                //     finalMenu = MenuEnum.meDev;
                //     break;
                case MenuItemEnum.miMenu_IssueStates:
                    exitMenu = MenuEnum.meIssue_States;
                    break;
                case MenuItemEnum.miMenu_JQL:
                    exitMenu = MenuEnum.meJQL;
                    break;
                case MenuItemEnum.miMenu_StatusConfig:
                    exitMenu = MenuEnum.meStatus_Config;
                    break;
                case MenuItemEnum.miMenu_Advanced_Search:
                    exitMenu = MenuEnum.meAdvanced_Search;
                    break;

                case MenuItemEnum.miSeparator:
                    exitMenu = lastMenu;
                    break;
//  ADVANCED SEARCH
                case MenuItemEnum.miAdvSearchViewCustomFields:
                    AdvancedSearch.Create().ViewJiraCustomFields();
                    exitMenu = MenuEnum.meAdvanced_Search;
                    break; 
                case MenuItemEnum.miAdvSearchViewIssueFields:
                    AdvancedSearch.Create().ViewJiraIssueFields();
                    exitMenu = MenuEnum.meAdvanced_Search;
                    break; 

                // case MenuItemEnum.miDev1:
                //     var menuDev1 = new MenuDev(JTISConfigHelper.config);
                //     menuDev1.DevTest1();
                //     ConsoleUtil.PressAnyKeyToContinue();
                //     break;
                // case MenuItemEnum.miDev2:
                //     var menuDev2 = new MenuDev(JTISConfigHelper.config);
                //     menuDev2.DevTest2();
                //     ConsoleUtil.PressAnyKeyToContinue();
                //     break;
/////////////////////////////////////
                case MenuItemEnum.miIssCfgEdit:
                    IssueStatesUtil.EditIssueStatus();
                    if (exitMenu == null){exitMenu = MenuEnum.meStatus_Config;}
                    break;

                case MenuItemEnum.miChangeTimeZoneDisplay:
                    JTISConfigHelper.ChangeTimeZone();
                    if (exitMenu == null){exitMenu = MenuEnum.meConfig;}
                    break;
                case MenuItemEnum.miVisualSnapshotAll:
                    if (exitMenu == null){exitMenu = MenuEnum.meMain;}
                    ConsoleUtil.PressAnyKeyToContinue("IN DEVELOPMENT");
                    var visAll = new VisualSnapshot(VisualSnapshotType.vsProject);
                    break;
                case MenuItemEnum.miExit:
                    exitMenu = null;                    
                    ConsoleUtil.ByeByeForced();
                    break;
                case MenuItemEnum.miShowChangeHistoryCards:
                    var chl1 = new ChangeLogsMgr(AnalysisType.atIssues);
                    if (exitMenu == null){exitMenu = MenuEnum.meMain;}
                    break;
                case MenuItemEnum.miIssCfgView:
                    ViewIssueConfig();
                    if (exitMenu == null){exitMenu = MenuEnum.meIssue_States;}
                    break;
                case MenuItemEnum.miTISIssues:
                    NewAnalysis(AnalysisType.atIssueSummary);
                    if (exitMenu == null){exitMenu = MenuEnum.meIssue_States;}
                    break;
                case MenuItemEnum.miJiraConfigView:
                    JTISConfigHelper.ViewAll();
                    if (exitMenu == null){exitMenu = MenuEnum.meConfig;}
                    if (ConsoleUtil.Confirm("SHOW API KEYS?",false))
                    {
                        JTISConfigHelper.ViewAll(true);
                        ConsoleUtil.PressAnyKeyToContinue();
                    }
                    break;
                case MenuItemEnum.miStartRecordingSession:
                    if (exitMenu == null){exitMenu = MenuEnum.meConfig;}
                    ConsoleUtil.StartRecording();
                    break;
                case MenuItemEnum.miJiraServerInfo:
                    JEnvironmentConfig.JiraEnvironmentInfo();
                    if (exitMenu == null){exitMenu = MenuEnum.meConfig;}
                    break;
                case MenuItemEnum.miSaveSessionToFile:
                    if (exitMenu == null){exitMenu = MenuEnum.meConfig;}
                    if (ConsoleUtil.IsConsoleRecording)
                    {
                        var fName = ConsoleUtil.SaveSessionFile();
                        ConsoleUtil.PressAnyKeyToContinue($"Saved to: {fName}");
                    }
                    else 
                    {
                        if (ConsoleUtil.Confirm($"Recording is not turned on. Turn on now?",defResp:false))
                        {
                            ConsoleUtil.StartRecording();
                        }
                    }
                    break;
/// SAVED JQL ///                    
                case MenuItemEnum.miSavedJQLView:
                    JQLUtil.ViewSavedJQL(JTISConfigHelper.config);
                    if (exitMenu == null){exitMenu = MenuEnum.meJQL;}
                    break;
                case MenuItemEnum.miSavedJQLAdd:
                    JQLUtil.AddJQL();
                    if (exitMenu == null){exitMenu = MenuEnum.meJQL;}
                    break;
                case MenuItemEnum.miSavedJQLRemove:
                    JQLUtil.RemoveJQL(JTISConfigHelper.config);
                    if (exitMenu == null){exitMenu = MenuEnum.meJQL;}
                    break;
                case MenuItemEnum.miSavedJQLDefault:
                    JTISConfigHelper.CheckDefaultJQL();
                    ConsoleUtil.PressAnyKeyToContinue("Default JQL Verified/Added");
                    if (exitMenu == null){exitMenu = MenuEnum.meJQL;}
                    break;

/// (END) SAVED JQL ///
                case MenuItemEnum.miChangeConnection:
                    if (exitMenu == null) {exitMenu = MenuEnum.meMain;}
                    var newCfg = JTISConfigHelper.ChangeCurrentConfig("Choose a Jira Configuration, or 'ADD NEW'");
                    if (newCfg != null)
                    {
                        JTISConfigHelper.config = newCfg;
                        ConsoleUtil.PressAnyKeyToContinue($"CONNECTED TO: {JTISConfigHelper.config.ToString()}");
                    }

                break;
                default:
                    string miName = Enum.GetName(typeof(MenuItemEnum),item.MenuItem);
                    AnsiConsole.Write(new Rule());
                    AnsiConsole.Write(new Rule($"[{Color.DarkRed.ToString()} on {Color.LightYellow3.ToString()}] a handler for '{miName}' does not exist, reverting to main menu [/]"));
                    AnsiConsole.Write(new Rule());
                    ConsoleUtil.PressAnyKeyToContinue();
                    exitMenu = MenuEnum.meMain;
                    break;
            }
            if (exitMenu == null)
            {
                exitMenu = MenuEnum.meMain;
            }
            ShowMenu(exitMenu.Value);
        }


        private static void NewAnalysis(AnalysisType anType)
        {
            AnalyzeIssues analyze = new AnalyzeIssues(anType);
            int issueCount = 0;
            if (analyze.HasSearchData)
            {
                try 
                {
                    issueCount = analyze.GetData();
                }
                catch 
                {
                    ConsoleUtil.PressAnyKeyToContinue("NO ISSUES WERE RETURNED");
                }
                if (analyze.GetDataFail)
                {
                    ConsoleUtil.PressAnyKeyToContinue();
                }
            } 
            if (issueCount > 0)
            {                
                analyze.ClassifyStates();                
                analyze.WriteToConsole();
            }
        }

        private static void ViewIssueConfig()
        {

            JTISConfigHelper.UpdateDefaultStatusConfigs();

            AnsiConsole.Status()
                .Start($"Getting Latest Issue Status data from Jira ...", ctx=>
                {
                    ctx.Spinner(Spinner.Known.Dots);
                    ctx.SpinnerStyle(new Style(AnsiConsole.Foreground,AnsiConsole.Background));
                    Thread.Sleep(100);
                    ctx.Status("[italic]Writing Local Issue Status Config ...[/]");
                    IssueStatesUtil.WriteJiraStatuses();

                });
            ConsoleUtil.PressAnyKeyToContinue();                    

        }



        


        private static void CheckMinConsoleSize(int cWidth, int cHeight)
        {
            AnsiConsole.WriteLine("checking minimum console ");
            AnsiConsole.WriteLine($"Current Width: {System.Console.WindowWidth}");
            AnsiConsole.WriteLine($"Current Height: {System.Console.WindowHeight}");
            if (System.Console.WindowWidth < cWidth || System.Console.WindowHeight < cHeight)
            {
                try 
                {   
                    #if WINDOWS                 
                        Console.SetWindowSize(cWidth,cHeight);
                    #endif
                }
                catch
                {
                    ConsoleUtil.PressAnyKeyToContinue($"Please adjust size of console window  Width and Height to {cWidth} X {cHeight} for the best experience");
                }
            }

        }
        public static void ShowMenu(MenuEnum menu)
        {

            lastMenu = menu;
            BuildMenuPanel(menu);
            List<MenuFunction> menuItems = BuildMenuItems(menu);
            if (menuItems.Count > 0)
            {
                var sp = new SelectionPrompt<MenuFunction>();
                
                sp.PageSize = 16;
                sp.AddChoices(menuItems);
                if (menu == MenuEnum.meMain)
                {
                    sp.AddChoiceGroup(
                            menuSeparator, 
                            new MenuFunction(MenuItemEnum.miChangeConnection,"Connect to other Jira Site","[dim]Connect to other Jira Site[/]"),
                            new MenuFunction(MenuItemEnum.miExit,"Exit App","[dim bold]Exit App[/]",true,Emoji.Known.SmallOrangeDiamond));
                }
                else 
                {
                    sp.AddChoiceGroup(
                            menuSeparator, 
                            new MenuFunction(MenuItemEnum.miMenu_Main,"Back to Main Menu","Back to [bold]Main Menu[/]"),
                            new MenuFunction(MenuItemEnum.miChangeConnection,"Connect to different Jira","[dim]Connect to other Jira Site[/]"),
                            new MenuFunction(MenuItemEnum.miExit,"Exit App","[dim bold]Exit App[/]",true,Emoji.Known.SmallOrangeDiamond));

                }                    
                var mnu = AnsiConsole.Prompt(sp);
                MenuManager.Execute(mnu);            
            }
            else 
            {
                string miName = Enum.GetName(typeof(MenuEnum),menu);
                AnsiConsole.Write(new Rule());
                AnsiConsole.Write(new Rule($"[{Color.DarkRed.ToString()} on {Color.LightYellow3.ToString()}] a handler for '{miName}' does not exist, reverting to main menu [/]"));
                AnsiConsole.Write(new Rule());
                ConsoleUtil.PressAnyKeyToContinue();
                ShowMenu(MenuEnum.meMain);
            }

        }

        private static void BuildMenuPanel(MenuEnum menu)
        {
            AnsiConsole.Clear();
            var menuName = Enum.GetName(typeof(MenuEnum),menu).Replace("me","").Replace("_"," ");

            var menuLabel = $"[bold black on lightyellow3]{Emoji.Known.DiamondWithADot} {menuName} Menu [/]| [dim italic]Connected: {JTISConfigHelper.config.ToString()}[/]";  


            var title = $"JIRA Time In Status :llama: [dim]by[/] [dim link=https://github.com/lopperman/jira-issue-analysis]Paul Brower[/]{ConsoleUtil.RecordingInfo}{ConsoleUtil.TimeZoneAlert}{Environment.NewLine}{menuLabel}";
            var panel = new Panel(title);
            panel.Border = BoxBorder.Rounded;
            panel.BorderColor(Color.Grey15);
            panel.Expand = true;
            AnsiConsole.Write(panel);
        }

        private static MenuFunction MakeMenuDetail(MenuItemEnum mi, string title, string? emojiFront = null)
        {
            var miFore = StdLine.slMenuDetail.FontMkp();
            var miBack = StdLine.slMenuDetail.BackMkp();
            string plainTitle = Markup.Remove(title);
            string markupTitle = string.Empty;
            markupTitle = $"[{miFore} on {miBack}]{plainTitle}[/]";
            return new MenuFunction(mi,plainTitle,markupTitle,emoji:emojiFront);
        }

        public static List<MenuFunction> BuildMenuItems(MenuEnum menu)
        {
            var ret = new List<MenuFunction>();
            
            var miFore = StdLine.slMenuDetail.FontMkp();
            var miBack = StdLine.slMenuDetail.BackMkp();

            switch (menu)
            {

//  MAIN MENU //

                case (MenuEnum.meMain):
                    ret.Add(MakeMenuDetail(MenuItemEnum.miMenu_IssueStates,"Menu: Issue Analysis"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miVisualSnapshotAll,"Project Summary Visualization"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miShowChangeHistoryCards,"Issue Change-Logs "));
                    ret.Add(menuSeparator);
                    ret.Add(MakeMenuDetail(MenuItemEnum.miMenu_Config,"Menu: Configuration"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miMenu_Advanced_Search,"Menu: Advanced Search" ));
                    // ret.Add(MakeMenuDetail(MenuItemEnum.miDev1,"DEV TEST 1"));
                    // ret.Add(MakeMenuDetail(MenuItemEnum.miDev2,"DEV TEST 2"));
                break;

//  CONFIG MENU //

                case(MenuEnum.meConfig):
                    ret.Add(MakeMenuDetail(MenuItemEnum.miMenu_JQL,"Menu: Manage Saved JQL"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miChangeTimeZoneDisplay,"Change Displayed Time Zone"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miJiraConfigAdd,"Add New Jira Connection"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miJiraConfigView,"View Configured Jira Profiles"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miJiraConfigRemove,"Remove Jira Connection"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miJiraServerInfo,$"View Jira Server Info"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miStartRecordingSession,"Start session recording"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miSaveSessionToFile,"Save session to file"));
                break;

//  ISSUE STATES MENU //

                case(MenuEnum.meIssue_States):
                    ret.Add(MakeMenuDetail(MenuItemEnum.miTISIssues,"Get Issue(s) Data"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miTISEpic,"Get Issue(s) Data by Epic"));
                    ret.Add(menuSeparator);
                    ret.Add(MakeMenuDetail(MenuItemEnum.miMenu_StatusConfig,"Menu: Issue Status Config"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miMenu_Config,"Menu: Configuration"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miMenu_JQL,"Menu: Manage Saved JQL"));
                break;

//  MANAGE JQL MENU //

                case(MenuEnum.meJQL):
                    ret.Add(MakeMenuDetail(MenuItemEnum.miSavedJQLView,"View Saved JQL / Issue Numbers"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miSavedJQLAdd,"Save New JQL / Issue Numbers"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miSavedJQLFind,"Find Saved JQL / Issue Numbers"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miSavedJQLRemove,"Remove Saved JQL / Issue Numbers"));
                    ret.Add(menuSeparator);
                    ret.Add(MakeMenuDetail(MenuItemEnum.miSavedJQLDefault,"Verify/Add Default JQL Snippets"));

                break;

//  ISSUE STATUS CONFIG MENU //

                case(MenuEnum.meStatus_Config):
                    ret.Add(MakeMenuDetail(MenuItemEnum.miIssCfgView,"View Issue Status Config"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miIssCfgEdit,"Edit Local Issue Status Config"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miIssCfgReset,"Reset Local Issue Status Config to Match Jira"));
                break;

//  ADVANCED SEARCH MENU //

                case(MenuEnum.meAdvanced_Search):
                    ret.Add(MakeMenuDetail(MenuItemEnum.miAdvSearchViewCustomFields,"View Jira Custom Field Info"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miAdvSearchViewIssueFields,"View Jira Issues Field Info"));
                break;

            }            

            return ret;
        }


/* OLD MENU STUFF BELOW  OLD MENU STUFF BELOW  OLD MENU STUFF BELOW  OLD MENU STUFF BELOW  OLD MENU STUFF BELOW  OLD MENU STUFF BELOW */ 
/* OLD MENU STUFF BELOW  OLD MENU STUFF BELOW  OLD MENU STUFF BELOW  OLD MENU STUFF BELOW  OLD MENU STUFF BELOW  OLD MENU STUFF BELOW */ 
/* OLD MENU STUFF BELOW  OLD MENU STUFF BELOW  OLD MENU STUFF BELOW  OLD MENU STUFF BELOW  OLD MENU STUFF BELOW  OLD MENU STUFF BELOW */ 


        public static void Start(JTISConfig cfg)
        {
            ShowMenu(MenuEnum.meMain);
            // while (DoMenu(new MenuMain(cfg)))
            // {

            // }
        }
        public static bool DoMenu(IMenuConsole menu)
        {
            // if (menu.ActiveConfig != JTISConfigHelper.config )
            // {
            //     menu.ActiveConfig = JTISConfigHelper.config;
            // }
            // while (menu.DoMenu())
            // {

            // }
            return false;
        }


    }
}