using Atlassian.Jira;
using JTIS.Config;
using JTIS.Console;
using Spectre.Console;

namespace JTIS.ManagedObjects
{
    public class AdvancedSearch
    {
        private JTISConfig cfg;

        private AdvancedSearch(JTISConfig config)
        {
            this.cfg = config;
        }
        public static AdvancedSearch Create()
        {
            return new AdvancedSearch(CfgManager.config);
        }

        internal bool ValidateQuery()
        {
            var result = false;
            

            return result;
        }

        internal void ViewJiraCustomFields()
        {

            var pr = new Progress(AnsiConsole.Console);
            pr.AutoClear(true);
            pr.AutoRefresh(true);
            pr.HideCompleted(false);
            pr.Columns(new ProgressColumn[]
            {
                new TaskDescriptionColumn(), 
                new ElapsedTimeColumn(), 
                new SpinnerColumn(Spinner.Known.BouncingBar).Style(new Style(Color.Blue3_1,Color.LightSkyBlue1)), 
            })
            .Start(ctx => 
            {
                var tsk1 = ctx.AddTask($"[dim blue on white] Getting custom fields [/]",false,maxValue:2);
                var tsk2 = ctx.AddTask($"[dim blue on white] Rendering fields [/]",false,maxValue:2);
                tsk1.StartTask();
                tsk1.Increment(1);

                var repo = JiraUtil.JiraRepo;
                IEnumerable<CustomField> fields = repo.GetJira().Fields.GetCustomFieldsAsync().GetAwaiter().GetResult();

                tsk1.Increment(1);
                tsk1.StopTask();
                tsk2.MaxValue(fields.Count());
                tsk2.StartTask();
                var tbl = new Table();
                tbl.AddColumns("Name","Id","CustomType","CustomIdentifier");
                Thread.Sleep(500);
                foreach (var field in fields.OrderBy(x=>x.Name))
                {
                    tsk2.Increment(1);
                    
                    tbl.AddRow(                                                
                        Markup.Escape($"{field.Name}"), 
                        Markup.Escape($"{field.Id}"), 
                        Markup.Escape($"{field.CustomIdentifier}"),
                        Markup.Escape($"{field.CustomIdentifier}")).Border(TableBorder.Rounded).HorizontalBorder();

                }
                AnsiConsole.Write(tbl);

            });         

            ConsoleUtil.PressAnyKeyToContinue();
        }

        internal void ViewJiraIssueFields()
        {
            // var pr = new Progress(AnsiConsole.Console);
            // pr.AutoClear(true);
            // pr.AutoRefresh(true);
            // pr.HideCompleted(false);
            // pr.Columns(new ProgressColumn[]
            // {
            //     new TaskDescriptionColumn(), 
            //     new ElapsedTimeColumn(), 
            //     new SpinnerColumn(Spinner.Known.BouncingBar).Style(new Style(Color.Blue3_1,Color.LightSkyBlue1)), 
            // })
            // .Start(ctx => 
            // {
            //     var tsk1 = ctx.AddTask($"[dim blue on white] Getting issue fields [/]",false,maxValue:2);
            //     var tsk2 = ctx.AddTask($"[dim blue on white] Rendering fields [/]",false,maxValue:2);
            //     tsk1.StartTask();
            //     tsk1.Increment(1);

            //     var repo = JiraUtil.JiraRepo;
            //     IEnumerable<IssueFields> fields = repo.GetJira().Issues.ValidateQuery()..GetCustomFieldsAsync().GetAwaiter().GetResult();

            //     tsk1.Increment(1);
            //     tsk1.StopTask();
            //     tsk2.MaxValue(fields.Count());
            //     tsk2.StartTask();
            //     var tbl = new Table();
            //     tbl.AddColumns("Name","Id","CustomType","CustomIdentifier");
            //     Thread.Sleep(500);
            //     foreach (var field in fields.OrderBy(x=>x.Name))
            //     {
            //         tsk2.Increment(1);
                    
            //         tbl.AddRow(                                                
            //             Markup.Escape($"{field.Name}"), 
            //             Markup.Escape($"{field.Id}"), 
            //             Markup.Escape($"{field.CustomIdentifier}"),
            //             Markup.Escape($"{field.CustomIdentifier}")).Border(TableBorder.Rounded).HorizontalBorder();

            //     }
            //     AnsiConsole.Write(tbl);

            // });         

            // ConsoleUtil.PressAnyKeyToContinue();            
        }
    }
}