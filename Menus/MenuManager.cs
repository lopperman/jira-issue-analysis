using System.Net.Http.Headers;
using System.IO.Pipes;




using Spectre.Console;

namespace JiraCon
{
    public interface IMenuConsole
    {        
        public JTISConfig ActiveConfig {get; set;}
        public bool DoMenu();
        public bool ProcessKey(ConsoleKey key);        
    }


    public static class MenuManager
    {
        private static MenuFunction menuSeparator = MakeMenuDetail(MenuItemEnum.miSeparator,string.Format("{0}{0}{0}{0}{0}{0}{0}",Emoji.Known.WavyDash),Emoji.Known.WavyDash);
//        private static MenuFunction menuSeparator = MakeMenuDetail(MenuItemEnum.miSeparator,string.Format("Connect to different Jira",Emoji.Known.WavyDash),Emoji.Known.WavyDash);
     
        public static void Execute(MenuFunction item, MenuEnum? returnToMenu = null)
        {
            MenuEnum? finalMenu = returnToMenu;
            switch (item.MenuItem)
            {
                case MenuItemEnum.miMenu_Main:
                    finalMenu = MenuEnum.meMain;
                    break;
                case MenuItemEnum.miMenu_Config:
                    finalMenu = MenuEnum.meConfig;
                    break;
                case MenuItemEnum.miMenu_Dev:
                    finalMenu = MenuEnum.meDev;
                    break;
                case MenuItemEnum.miMenu_IssueStates:
                    finalMenu = MenuEnum.meIssue_States;
                    break;
                case MenuItemEnum.miMenu_JQL:
                    finalMenu = MenuEnum.meJQL;
                    break;
                case MenuItemEnum.miMenu_StatusConfig:
                    finalMenu = MenuEnum.meStatus_Config;
                    break;
                case MenuItemEnum.miDev1:
                    var menuDev1 = new MenuDev(JTISConfigHelper.config);
                    menuDev1.DevTest1();
                    ConsoleUtil.PressAnyKeyToContinue();
                    break;
                case MenuItemEnum.miDev2:
                    var menuDev2 = new MenuDev(JTISConfigHelper.config);
                    menuDev2.DevTest2();
                    ConsoleUtil.PressAnyKeyToContinue();
                    break;
                case MenuItemEnum.miChangeTimeZoneDisplay:
                    JTISConfigHelper.ChangeTimeZone();
                    if (finalMenu == null){finalMenu = MenuEnum.meConfig;}
                    break;
                case MenuItemEnum.miVisualSnapshotAll:
                    finalMenu = MenuEnum.meMain;
                    var visAll = new VisualSnapshot(VisualSnapshotType.vsProject);
                    break;
                case MenuItemEnum.miExit:
                    finalMenu = null;                    
                    ConsoleUtil.ByeByeForced();
                    break;
                case MenuItemEnum.miShowChangeHistoryCards:
                    ShowChangeLog();
                    if (finalMenu == null){finalMenu = MenuEnum.meMain;}
                    break;
                case MenuItemEnum.miIssCfgView:
                    ViewIssueConfig();
                    if (finalMenu == null){finalMenu = MenuEnum.meIssue_States;}
                    break;
                case MenuItemEnum.miTISIssueSummary:
                    NewAnalysis(AnalysisType.atIssueSummary);
                    if (finalMenu == null){finalMenu = MenuEnum.meIssue_States;}
                    ConsoleUtil.PressAnyKeyToContinue();
                    break;
                case MenuItemEnum.miJiraConfigView:
                    JTISConfigHelper.ViewAll();
                    if (finalMenu == null){finalMenu = MenuEnum.meConfig;}
                    if (ConsoleUtil.Confirm("SHOW API KEYS?",false))
                    {
                        JTISConfigHelper.ViewAll(true);
                        ConsoleUtil.PressAnyKeyToContinue();
                    }
                    break;
                case MenuItemEnum.miStartRecordingSession:
                    if (finalMenu == null){finalMenu = MenuEnum.meConfig;}
                    if (JTISConfigHelper.IsConsoleRecording)
                    {
                        ConsoleUtil.PressAnyKeyToContinue("Recording is already in progress");
                    }
                    else 
                    {
                        JTISConfigHelper.IsConsoleRecording = true;
                        AnsiConsole.Record();
                        ConsoleUtil.PressAnyKeyToContinue("Recording has started");
                    }
                    break;
                case MenuItemEnum.miSaveSessionToFile:
                    var fName = SaveSessionFile();
                    if (finalMenu == null){finalMenu = MenuEnum.meConfig;}
                    ConsoleUtil.PressAnyKeyToContinue($"Saved to: {fName}");
                    break;
                case MenuItemEnum.miSavedJQLView:
                    JQLUtil.ViewSavedJQL(JTISConfigHelper.config);
                    if (finalMenu == null){finalMenu = MenuEnum.meJQL;}
                    break;
                case MenuItemEnum.miChangeConnection:
                    finalMenu = MenuEnum.meMain;
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
                    finalMenu = MenuEnum.meMain;
                    break;
            }
            if (finalMenu != null)
            {
                ShowMenu(finalMenu.Value);
            }
        }

        public static string SaveSessionFile()
        {
            string sessFile = Path.Combine(JTISConfigHelper.ConfigFolderPath,"SessionFiles");
            if (!Directory.Exists(sessFile)){Directory.CreateDirectory(sessFile);}
            string fName = string.Format("SessionFile_{0}.html",DateTime.Now.ToString("yyyyMMMddHHmmss"));
            sessFile = Path.Combine(sessFile,fName);
            using (StreamWriter writer = new StreamWriter(sessFile,false))
            {
                writer.Write(AnsiConsole.ExportHtml());
            }
            return sessFile ;
        }

        private static void NewAnalysis(AnalysisType anType)
        {
            AnalyzeIssues analyze = new AnalyzeIssues(anType);
            int issueCount = 0;
            if (analyze.HasSearchData)
            {
                issueCount = analyze.GetData();
                if (analyze.GetDataFail)
                {
                    ConsoleUtil.PressAnyKeyToContinue();
                }
            } 
            if (issueCount > 0)
            {                
                analyze.ClassifyStates();                
                analyze.WriteToConsole();

                if (anType != AnalysisType.atIssueSummary)
                {
                    if (ConsoleUtil.Confirm("Save to csv file?",false))
                    {
                        var csvFileName = analyze.WriteToCSV();
                        ConsoleUtil.PressAnyKeyToContinue($"File saved to: {csvFileName}");
                    }
                }

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
                    WriteJiraStatuses();

                });
            ConsoleUtil.PressAnyKeyToContinue();                    

        }

        private static void WriteJiraStatuses(string? searchTerm = null)
        {
            var usedInCol = string.Format("UsedIn: {0}",JTISConfigHelper.config.defaultProject);
            Table table = new Table();
            table.AddColumns("JiraId","Name","LocalState","DefaultState",usedInCol,"Override");
            table.Columns[2].Alignment(Justify.Center);
            table.Columns[3].Alignment(Justify.Center);
            table.Columns[4].Alignment(Justify.Center);
            table.Columns[5].Alignment(Justify.Center);

            foreach (var jStatus in JTISConfigHelper.config.StatusConfigs.OrderByDescending(d=>d.DefaultInUse).ThenBy(x=>x.Type).ThenBy(y=>y.StatusName).ToList())
            {
                bool includeStatus = false;
                if (searchTerm == null || searchTerm.Length == 0)
                {
                    includeStatus = true;
                }
                else 
                {
                    if (jStatus.StatusName.ToLower().Contains(searchTerm.ToLower()))
                    {
                        includeStatus = true;
                    }
                }
                if (includeStatus)
                {
                    JiraStatus  defStat = JTISConfigHelper.config.DefaultStatusConfigs.Single(x=>x.StatusId == jStatus.StatusId );
                    string usedIn = string.Empty;   
                    string overridden = string.Empty;      
                    string locState = Enum.GetName(typeof(StatusType),jStatus.Type);     
                    if (jStatus.DefaultInUse)
                    {
                        usedIn = "YES";
                    }
                    if (jStatus.Type != defStat.Type)
                    {
                        
                        overridden = string.Format("[bold red on yellow]{0}{0}{0}[/]",":triangular_Flag:");
                        locState = string.Format("[bold blue on lightyellow3]{0}[/]",locState);
                    }
                    table.AddRow(new string[]{jStatus.StatusId.ToString(), jStatus.StatusName,locState,Enum.GetName(typeof(StatusType),defStat.Type),usedIn, overridden});
                }
            }
            AnsiConsole.Write(table);
        }

        public static string[]? GetIssueNumbers()
        {
            var p = new TextPrompt<string>($"[{StdLine.slResponse.FontMkp()} on {StdLine.slResponse.BackMkp()}]Enter 1 or more issue numbers, separated by a [underline]SPACE[/][/]{Environment.NewLine}[dim](Any values lacking a project prefix will automatically have '{JTISConfigHelper.config.defaultProject}-' added (e.g. '100' becomes '{JTISConfigHelper.config.defaultProject}-100')[/]{Environment.NewLine}:");
            var keys = AnsiConsole.Prompt<string>(p);
            if (keys != null && keys.Length>0)
            {
                string[] arr = keys.Split(' ',StringSplitOptions.RemoveEmptyEntries & StringSplitOptions.TrimEntries);
                // string[] arr = keys.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (arr.Length >= 1)
                {
                    for (int i = 0; i < arr.Length; i ++)
                    {
                        if (!arr[i].Contains('-'))
                        {
                            arr[i] = $"{JTISConfigHelper.config.defaultProject}-{arr[i]}";
                        }
                    }      
                    return arr;
                }
            }      
            return null;
        }
        
        private static void ShowChangeLog()
        {
            string[]? arr = GetIssueNumbers();
            if (arr==null || arr.Length == 0){return;}
            
            for (int i = 0; i < arr.Length; i ++)
            {
                if (!arr[i].Contains("-"))
                {
                    arr[i] = $"{JTISConfigHelper.config.defaultProject}-{arr[i]}";
                }
            }
            List<JIssue>? retIssues;
            retIssues = MainClass.AnalyzeIssues(string.Join(" ",arr));


            if (retIssues != null)
            {
                if (ConsoleUtil.Confirm("Save to csv file?",false)==true)
                {
                    string savedFilePath = string.Empty;
                    AnsiConsole.Status()
                        .Start($"Analyzing {arr.Length} issues ...", ctx=>
                        {
                            ctx.Spinner(Spinner.Known.Dots);
                            ctx.SpinnerStyle(new Style(AnsiConsole.Foreground,AnsiConsole.Background));
                            Thread.Sleep(100);

                        ctx.Status("[italic]Saving to csv file ...[/]");
                        savedFilePath = MainClass.WriteChangeLogCSV(retIssues);
                        });
                    ConsoleUtil.PressAnyKeyToContinue($"results were saved to [bold]{Environment.NewLine}{savedFilePath}[/]");
                }
            }
        }

        private static void CheckMinConsoleSize(int cWidth, int cHeight)
        {
            AnsiConsole.WriteLine("checking minimum console ");
            AnsiConsole.WriteLine($"Current Width: {Console.WindowWidth}");
            AnsiConsole.WriteLine($"Current Height: {Console.WindowHeight}");
            if (Console.WindowWidth < cWidth || Console.WindowHeight < cHeight)
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
            // CheckMinConsoleSize(100,40);

            

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
                case (MenuEnum.meMain):
                    ret.Add(MakeMenuDetail(MenuItemEnum.miVisualSnapshotAll,"Project Summary Visualization"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miMenu_IssueStates,"Menu: Analyze Issues(s)"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miShowChangeHistoryCards,"View ChangeLog for Issue(s)"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miMenu_Config,"Menu: Configuration"));
                    ret.Add(menuSeparator);
                    ret.Add(MakeMenuDetail(MenuItemEnum.miDev1,"DEV TEST 1"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miDev2,"DEV TEST 2"));
                break;
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
                case(MenuEnum.meDev):
                    ret.Add(MakeMenuDetail(MenuItemEnum.miDev1,"Developer Test 1"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miDev2,"Developer Test 2"));

                break;
                case(MenuEnum.meIssue_States):
                    ret.Add(MakeMenuDetail(MenuItemEnum.miMenu_Config,"Menu: Configuration"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miTISIssueSummary,"Create Issue Summary"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miTISIssues,"Get Issue(s) Data"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miTISEpic,"Get Issue(s) Data by Epic"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miTISJQL,"Get Issue(s) Data by JQL Query"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miMenu_StatusConfig,"Menu: Issue Status Config"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miMenu_JQL,"Menu: Manage Saved JQL"));
                break;
                case(MenuEnum.meJQL):
                    ret.Add(MakeMenuDetail(MenuItemEnum.miSavedJQLView,"View Saved JQL / Issue Numbers"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miSavedJQLAdd,"Save New JQL / Issue Numbers"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miSavedJQLFind,"Find Saved JQL / Issue Numbers"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miSavedJQLRemove,"Remove Saved JQL / Issue Numbers"));

                break;
                case(MenuEnum.meStatus_Config):
                    ret.Add(MakeMenuDetail(MenuItemEnum.miIssCfgView,"View Issue Status Config"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miIssCfgEdit,"Edit Local Issue Status Config"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miIssCfgReset,"Reset Local Issue Status Config to Match Jira"));

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