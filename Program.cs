using Atlassian.Jira;
using Newtonsoft.Json;
using Spectre.Console;

namespace JiraCon
{
    class MainClass
    {
        ///QUICK TESTING AREA - USE COMMAND LINE ARG 'DEV'
        private static void DevQuick()
        {        
            ConsoleUtil.PressAnyKeyToContinue();
        }

        //Valid Args are either empty, or a single arg which is the filepath to your desired config file
        public static void Main(string[] args) 
        {
            if (args.Length == 1 && args[0].ToUpper()=="DEV")
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
