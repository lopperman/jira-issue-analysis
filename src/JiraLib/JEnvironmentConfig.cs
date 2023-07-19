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
