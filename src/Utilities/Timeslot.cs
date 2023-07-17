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
        } else {
            lastBusDay = end;
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
        dates.OrderBy(x=>x.StartDate);

        foreach (var ts1 in dates)
        {            
            if (ts1.IsInvalid==false)
            {
                foreach (var ts2 in dates)
                {
                    if (ts2.IsInvalid==false && ts2.Key != ts1.Key)
                    {
                        if (ts2.StartDate <= ts1.EndDate)
                        {
                            ts2.StartDate = ts1.EndDate.AddSeconds(1);
                            if (ts2.StartDate > ts2.EndDate)
                            {
                                ts2.IsInvalid = true;
                            }
                        }
                    }
                }

            }
        }
        List<TimeSlot> slicedDates = new List<TimeSlot>();
        foreach (var tsFinal in dates)
        {
            if (tsFinal.IsInvalid==false)
            {
                slicedDates.Add(new TimeSlot(tsFinal.StartDate,tsFinal.EndDate));
            }
        }
        return slicedDates;
    }
}


public class TimeSlot
{

    public DateTime StartDate;
    public DateTime EndDate;
    public Guid Key = Guid.NewGuid();
    public bool IsInvalid {get;set;}

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