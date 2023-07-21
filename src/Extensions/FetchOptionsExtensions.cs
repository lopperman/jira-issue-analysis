namespace JTIS.Data;


public static class FetchOptionsExtensions
{
    public static FetchOptions IncludeChangeLogs(this FetchOptions options, bool value=true)
    {
        if (options is null){throw new ArgumentNullException(nameof(options));}
        options.IncludeChangeLogs=value;
        return options;
    }

    // public static FetchOptions SingleIssueOnly(this FetchOptions options, bool value=true)
    // {
    //     if (options is null){throw new ArgumentNullException(nameof(options));}
    //     options.SingleIssueOnly = value;
    //     return options;
    // }

    public static FetchOptions RequiredIssueStatusSequence(this FetchOptions options, bool value=true)
    {
        if (options is null){throw new ArgumentNullException(nameof(options));}
        options.RequiredIssueStatusSequence = value;
        return options;
    }

    public static FetchOptions FetchEpicChildren(this FetchOptions options, bool value=true)
    {
        if (options is null){throw new ArgumentNullException(nameof(options));}
        options.FetchEpicChildren = value;
        return options;
    }
    public static FetchOptions AllowCachedSelection(this FetchOptions options, bool value=true)
    {
        if (options is null){throw new ArgumentNullException(nameof(options));}
        options.AllowCachedSelection=value;
        return options;
    }
    public static FetchOptions AllowJQLSnippets(this FetchOptions options, bool value=true)
    {
        if (options is null){throw new ArgumentNullException(nameof(options));}
        options.AllowJQLSnippets=value;
        return options;
    }
    public static FetchOptions AllowManualJQL(this FetchOptions options, bool value=true)
    {
        if (options is null){throw new ArgumentNullException(nameof(options));}
        options.AllowManualJQL=value;
        return options;
    }
    public static FetchOptions CacheResults(this FetchOptions options, bool value=true, string? desc=null)
    {
        if (options is null){throw new ArgumentNullException(nameof(options));}
        options.CacheResults=value;
        if (desc != null){
            options.CacheResultsDesc=desc;
        }
        return options;
    }
    public static FetchOptions CacheResultsDesc(this FetchOptions options, string desc)
    {
        if (options is null){throw new ArgumentNullException(nameof(options));}
        options.CacheResults=true;
        options.CacheResultsDesc=desc;
        return options;
    }


}
