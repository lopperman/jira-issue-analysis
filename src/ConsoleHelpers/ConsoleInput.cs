

using JTIS.Config;
using JTIS.Console;
using Spectre.Console;

namespace JTIS
{
    public static class ConsoleInput
    {

        public static string GetJQLOrIssueKeys(bool allowFromSaved, bool findEpicLinks = false, bool manualJQLValidation = false)
        {
            if (findEpicLinks)
            {
                AnsiConsole.MarkupLine("[bold underline]\tNOTE: [/][bold]Any Epic type issues returned will automatically find and return all related issues[/]");
            }
            if (allowFromSaved == false)
            {
                var rLine = new Rule("[bold blue on white]ENTER JQL OR LIST OF ISSUE KEYS[/]").Centered();
                AnsiConsole.Write(rLine);
                AnsiConsole.MarkupLine("Enter a valid [bold]JQL statement[/], or a list of [bold]delimited issue Keys[/]");
                AnsiConsole.MarkupLine($"\t[dim italic](If entering list of issue keys for current project, '{CfgManager.config.defaultProject}-' will be prepended automatically if missing -- '100' becomes '{CfgManager.config.defaultProject}-100')[/]");
                AnsiConsole.MarkupLine($"\t[dim]Example of valid issue keys:  100, 101, 102[/]");
                AnsiConsole.MarkupLine($"\t[dim]Example of valid issue keys:  100 101 102[/]");
                AnsiConsole.MarkupLine($"\t[dim]Example of valid issue keys:  WWT-100 QA-101 102[/]");
                var data = ConsoleUtil.GetInput<string>("JQL or Issue List:",allowEmpty:true);                
                if (data.Length > 0)
                {
                    bool isJQL = JQLUtil.JQLSyntax(data);
                    if (isJQL == false)
                    {
                        data = IssueKeysToJQL(data);
                    }
                    if (!manualJQLValidation)
                    {
                        return data;
                    }
                    if (JQLUtil.ValidJQL(data)==false)
                    {
                        ConsoleUtil.WriteError($"The JQL you entered is not valid ({data})",pause:true);
                        return string.Empty;
                    }
                    ConsoleUtil.WriteStdLine($"[dim] * JQL QUERY * [/]{Environment.NewLine}\t{data}",StdLine.slInfo);
                    if (ConsoleUtil.Confirm($"Would you like to save this [bold]JQL Query[/] to use in the future?",false,true))
                    {
                        var saveName = isJQL ? $"JQL for:" : "Issue List for:";
                        saveName = ConsoleUtil.GetInput<string>("Enter short desc:",saveName,true);
                        if (saveName.Length > 0){
                            CfgManager.config.AddJQL(saveName,data);
                            return data;
                        }
                        else 
                        {
                            return string.Empty;
                        }
                    }
                    return data;
                }
                else 
                {
                    return string.Empty;
                }
            }
            else 
            {
                if (CfgManager.config.SavedJQLCount > 0)
                {
                    AnsiConsole.Clear();
                    JQLUtil.ViewSavedJQL(CfgManager.config,false);
                    var sjqlId = ConsoleUtil.GetInput<string>("Enter Saved JqlId, or press 'ENTER' to manually create a filter",allowEmpty:true);
                    int jqlId = 0;
                    int.TryParse(sjqlId, out jqlId);
                    if (jqlId < 1 || jqlId > CfgManager.config.SavedJQLCount)
                    {
                        return GetJQLOrIssueKeys(false);
                    }
                    else 
                    {
                        var savedJQL = CfgManager.config.SavedJQL.FirstOrDefault(x=>x.jqlId == jqlId);
                        if (savedJQL != null)
                        {
                            if (JQLUtil.JQLSyntax(savedJQL.jql)==false)
                            {
                                savedJQL.jql = IssueKeysToJQL(savedJQL.jql);
                            }
                            return savedJQL.jql;
                        }
                    }
                }
            }
            return string.Empty;
        }
        private static string IssueKeysToJQL(string? previousInput = null)
        {
            string? retJQL = null;
            string colName = "key";
            string prefix = string.Format($"{CfgManager.config.defaultProject}-");
            string input = string.Empty;
            if (previousInput != null)
            {
                if (JQLUtil.JQLSyntax(previousInput))
                {
                    return previousInput;
                }
                input = previousInput;
            }
            else 
            {
                return GetJQLOrIssueKeys(true);
            }

            if (input != null) 
            {
                char delimChar = input.Contains(',') ? ',' : ' ';
                retJQL = JQLBuilder.BuildInList(colName,input,delimChar,prefix);
            }

            return retJQL;
        }


        public static string? IssueKeys()
        {
            var p = new TextPrompt<string>($"[{StdLine.slResponse.FontMkp()} on {StdLine.slResponse.BackMkp()}]Enter 1 or more issue numbers, separated by a [underline]space or comma[/][/]{Environment.NewLine}[dim](Any values lacking a project prefix will have '{CfgManager.config.defaultProject}-' added (e.g. '100' becomes '{CfgManager.config.defaultProject}-100')[/]{Environment.NewLine}:");
            var keys = AnsiConsole.Prompt<string>(p);
            if (keys != null && keys.Length>0)
            {
                return keys;
            }
            return null;
        }

        


    }

}