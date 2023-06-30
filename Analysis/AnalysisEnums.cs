namespace JTIS.Analysis
{
    public enum StatusType
    {
        stActiveState = 1, 
        stPassiveState = 2, 
        stIgnoreState = 3, 
        stStart = 4, 
        stEnd = 5, 
        stUnknown = 6
        // for any issue, 'Start' is the first active state that occurred
        // stStart = 5
    }    

    public enum AnalysisType
    {
        _atUnknown = -1, 
        atJQL = 1, 
        atIssues = 2, 
        atEpics = 3, 
        atIssueSummary = 4
    }

}
