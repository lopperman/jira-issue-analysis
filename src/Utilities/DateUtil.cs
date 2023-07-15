using System;
using JTIS.Config;
using JTIS.Console;

namespace JTIS
{

    public static class ColorUtil
        {
            public static IEnumerable<T> ReverseEnumerable<T>(this IEnumerable<T> source)
            {
                if (source is null)
                {
                    throw new ArgumentNullException(nameof(source));
                }

                return source.Reverse();
            }

            public static Spectre.Console.Style? CStyle(this StdLine line)
            {
                return ConsoleUtil.StdStyle(line);
            }
            public static string FontMkp(this StdLine line)
            {            
                return line.CStyle().Foreground.ToString();
            }
            public static Spectre.Console.Color FontColor(this StdLine line)
            {            
                return line.CStyle().Foreground;
            }
            public static string BackMkp(this StdLine line)
            {            
                return line.CStyle().Background.ToString();
            }
            public static Spectre.Console.Color BackColor(this StdLine line)
            {            
                return line.CStyle().Background;
            }

        }
    
    public static class DateUtil
    {
        public static string CheckTimeZone(this string item) 
        {
            if (string.IsNullOrWhiteSpace(item))
            {
                return item;
            }
            DateTime tmpDt = DateTime.MinValue;
            if (DateTime.TryParse(item, out tmpDt))
            {
                return JTISTimeZone.CheckDate(tmpDt).ToString();
            }
            return item;
        }
        public static DateTime CheckTimeZone(this DateTime dtm)
        {
            return JTISTimeZone.CheckDate(dtm);
        }
        public static DateTime? CheckTimeZoneNullable(this DateTime? dtm)
        {
            if (dtm.HasValue)
            {
                return JTISTimeZone.CheckDate(dtm.Value);
            }
            return null;
        }

        /// <summary> Get working days between two dates (Excluding a list of dates - Holidays) </summary>
        /// <param name="dtmCurrent">Current date time</param>
        /// <param name="dtmFinishDate">Finish date time</param>
        /// <param name="lstExcludedDates">List of dates to exclude (Holidays)</param>
        public static double fwGetWorkingDays(this DateTime dtmCurrent, DateTime dtmFinishDate, List<DateTime>? lstExcludedDates = null)
        {
            Func<DateTime, bool> workDay = currentDate =>
                    (
                        currentDate.DayOfWeek == DayOfWeek.Saturday ||
                        currentDate.DayOfWeek == DayOfWeek.Sunday ||
                        lstExcludedDates.Exists(evalDate => evalDate.Date.Equals(currentDate.Date))
                    );

            return Enumerable.Range(0, 1 + (dtmFinishDate - dtmCurrent).Days).Count(intDay => workDay(dtmCurrent.AddDays(intDay)));
        }

        public static double BusinessDays(this DateTime first, DateTime other)
        {
            Func<DateTime, bool> aDay = checkDate => (checkDate.DayOfWeek == DayOfWeek.Sunday || checkDate.DayOfWeek == DayOfWeek.Saturday);
            var weekendCount = Enumerable.Range(0,(first-other).Days).Count(intDay=>aDay(first.AddDays(intDay)));
            return (first-other).Days - weekendCount;
        }

        public static void testDT()
        {
            DateTime dt = DateTime.Now;
            System.Console.WriteLine(dt.fwGetWorkingDays(DateTime.Now.AddHours(-37.25)));
                
        }

    }
}