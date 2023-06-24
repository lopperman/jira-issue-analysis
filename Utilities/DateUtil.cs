using System.Text.RegularExpressions;
namespace JiraCon
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
                var c = ConsoleUtil.StdStyle(line).Foreground;
                return c.ToString();
            }
            public static Spectre.Console.Color FontColor(this StdLine line)
            {            
                return ConsoleUtil.StdStyle(line).Foreground;
            }
            public static string BackMkp(this StdLine line)
            {            
                var c = ConsoleUtil.StdStyle(line).Background;
                return c.ToString();
            }
            public static Spectre.Console.Color BackColor(this StdLine line)
            {            
                return ConsoleUtil.StdStyle(line).Background;
            }

        }
    
    public static class DateUtil
    {


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
            // Func<DateTime, bool> workDay = first => 
            // (
            //     first.DayOfWeek == DayOfWeek.Saturday || first.DayOfWeek == DayOfWeek.Sunday
            // );
            // return Enumerable.Range(0, 1 + (first-other).Days).Count(intDay=> workDay(first.AddDays(intDay)));
        }

        public static void testDT()
        {
            DateTime dt = DateTime.Now;
            Console.WriteLine(dt.fwGetWorkingDays(DateTime.Now.AddHours(-37.25)));
                
        }

    }
}