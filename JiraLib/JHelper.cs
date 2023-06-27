using System.Net.Mime;
using System.ComponentModel.Design.Serialization;
using System;
using JTIS.ManagedObjects;
using Spectre.Console;
using Atlassian.Jira;
using Spectre.Console.Json;
using Newtonsoft.Json;
using JTIS.Console;

namespace JiraCon
{
    public static class JHelper
    {

        public static string RemoveCommas(string text)
        {
            return RemoveCommas(text, " ");
        }

        public static string RemoveCommas(string text, string replaceWith)
        {
            string ret = null;

            if (text != null && !string.IsNullOrWhiteSpace(text))
            {
                ret = text.Replace(",", replaceWith);
            }

            return ret;
        }

        public static T GetValue<T>(String value)
        {
            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Unable to convert '{0}' to Type: {1}\r\n\r\n{2}", value, typeof(T).FullName,ex.Message));
            }
        }

        /// <summary>
        /// Calculates number of business days, taking into account:
        ///  - weekends (Saturdays and Sundays)
        ///  - bank holidays in the middle of the week
        /// </summary>
        /// <param name="firstDay">First day in the time interval</param>
        /// <param name="lastDay">Last day in the time interval</param>
        /// <param name="bankHolidays">List of bank holidays excluding weekends</param>
        /// <returns>Number of business days during the 'span'</returns>
        public static int BusinessDaysUntil(this DateTime firstDay, DateTime lastDay, params DateTime[] bankHolidays)
        {
            firstDay = firstDay.Date;
            lastDay = lastDay.Date;
            if (firstDay > lastDay)
                throw new ArgumentException("Incorrect last day " + lastDay);

            TimeSpan span = lastDay - firstDay;
            int businessDays = span.Days + 1;
            int fullWeekCount = businessDays / 7;
            // find out if there are weekends during the time exceedng the full weeks
            if (businessDays > fullWeekCount * 7)
            {
                // we are here to find out if there is a 1-day or 2-days weekend
                // in the time interval remaining after subtracting the complete weeks
                int firstDayOfWeek = (int)firstDay.DayOfWeek;
                int lastDayOfWeek = (int)lastDay.DayOfWeek;
                if (lastDayOfWeek < firstDayOfWeek)
                    lastDayOfWeek += 7;
                if (firstDayOfWeek <= 6)
                {
                    if (lastDayOfWeek >= 7)// Both Saturday and Sunday are in the remaining time interval
                        businessDays -= 2;
                    else if (lastDayOfWeek >= 6)// Only Saturday is in the remaining time interval
                        businessDays -= 1;
                }
                else if (firstDayOfWeek <= 7 && lastDayOfWeek >= 7)// Only Sunday is in the remaining time interval
                    businessDays -= 1;
            }

            // subtract the weekends during the full weeks in the interval
            businessDays -= fullWeekCount + fullWeekCount;

            // subtract the number of bank holidays during the time interval
            foreach (DateTime bankHoliday in bankHolidays)
            {
                DateTime bh = bankHoliday.Date;
                if (firstDay <= bh && bh <= lastDay)
                    --businessDays;
            }

            return businessDays;
        }

        public static void ShowIssueJSON()
        {
            var searchJQL = ConsoleInput.IssueKeysToJQL();
            if (string.IsNullOrWhiteSpace(searchJQL)) return;

            List<Issue> issues = new List<Issue>();

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
                .Start(async ctx => 
                {
                    var tsk1 = ctx.AddTask($"[bold blue on white]Querying Jira[/]",true,2);
                    var tsk2 = ctx.AddTask($"[dim blue on white]---[/]",true,2);
                    tsk1.Increment(1);
                    issues = JiraUtil.JiraRepo.GetIssues(searchJQL);
                    tsk1.Increment(1);

                    if (issues.Count > 0)
                    {
                        tsk1.Description=string.Format($"[dim blue on white]Querying Jira[/]");

                        tsk2.MaxValue = issues.Count + 1;
                        foreach (var iss in issues)
                        {
                            tsk2.Description=$"[bold blue on white]Parsing to Json: {iss.Key.Value}[/]";
                            //await iss.GetChangeLogsAsync();
                            tsk2.Increment(1);
                        }
                    }

                });    
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

                foreach (var iss in issues)       
                {
                    string data = JsonConvert.SerializeObject(iss,Formatting.None,settings);
                    //var json = new JsonText(data)
                    app.ShowJson($"Issue: {iss.Key.Value}",data);
                    ConsoleUtil.PressAnyKeyToContinue();
                }


        }
    }



}
