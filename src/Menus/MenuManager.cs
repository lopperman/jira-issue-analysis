using System.Reflection.Metadata;
using Spectre.Console;
using JTIS.Config;
using JTIS.Console;
using JTIS.ManagedObjects;
using JTIS.Analysis;
using JTIS.Extensions;
using JTIS.Data;


namespace JTIS
{
    public static class Info
    {
        public static bool IsDev
        {
            get{
                // return false;
                return Environment.UserName.StringsMatch("paulbrower");
            }
        }

        internal static void AppURLs()
        {
            ConsoleUtil.WriteAppTitle();
            ConsoleUtil.WriteBanner("Jira Issue Analysis URLs",Color.Blue);
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[dim] Right-click on URL to open link (if supported by your terminal)[/]");
            AnsiConsole.WriteLine();

            Markup mk = new Markup($"Project Wiki Page [link]https://github.com/lopperman/jira-issue-analysis/wiki[/]");
            AnsiConsole.Write(new Panel(mk));
            mk = new Markup($"Latest Release [link]https://github.com/lopperman/jira-issue-analysis/releases[/]");
            AnsiConsole.Write(new Panel(mk));
            mk = new Markup($"Discussion Area, Enhancement Requests [link]https://github.com/lopperman/jira-issue-analysis/discussions[/]");
            AnsiConsole.Write(new Panel(mk));
            ConsoleUtil.PressAnyKeyToContinue();
        }
    }
}

namespace JTIS.Menu
{
    
    public static class MenuManager
    {
        public static int MenuPageSize
        {
            get{
                int halfHeight = System.Console.WindowHeight / 2;
                return halfHeight;
            }
        }
        private static MenuEnum? exitMenu;        
        private static MenuEnum lastMenu = MenuEnum.meMain;
        private static MenuFunction menuSeparator = MenuFunction.Separator;

        private static MenuFunction menuGroupheader(string menuTitle)
        {
            return MenuFunction.GroupHeader(menuTitle);
        }
        public static IEnumerable<T> MultiSelect<T>(string title, List<T> choices, int? pageSize = null, bool required = false) where T:notnull
        {
            if (!pageSize.HasValue){pageSize = MenuPageSize;}

            ConsoleUtil.WriteAppTitle();
            var msp = new MultiSelectionPrompt<T>()
                .Title(title)
                .Required(required)
                .PageSize(pageSize.Value)
                .MoreChoicesText("[grey](Move up and down to reveal more choices)[/]")
                .InstructionsText(
                    "[grey](Press [blue]<space>[/] to toggle a choice, " + 
                    "[green]<enter>[/] to accept)[/]")
                .AddChoices(choices);
            var response = AnsiConsole.Prompt(msp);
            return response;
        }

        public static void Execute(MenuFunction item, MenuEnum? returnToMenu = null)
        {            
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
#region MENUS - MENU ITEMS                
                case MenuItemEnum.miMenu_Main:
                    exitMenu = MenuEnum.meMain;
                    break;
                case MenuItemEnum.miMenu_Config:
                    exitMenu = MenuEnum.meConfig;
                    break;
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
                case MenuItemEnum.miMenu_Issue_Summary_Visualization:
                    exitMenu = MenuEnum.meIssue_Summary_Visualization;
                    break;
                case MenuItemEnum.miMenu_Dev:
                    exitMenu = MenuEnum.meDev;
                    break;
                case MenuItemEnum.miMenu_Change_Log:
                    exitMenu = MenuEnum.meChangeLog;
                    break;
                case MenuItemEnum.miMenu_Issue_Notes:
                    exitMenu = MenuEnum.meIssue_Notes;
                    break;
                case MenuItemEnum.miMenu_Cached_Searches:
                    exitMenu = MenuEnum.meCached_Searches;
                    break;
#endregion
//miMenu_Issue_Summary_Visualization
#region ISSUE VISUALIZATION

                case MenuItemEnum.miIssue_Summary_Visualization:
                    if (exitMenu == null){exitMenu = MenuEnum.meIssue_Summary_Visualization;}
                    VisualSnapshot.Create(VisualSnapshotType.vsIssueStatus, AnalysisType.atIssues).Build();
                    break;

                case MenuItemEnum.miIssue_Summary_Visualization_Epic :
                    if (exitMenu == null){exitMenu = MenuEnum.meIssue_Summary_Visualization;}
                    VisualSnapshot.Create(VisualSnapshotType.vsIssueStatus, AnalysisType.atEpics).Build();
                    break;

                case MenuItemEnum.miIssue_Summary_Overall1:
                    if (exitMenu == null){exitMenu = MenuEnum.meIssue_Summary_Visualization;}
                    Overview1.Create(FetchOptions.DefaultFetchOptions.IncludeChangeLogs());
                    break;

                case MenuItemEnum.miIssue_Summary_Overall1_Epic :
                    if (exitMenu == null){exitMenu = MenuEnum.meIssue_Summary_Visualization;}
                    Overview1.Create(FetchOptions.DefaultFetchOptions.IncludeChangeLogs().FetchEpicChildren());
                    break;

#endregion

                case MenuItemEnum.miCachedSearch_ClearAll:
                    IssueFetcher.ClearCachedData();
                    exitMenu = MenuEnum.meCached_Searches;
                    break;
                case MenuItemEnum.miCachedSearch_View:
                    IssueFetcher.DisplayCachedResults();
                    exitMenu = MenuEnum.meCached_Searches;
                    break;


                case MenuItemEnum.miHelpfulURLs:
                    exitMenu = MenuEnum.meMain;
                    JTIS.Info.AppURLs();
                    break;

#region ADVANCED SEARCH MENUS


                case MenuItemEnum.miAdvSearchViewCustomFields:
                    AdvancedSearch.Create().ViewJiraCustomFields();
                    exitMenu = MenuEnum.meAdvanced_Search;
                    break;  
                case MenuItemEnum.miAdvSearchViewIssueFields:
                    AdvancedSearch.Create().ViewJiraIssueFields();
                    exitMenu = MenuEnum.meAdvanced_Search;
                    break; 
#endregion

#region ISSUE NOTE MENU ITEMS

                case MenuItemEnum.miIssueNotesView:
                    IssueNotesUtil.View();
                    exitMenu = MenuEnum.meIssue_Notes;                    
                    break;
                case MenuItemEnum.miIssueNotesAdd:
                    IssueNotesUtil.AddEdit();
                    exitMenu = MenuEnum.meIssue_Notes;
                    break;
                case MenuItemEnum.miIssueNotesDelete:
                    IssueNotesUtil.Delete();
                    exitMenu = MenuEnum.meIssue_Notes;
                    break;
#endregion


#region DEV MENU ITEMS

                case MenuItemEnum.miDev1:
                    MenuManager.Dev1();
                    exitMenu = MenuEnum.meConfig;
                    break;
                case MenuItemEnum.miDev2:
                    MenuManager.Dev2();
                    exitMenu = MenuEnum.meConfig;
                    break;
                case MenuItemEnum.miDevScrubEdit:
                    JiraUtil.DevScrub();
                    exitMenu = MenuEnum.meConfig;
                    break;
#endregion


#region ISSUE STATES & CHANGE LOGS

                case MenuItemEnum.miShowChangeHistoryCards:
                    var chl1 = new ChangeLogsMgr(AnalysisType.atIssues);
                    if (exitMenu == null){exitMenu = MenuEnum.meMain;}
                    break;
                case MenuItemEnum.miShowChangeHistoryEpics:
                    var chl2 = new ChangeLogsMgr(AnalysisType.atEpics);
                    if (exitMenu == null){exitMenu = MenuEnum.meMain;}
                    break;
                case MenuItemEnum.miIssCfgView:
                    ViewIssueConfig(CfgManager.config.defaultProject);
                    if (exitMenu == null){exitMenu = MenuEnum.meIssue_States;}
                    break;
                case MenuItemEnum.miTISIssues:
                    AnalyzeIssues analyzeIssSum = new AnalyzeIssues(AnalysisType.atIssueSummary );
                    if (exitMenu == null){exitMenu = MenuEnum.meIssue_States;}
                    break;
                case MenuItemEnum.miTISEpic:
                    AnalyzeIssues analyzeEpics = new AnalyzeIssues(AnalysisType.atEpics);
                    if (exitMenu == null){exitMenu = MenuEnum.meIssue_States;}
                    break;
                case MenuItemEnum.miTISIssueTree:
                    IssueTree tree = new IssueTree();
                    if (exitMenu == null){exitMenu = MenuEnum.meIssue_Summary_Visualization;}
                    break;
                case MenuItemEnum.miTISCycleTime:
                    CycleTime ct = new CycleTime(AnalysisType.atIssues);
                    
                    if (exitMenu == null){exitMenu = MenuEnum.meIssue_Summary_Visualization;}
                    break;

                case MenuItemEnum.miIssCfgEdit:
                    IssueStatesUtil.EditIssueStatus();
                    if (exitMenu == null){exitMenu = MenuEnum.meIssue_States;}
                    break;

                case MenuItemEnum.miIssCfgSequence:
                    IssueStatesUtil.EditIssueSequence();
                    if (exitMenu == null){exitMenu = MenuEnum.meIssue_States;}
                    break;



#endregion                    
                    

#region JQL MANAGEMENT

                case MenuItemEnum.miSavedJQLView:
                    JQLUtil.ViewSavedJQL(CfgManager.config);
                    if (exitMenu == null){exitMenu = MenuEnum.meJQL;}
                    break;
                case MenuItemEnum.miSavedJQLFind:
                    JQLUtil.FindSavedJQLl();
                    if (exitMenu == null){exitMenu = MenuEnum.meJQL;}
                    break;
                case MenuItemEnum.miSavedJQLAdd:
                    JQLUtil.AddJQL();
                    if (exitMenu == null){exitMenu = MenuEnum.meJQL;}
                    break;
                case MenuItemEnum.miSavedJQLRemove:
                    JQLUtil.RemoveJQL(CfgManager.config);
                    if (exitMenu == null){exitMenu = MenuEnum.meJQL;}
                    break;
                case MenuItemEnum.miCheckJQLStatement:
                    JQLUtil.CheckManualJQL();
                    if (exitMenu == null){exitMenu = MenuEnum.meJQL;}
                    break;
                case MenuItemEnum.miSavedJQLDefault:
                    JQLUtil.CheckDefaultJQL(CfgManager.config);
                    ConsoleUtil.PressAnyKeyToContinue("Default JQL Verified/Added");
                    if (exitMenu == null){exitMenu = MenuEnum.meJQL;}
                    break;

#endregion

#region JIRA CONFIG MENU ITEMS

                case MenuItemEnum.miJiraConfigView:
                    CfgManager.ViewAll();
                    if (exitMenu == null){exitMenu = MenuEnum.meConfig;}
                    if (ConsoleUtil.Confirm("SHOW API KEYS?",false))
                    {
                        CfgManager.ViewAll(true);
                        ConsoleUtil.PressAnyKeyToContinue();
                    }
                    break;

                case MenuItemEnum.miJiraConfigAdd:
                    CfgManager.AddNewConfig();
                    if (exitMenu == null) {exitMenu = MenuEnum.meConfig;}
                break;

                case MenuItemEnum.miJiraConfigRemove:
                    if (exitMenu == null) {exitMenu = MenuEnum.meConfig;}
                    CfgManager.DeleteConfig();
                break;


#endregion

#region CONFIG MENU MANAGEMENT



                case MenuItemEnum.miChangeTimeZoneDisplay:
                    CfgManager.ChangeTimeZone();
                    if (exitMenu == null){exitMenu = MenuEnum.meConfig;}
                    break;

                case MenuItemEnum.miChangeConnection:
                    if (exitMenu == null) {exitMenu = MenuEnum.meMain;}
                    var newCfg = CfgManager.ChangeCurrentConfig();
                    if (newCfg != null)
                    {
                        CfgManager.config = newCfg;
                        ConsoleUtil.PressAnyKeyToContinue($"CONNECTED TO: {CfgManager.config.ToString()}");
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

#endregion                 

#region MISC MENU ITEMS

                case MenuItemEnum.miExit:
                    exitMenu = null;                    
                    ConsoleUtil.ByeByeForced();
                    break;
                    

                case MenuItemEnum.miSeparator:
                    exitMenu = lastMenu;
                    break;

                default:
                    string miName = Enum.GetName(typeof(MenuItemEnum),item.MenuItem);
                    AnsiConsole.Write(new Rule());
                    AnsiConsole.Write(new Rule($"[{Color.DarkRed.ToString()} on {Color.LightYellow3.ToString()}] a handler for '{miName}' does not exist, reverting to main menu [/]"));
                    AnsiConsole.Write(new Rule());
                    ConsoleUtil.PressAnyKeyToContinue();
                    exitMenu = MenuEnum.meMain;
                    break;
#endregion                    

            }
            if (exitMenu == null)
            {
                exitMenu = MenuEnum.meMain;
            }
            ShowMenu(exitMenu.Value);
        }
        private static void Dev2()
        {
            var slice = new SliceDice();
            slice.AddIssues(
                CfgManager.config.DefaultStatuses.Single(x=>x.StatusName.StringsMatch("in progress")),
                CfgManager.config.DefaultStatuses.Single(x=>x.StatusName.StringsMatch("ready for review")), 
                "Story", 16);
            

            ConsoleUtil.PressAnyKeyToContinue();
        }
        private static void Dev1()
        {
            var options = FetchOptions.DefaultFetchOptions;
            options.IncludeChangeLogs().AllowJQLSnippets(false).AllowCachedSelection(false).CacheResults(false);
            options.JQL = "project=WWT and status was in ('in progress') and status=done and issueType=story";
            var data = IssueFetcher.FetchIssues(options);
            SortedDictionary<string,List<double>> statusValues = new SortedDictionary<string, List<double>>();
            IEnumerable<double> issueValues = new List<double>();
            foreach (var iss in data.jtisIssuesList)
            {
                if (iss.StatusItems != null)
                {
                    var issDays = Math.Round(iss.StatusItems.IssueTotalActiveBusTime.TotalDays,0);    
                    if (issDays > 0)
                    {
                        issueValues = issueValues.Append(issDays);

                        foreach (var stat in iss.StatusItems.Statuses)
                        {
                            if (!statusValues.ContainsKey(stat.IssueStatus))
                            {
                                statusValues.Add(stat.IssueStatus,new List<double>());
                            }
                            statusValues[stat.IssueStatus].Add(Math.Round(stat.StatusBusinessTimeTotal.TotalDays,2));
                        }
                    }
                }
            }

            // var stdDevIssues = issueValues.StandardDeviation();

            AnsiConsole.WriteLine($"Issue Count: {issueValues.Count()}");
            AnsiConsole.WriteLine($"Min/Max: {issueValues.Min()} / {issueValues.Max()}");
            AnsiConsole.WriteLine($"Mean: {issueValues.Average()}");
            AnsiConsole.WriteLine($"StdDev: {issueValues.StandardDeviation()}");
            
            var tLower = issueValues.Average() - (issueValues.StandardDeviation());
            var tUpper = issueValues.Average() + (issueValues.StandardDeviation());

            var within2StdDev = issueValues.Where(x=>x>=tLower && x<=tUpper ).Count();
            double DevPerc = ((double)within2StdDev/(double)issueValues.Count());
            var tOutsideLower = issueValues.Where(x=>x<tLower).Count();
            var tOutsideUpper = issueValues.Where(x=>x>tUpper).Count();

            AnsiConsole.WriteLine($"Count within 2 stddev: {within2StdDev}");

            AnsiConsole.WriteLine($"Normal Dist = 68.2% - Your Dist is : {DevPerc:0.00%}");

            AnsiConsole.WriteLine($"Count outsidelower: {tOutsideLower}");
            AnsiConsole.WriteLine($"Count outsideupper: {tOutsideUpper}");



            //var nbrs = new List<double>(){30.99,38.22,138.87,134.1244}


            ConsoleUtil.PressAnyKeyToContinue();
        }

        private static void ViewIssueConfig(string defProject) 
        {

            CfgManager.config.UpdateDefaultStatusConfigs(defProject);

            AnsiConsole.Status()
                .Start($"Getting Latest Issue Status data from Jira ...", ctx=>
                {
                    ctx.Spinner(Spinner.Known.Dots);
                    ctx.SpinnerStyle(new Style(AnsiConsole.Foreground,AnsiConsole.Background));
                    Thread.Sleep(100);
                    ctx.Status("[italic]Writing Local Issue Status Config ...[/]");
                    IssueStatesUtil.WriteJiraStatuses();

                });

            if (ConsoleUtil.Confirm("Save Issue Status Config to csv?",false))
            {
                IssueStatesUtil.WriteJiraStatusesToCSV();
            }

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

        private static void ShowMenu_SummaryVisualization()
        {
            lastMenu = MenuEnum.meIssue_Summary_Visualization;
            BuildMenuPanel(lastMenu);

            var sp = new SelectionPrompt<MenuFunction>();      
            sp.HighlightStyle(new Style(decoration:Decoration.Bold));      
            sp.PageSize = MenuPageSize;
            sp.AddChoice(MakeMenuDetail(MenuItemEnum.miIssue_Summary_Visualization,"Issue Status and Blocker Summary"));
            sp.AddChoice(MakeMenuDetail(MenuItemEnum.miIssue_Summary_Visualization_Epic,"Issue Status and Blocker Summary (by Epic)"));
            sp.AddChoice(MakeMenuDetail(MenuItemEnum.miTISIssueTree,"Build Status Tree"));
            sp.AddChoice(MakeMenuDetail(MenuItemEnum.miTISCycleTime,"Build Cycle Time"));

            sp.AddChoice(MakeMenuDetail(MenuItemEnum.miIssue_Summary_Overall1,"Overall Status Summary 1"));
            sp.AddChoice(MakeMenuDetail(MenuItemEnum.miIssue_Summary_Overall1_Epic ,"Overall Status Summary 1 (by Epic)"));

            AddCommonMenuItems(sp,lastMenu);
            MenuManager.Execute(AnsiConsole.Prompt(sp));            

        }

        private static void ShowMenu_ChangeLog()
        {
            lastMenu = MenuEnum.meChangeLog;
            BuildMenuPanel(lastMenu);
            var sp = new SelectionPrompt<MenuFunction>();      
            sp.HighlightStyle(new Style(decoration:Decoration.Bold));      
            sp.PageSize = MenuPageSize;
            sp.AddChoice(MakeMenuDetail(MenuItemEnum.miShowChangeHistoryCards,"Issue Change-Logs "));
            sp.AddChoice(MakeMenuDetail(MenuItemEnum.miShowChangeHistoryEpics,"Issue Change-Logs (by Epic) "));

            AddCommonMenuItems(sp,lastMenu);
            MenuManager.Execute(AnsiConsole.Prompt(sp));            

        }

        private static void ShowMenu_IssueNotes()
        {

            lastMenu = MenuEnum.meIssue_Notes ;
            BuildMenuPanel(lastMenu);
            var sp = new SelectionPrompt<MenuFunction>();          
            sp.HighlightStyle(new Style(decoration:Decoration.Bold));  
            sp.PageSize = MenuPageSize;
            sp.AddChoice(MakeMenuDetail(MenuItemEnum.miIssueNotesView, "View Issue Notes"));
            sp.AddChoice(MakeMenuDetail(MenuItemEnum.miIssueNotesAdd, "Add Issue Note"));
            sp.AddChoice(MakeMenuDetail(MenuItemEnum.miIssueNotesDelete, "Delete Issue Note"));

            AddCommonMenuItems(sp,lastMenu);
            MenuManager.Execute(AnsiConsole.Prompt(sp));            

        }
        private static void ShowMenu_DEV()
        {
            lastMenu = MenuEnum.meDev;
            BuildMenuPanel(lastMenu);
            var sp = new SelectionPrompt<MenuFunction>();       
            sp.HighlightStyle(new Style(decoration:Decoration.Bold));     
            sp.PageSize = MenuPageSize;
            sp.AddChoice(MakeMenuDetail(MenuItemEnum.miDev1, $"{Environment.UserName} TEST 1"));
            sp.AddChoice(MakeMenuDetail(MenuItemEnum.miDev2, $"{Environment.UserName} TEST 2"));
            sp.AddChoice(MakeMenuDetail(MenuItemEnum.miDevScrubEdit, "Managed Scrubbed Terms"));
            AddCommonMenuItems(sp,lastMenu);
            MenuManager.Execute(AnsiConsole.Prompt(sp));            
  
        }

        private static void ShowMenu_CachedSearches()
        {
            lastMenu = MenuEnum.meCached_Searches;
            BuildMenuPanel(lastMenu);
            var sp = new SelectionPrompt<MenuFunction>();       
            sp.HighlightStyle(new Style(decoration:Decoration.Bold));
            
            sp.PageSize = MenuPageSize;

            sp.AddChoice(MakeMenuDetail(MenuItemEnum.miCachedSearch_View, "View Cached Searches"));
            sp.AddChoice(MakeMenuDetail(MenuItemEnum.miCachedSearch_ClearAll, "Clear Cached Search Results"));

            sp.AddChoice(menuSeparator);
            sp.AddChoice(MakeMenuDetail(MenuItemEnum.miMenu_Config, "Menu: Configuration"));
            sp.AddChoice(MakeMenuDetail(MenuItemEnum.miMenu_JQL,"Menu: Manage Saved JQL"));       

            AddCommonMenuItems(sp,lastMenu);
            MenuManager.Execute(AnsiConsole.Prompt(sp));       
        }
        private static void ShowMenu_Config()
        {
            lastMenu = MenuEnum.meConfig;
            BuildMenuPanel(lastMenu);
            var sp = new SelectionPrompt<MenuFunction>();       
            sp.HighlightStyle(new Style(Color.Black, Color.Cornsilk1,decoration:Decoration.Bold));
            sp.Mode(SelectionMode.Leaf);
            
            sp.PageSize = MenuPageSize;

            sp.AddChoice(MakeMenuDetail(MenuItemEnum.miMenu_JQL,"Menu: Manage Saved JQL"));       
            sp.AddChoice(MakeMenuDetail(MenuItemEnum.miMenu_Cached_Searches,"Menu: Cached Searches"));
            sp.AddChoiceGroup(MenuFunction.GroupHeader("JIRA CONNECTION PROFILES"), 
                MakeMenuDetail(MenuItemEnum.miJiraConfigView,"View Configured Jira Profiles"), 
                MakeMenuDetail(MenuItemEnum.miChangeConnection,"Change to another Jira connection"), 
                MakeMenuDetail(MenuItemEnum.miJiraConfigAdd,"Add New Jira Connection"), 
                MakeMenuDetail(MenuItemEnum.miJiraConfigRemove,"Remove Jira Connection")
            );
            sp.AddChoiceGroup(MenuFunction.GroupHeader("CONSOLE SESSION RECORDING"), 
                MakeMenuDetail(MenuItemEnum.miStartRecordingSession,"Start session recording"), 
                MakeMenuDetail(MenuItemEnum.miSaveSessionToFile,"Save session to file")
            );
            sp.AddChoiceGroup(MenuFunction.GroupHeader("MISCELLANEOUS"), 
                MakeMenuDetail(MenuItemEnum.miJiraServerInfo,$"View Jira Server Info"), 
                MakeMenuDetail(MenuItemEnum.miChangeTimeZoneDisplay,"Change Displayed Time Zone")
            );            
            AddCommonMenuItems(sp,lastMenu);
            MenuManager.Execute(AnsiConsole.Prompt(sp));            
        }

        private static void  AddCommonMenuItems(SelectionPrompt<MenuFunction> sp, MenuEnum fromMenu)
        {
            if (Info.IsDev && fromMenu != MenuEnum.meDev)
            {
                sp.AddChoice(new MenuFunction(MenuItemEnum.miMenu_Dev,"Developer","[bold blue on white]Developer[/]"));
            }
            sp.AddChoice(MenuFunction.Separator);
            if (fromMenu != MenuEnum.meMain)
            {
                sp.AddChoice(new MenuFunction(MenuItemEnum.miMenu_Main,"Back to Main Menu","Back to [bold]Main Menu[/]"));
            }
            sp.AddChoice(new MenuFunction(MenuItemEnum.miExit,"Exit App","[dim bold]Exit App[/]",true,Emoji.Known.SmallOrangeDiamond));
        }
        private static void ShowMenu_IssueStates()
        {

            lastMenu = MenuEnum.meIssue_States;
            BuildMenuPanel(lastMenu);
            var sp = MenuFunction.DefaultPrompt;

            sp.AddChoice(MakeMenuDetail(MenuItemEnum.miTISIssues,"Get Issue(s) Data"));
            sp.AddChoice(MakeMenuDetail(MenuItemEnum.miTISEpic,"Get Issue(s) Data by Epic"));
            sp.AddChoiceGroup(MenuFunction.GroupHeader("GO TO MENU"), 
                MakeMenuDetail(MenuItemEnum.miMenu_StatusConfig,"Menu: Issue Status Config"), 
                MakeMenuDetail(MenuItemEnum.miMenu_Config,"Menu: Configuration"), 
                MakeMenuDetail(MenuItemEnum.miMenu_JQL,"Menu: Manage Saved JQL"));

            AddCommonMenuItems(sp,lastMenu);
            MenuManager.Execute(AnsiConsole.Prompt(sp));            
        }
        private static void ShowMenu_JQL()
        {
            lastMenu = MenuEnum.meJQL;
            BuildMenuPanel(lastMenu);
            var sp = MenuFunction.DefaultPrompt;

            sp.AddChoice(MakeMenuDetail(MenuItemEnum.miSavedJQLView,"View Saved JQL / Issue Numbers"));
            sp.AddChoice(MakeMenuDetail(MenuItemEnum.miSavedJQLAdd,"Save New JQL / Issue Numbers"));
            sp.AddChoice(MakeMenuDetail(MenuItemEnum.miSavedJQLFind,"Find Saved JQL / Issue Numbers"));
            sp.AddChoice(MakeMenuDetail(MenuItemEnum.miSavedJQLRemove,"Remove Saved JQL / Issue Numbers"));
            sp.AddChoice(MakeMenuDetail(MenuItemEnum.miCheckJQLStatement, "Check Manual JQL Statement"));
            sp.AddChoice(menuSeparator);
            sp.AddChoice(MakeMenuDetail(MenuItemEnum.miSavedJQLDefault,"Verify/Add Default JQL Snippets"));

            AddCommonMenuItems(sp,lastMenu);
            MenuManager.Execute(AnsiConsole.Prompt(sp));            
        }

        private static void ShowMenu_StatusConfig()
        {
            lastMenu = MenuEnum.meStatus_Config;
            BuildMenuPanel(lastMenu);
            var sp = MenuFunction.DefaultPrompt;

            sp.AddChoice(MakeMenuDetail(MenuItemEnum.miIssCfgView,"View Issue Status Config"));
            sp.AddChoice(MakeMenuDetail(MenuItemEnum.miIssCfgEdit,"Edit Local Issue Status Config"));
            sp.AddChoice(MakeMenuDetail(MenuItemEnum.miIssCfgSequence,"Manage Issue Status Progress Sequence"));
            sp.AddChoice(MakeMenuDetail(MenuItemEnum.miIssCfgReset,"Reset Local Issue Status Config to Match Jira"));

            AddCommonMenuItems(sp,lastMenu);
            MenuManager.Execute(AnsiConsole.Prompt(sp));            


        }
        private static void ShowMenu_Main()
        {
            lastMenu = MenuEnum.meMain;;
            BuildMenuPanel(lastMenu);
            var sp = MenuFunction.DefaultPrompt;

            sp.AddChoice(MakeMenuDetail(MenuItemEnum.miMenu_IssueStates,"Menu: Issue Analysis"));
            sp.AddChoice(MakeMenuDetail(MenuItemEnum.miMenu_Issue_Summary_Visualization,"Menu: Issue Summary Visualization"));
            sp.AddChoice(MakeMenuDetail(MenuItemEnum.miMenu_Change_Log,"Change Logs"));
            sp.AddChoice(MakeMenuDetail(MenuItemEnum.miMenu_Issue_Notes,"Issue Notes"));
            sp.AddChoice(menuSeparator);
            sp.AddChoice(MakeMenuDetail(MenuItemEnum.miMenu_Config,"Menu: Configuration"));
            sp.AddChoice(MakeMenuDetail(MenuItemEnum.miHelpfulURLs, "Application URLs"));

            AddCommonMenuItems(sp,lastMenu);
            MenuManager.Execute(AnsiConsole.Prompt(sp));            
        }

        public static void ShowMenu(MenuEnum menu)
        {
            switch (menu)
            {
                case MenuEnum.meMain:
                    ShowMenu_Main();
                    return;
                case MenuEnum.meJQL:
                    ShowMenu_JQL();
                    return;
                case MenuEnum.meStatus_Config:
                    ShowMenu_StatusConfig();
                    return;
                case MenuEnum.meCached_Searches:
                    ShowMenu_CachedSearches();
                    return;
                case MenuEnum.meConfig:
                    ShowMenu_Config();
                    return;
                case MenuEnum.meDev:
                    ShowMenu_DEV();
                    return;
                case MenuEnum.meChangeLog:
                    ShowMenu_ChangeLog();
                    return;
                case MenuEnum.meIssue_Notes:
                    ShowMenu_IssueNotes();
                    return;
                case MenuEnum.meIssue_Summary_Visualization:
                    ShowMenu_SummaryVisualization();
                    return;
                case MenuEnum.meIssue_States:
                    ShowMenu_IssueStates();
                    return;
                default:
                    string miName = Enum.GetName(typeof(MenuEnum),menu);
                    ConsoleUtil.WriteError($"A HANDLER FOR MENU: '{miName}' DOES NOT EXIST -- REVERTING TO MAIN MENU",pause:true);
                    ShowMenu(MenuEnum.meMain);
                    return;
            }

            // lastMenu = menu;
            // BuildMenuPanel(menu);
            // List<MenuFunction> menuItems = BuildMenuItems(menu);
            // if (menuItems.Count > 0)
            // {
            //     var sp = new SelectionPrompt<MenuFunction>();
            //     sp.HighlightStyle(new Style(decoration:Decoration.Bold));
                
            //     sp.PageSize = MenuPageSize;

            //     sp.AddChoices(menuItems);



            //     if (menu == MenuEnum.meMain)
            //     {
            //         sp.AddChoiceGroup(
            //                 menuSeparator, 
            //                 new MenuFunction(MenuItemEnum.miExit,"Exit App","[dim bold]Exit App[/]",true,Emoji.Known.SmallOrangeDiamond));
            //     }
            //     else 
            //     {
            //         sp.AddChoiceGroup(
            //                 menuSeparator, 
            //                 new MenuFunction(MenuItemEnum.miMenu_Main,"Back to Main Menu","Back to [bold]Main Menu[/]"),
            //                 new MenuFunction(MenuItemEnum.miExit,"Exit App","[dim bold]Exit App[/]",true,Emoji.Known.SmallOrangeDiamond));

            //     }                    
            //     var mnu = AnsiConsole.Prompt(sp);
            //     try
            //     {
            //         MenuManager.Execute(mnu);            
            //     }
            //     catch(Exception errEx)
            //     {
            //         ConsoleUtil.WriteError($"An error occurred processing request. Please double-check syntax of any JQL statements you are working with.",false,ex:errEx,true);

            //         ShowMenu(menu);
            //     }
            // }
            // else 
            // {
            //     string miName = Enum.GetName(typeof(MenuEnum),menu);
            //     AnsiConsole.Write(new Rule());
            //     AnsiConsole.Write(new Rule($"[{Color.DarkRed.ToString()} on {Color.LightYellow3.ToString()}] a handler for '{miName}' does not exist, reverting to main menu [/]"));
            //     AnsiConsole.Write(new Rule());
            //     ConsoleUtil.PressAnyKeyToContinue();
            //     ShowMenu(MenuEnum.meMain);
            // }

        }
        private static void BuildMenuPanel(MenuEnum menu)
        {
            AnsiConsole.Clear();
            var menuName = Enum.GetName(typeof(MenuEnum),menu).Replace("me","").Replace("_"," ");
            var menuLabel = $"[bold black on lightyellow3]{Emoji.Known.DiamondWithADot} {menuName} Menu [/]| [dim italic]Connected: {CfgManager.config.ToString()}[/]";  
            var title = $"  JIRA Time In Status :llama: [dim]by[/] [dim link=https://github.com/lopperman/jira-issue-analysis]Paul Brower[/]{ConsoleUtil.scrubMode}{ConsoleUtil.RecordingInfo}{ConsoleUtil.TimeZoneAlert}{Environment.NewLine}  {menuLabel}";
            title=ConsoleUtil.Scrub(title);
            AnsiConsole.Write(new Rule());
            AnsiConsole.MarkupLine(title);
            AnsiConsole.Write(new Rule().HeavyBorder());
        }

        public static bool IsMenu(MenuItemEnum mi)
        {
            var miString = Enum.GetName(typeof(MenuItemEnum),mi);
            return miString.StringsMatch("miMenu",StringCompareType.scStartsWith);
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

        // public static List<MenuFunction> BuildMenuItems(MenuEnum menu)
        // {
        //     var ret = new List<MenuFunction>();
            
        //     var miFore = StdLine.slMenuDetail.FontMkp();
        //     var miBack = StdLine.slMenuDetail.BackMkp();

        //     switch (menu)
        //     {


        //         case(MenuEnum.meAdvanced_Search):
        //             ret.Add(MakeMenuDetail(MenuItemEnum.miAdvSearchViewCustomFields,"View Jira Custom Field Info"));
        //             ret.Add(MakeMenuDetail(MenuItemEnum.miAdvSearchViewIssueFields,"View Jira Issues Field Info"));
        //         break;

        //     }            

        //     return ret;
        // }
        public static void Start(JTISConfig cfg)
        {
            ShowMenu(MenuEnum.meMain);
        }


    }
}