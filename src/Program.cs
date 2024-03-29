﻿using JTIS.Analysis;
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
           var jc = AnsiConsole.Console;
           System.Console.ForegroundColor = Color.Maroon;
           jc.WriteLine("testing testing");

// ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ 
// ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ ~~~ 
            ConsoleUtil.PressAnyKeyToContinue();
            Console.ConsoleUtil.ByeByeForced();
        }


        public static void Main(string[] args) 
        {
             
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

            if (JTIS.Info.IsDev)
            {
                ConsoleUtil.ScrubData = ConsoleUtil.Confirm("DEV: SCRUB DATA?",false); 
            }
            //ConsoleUtil.WriteAppTitle();
            ConsoleUtil.WriteAppHello();

            AnsiConsole.Write(new Rule());




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
                        JQLUtil.CheckDefaultJQL(newConfig);
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
