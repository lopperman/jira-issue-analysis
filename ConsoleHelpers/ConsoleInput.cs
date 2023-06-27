

using Spectre.Console;

namespace JiraCon
{
    public static class ConsoleInput
    {

        public static string IssueKeysToJQL()
        {
            string? retJQL = null;
            string colName = "key";
            string prefix = string.Format($"{JTISConfigHelper.config.defaultProject}-");
            var input = IssueKeys();
            if (input != null) 
            {
                char delimChar = input.Contains(',') ? ',' : ' ';
                retJQL = JQLBuilder.BuildInList(colName,input,delimChar,prefix);
            }

            return retJQL;
        }

        public static string? JQL(string? msg = null)
        {
            if (msg == null) 
            {
                msg = "Enter JQL Statement:";
            }
            var tJQL = ConsoleUtil.GetInput<string>(msg,allowEmpty:true);
            if (tJQL != null)
            {
                if (ConsoleUtil.Confirm(string.Format($"Use this JQL statement?{Environment.NewLine}\t{tJQL}{Environment.NewLine}"),true))
                {
                    return tJQL;
                }
            } 
            return null;

        }
        public static string? IssueKeys()
        {
            var p = new TextPrompt<string>($"[{StdLine.slResponse.FontMkp()} on {StdLine.slResponse.BackMkp()}]Enter 1 or more issue numbers, separated by a [underline]space or comma[/][/]{Environment.NewLine}[dim](Any values lacking a project prefix will have '{JTISConfigHelper.config.defaultProject}-' added (e.g. '100' becomes '{JTISConfigHelper.config.defaultProject}-100')[/]{Environment.NewLine}:");
            var keys = AnsiConsole.Prompt<string>(p);
            if (keys != null && keys.Length>0)
            {
                return keys;
            }
            return null;
        }

        


    }

}