using System.Text.RegularExpressions;


using JTIS.Config;
using JTIS.Console;
using Spectre.Console;

namespace JTIS
{
    public static class ConsoleInput
    {

        public static string GetJQLOrIssueKeys(bool findEpicLinks = false, bool manualJQLValidation = false)
        {
            if (findEpicLinks)
            {
                var rLine = new Rule("[bold blue on white]ENTER JQL OR LIST OF ISSUE KEYS[/]").Centered();
                AnsiConsole.MarkupLine("[dim underline blue on cornsilk1]\tNOTE: [bold]Any Epic type issues returned will automatically find and return all related issues[/][/]");
            }
            if (CfgManager.config.SavedJQLCount == 0) {JQLUtil.CheckDefaultJQL(CfgManager.config);}

            AnsiConsole.Clear();
            ConsoleUtil.WriteAppTitle();
            JQLUtil.ViewSavedJQL(CfgManager.config,false);
            string jqlIdHelp = $"Enter Saved JQL Id ('1'-'{CfgManager.config.SavedJQLCount}'), ** OR ** Enter a valid JQL statement, ** OR ** Enter 1 or more Issue Keys separated by a SPACE";

            var jqlHelp = new Markup($"[dim italic](If entering list of issue keys for current project, '{CfgManager.config.defaultProject}-' will be prepended automatically if missing -- '100' becomes '{CfgManager.config.defaultProject}-100')[/]{Environment.NewLine}[bold]NOTE: If querying a single issue (e.g. '5') that has a number matching a JQL Snippet Id, type the full issue key.[/]{Environment.NewLine}[dim]Example of valid issue keys:  100, 101, 102[/]{Environment.NewLine}[dim]Example of valid issue keys:  100 101 102[/]{Environment.NewLine}[dim]Example of valid issue keys:  WWT-100 QA-101 102[/]");

            var r = new Rule();
            r.Style = new Style(Color.Blue,Color.Cornsilk1).Decoration(Decoration.Dim);
            r.Border(BoxBorder.Heavy);            
            AnsiConsole.Write(r);

            AnsiConsole.Write(new Panel(jqlHelp).Header($"[dim]Help - Entering Issue Id's)[/]").Border(BoxBorder.None));
            var tmpResponse = ConsoleUtil.GetInput<string>($"{jqlIdHelp}",allowEmpty:true);
            if (tmpResponse.Trim().Length == 0)
            {
                return string.Empty;
            }

            var data = string.Empty;
            var reg1 = new Regex("^\\d+$",RegexOptions.Singleline);
            if (reg1.IsMatch(tmpResponse))
            {
                int selectedJqlId = 0;
                if (int.TryParse(tmpResponse, out selectedJqlId))
                {
                    if (selectedJqlId > 0 && selectedJqlId <= CfgManager.config.SavedJQLCount)
                    {
                        data = CfgManager.config.GetJQLConfig(selectedJqlId).jql;
                    }
                }
            }
            if (data.Length==0)
            {
                bool isJQL = JQLUtil.JQLSyntax(tmpResponse);
                if (isJQL == false)
                {
                    data = IssueKeysToJQL(tmpResponse);
                }
                else 
                {
                    data = tmpResponse;
                }
                if (data.Length > 0)
                {
                    if (JQLUtil.ValidJQL(data,manualJQLValidation)==false)
                    {
                        if (manualJQLValidation)
                        {
                            return string.Empty;
                        }
                        ConsoleUtil.WriteError($"The JQL you entered is not valid ({data})",pause:true);
                        return string.Empty;
                    }
                    ConsoleUtil.WriteStdLine($"[dim] * JQL QUERY * [/]{Environment.NewLine}\t{data}",StdLine.slInfo);
                    if (ConsoleUtil.Confirm($"Would you like to save this [bold]JQL Query[/] to use in the future?",false))
                    {
                        var saveName = $"Save JQL for: " ;
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
            }
            return data;
        }


            // if (allowFromSaved == false)
            // {
            //     var rLine = new Rule("[bold blue on white]ENTER JQL OR LIST OF ISSUE KEYS[/]").Centered();
            //     AnsiConsole.Write(rLine);
            //     AnsiConsole.MarkupLine("Enter a valid [bold]JQL statement[/], or a list of [bold]delimited issue Keys[/]");
            //     AnsiConsole.MarkupLine($"\t[dim italic](If entering list of issue keys for current project, '{CfgManager.config.defaultProject}-' will be prepended automatically if missing -- '100' becomes '{CfgManager.config.defaultProject}-100')[/]");
            //     AnsiConsole.MarkupLine($"\t[dim]Example of valid issue keys:  100, 101, 102[/]");
            //     AnsiConsole.MarkupLine($"\t[dim]Example of valid issue keys:  100 101 102[/]");
            //     AnsiConsole.MarkupLine($"\t[dim]Example of valid issue keys:  WWT-100 QA-101 102[/]");
            //     var data = ConsoleUtil.GetInput<string>("JQL or Issue List:",allowEmpty:true);                
            //     if (data.Length > 0)
                // {
                    // bool isJQL = JQLUtil.JQLSyntax(data);
                    // if (isJQL == false)
                    // {
                    //     data = IssueKeysToJQL(data);
                    // }
                    // if (!manualJQLValidation)
                    // {
                    //     return data;
                    // }
            //         if (JQLUtil.ValidJQL(data)==false)
            //         {
            //             ConsoleUtil.WriteError($"The JQL you entered is not valid ({data})",pause:true);
            //             return string.Empty;
            //         }
            //         ConsoleUtil.WriteStdLine($"[dim] * JQL QUERY * [/]{Environment.NewLine}\t{data}",StdLine.slInfo);
            //         if (ConsoleUtil.Confirm($"Would you like to save this [bold]JQL Query[/] to use in the future?",false))
            //         {
            //             var saveName = isJQL ? $"JQL for:" : "Issue List for:";
            //             saveName = ConsoleUtil.GetInput<string>("Enter short desc:",saveName,true);
            //             if (saveName.Length > 0){
            //                 CfgManager.config.AddJQL(saveName,data);
            //                 return data;
            //             }
            //             else 
            //             {
            //                 return string.Empty;
            //             }
            //         }
            //         return data;
            //     }
            //     else 
            //     {
            //         return string.Empty;
            //     }
            // }
            // else 
            // {
            //     if (CfgManager.config.SavedJQLCount > 0)
            //     {
            //         AnsiConsole.Clear();
            //         JQLUtil.ViewSavedJQL(CfgManager.config,false);
            //         var sjqlId = ConsoleUtil.GetInput<string>("Enter Saved JqlId, or press 'ENTER' to manually create a filter",allowEmpty:true);
            //         int jqlId = 0;
            //         int.TryParse(sjqlId, out jqlId);
            //         if (jqlId < 1 || jqlId > CfgManager.config.SavedJQLCount)
            //         {
            //             return GetJQLOrIssueKeys(false);
            //         }
            //         else 
            //         {
            //             var savedJQL = CfgManager.config.SavedJQL.FirstOrDefault(x=>x.jqlId == jqlId);
            //             if (savedJQL != null)
            //             {
            //                 if (JQLUtil.JQLSyntax(savedJQL.jql)==false)
            //                 {
            //                     savedJQL.jql = IssueKeysToJQL(savedJQL.jql);
            //                 }
            //                 return savedJQL.jql;
            //             }
            //         }
            //     }
        //     }
        //     // return string.Empty;
        // }

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