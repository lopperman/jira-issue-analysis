using System.Diagnostics;
using System.Text;
using Atlassian.Jira;
using JTIS.Config;
using JTIS.Console;
using JTIS.Extensions;
using JTIS.Menu;
using Newtonsoft.Json;
using Spectre.Console;

namespace JTIS
{
    class MainClass
    {
        ///QUICK TESTING AREA - USE COMMAND LINE ARG 'DEV'
        private static void RenderManualConfigStatus(string login, string apiToken, string baseUrl, string proj)
        {
                ConsoleUtil.WriteAppTitle();
                var p = new Panel($" :llama: [bold underline]CREATE NEW JIRA CONFIGURATION[/]  ");
                p.Expand();
                p.Border(BoxBorder.None);
                p.HeaderAlignment(Justify.Left);

                // TableRow hdr = new TableRow
                //     (
                //         new Markup[]
                //             {
                //                 new Markup($"[dim]LOGIN[/]").Centered(), 
                //                 new Markup($"[dim]API TOKEN[/]").Centered(), 
                //                 new Markup($"[dim]JIRA ROOT URL[/]").Centered(), 
                //                 new Markup($"[dim]DEFAULT PROJECT KEY[/]").Centered()
                //             }
                //     );                
                // TableRow det = new TableRow
                //     (
                //         new Markup[]
                //             {
                //                 new Markup($"[bold]{login}[/]").Centered(), 
                //                 new Markup($"[bold]{apiToken}[/]").LeftJustified(), 
                //                 new Markup($"[bold]{baseUrl}[/]").Centered(), 
                //                 new Markup($"[bold]{proj}[/]").Centered()
                //             }
                //     );

                var tbl = new Table().Border(TableBorder.Horizontal);
                tbl.AddColumn(new TableColumn($"[dim]LOGIN[/]").Alignment(Justify.Center)).HorizontalBorder();
                tbl.AddColumn(new TableColumn($"[dim]API TOKEN[/]").Alignment(Justify.Center)).HorizontalBorder();
                tbl.AddColumn(new TableColumn($"[dim]JIRA ROOT URL[/]").Alignment(Justify.Center)).HorizontalBorder();
                tbl.AddColumn(new TableColumn($"[dim]DEFAULT PROJECT KEY[/]").Alignment(Justify.Center)).HorizontalBorder();
                // tbl.AddRow(
                //     new Markup($"[bold]{login}[/]"), 
                //     new Markup($"[bold]{apiToken}[/]").LeftJustified(), 
                //     new Markup($"[bold]{baseUrl}[/]"), 
                //     new Markup($"[bold]{proj}[/]"));
                AnsiConsole.Write(p);
                AnsiConsole.Write(tbl);

        }
        private static void DevQuick()
        {
            try{        

                string tLogin = string.Empty;
                string tAPIToken = string.Empty;
                string tURL = string.Empty;
                string tProj = string.Empty;

                RenderManualConfigStatus(tLogin,tAPIToken,tURL,tProj);
                tLogin = ConsoleUtil.GetInput<string>("Enter Jira Login");

                RenderManualConfigStatus(tLogin,tAPIToken,tURL,tProj);
                tAPIToken = ConsoleUtil.GetInput<string>("Enter Jira API Token");

                RenderManualConfigStatus(tLogin,tAPIToken,tURL,tProj);
                AnsiConsole.Write(new Rule());                
                AnsiConsole.MarkupLine($"[dim](Example of Url: https://yourcompany.Atlassian.net/)[/]");
                tURL = ConsoleUtil.GetInput<string>("Enter Jira base URL");

                RenderManualConfigStatus(tLogin,tAPIToken,tURL,tProj);
                AnsiConsole.Write(new Rule());                
                AnsiConsole.MarkupLine($"[dim](A Jira Project is usually the characters that appear [italic]before[/] the number in a Jira Issue, such as 'WWT' in Jira Issues 'WWT-100')[/]");
                tProj = ConsoleUtil.GetInput<string>("Enter Default Project Key");
                
                RenderManualConfigStatus(tLogin,tAPIToken,tURL,tProj);

                if (ConsoleUtil.Confirm($"A successful connection is needed to verify the information you provided.{Environment.NewLine}[bold]Attempt to authenticate to Jira now?[/]",true,true))
                {

                }
                




        //         var tbl = new Table();
        //         tbl.Border(TableBorder.None);
        //         var llamas = $"[white on deepskyblue4_2]     :cool_button:   :llama::llama:   :cool_button:     [/]";
        //         var fill2  = $"[white on deepskyblue4_2]                        [/]";
        //         var tblcol = new TableColumn("").Alignment(Justify.Left);
        //         var title = $"[white on deepskyblue4_2] :smiling_face_with_sunglasses: HAVE A GREAT DAY :smiling_face_with_sunglasses: [/]";
        //         tbl.AddColumn(tblcol).Centered();
        //         tbl.AddRow(fill2);
        //         tbl.AddRow(llamas);
        //         tbl.AddRow(fill2);
        //         // tbl.AddEmptyRow();            
        //         tbl.AddRow(title);
        //         tbl.AddRow(fill2);
        //         tbl.AddEmptyRow();            
        //         tbl.Columns[0].Centered();
        // //            var mk2 = new Markup(title).Centered();
        // //            AnsiConsole.MarkupLine("   :llama:  :llama:  :llama:  ");
                
        //         var panel = new Panel(tbl).Border(BoxBorder.Rounded).BorderColor(AnsiConsole.Foreground);
        // //            panel.Padding(2,0,2,0);

        //         AnsiConsole.Write(panel);                

            }
            catch (Exception e)
            {
                ConsoleUtil.WriteError(e.Message,false,e,false);
            }

            ConsoleUtil.PressAnyKeyToContinue();
        }


        public static void Main(string[] args) 
        {
            
            // System.Console.ForegroundColor = Color.Black;
            // System.Console.BackgroundColor = Color.LightYellow3;

            List<JTISConfig> tmpCfgList  = new List<JTISConfig>();
            var tmpConfigFilePath = string.Empty;
            if (args.Length == 1)            
            {
                if(args[0].StringsMatch("dev"))
                {
                    DevQuick(); 
                    ConsoleUtil.ByeByeForced();                   
                }
                else
                {
                    var manualFilePath = CfgManager.CheckManualFilePath(args[0]);
                    if (!string.IsNullOrWhiteSpace(manualFilePath))
                    {
                        tmpConfigFilePath = manualFilePath;
                    }
                    else 
                    {
                        tmpConfigFilePath = CfgManager.ConfigFilePath;
                    }
                }
            }
            else 
            {
                tmpConfigFilePath = CfgManager.ConfigFilePath;
            }

            var tmpConfigs = CfgManager.ReadConfigFile(tmpConfigFilePath);
            if (tmpConfigs == null)
            {
                var link = new Text("https://support.atlassian.com/atlassian-account/docs/manage-api-tokens-for-your-atlassian-account/");
                AnsiConsole.MarkupLine($"[bold](You will need to have your Jira API Token to create a new connection profile.  see this link if you need help creating one:[/] {Environment.NewLine}[dim italic]\t{link}[/])");
                if (ConsoleUtil.Confirm("Add a new Jira Connection Profile?",true))
                {
                    
                    var newConfig = JTISConfig.ManualCreate();
                    if (newConfig !=  null) 
                    {
                        CfgManager.config = newConfig;

                    }
                }
                else 
                {
                    ConsoleUtil.ByeByeForced();
                }
            }
            else 
            {
                CfgManager.SetConfigList(tmpConfigs); 
            }

            if (CfgManager.config != null)
            {
                MenuManager.Start(CfgManager.config);
            }
            ConsoleUtil.ByeByeForced();
        }


    }
}
