using System.Runtime.InteropServices;
using Spectre.Console;

namespace JiraCon
{
    public class MenuDev : IMenuConsole
    {

        public JTISConfig ActiveConfig {get;set;}
        public MenuDev(JTISConfig cfg)
        {
            ActiveConfig = cfg;                        
        }

        public void BuildMenu()
        {
            var cfgName = string.Format("Connected: {0} ",JTISConfigHelper.config.configName);
            string padd = new string('-',cfgName.Length + 1 );
            ConsoleLines lines = new ConsoleLines();
            lines.AddConsoleLine(" ------------ " + padd, ConsoleUtil.StdStyle(StdLine.slMenuName));
            lines.AddConsoleLine("|  DEV Menu  |" + " " + cfgName, ConsoleUtil.StdStyle(StdLine.slMenuName));
            lines.AddConsoleLine(" ------------ " + padd, ConsoleUtil.StdStyle(StdLine.slMenuName));
            lines.AddConsoleLine("(V) View Console Fore/Back Colors", ConsoleUtil.StdStyle(StdLine.slMenuDetail));
            lines.AddConsoleLine("(S) View Configured Console Styles", ConsoleUtil.StdStyle(StdLine.slMenuDetail));

            lines.AddConsoleLine("(D) DEVTEST1()", ConsoleUtil.StdStyle(StdLine.slMenuDetail));
            lines.AddConsoleLine("(T) DEVTEST2()", ConsoleUtil.StdStyle(StdLine.slMenuDetail));

            lines.AddConsoleLine("",ConsoleUtil.StdStyle(StdLine.slMenuDetail));
            lines.AddConsoleLine("(B) Back to Main Menu", ConsoleUtil.StdStyle(StdLine.slMenuDetail));
            lines.AddConsoleLine("Enter selection or (X) to exit.", ConsoleUtil.StdStyle(StdLine.slResponse));
            lines.WriteQueuedLines(true,true);
            lines = null;
        }

        public bool DoMenu()
        {
            BuildMenu();
            var resp = Console.ReadKey(true);
            return ProcessKey(resp.Key);
        }

        public bool ProcessKey(ConsoleKey key)
        {
            // ConsoleKeyInfo resp = default(ConsoleKeyInfo);

            if (key == ConsoleKey.V)
            {
                for (int iBack = 0; iBack <=15; iBack ++)
                {
                    for (int iFore = 0; iFore <= 15; iFore ++)
                    {
                        if (iBack != iFore) 
                        {
                            ConsoleColor ccFore = (ConsoleColor)iFore;
                            ConsoleColor ccBack = (ConsoleColor)iBack;
                            
                            string clrTest = string.Format("BackColor: {0}, ForeColor: {1}, Testing Standing Console Colors",ccBack,ccFore);
                            ConsoleUtil.WriteStdLine(clrTest,ccFore, ccBack);
                        }
                    }
                    Console.WriteLine("** PRESS ANY KEY TO SEE NEXT BACKCOLOR **");
                    Console.ReadKey(true);
                }
                Console.WriteLine("** PRESS ANY KEY TO RETURN TO CONFIG MENU **");
                Console.ReadKey(true);
                return true;                    
            }
            else if (key == ConsoleKey.S)
            {
                ConsoleUtil.WriteStdLine("CONFIGURED CONSOLE LINE STYLES",StdLine.slOutputTitle,false);
                ConsoleUtil.WriteStdLine("CONFIGURED CONSOLE LINE STYLE FOR: TITLE",StdLine.slTitle,false);
                ConsoleUtil.WriteStdLine("CONFIGURED CONSOLE LINE STYLE FOR: MENU NAME",StdLine.slMenuName,false);
                ConsoleUtil.WriteStdLine("CONFIGURED CONSOLE LINE STYLE FOR: MENU DETAIL",StdLine.slMenuDetail,false);
                ConsoleUtil.WriteStdLine("CONFIGURED CONSOLE LINE STYLE FOR: RESPONSE NEEDED",StdLine.slResponse,false);
                ConsoleUtil.WriteStdLine("CONFIGURED CONSOLE LINE STYLE FOR: ERROR",StdLine.slError,false);
                ConsoleUtil.WriteStdLine("CONFIGURED CONSOLE LINE STYLE FOR: OUTPUT TITLE",StdLine.slOutputTitle,false);
                ConsoleUtil.WriteStdLine("CONFIGURED CONSOLE LINE STYLE FOR: OUTPUT",StdLine.slOutput,false);
                ConsoleUtil.WriteStdLine("CONFIGURED CONSOLE LINE STYLE FOR: CODE",StdLine.slCode,false);
                Console.WriteLine();
                ConsoleUtil.WriteStdLine("PRESS ANY KEY",StdLine.slResponse,false);
                Console.ReadKey(true);
                return true;
            }
            else if (key == ConsoleKey.X)
            {
                if (ConsoleUtil.ByeBye())
                {
                    Environment.Exit(0);
                }
                return true;
            }
            else if (key == ConsoleKey.B)
            {
                return false;
            }
            else if (key == ConsoleKey.D)
            {
                DevTest1();
                Console.WriteLine("** PRESS ANY KEY **");
                Console.ReadKey(true);
                return true;
            }            
            else if (key == ConsoleKey.T)
            {
                DevTest2();
                ConsoleUtil.PressAnyKeyToContinue();
                return true;
            }            
            return true;
        }

        private void DevTest2()
        {
            var resp = AnsiConsole.Confirm("Save file to csv?",defaultValue:false);
            Console.WriteLine("Response: " + resp);
            var age = AnsiConsole.Ask<int>("how old are you?",21);

            

            var favorites = AnsiConsole.Prompt(
                new MultiSelectionPrompt<string>()
                    .PageSize(10)
                    .Title("What are your [green]favorite fruits[/]?")
                    .MoreChoicesText("[blue](Move up and down to reveal more fruits)[/]")
                    .InstructionsText("[blue](Press [blue][/] to toggle a fruit, [green][/] to accept)[/]")
                    .AddChoiceGroup("Berries", new[]
                    {
                        "Blackcurrant", "Blueberry", "Cloudberry",
                        "Elderberry", "Honeyberry", "Mulberry"
                    })
                    .AddChoices(new[]
                    {
                        "Apple", "Apricot", "Avocado", "Banana",
                        "Cherry", "Cocunut", "Date", "Dragonfruit", "Durian",
                        "Egg plant",  "Fig", "Grape", "Guava",
                        "Jackfruit", "Jambul", "Kiwano", "Kiwifruit", "Lime", "Lylo",
                        "Lychee", "Melon", "Nectarine", "Orange", "Olive"
                    }));

            var fruit = favorites.Count == 1 ? favorites[0] : null;
            if (string.IsNullOrWhiteSpace(fruit))
            {
                fruit = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Ok, but if you could only choose [green]one[/]?")
                        .MoreChoicesText("[blue](Move up and down to reveal more fruits)[/]")
                        .AddChoices(favorites));
            }

            AnsiConsole.MarkupLine("You selected: [yellow]{0}[/]", fruit);

            // Ask for the user's favorite fruit
            fruit = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("What's your [green]favorite fruit[/]?")
                    .PageSize(10)
                    .MoreChoicesText("[grey](Move up and down to reveal more fruits)[/]")
                    .AddChoices(new[] {
                        "Apple", "Apricot", "Avocado", 
                        "Banana", "Blackcurrant", "Blueberry",
                        "Cherry", "Cloudberry", "Cocunut",
                    }));

            // Echo the fruit back to the terminal
            AnsiConsole.WriteLine($"I agree. {fruit} is tasty!");



            ConsoleUtil.PressAnyKeyToContinue();
        }

        private void DevTest1()
        {

            ConsoleUtil.PressAnyKeyToContinue();
        }

    }
}