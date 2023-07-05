

using Atlassian.Jira;
using Spectre.Console;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace JTIS.Wait
{

    public delegate T ParamsFunc<out T>(params object[] args);
    public delegate void ParamsAction(params object[] args);


    public class AsyncWait
    {
        //'EPICS 262 173 156 1 2
        private string[] tmpEpics = new string[]{"WWT-1", "WWT-2", "WWT-262", "WWT-173", "WWT-156"};


        public AsyncWait()
        {

        }
        public static AsyncWait Create()
        {
            AsyncWait aw = new AsyncWait();

            return aw;
        }


        // public async Task<IEnumerable<Issue>> GetEpicIssues(string epicKey, CancellationToken token = default(CancellationToken)) 
        // {
        //     return null;

        //     //if company managed project, can use: parentEpic=WWT-262
        //     //otherwise try: parent = CSSK-85
        //     // IEnumerable<Issue>? resp = null;
        //     // var jql = 
        // }
// async Task Download(HttpClient client, ProgressTask task, string url)
// {
//     try
//     {
//         using (HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
//         {
//             response.EnsureSuccessStatusCode();

//             // Set the max value of the progress task to the number of bytes
//             task.MaxValue(response.Content.Headers.ContentLength ?? 0);
//             // Start the progress task
//             task.StartTask();

//             var filename = url.Substring(url.LastIndexOf('/') + 1);
//             AnsiConsole.MarkupLine($"Starting download of [u]{filename}[/] ({task.MaxValue} bytes)");

//             using (var contentStream = await response.Content.ReadAsStreamAsync())
//             using (var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
//             {
//                 var buffer = new byte[8192];
//                 while (true)
//                 {
//                     var read = await contentStream.ReadAsync(buffer, 0, buffer.Length);
//                     if (read == 0)
//                     {
//                         AnsiConsole.MarkupLine($"Download of [u]{filename}[/] [green]completed![/]");
//                         break;
//                     }

//                     // Increment the number of read bytes for the progress task
//                     task.Increment(read);

//                     // Write the read bytes to the output stream
//                     await fileStream.WriteAsync(buffer, 0, read);
//                 }
//             }
//         }
//     }
//     catch (Exception ex)
//     {
//         // An error occured
//         AnsiConsole.MarkupLine($"[red]Error:[/] {ex}");
//     }
// }        

        
    }
}

//         await AnsiConsole.Progress()
//             .Columns(new ProgressColumn[]
//             {
//                 new TaskDescriptionColumn(),
//                 new ProgressBarColumn(),
//                 new PercentageColumn(),
//                 new RemainingTimeColumn(),
//                 new SpinnerColumn(),
//             })
//             .StartAsync(async ctx =>
//             {
//                 await Task.WhenAll(items.Select(async item =>
//                 {
//                     var task = ctx.AddTask(item.name, new ProgressTaskSettings
//                     {
//                         AutoStart = false
//                     });

//                     await Download(client, task, item.url);
//                 }));
//             });

//     }     
// }




// var client = new HttpClient();
// var items = new (string name, string url)[]
// {
//     ("Ubuntu 20.04", "https://releases.ubuntu.com/20.04.1/ubuntu-20.04.1-desktop-amd64.iso"),
//     ("Spotify", "https://download.scdn.co/SpotifySetup.exe"),
//     ("Windows Terminal", "https://github.com/microsoft/terminal/releases/download/v1.5.3242.0/Microsoft.WindowsTerminalPreview_1.5.3242.0_8wekyb3d8bbwe.msixbundle"),
// };

// Progress


// This methods downloads a file and updates progress
// async Task Download(HttpClient client, ProgressTask task, string url)
// {
//     try
//     {
//         using (HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
//         {
//             response.EnsureSuccessStatusCode();

//             // Set the max value of the progress task to the number of bytes
//             task.MaxValue(response.Content.Headers.ContentLength ?? 0);
//             // Start the progress task
//             task.StartTask();

//             var filename = url.Substring(url.LastIndexOf('/') + 1);
//             AnsiConsole.MarkupLine($"Starting download of [u]{filename}[/] ({task.MaxValue} bytes)");

//             using (var contentStream = await response.Content.ReadAsStreamAsync())
//             using (var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
//             {
//                 var buffer = new byte[8192];
//                 while (true)
//                 {
//                     var read = await contentStream.ReadAsync(buffer, 0, buffer.Length);
//                     if (read == 0)
//                     {
//                         AnsiConsole.MarkupLine($"Download of [u]{filename}[/] [green]completed![/]");
//                         break;
//                     }

//                     // Increment the number of read bytes for the progress task
//                     task.Increment(read);

//                     // Write the read bytes to the output stream
//                     await fileStream.WriteAsync(buffer, 0, read);
//                 }
//             }
//         }
//     }
//     catch (Exception ex)
//     {
//         // An error occured
//         AnsiConsole.MarkupLine($"[red]Error:[/] {ex}");
//     }
// }