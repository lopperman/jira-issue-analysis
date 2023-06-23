



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
     
        private static MenuFunction menuSeparator = MakeMenuDetail(MenuItemEnum.miSeparator,string.Format("  {0} {0} {0}","-- "),Emoji.Known.WhiteSmallSquare);
     
        public static void Execute(MenuFunction item)
        {
            switch (item.MenuItem)
            {
                case MenuItemEnum.miMenu_Main:
                    ShowMenu(MenuEnum.meMain);
                    break;
                case MenuItemEnum.miMenu_Config:
                    ShowMenu(MenuEnum.meConfig);
                    break;
                case MenuItemEnum.miMenu_Dev:
                    ShowMenu(MenuEnum.meDev);
                    break;
                case MenuItemEnum.miMenu_IssueStates:
                    ShowMenu(MenuEnum.meIssue_States);
                    break;
                case MenuItemEnum.miMenu_JQL:
                    ShowMenu(MenuEnum.meJQL);
                    break;
                case MenuItemEnum.miMenu_StatusConfig:
                    ShowMenu(MenuEnum.meStatus_Config);
                    break;
                case MenuItemEnum.miExit:
                    ConsoleUtil.ByeByeForced();
                    break;
                case MenuItemEnum.miShowChangeHistoryCards:
                    ShowChangeLog();
                    ShowMenu(MenuEnum.meMain);
                    break;
                default:
                    string miName = Enum.GetName(typeof(MenuItemEnum),item.MenuItem);
                    AnsiConsole.Write(new Rule());
                    AnsiConsole.Write(new Rule($"[{Color.DarkRed.ToString()} on {Color.LightYellow3.ToString()}] a handler for '{miName}' does not exist, reverting to main menu [/]"));
                    AnsiConsole.Write(new Rule());
                    ConsoleUtil.PressAnyKeyToContinue();
                    ShowMenu(MenuEnum.meMain);
                    break;

            }
        }

        private static void ShowChangeLog()
        {
            var p = new TextPrompt<string>($"[{StdLine.slResponse.FontMkp()} on {StdLine.slResponse.BackMkp()}]Enter 1 or more issue numbers, separated by a [underline]SPACE[/][/]{Environment.NewLine}[dim](Any values lacking a project prefix will automatically have '{JTISConfigHelper.config.defaultProject}-' added (e.g. '100' becomes '{JTISConfigHelper.config.defaultProject}-100')[/]{Environment.NewLine}");
            var keys = AnsiConsole.Prompt<string>(p);
            if (keys != null && keys.Length>0)
            {
                string[] arr = keys.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (arr.Length >= 1)
                {

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
            }
        }

        public static void ShowMenu(MenuEnum menu)
        {
            BuildMenuPanel(menu);
            List<MenuFunction> menuItems = BuildMenuItems(menu);
            if (menuItems.Count > 0)
            {
                var sp = new SelectionPrompt<MenuFunction>();
                sp.PageSize = 10;
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
                            new MenuFunction(MenuItemEnum.miChangeConnection,"Connect to other Jira Site","[dim]Connect to other Jira Site[/]"),
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
            var title = $"JIRA Time In Status :llama: [dim]by[/] [dim link=https://github.com/lopperman/jiraTimeInStatus]Paul Brower[/]{Environment.NewLine}{menuLabel}";
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
                    ret.Add(MakeMenuDetail(MenuItemEnum.miMenu_IssueStates,"Menu: Analyze Issues(s)"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miShowChangeHistoryCards,"View ChangeLog for Issue(s)"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miMenu_Config,"Menu: Configuration"));
                break;
                case(MenuEnum.meConfig):
                    ret.Add(MakeMenuDetail(MenuItemEnum.miMenu_JQL,"Menu: Manage Saved JQL"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miJiraConfigAdd,"Add New Jira Connection"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miJiraConfigView,"View Jira Connections"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miJiraConfigRemove,"Remove Jira Connection"));
                    ret.Add(MakeMenuDetail(MenuItemEnum.miJiraServerInfo,$"View Info: {JiraUtil.JiraRepo.ServerInfo.BaseUrl}"));

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