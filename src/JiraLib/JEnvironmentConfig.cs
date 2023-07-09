using System;
using System.Collections.Generic;
using System.Linq;
using Atlassian.Jira;
using JTIS.Console;
using JTIS.Waiter;
using Spectre.Console;

namespace JTIS 
{
    public static class JEnvironmentConfig
    {
        public static void JiraEnvironmentInfo()
        {
            WaitProgress wp = WaitProgress.Create();
            wp.ShowSimpleWait("querying server info",GetServerInfo);
            ConsoleUtil.PressAnyKeyToContinue();
        }

        // private static void GetIssueTypesForProject()
        // {
        //     ConsoleUtil.WriteStdLine("***** PROJECTS *********", StdLine.slOutputTitle);

        //     var projects = JiraUtil.JiraRepo.GetJira().Projects.GetProjectsAsync().GetAwaiter().GetResult();
        //     List<Project> projectList = new List<Project>();
        //     projectList.AddRange(projects);
        //     projectList = projectList.OrderBy(x => x.Key).ToList();

        //     var table = new ConsoleTable("Key","Name","Id");

        //     foreach (var p in projectList)
        //     {
        //         table.AddRow(p.Key, p.Name, p.Id);
        //     }
        //     table.Write();

        //     ConsoleUtil.WriteStdLine("",StdLine.slOutput);
        //     ConsoleUtil.WriteStdLine("Would you like to see valid issue types for one or more of the above projects?",StdLine.slResponse);
        //     ConsoleUtil.WriteStdLine("Press 'Y' to Enter 1 or more Project Key, otherwise PRESS ANY KEY TO CONTINUE", StdLine.slResponse);
        //     var key = Console.ReadKey(true);
        //     if (key.Key == ConsoleKey.Y)
        //     {
        //         while (true)
        //         {
        //             ConsoleUtil.WriteStdLine("Enter 1 or more project keys (listed above) separated by a space (e.g. POS BAM)",StdLine.slResponse);
        //             var read = ConsoleUtil.GetInput<string>()
        //             if (read != null && read.Length > 0)
        //             {
        //                 string[] keys = read.ToUpper().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        //                 bool invalid = false;
        //                 foreach (var k in keys)
        //                 {
        //                     if (projectList.Where(y => y.Key != null && y.Key == k).FirstOrDefault() == null)
        //                     {
        //                         invalid = true;
        //                         ConsoleUtil.WriteError(string.Format("'{0}' is not a valid project. Try again.",k));
        //                         ConsoleUtil.PressAnyKeyToContinue();
        //                     }
        //                 }
        //                 if (!invalid)
        //                 {
        //                     foreach (var k in keys)
        //                     {
        //                         var issueTypes = JiraUtil.JiraRepo.GetIssueTypes(k).OrderBy(x => x.Name);
        //                         ConsoleUtil.WriteStdLine("",StdLine.slCode);
        //                         ConsoleUtil.WriteStdLine(string.Format("***** PROJECT '{0}' ISSUE TYPES *********",k), StdLine.slOutputTitle);

        //                         table = new ConsoleTable("Proj","Id","Name","Description","IsSubTask");
        //                         foreach (var type in issueTypes)
        //                         {
        //                             table.AddRow(string.Format("({0})",k), type.Id, type.Name, type.Description, type.IsSubTask);
        //                         }
        //                         table.Write();
        //                         ConsoleUtil.PressAnyKeyToContinue();

        //                     }
        //                     break;
        //                 }
        //             }
        //         }
        //     }

        //     ConsoleUtil.PressAnyKeyToContinue();

        // }




        private static void GetServerInfo()
        {
            var repo = JiraUtil.JiraRepo;

            

            ConsoleUtil.WriteAppTitle();
            AnsiConsole.Write(new Rule());
            var tbl = new Table();
            tbl.AddColumn("Key").RightAligned().Border(TableBorder.Heavy).Alignment(Justify.Right);
            tbl.AddColumn("Value").RightAligned().Border(TableBorder.Heavy).Alignment(Justify.Left);
            tbl.AddRow(new Text("Base Url:"), new Markup($"[bold]{repo.ServerInfo.BaseUrl}[/]"));
            tbl.AddRow(new Text("Build:"), new Markup($"[bold]{repo.ServerInfo.BuildNumber}[/]"));
            tbl.AddRow(new Text("Deployment Type:"), new Markup($"[bold]{repo.ServerInfo.DeploymentType}[/]"));
            tbl.AddRow(new Text("Server Time:"), new Markup($"[bold]{repo.ServerInfo.ServerTime}[/]"));
            tbl.AddRow(new Text("Server Title:"), new Markup($"[bold]{repo.ServerInfo.ServerTitle}[/]"));
            tbl.AddRow(new Text("Version:"), new Markup($"[bold]{repo.ServerInfo.Version}[/]"));
            AnsiConsole.Write(tbl);

        }


    }
}
