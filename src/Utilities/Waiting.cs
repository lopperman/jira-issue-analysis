using Spectre.Console;

namespace JTIS.Waiter
{

    public class WaitProgress
    {
        static WaitProgress()
        {

        }
        public static WaitProgress Create()
        {
            var wp = new WaitProgress();


            return wp;
        }

        public void ShowSimpleWait(string message, Action action)
        {
            var spinCol = new SpinnerColumn(Spinner.Known.Dots);
            spinCol.PendingText="please wait ... ";
            spinCol.CompletedText = "completed";
            spinCol.Style(new Style(Color.Black, Color.Cornsilk1));
            var elapTimeCol = new ElapsedTimeColumn();
            var infoCol = new TaskDescriptionColumn();
    
            var status = new Progress(AnsiConsole.Console);
            status.AutoClear = true;
            status.HideCompleted = true;
            status.Columns(infoCol, spinCol, elapTimeCol);
            status.Start(ctx=> 
                {
                    var task = ctx.AddTask(message);
                    task.Increment(1);
                    action.Invoke();
                    Thread.Sleep(500);                
                }
            );


   
        }

    }

}