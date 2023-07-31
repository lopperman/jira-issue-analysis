namespace JTIS.Data;

public interface ITimeSummary
{
    //CALENDAR TIME
    TimeSpan tsCalendarTime { get;}
    TimeSpan tsActiveCalTime { get;}
    TimeSpan tsPassiveCalTime { get;}
    TimeSpan tsBlockedCalendarTime { get;}
    TimeSpan tsUnblockedCalendarTime { get;}
    TimeSpan tsBlockedActiveCalTime { get;}
    TimeSpan tsUnblockedActiveCalTime { get;}
    TimeSpan tsBlockedPassiveCalTime { get;}
    TimeSpan tsUnblockedPassiveCalTime { get;}

    //BUSINESS TIME (EXCLUDES WEEKENDS)
    TimeSpan tsBusinessTime { get;}
    TimeSpan tsActiveBusTime { get;}
    TimeSpan tsPassiveBusTime { get;}
    TimeSpan tsBlockedBusinessTime { get;}
    TimeSpan tsUnblockedBusinessTime { get;}
    TimeSpan tsBlockedActiveBusTime { get;}
    TimeSpan tsUnblockedActiveBusTime { get;}
    TimeSpan tsBlockedPassiveBusTime { get;}
    TimeSpan tsUnblockedPassiveBusTime { get;}

}
