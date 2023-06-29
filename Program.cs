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

            string? s1 = null;
            string s2 = "1";
            string s3 = "A";
            string s4 = "001";
            int tst = 0;
            try 
            {
                tst = -1;
                AnsiConsole.WriteLine("s1 (null)");
                tst = -1;
                int.TryParse(s1, out tst);
                AnsiConsole.WriteLine($"tst value: {tst}");

                tst = -1;
                AnsiConsole.WriteLine("s2 (1)");
                int.TryParse(s2, out tst);
                AnsiConsole.WriteLine($"tst value: {tst}");

                tst = -1;
                AnsiConsole.WriteLine("s3 (A)");
                int.TryParse(s3, out tst);
                AnsiConsole.WriteLine($"tst value: {tst}");

                tst = -1;
                AnsiConsole.WriteLine("s4 (001)");
                int.TryParse(s4, out tst);
                AnsiConsole.WriteLine($"tst value: {tst}");

            }
            catch (Exception e)
            {
                ConsoleUtil.WriteError(e.Message,false,e,false);
            }

            ConsoleUtil.PressAnyKeyToContinue();
        }


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
                    JTISConfigHelper.CheckDefaultJQL();
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
