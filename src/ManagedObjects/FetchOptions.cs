namespace JTIS.Data
{
    public class FetchOptions
    {
        public bool RequiredIssueStatusSequence {get;set;} = false;
        public bool AllowJQLSnippets {get;set;} = true;
        public bool AllowManualJQL {get;set;} = true;
        public bool AllowCachedSelection {get;set;} = true;
        public bool IncludeChangeLogs {get;set;} = true;
        public bool CacheResults {get;set;} = false;
        public bool FetchEpicChildren {get;set;} = false;
        public string CacheResultsDesc {get;set;} = string.Empty;
        // public bool SingleIssueOnly {get;set;} = false;
        public string JQL {get;set;} = string.Empty;

        public FetchOptions()
        {
            //set defaults
            AllowJQLSnippets = true;
            AllowManualJQL = true;
            AllowCachedSelection = false;
            IncludeChangeLogs = true;
            CacheResults = false;       
            FetchEpicChildren = false;     

        }
        public static FetchOptions Clone(FetchOptions options)
        {
            var response = new FetchOptions();
            response.AllowCachedSelection = options.AllowCachedSelection;
            response.AllowJQLSnippets = options.AllowJQLSnippets;
            response.AllowManualJQL = options.AllowManualJQL;
            response.CacheResults = options.CacheResults;
            response.CacheResultsDesc = options.CacheResultsDesc;
            response.FetchEpicChildren = options.FetchEpicChildren;
            response.IncludeChangeLogs = options.IncludeChangeLogs;
            response.JQL = options.JQL;
            return response;
        }
        public static FetchOptions DefaultFetchOptions
        {
            get
            {
                var result = new FetchOptions();
                return result;
            }
        }

        public bool Cancelled { get; internal set; }
    }


}