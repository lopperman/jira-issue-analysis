using Spectre.Console;
using JTIS.Config;
using JTIS.Console;
using JTIS.ManagedObjects;
using JTIS.Analysis;
using Atlassian.Jira;
using JTIS.Extensions;
using JTIS.Data;

namespace JTIS{
    public static class Info
    {
        public static bool IsDev
        {
            get{
                return Environment.UserName.StringsMatch("paulbrower");
            }
        }

    }
}

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
        private static MenuFunction menuSeparator = MakeMenuDetail(MenuItemEnum.miSeparator,string.Format("{0}{0}{0}","---"),"  ");
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
#endregion
//miMenu_Issue_Summary_Visualization
#region ISSUE VISUALIZATION

                case MenuItemEnum.miIssue_Summary_Visualization:
                    if (exitMenu == null){exitMenu = MenuEnum.meIssue_Summary_Visualization;}
                    VisualSnapshot.Create(VisualSnapshotType.vsIssueStatus, AnalysisType.atIssues).BuildSearch();
                    break;

                case MenuItemEnum.miIssue_Summary_Visualization_Epic :
                    if (exitMenu == null){exitMenu = MenuEnum.meIssue_Summary_Visualization;}
                    VisualSnapshot.Create(VisualSnapshotType.vsIssueStatus, AnalysisType.atEpics).BuildSearch();
                    break;

#endregion
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

#region DEV MENU ITEMS

                case MenuItemEnum.miDev1:
                    MenuManager.Dev1();
                    exitMenu = MenuEnum.meConfig;
                    break;
                case MenuItemEnum.miDev2:
                    MenuManager.Dev2();
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
                    NewAnalysis(AnalysisType.atIssueSummary);
                    if (exitMenu == null){exitMenu = MenuEnum.meIssue_States;}
                    break;
                case MenuItemEnum.miTISEpic:
                    NewAnalysis(AnalysisType.atEpics);
                    if (exitMenu == null){exitMenu = MenuEnum.meIssue_States;}
                    break;

#endregion                    
                    

#region JQL MANAGEMENT

                case MenuItemEnum.miSavedJQLView:
                    JQLUtil.ViewSavedJQL(CfgManager.config);
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
                case MenuItemEnum.miSavedJQLDefault:
                    CfgManager.CheckDefaultJQL(CfgManager.config);
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

                case MenuItemEnum.miIssCfgEdit:
                    IssueStatesUtil.EditIssueStatus();
                    if (exitMenu == null){exitMenu = MenuEnum.meStatus_Config;}
                    break;

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
            //var rslt = await Task.WhenAll(jira.Issues.GetChangeLogsAsync(issueKey,token));
            var xxx = JiraUtil.JiraRepo.GetJira().Issues.GetChangeLogsAsync("WWT-311").GetAwaiter().GetResult();

            return;

            // var iss = jtisIssues.Create(JiraUtil.JiraRepo);
            // var queries = new List<string>();
            // queries.Add("key in (WWT-310, WWT-302, WWT-297, WWT-296, WWT-295, WWT-294, WWT-293, WWT-292, WWT-291, WWT-311)");
            // // queries.Add("project=WWT and status not in (backlog, done) and (priority = Blocked OR Flagged in (Impediment))");
            // var jIssues = iss.GetIssues(queries);
            // foreach (var j in jIssues)
            // {
            //     AnsiConsole.WriteLine($"{j.Key}, Change Logs: {j.ChangeLogs.Count}");
            // }

            // ConsoleUtil.PressAnyKeyToContinue($"JIssues: {jIssues.Count()}");

            // var issAsync =jtisIssues.Create(JiraUtil.JiraRepo);
            
            // //"project=WWT and status not in (backlog, done) and (priority = Blocked OR Flagged in (Impediment))"
            // var results = Task<IEnumerable<JIssue>>.WhenAll( issAsync.GetIssues2("project=WWT and status not in (backlog, done) and (priority = Blocked OR Flagged in (Impediment))"));


            // AnsiConsole.WriteLine($"returned {results.Result.Count()} issues");
            // ConsoleUtil.PressAnyKeyToContinue();
        }

        private static void Dev24()
        {
            var _issues = new List<Issue>();
            var _jIssues = new List<JIssue>();

            var tst = AsyncTesting.Create();
            var tsk = tst.FindIssuesAsync();
            List<Task> tasks  = new List<Task>();
            tasks.Add(tsk);
            

            AnsiConsole.Progress()
                .Columns(new ProgressColumn[]
                {
                    new TaskDescriptionColumn(),
                    new ProgressBarColumn(),
                    new PercentageColumn(),
                    new RemainingTimeColumn(),
                    new SpinnerColumn(),
                })
                .StartAsync(async ctx =>
                {
                    await Task.WhenAll(tasks.Select(async item =>
                    {
                        var task = ctx.AddTask("a task name", new ProgressTaskSettings
                        {
                            AutoStart = true
                        });
                        await item;
                        // await Download(client, task, item.url);
                    }));
                });

            ConsoleUtil.PressAnyKeyToContinue($"results {tsk.Result.Count()}");

        }

        private static void Dev22()
        {
            // Task tsk = Dev22();
            // Task.WhenAll(tsk);
            

            // ConsoleUtil.PressAnyKeyToContinue();

        }
        private static void Dev23()
        {

            var startVal = DateTime.Now;
            
            var tst = AsyncTesting.Create();
            // var issuesTask = tst.test1();
            // await Task.WhenAll(issuesTask);
            // var issues = issuesTask.Result;

            AnsiConsole.Write(new Rule("ASYNC GET ISSUES"));
            var issues = tst.FindIssuesAsync().Result;
            var endVal = DateTime.Now;
            var ts = endVal.Subtract(startVal);
            var PERFIssues = $"Issues ({issues.Count()}) Elapsed seconds: {ts.TotalMilliseconds/1000}";

            AnsiConsole.Write(new Rule("ASYNC GET CHANGE LOGS"));
            startVal = DateTime.Now;
            var clogs = tst.getChangeLogs(issues).Result;            
            endVal = DateTime.Now;
            ts = endVal.Subtract(startVal);
            var clPerf = $"Elapsed seconds: {ts.TotalMilliseconds/1000}";

            var clCount = 0;

            foreach (var cl in clogs)
            {
                clCount += cl.Items.Count();
                AnsiConsole.WriteLine($"change log id: {cl.Id}, count: {cl.Items.Count()}");
            }
            AnsiConsole.WriteLine(PERFIssues);
            AnsiConsole.WriteLine($"Change Log count: ({clCount}), Elapsed seconds: {ts.TotalMilliseconds/1000}");

            // AnsiConsole.Clear();
            // ConsoleUtil.WriteAppTitle();

            ConsoleUtil.PressAnyKeyToContinue($"returned {issues.Count()} issues, with {clCount} change logs");
            

            // string jql = "project=wwt and updated >= -30d and issueType=story";

            // var getCL  = AsyncChangeLogs.Create(JiraUtil.JiraRepo);
            // DateTime startDt = DateTime.Now;
            // var jIssues = getCL.GetIssuesAsync(jql).GetAwaiter().GetResult();
            // DateTime endDt = DateTime.Now;
            
            // ConsoleUtil.PressAnyKeyToContinue(Convert.ToString(endDt.Subtract(startDt).TotalMilliseconds/1000) + " seconds");            
            // string jql = "project=wwt and updated >= -30d and issueType=story";
            // var getCL  = AsyncChangeLogs.Create(JiraUtil.JiraRepo);

            // foreach (var ji in Dev3(getCL,jql).GetAwaiter().GetResult())
            // {
            //     AnsiConsole.MarkupLine($"[bold]Issue: {ji.Key}[/], ChangeLog count: {ji.ChangeLogs.Count}");
            // }
            

            // string jql = "project=wwt and updated >= -30d and issueType=story";
            // var getCL  = AsyncChangeLogs.Create(JiraUtil.JiraRepo);
            // // var jIssues = getCL.GetIssuesAsync(jql).GetAwaiter().GetResult();
            // List<JIssue> testList = new List<JIssue>();
            // await Task.WhenAll(
            //     async item => 
            // {
            //     var rslt = await getCL.GetIssuesAsync(jql)
                
            //     // await getCL.GetIssuesAsync(jql);
            // });

            // var someTing = await getCL.GetIssuesAsync(jql);
            // foreach (var ji in someTing)
            // {
            //     AnsiConsole.MarkupLine($"[bold]Issue: {ji.Key}[/], ChangeLog count: {ji.ChangeLogs.Count}");
            // }




            
            ConsoleUtil.PressAnyKeyToContinue();

            

            // ConsoleUtil.WriteAppTitle();
            // AnsiConsole.WriteLine("should end up right after this line");


            // AnsiConsole.MarkupLine($"Supports No Colors: {AnsiConsole.Console.Profile.Supports(ColorSystem.NoColors)}");
            // AnsiConsole.MarkupLine($"Supports Legacy: {AnsiConsole.Console.Profile.Supports(ColorSystem.Legacy)}");
            // AnsiConsole.MarkupLine($"Supports EightBit: {AnsiConsole.Console.Profile.Supports(ColorSystem.EightBit)}");
            // AnsiConsole.MarkupLine($"Supports Standard: {AnsiConsole.Console.Profile.Supports(ColorSystem.Standard)}");
            // AnsiConsole.MarkupLine($"Supports TrueColor: {AnsiConsole.Console.Profile.Supports(ColorSystem.TrueColor)}");
            // ConsoleUtil.PressAnyKeyToContinue();



            // ConsoleUtil.Confirm("Confirm",true);
            // ConsoleUtil.Confirm("Confirm 2",true);
            // ConsoleUtil.Confirm("Confirm 3",true);
            // AnsiConsole.WriteLine($"This line should be next!");
            // ConsoleUtil.PressAnyKeyToContinue();

            // ConsoleUtil.WriteAppTitle();
            // AnsiConsole.WriteLine("should end up right after this line");
            // ConsoleUtil.PressAnyKeyToContinue("1");
            // ConsoleUtil.PressAnyKeyToContinue("2");
            // ConsoleUtil.PressAnyKeyToContinue("3");
            // AnsiConsole.WriteLine($"This line should be next!");

            // if (ConsoleUtil.Confirm("do again?",false))
            // {
            //     Dev2();
            // }


////    KEEP THIS       ////         
            // var asdf = AutoCompData.Create();
////    KEEP THIS       ////         
        }

        private static void Dev1()
        {

            var tst = jtisIssueData.Create(JiraUtil.JiraRepo);
            var jql = "project=WWT and status='backlog'";
            var jtisIssueList = tst.GetIssuesWithChangeLogs(jql);
            foreach (var prf in tst.Performance)
            {
                AnsiConsole.MarkupLine($"[bold]{prf.Value.TotalSeconds} seconds - [/] {prf.Key}");
            }
            ConsoleUtil.PressAnyKeyToContinue();

            // var ti = jtisIssueData.Create(JiraUtil.JiraRepo);
            // foreach (var iss in ti.Test.)



            // var tt = JiraUtil.JiraRepo.GetJQLAutoCompleteDataAsync().GetAwaiter().GetResult();
            // if (tt != null)
            // {
            //     if (tt.ReservedWords.Count > 0)
            //     {
            //         foreach (var wd in tt.ReservedWords)
            //         {
            //             AnsiConsole.WriteLine(wd);
            //         }
            //     }
            // }

            // using (StreamWriter w = new StreamWriter(Path.Combine(CfgManager.JTISRootPath,"dev1.json"),false))
            // {
            //     w.Write(t);
            // }

            // JObject jo = JObject.Parse(t);
            // foreach (var t1 in jo.Children())
            // {
            //     AnsiConsole.WriteLine(t1.ToString());
            //     ConsoleUtil.PressAnyKeyToContinue("t1");

            // }

            



            ConsoleUtil.PressAnyKeyToContinue("DEV1 COMPLETED");
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
                if (JTIS.Info.IsDev)
                {
                    sp.AddChoiceGroup(
                        menuSeparator, 
                        MakeMenuDetail(MenuItemEnum.miDev1, $"{Environment.UserName} TEST 1"), 
                        MakeMenuDetail(MenuItemEnum.miDev2, $"{Environment.UserName} TEST 2")
                     );
                }

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
                try
                {
                    MenuManager.Execute(mnu);            
                }
                catch(Exception errEx)
                {
                    ConsoleUtil.WriteError($"An error occurred processing request. Please double-check syntax of any JQL statements you are working with.",false,ex:errEx,true);

                    ShowMenu(menu);
                }
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




            // AnsiConsole.Clear();
            // var title = $"  JIRA Time In Status :llama: [dim]by[/] [dim link=https://github.com/lopperman/jira-issue-analysis]Paul Brower[/]{ConsoleUtil.RecordingInfo}{ConsoleUtil.TimeZoneAlert}{Environment.NewLine}  [dim italic][link]https://github.com/lopperman/jira-issue-analysis[/][/]";            
            // AnsiConsole.Write(new Rule());
            // AnsiConsole.MarkupLine(title);
            // var tr = new Rule().DoubleBorder();
            // AnsiConsole.Write(tr);

        private static void BuildMenuPanel(MenuEnum menu)
        {
            AnsiConsole.Clear();
            var menuName = Enum.GetName(typeof(MenuEnum),menu).Replace("me","").Replace("_"," ");

            var menuLabel = $"[bold black on lightyellow3]{Emoji.Known.DiamondWithADot} {menuName} Menu [/]| [dim italic]Connected: {CfgManager.config.ToString()}[/]";  


            var title = $"  JIRA Time In Status :llama: [dim]by[/] [dim link=https://github.com/lopperman/jira-issue-analysis]Paul Brower[/]{ConsoleUtil.RecordingInfo}{ConsoleUtil.TimeZoneAlert}{Environment.NewLine}  {menuLabel}";
            // var panel = new Panel(title);
            // panel.Border = BoxBorder.Rounded;
            // panel.BorderColor(Color.Grey15);
            // panel.Expand = true;
            // AnsiConsole.Write(panel);
            AnsiConsole.Write(new Rule());
            AnsiConsole.MarkupLine(title);
            AnsiConsole.Write(new Rule().HeavyBorder());

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
                    ret.Add(MakeMenuDetail(MenuItemEnum.miMenu_Issue_Summary_Visualization,"Menu: Issue Summary Visualization"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miShowChangeHistoryCards,"Issue Change-Logs "));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miShowChangeHistoryEpics,"Issue Change-Logs (by Epic) "));

                    ret.Add(menuSeparator);
                    ret.Add(MakeMenuDetail(MenuItemEnum.miMenu_Config,"Menu: Configuration"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miMenu_Advanced_Search,"Menu: Advanced Search" ));
                break;

//  ISSUE VISUALIZAITON //

                case (MenuEnum.meIssue_Summary_Visualization):
                    ret.Add(MakeMenuDetail(MenuItemEnum.miIssue_Summary_Visualization,"Issue Summary Visualization"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miIssue_Summary_Visualization_Epic,"Issue Summary Visualization (by Epic)"));

                    ret.Add(menuSeparator);
                    ret.Add(MakeMenuDetail(MenuItemEnum.miMenu_Config,"Menu: Configuration"));
                break;


//  CONFIG MENU //

                case(MenuEnum.meConfig):
                    ret.Add(MakeMenuDetail(MenuItemEnum.miChangeConnection,"Change to another Jira connection"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miJiraConfigAdd,"Add New Jira Connection"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miJiraConfigView,"View Configured Jira Profiles"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miJiraConfigRemove,"Remove Jira Connection"));

                    ret.Add(MakeMenuDetail(MenuItemEnum.miJiraServerInfo,$"View Jira Server Info"));

                    ret.Add(MakeMenuDetail(MenuItemEnum.miStartRecordingSession,"Start session recording"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miSaveSessionToFile,"Save session to file"));

                    ret.Add(MakeMenuDetail(MenuItemEnum.miMenu_JQL,"Menu: Manage Saved JQL"));

                    ret.Add(MakeMenuDetail(MenuItemEnum.miChangeTimeZoneDisplay,"Change Displayed Time Zone"));

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
            // if (menu.ActiveConfig != CfgManager.config )
            // {
            //     menu.ActiveConfig = CfgManager.config;
            // }
            // while (menu.DoMenu())
            // {

            // }
            return false;
        }


    }
}