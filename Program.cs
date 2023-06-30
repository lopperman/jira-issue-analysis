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
        private static void DevQuick()
        {
            try{        

                var tLogin = ConsoleUtil.GetInput<string>("Enter Jira Login email address");
                var tAPIToken = ConsoleUtil.GetInput<string>("Enter Jira API Token");
                AnsiConsole.MarkupLine($"[dim](Example of Url: https://yourcompany.Atlassian.net/)[/]");
                var tURL = ConsoleUtil.GetInput<string>("Enter Jira base URL {}");
                AnsiConsole.MarkupLine($"[dim](A Jira Project is usually the character that appear [italic]before[/] the number in a Jira Issue, such as 'WWT' in Jira Issues 'WWT-100')[/]");
                var tProj = ConsoleUtil.GetInput<string>("Enter Default Project Key");

            }
            catch (Exception e)
            {
                ConsoleUtil.WriteError(e.Message,false,e,false);
            }

            ConsoleUtil.PressAnyKeyToContinue();
        }


        public static void Main(string[] args) 
        {
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
