using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading;
using Spectre.Console;
using JTIS.ManagedObjects;
using JTIS.Console;

namespace JTIS.Data
{
    [JsonObject]
    public class AutoCompData
    {
        public AutoCompData()
        {

        }
        public static AutoCompData? Create()
        {
            AnsiConsole.Progress()
                .AutoClear(true)
                .Columns(new ProgressColumn[]
                {
                    new TaskDescriptionColumn(), 
                    new ProgressBarColumn(), 
                    new PercentageColumn(), 
                    new SpinnerColumn()
                })
                .Start(ctx => 
                {
                    var random = new Random(DateTime.Now.Millisecond);  
                    var tasks = CreateTasks(ctx, random);   
                    var warpTask = ctx.AddTask("test it out",autoStart:false).IsIndeterminate();                    
                    while(!ctx.IsFinished)
                    {
                        foreach (var (task, increment) in tasks)
                        {
                            task.Increment(random.NextDouble() * increment);
                        }
                        if (random.NextDouble() < 0.1)
                        {
                            AnsiConsole.MarkupLine($"[dim]LOG: [/]{TestOutputThing.Generate()}");
                        }
                        Thread.Sleep(1000);                        
                    }
                    warpTask.StartTask();
                    warpTask.IsIndeterminate(false);
                    while(!ctx.IsFinished)
                    {
                        warpTask.Increment(12 * random.NextDouble());
                        Thread.Sleep(100);
                    }
                });
                ConsoleUtil.PressAnyKeyToContinue();
                return null;
        }

        private static List<(ProgressTask,int Delay)> CreateTasks(ProgressContext progress, Random random)
        {
                var tasks = new List<(ProgressTask, int)>();
                while  (tasks.Count < 5)
                {
                    if (TestOutputThing.TryGenerate(out long counter))
                    {
                        tasks.Add((progress.AddTask($"name: {counter}"),random.Next(2,10)));
                    }
                }
                return tasks;
        }


        [JsonProperty("visibleFieldNames")]
        public List<VisibleField>? VisibleFields {get;set;}

        [JsonProperty("visibleFunctionNames")]
        public List<FunctionName>? FunctionNames {get;set;}


        [JsonProperty("jqlReservedWords")]
        public List<string>? ReservedWords {get;set;}
    }

 
    [JsonObjectAttribute]
    public class FunctionName
    {
        public FunctionName()
        {

        }

        [JsonProperty("value")]
        public string? FunctionNameValue {get;set;}
        
        [JsonProperty("displayName")]
        public string? DisplayName {get;set;}
        

        [JsonProperty("types")]
        public List<string>? DataTypes {get;set;}



    }

    [JsonObjectAttribute()]    
    public class VisibleField
    {
        public VisibleField()
        {
            Operators = new List<string>();
            DataTypes = new List<string>();
            cfid = string.Empty;
            DisplayName = string.Empty;
            FieldNameValue = string.Empty;
        }

        [JsonProperty("value")]
        public string FieldNameValue {get;set;}
        
        [JsonProperty("displayName")]
        public string DisplayName {get;set;}
        
        [JsonProperty("orderable")]
        public bool IsOrderable {get;set;}

        [JsonProperty("searchable")]
        public bool IsSearchable {get;set;}

        [JsonProperty("cfid")]
        public string cfid {get;set;}

        [JsonProperty("operators")]
        public List<string> Operators {get;set;}

        [JsonProperty("types")]
        public List<string> DataTypes {get;set;}



    }
    
}