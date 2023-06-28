using System.Diagnostics;
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

            var j = new JQLConfig();
            j.jql = "a b c d e";
            Debug.Assert(j.JQLSyntax == false);
            j.jql = "project=wwt and status=priority";
            Debug.Assert(j.JQLSyntax == true);
            j.jql = "status in (priority, blocked)";
            Debug.Assert(j.JQLSyntax == true);
            j.jql = "status <>> (priority, blocked)";
            Debug.Assert(j.JQLSyntax == true);


            //StringExt.StringsMatchTest();
        }

        //Valid Args are either empty, or a single arg which is the filepath to your desired config file
        public static void Main(string[] args) 
        {
            if (args.Length == 1 && args[0].Equals("dev",StringComparison.OrdinalIgnoreCase))
            {
                DevQuick();
                return;                
            }
            bool requireManualConfig = false ;
            if (args!=null && args.Length == 1)
            {
                if (JTISConfigHelper.ValidateConfigFileArg(args[0])==false)
                {
                    return;
                }
                else 
                {
                    JTISConfigHelper.JTISConfigFilePath = args[0];
                }
            }
            if (args == null || args.Length == 0)
            {
                if (JTISConfigHelper.ValidateConfigFileArg(JTISConfigHelper.ConfigFilePath)==false) 
                {
                    return;
                }
            }
            if (File.Exists(JTISConfigHelper.ConfigFilePath))
            {
                JTISConfigHelper.ReadConfigList();
            }
            else 
            {
                return;
            }

            if (JTISConfigHelper.Configs.Count > 0)
            {
                JTISConfig? changeCfg = JTISConfigHelper.ChangeCurrentConfig(null);
                if (changeCfg != null && changeCfg.configId > 0)
                {
                    JTISConfigHelper.config = changeCfg;
                }
            }
            else 
            {
                return;
            }

            if (JTISConfigHelper.config==null)
            {
                requireManualConfig = true;
            }

            if (requireManualConfig==true)
            {
                JTISConfig? manualConfig = JTISConfigHelper.CreateConfig(); 
                if (manualConfig != null)
                {
                    JTISConfigHelper.config = manualConfig;
                }
            }
            if (JTISConfigHelper.config != null)
            {
                MenuManager.Start(JTISConfigHelper.config);
            }
            ConsoleUtil.ByeByeForced();
        }
    }
}
