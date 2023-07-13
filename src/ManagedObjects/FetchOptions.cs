namespace JTIS.Data
{
    public class FetchOptions
    {
        public bool AllowJQLSnippets {get;set;} = true;
        public bool AllowManualJQL {get;set;} = true;
        public bool AllowCachedSelection {get;set;} = true;
        public bool IncludeChangeLogs {get;set;} = true;
        public bool CacheResults {get;set;} = false;
        public bool FetchEpicChildren {get;set;} = false;
        public string CacheResultsDesc {get;set;} = string.Empty;
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
        public static FetchOptions DefaultFetchOptions
        {
            get
            {
                var result = new FetchOptions();
                return result;
            }
        }

    }


}