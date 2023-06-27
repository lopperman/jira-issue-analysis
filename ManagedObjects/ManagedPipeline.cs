using System;
using Spectre.Console;

namespace JTIS.ManagedObjects
{



    public class ManagedPipeline
    {
        private List<string> _taskMessages = new List<string>();
        private List<Action> _taskActions = new List<Action>();

        public ManagedPipeline Add(string taskMsg,Action taskAction)
        {
            _taskMessages.Add(taskMsg);
            _taskActions.Add(taskAction);
            return this;
        }
        public void ExecutePipeline()
        {
            if (_taskActions.Count == 0) {return;}

            List<ProgressTask> tasks = new List<ProgressTask>();

            var pr = new Progress(AnsiConsole.Console);
            pr.AutoClear(true);
            pr.AutoRefresh(true);
            pr.HideCompleted(false);
            pr.Columns(new ProgressColumn[]
            {
                new TaskDescriptionColumn(), 
                new ProgressBarColumn(), 
                new ElapsedTimeColumn(), 
                new SpinnerColumn(Spinner.Known.BouncingBar).Style(new Style(Color.Blue3_1,Color.LightSkyBlue1)), 
            })
            .Start(ctx => 
            {
                var actionArr = _taskActions.ToArray();
                var msgArr = _taskMessages.ToArray();
                for (int i = 0; i < actionArr.Length; i ++)
                {
                    var tStg = new ProgressTaskSettings();
                    tStg.AutoStart = false;
                    tStg.MaxValue = 2;
                    tasks.Add(ctx.AddTask($"[dim blue on white] {msgArr[i]} [/]",tStg));                            
                }
                var taskArr = tasks.ToArray();
                for (int i = 0; i < actionArr.Length; i ++)
                {
                    var tmpTask = taskArr[i];
                    tmpTask.Description = $"[bold blue on white] {msgArr[i]} [/]";
                    Thread.Sleep(500);
                    tmpTask.StartTask();
                    tmpTask.Increment(1);
                    actionArr[i].Invoke();
                    tmpTask.Description = $"[dim blue on white] {msgArr[i]} [/]";
                    tmpTask.Increment(1);
                    tmpTask.StopTask();
                }
            });                    
        }
    }
    
}