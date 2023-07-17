using System.Data;
using Spectre.Console;

namespace JTIS.Data;


public static class TimeSlots
{
    public static TimeSpan CalendarTime(DateTime dt1, DateTime dt2)
    {
        DateTime start = dt1 < dt2 ? dt1 : dt2;
        DateTime end = start == dt1 ? dt2 : dt1;
        return end.Subtract(start);
    }


    public static TimeSpan BusinessTime(DateTime dt1, DateTime dt2)
    {
        DateTime start = dt1 < dt2 ? dt1 : dt2;
        DateTime end = start == dt1 ? dt2 : dt1;
        TimeSpan ts = end.Subtract(start);

        //if start/end is same weekend day
        if (start.Date.Equals(end.Date))
        {
            if (start.DayOfWeek == DayOfWeek.Sunday || start.DayOfWeek == DayOfWeek.Saturday)
            {
                return new TimeSpan();
            }
        }
        //if start is saturday, and end is following sunday
        if (start.DayOfWeek == DayOfWeek.Saturday)
        {
            if (start.Date.AddDays(1).Equals(end.Date))
            {
                return new TimeSpan();
            }
        }
        //if start is sunday, remove hours up until following monday
        if (start.DayOfWeek==DayOfWeek.Sunday)
        {
            ts = ts.Subtract(start.Date.AddDays(1).Subtract(start));
        }
        //if start is saturday, removeo hours up until following monday
        if (start.DayOfWeek==DayOfWeek.Saturday)
        {
            ts = ts.Subtract(start.Date.AddDays(2).Subtract(start));
        }
        //if end is sunday, subtract back to saturday at 12:00AM
        if (end.DayOfWeek == DayOfWeek.Sunday)
        {
            ts = ts.Subtract(end.Subtract(end.Date.AddDays(-1)));
        }
        if (end.DayOfWeek == DayOfWeek.Saturday)
        {
            ts = ts.Subtract(end.Subtract(end.Date));
        }
        //remove full day for any sat/sun between first Monday and last Friday
        DateTime firstBusDay = DateTime.MinValue;
        DateTime lastBusDay = DateTime.MinValue;
        if (start.DayOfWeek == DayOfWeek.Sunday){
            firstBusDay = start.Date.AddDays(1);
        } else if (start.DayOfWeek==DayOfWeek.Saturday){
            firstBusDay = start.Date.AddDays(2);
        } else {
            firstBusDay = start;
        }
        if (end.DayOfWeek==DayOfWeek.Sunday){
            lastBusDay = end.Date.AddDays(-1);
        } else if (end.DayOfWeek==DayOfWeek.Saturday) {
            lastBusDay = end.Date;
        }
        DateTime checkDate = firstBusDay;
        while (checkDate.Date <= lastBusDay.Date)
        {
            if (checkDate.DayOfWeek==DayOfWeek.Sunday || checkDate.DayOfWeek==DayOfWeek.Saturday)
            {
                ts = ts.Subtract(new TimeSpan(24,0,0));
            }
            checkDate = checkDate.AddDays(1);
        }
        return ts;



    }
    public static List<TimeSlot> SlicedTimeSlots(params TimeSlot[] timeSlots)
    {
        List<TimeSlot> dates = new List<TimeSlot>();
        dates.AddRange(timeSlots.ToList());
        List<TimeSlot> slicedDates = new List<TimeSlot>();

        IEnumerable<TimeSlot> dateContainer = dates;

        // Created an ordered list of Start & End dates.
        var times = dateContainer.Select(x => x.StartDate);
        times = times.Concat(dateContainer.Select(x => x.EndDate));
        var orderedTimes = times.Distinct().OrderBy(x => x);
        var prev = orderedTimes.First();
        times = orderedTimes.Skip(1);

        foreach (var time in times)
        {
            var names = new List<Guid>();
            foreach (TimeSlot date in dateContainer)
            {
                // Add the TimeSlot if it's in range
                if (prev >= date.StartDate && time <= date.EndDate)
                {
                    names.Add(date.Key);
                }
            }

            var name = string.Join(",",names);
            TimeSlot slot = new TimeSlot(prev, time);
            slicedDates.Add(slot);
            prev = time;
        }
        return slicedDates;
        // foreach (var x in slicedDates)
        // {
        //     AnsiConsole.WriteLine($"{x.StartDate}, {x.EndDate}, {x.Key}");            
        // }
    }
}


public class TimeSlot
{

    public DateTime StartDate;
    public DateTime EndDate;
    public Guid Key = Guid.NewGuid();

    public TimeSlot(DateTime start, DateTime end)
    {
        StartDate = start;
        EndDate = end;
    }

    public override string ToString()
    {
        return $"{StartDate} => {EndDate} : {Key}";
    }

}