using JTIS.Config;
using JTIS.Console;
using JTIS.Extensions;
using JTIS.Menu;
using Spectre.Console;

namespace JTIS
{
    class MainClass
    {
        private static void DevTestAsync(string cfgPath)
        {

            
            ConsoleUtil.PressAnyKeyToContinue();

        }

        ///QUICK TESTING AREA - USE COMMAND LINE ARG 'DEV'
        private static void DevQuick()
        {
            var r1 = new Tree("Brower Family");
            r1.Guide(TreeGuide.BoldLine);
            var r1n1 = r1.AddNode("Paul");
            
            var r1n2 = r1.AddNode("Emily");
            var r1n1n1 = r1n1.AddNode("Ethan");
            var r1n1n2 = r1n1.AddNode("Chase");
            
            var r1n2n1 = new Panel($":llama::llama:{Environment.NewLine}Elena").HeaderAlignment(Justify.Center).Border(BoxBorder.Rounded).BorderColor(Color.DeepPink1_1);
            r1n2n1.Padding(2,0,2,0);
            r1n2.AddNode(r1n2n1);

            var r1r1 = r1.AddNode(new Tree("Pets"));

            
            

            AnsiConsole.Write(r1);





// ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ 
// ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ 
            ConsoleUtil.PressAnyKeyToContinue();
            Console.ConsoleUtil.ByeByeForced();
        }


        public static void Main(string[] args) 
        {
            
            ConsoleUtil.WriteAppTitle();
            AnsiConsole.Write(new Rule());

            List<JTISConfig> tmpCfgList  = new List<JTISConfig>();
            var tmpConfigFilePath = string.Empty;
            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    var arg = args[i];
                    AnsiConsole.WriteLine($"processing argument: {arg}");
                    if (arg.StringsMatch("dev"))
                    {
                        DevQuick();
                    }        
                    if (arg.StringsMatch("async"))
                    {
                        if (args.Length-1 > i)
                        {
                            if (args[i+1].EndsWith(".json"))
                            {
                                DevTestAsync(args[i+1]);
                                ConsoleUtil.ByeByeForced();
                            }
                        }
                    }
                    else if (CfgManager.CheckManualFilePath(arg)!=null)
                    {
                        tmpConfigFilePath=arg;
                        CfgManager.JTISConfigFilePath=tmpConfigFilePath;
                    }
                    else 
                    {
                        System.Console.Beep();
                        ConsoleUtil.PressAnyKeyToContinue($"argument '{arg}' was ignored");
                    }
                }
            }
            if (tmpConfigFilePath.Length == 0)
            {
                tmpConfigFilePath=CfgManager.ConfigFilePath;
                AnsiConsole.MarkupLine($"using default Jira Config file path: {tmpConfigFilePath}");
                Thread.Sleep(300);
            }
            var tmpConfigs = CfgManager.ReadConfigFile(tmpConfigFilePath);
            if (tmpConfigs == null)
            {
                var link = new Text("https://support.atlassian.com/atlassian-account/docs/manage-api-tokens-for-your-atlassian-account/");
                AnsiConsole.MarkupLine($"[bold](You will need to have your Jira API Token to create a new connection profile.  see link below if you need help creating one)[/]{Environment.NewLine}[dim italic]https://support.atlassian.com/atlassian-account/docs/manage-api-tokens-for-your-atlassian-account/[/]");
                if (ConsoleUtil.Confirm("Add a new Jira Connection Profile?",true))
                {
                    
                    var newConfig = CfgManager.CreateConfig(addToConfigList:false);
                    if (newConfig !=  null) 
                    {                        
                        var newConfigList = new List<JTISConfig>();
                        newConfigList.Add(newConfig);
                        CfgManager.SetConfigList(newConfigList);
                        CfgManager.config = newConfig;
                        CfgManager.CheckDefaultJQL(newConfig);
                        CfgManager.SaveConfigList();
                        ConsoleUtil.PressAnyKeyToContinue($"New configuration file has been created at '{CfgManager.ConfigFilePath}'");
                    }
                }
                else 
                {
                    ConsoleUtil.ByeByeForced();
                }
            }

            if (tmpConfigs != null && tmpConfigs.Count > 0) 
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
