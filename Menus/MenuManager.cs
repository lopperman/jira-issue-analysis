



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
                            new MenuFunction(MenuItemEnum.miSeparator," "," "),                    
                            new MenuFunction(MenuItemEnum.miChangeConnection,"Connect to other Jira Site","[dim]Connect to other Jira Site[/]"),
                            new MenuFunction(MenuItemEnum.miExit,"Exit App","[dim bold]Exit App :right_arrow_curving_left:[/]"));                        
                }
                else 
                {
                    sp.AddChoiceGroup(
                            new MenuFunction(MenuItemEnum.miSeparator," "," "),                    
                            new MenuFunction(MenuItemEnum.miMenu_Main,"Back to Main Menu","Back to [bold]Main Menu[/]"),
                            new MenuFunction(MenuItemEnum.miChangeConnection,"Connect to other Jira Site","[dim]Connect to other Jira Site[/]"),
                            new MenuFunction(MenuItemEnum.miExit,"Exit App","[dim bold]Exit App :right_arrow_curving_left:[/]"));                        

                }                    
                    
                var mnu = AnsiConsole.Prompt(sp);
                
                // var mnu  = AnsiConsole.Prompt(
                //     new SelectionPrompt<MenuFunction>()
                //         // .Title("What's your [green]favorite fruit[/]?")
                //         .PageSize(10)
                //         // .MoreChoicesText("[grey](Move up and down to reveal more fruits)[/]")
                //         .AddChoices(menuItems)                     
                // );

//                ConsoleUtil.PressAnyKeyToContinue();
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
            var menuLabel = $"[bold black on lightyellow3]{menuName} Menu [/]| [dim italic]Connected: {JTISConfigHelper.config.ToString()}[/]";  

            var title = $"JIRA Time In Status :llama: [dim]by[/] [dim link=https://github.com/lopperman/jiraTimeInStatus]Paul Brower[/]{Environment.NewLine}{menuLabel}";
//{Environment.NewLine}[dim italic][link]https://github.com/lopperman/jiraTimeInStatus[/][/]{Environment.NewLine}{menuLabel}";            
            var panel = new Panel(title);
            panel.Border = BoxBorder.Rounded;
            panel.BorderColor(Color.Grey15);
            panel.Expand = true;
            AnsiConsole.Write(panel);

            // var menuName = Enum.GetName(typeof(MenuEnum),menu).Replace("me","").Replace("_"," ");
            // var menuLabel = $"[bold black on lightyellow3]{menuName} Menu [/]| [dim italic]Connected: {JTISConfigHelper.config.ToString()}[/]";  
            // var panel = new Panel(menuLabel);
            // panel.Border = BoxBorder.Rounded;
            // panel.BorderColor(Console.ForegroundColor);
            // panel.HeaderAlignment(Justify.Left);
            // AnsiConsole.Write(panel);
        }
        public static List<MenuFunction> BuildMenuItems(MenuEnum menu)
        {
            var ret = new List<MenuFunction>();
            
            var miFore = StdLine.slMenuDetail.FontMkp();
            var miBack = StdLine.slMenuDetail.BackMkp();

            switch (menu)
            {
                case (MenuEnum.meMain):
                    ret.Add(new MenuFunction(MenuItemEnum.miMenu_IssueStates,"Menu: Analyze Issue(s)",$"[{miFore} on {miBack}]Menu: Analyze Issue(s) Time in Status[/]"));
                    ret.Add(new MenuFunction(MenuItemEnum.miShowChangeHistoryCards,"View ChangeLog for Issue(s)",$"[{miFore} on {miBack}]View ChangeLog for Issue(s)[/]"));
                    ret.Add(new MenuFunction(MenuItemEnum.miMenu_Config,"Menu: Configuration",$"[{miFore} on {miBack}]Menu: Configuration[/]"));
                break;
                case(MenuEnum.meConfig):
                break;
                case(MenuEnum.meDev):
                break;
                case(MenuEnum.meIssue_States):
                    ret.Add(new MenuFunction(MenuItemEnum.miTISIssueSummary,"Create Issue Summary",$"[{miFore} on {miBack}]Create Issue Summary[/]"));
                    ret.Add(new MenuFunction(MenuItemEnum.miTISIssues,"Get Issue(s) Data",$"[{miFore} on {miBack}]Get Issue(s) Data[/]"));
                    ret.Add(new MenuFunction(MenuItemEnum.miTISEpic,"Get Issue(s) Data by Epic",$"[{miFore} on {miBack}]Get Issue(s) Data by Epic[/]"));
                    ret.Add(new MenuFunction(MenuItemEnum.miTISJQL,"Get Issue(s) Data by JQL Query",$"[{miFore} on {miBack}]Get Issue(s) Data by JQL Query[/]"));

                    ret.Add(new MenuFunction(MenuItemEnum.miMenu_StatusConfig,"Menu: Issue Status Config",$"[{miFore} on {miBack}]Menu: Issue Status Config[/]"));
                    ret.Add(new MenuFunction(MenuItemEnum.miMenu_JQL ,"Menu: Manage Saved JQL",$"[{miFore} on {miBack}]Menu: Manage Saved JQL[/]"));
                break;
                case(MenuEnum.meJQL):
                break;
                case(MenuEnum.meStatus_Config):
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