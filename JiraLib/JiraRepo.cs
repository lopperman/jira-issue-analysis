using System.Text;
using Atlassian.Jira;
using Newtonsoft.Json;
using RestSharp;
using Newtonsoft.Json.Linq;
using JTIS.Console;
using JTIS.Data;
using JTIS.Extensions;
using Spectre.Console;

namespace JTIS
{


    public class JiraRepo
    {
        private Jira _jira;

        private List<JField> _fieldList = new List<JField>();
        private string _epicLinkFieldKey = string.Empty;
        public JiraRepo(string server, string userName, string password)
        {
            JiraRestClientSettings settings = new JiraRestClientSettings();
            // settings.JsonSerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Unspecified;
            // settings.JsonSerializerSettings.DateParseHandling = DateParseHandling.DateTime ;
            // settings.JsonSerializerSettings
            settings.EnableUserPrivacyMode = true;            
            _jira = Atlassian.Jira.Jira.CreateRestClient(server, userName, password,settings);
            _jira.Issues.MaxIssuesPerRequest = 500;
            _fieldList = GetJFields();
            JField jField = _fieldList.Where(x => x.Name == "Epic Link").FirstOrDefault();
            if (jField != null)
            {
                _epicLinkFieldKey = jField.Key;
            }
        }

        public string EpicLinkFieldName
        {
            get
            {
                return _epicLinkFieldKey;
            }
        }
        public Jira jira 
        {
            get{
                return _jira;
            }
        }
        public ServerInfo ServerInfo
        {
            get
            {
                return _jira.ServerInfo.GetServerInfoAsync().Result;
            }
        }


        public Project GetProject(string key)
        {
            return _jira.Projects.GetProjectAsync(key).GetAwaiter().GetResult();
        }

        public async Task<Issue> GetIssueAsync(string key)
        {
            return await _jira.Issues.GetIssueAsync(key) as Issue;
        }
        public List<JiraFilter> GetJiraFiltersFavorites()
        {
            return _jira.Filters.GetFavouritesAsync().GetAwaiter().GetResult().ToList();
        }
        public JArray GetFieldsAsJson()
        {
            string data = GetFieldsAsync().GetAwaiter().GetResult();
            JArray json = JArray.Parse(data);
            return json;
        }
        public List<JField> GetJFields()
        {
            var ret = new List<JField>();

            string data = GetFieldsAsync().GetAwaiter().GetResult();

            JArray json = JArray.Parse(data);

            for (int i = 0; i < json.Count; i++)
            {
                try
                {
                    JToken j = json[i];
                    var k = j["key"].Value<string>();
                    var n = j["name"].Value<string>();
                    ret.Add(new JField(k, n));
                }
                catch (Exception ex)
                {
                    ConsoleUtil.WriteError(ex.Message,ex:ex,pause:false);
                }
            }

            return ret;
        }

        public int GetJQLResultsCount( string jql, bool ignoreError = false)
        {
            int ret = -1;
            try 
            {
                string data = GetJQLResultsCountAsync(jql).GetAwaiter().GetResult();
                Dictionary<string, object> values = JsonConvert.DeserializeObject<Dictionary<string,object>>(data);
                if (values.ContainsKey("total"))
                {
                    ret = Convert.ToInt32(values["total"].ToString());
                }
            }
            catch(Exception exc)
            {                
                ret = -1;
                if (ignoreError == false)
                {
                    ConsoleUtil.WriteError($"Error parsing jql: {jql}");
                    ConsoleUtil.WriteError($"Error: {exc.Message}",pause:true);
                }
            }
            return ret;
        }

        private async Task<string> GetJQLResultsCountAsync(string jql, CancellationToken token = default(CancellationToken))
        {
            var resourceUrl = $"rest/api/3/search?jql={jql}&maxResults=0";

            var response = await _jira.RestClient.ExecuteRequestAsync(Method.GET, resourceUrl, null, token)
                .ConfigureAwait(false);

            return response.ToString();
        }



        private async Task<string>GetFieldsAsync(CancellationToken token = default(CancellationToken))
        {
            var resourceUrl = String.Format("rest/api/3/field");
            var response = await _jira.RestClient.ExecuteRequestAsync(Method.GET, resourceUrl, null, token)
                .ConfigureAwait(false);

            return response.ToString();
        }

        public async Task<string> GetItemStatusesAsync(CancellationToken token = default(CancellationToken))
        {
            var resourceUrl = String.Format("rest/api/3/status");
            var response = await _jira.RestClient.ExecuteRequestAsync(Method.GET, resourceUrl, null, token)
                .ConfigureAwait(false);

            return response.ToString();
        }

        
        public async Task<AutoCompData> GetJQLAutoCompleteDataAsync(CancellationToken token = default(CancellationToken))
        {
            var resourceUrl = "rest/api/3/jql/autocompletedata";
            JToken data = await _jira.RestClient.ExecuteRequestAsync(Method.GET, resourceUrl, null, token)
                .ConfigureAwait(false);

            var retClass = new AutoCompData();
            var jsonResWords = data["jqlReservedWords"];
            if (jsonResWords != null)
            {
                List<string> tmpResWords = JsonConvert.DeserializeObject<List<string>>(jsonResWords.ToString());
                if (tmpResWords != null)
                {
                    retClass.ReservedWords = tmpResWords;
                }
            }
            var jsonFuncNames = data[key:"visibleFunctionNames"];
            if (jsonFuncNames != null)
            {
                var t1 = JsonConvert.DeserializeObject<List<FunctionName>>(jsonFuncNames.ToString());
                if (t1 != null)
                {
                    retClass.FunctionNames = t1;
                }

            }
            var jsonFieldNames = data[key:"visibleFieldNames"];
            if (jsonFieldNames != null)
            {
                var t2 = JsonConvert.DeserializeObject<List<VisibleField>>(jsonFuncNames.ToString());
                if (t2 != null)
                {
                    retClass.VisibleFields = t2;
                }

            }
            return retClass;
        }

        public async Task<string> GetProjectItemStatusesAsync(string defProject, CancellationToken token = default(CancellationToken))
        {
            var resourceUrl = String.Format("rest/api/3/project/{0}/statuses",defProject);
            var response = await _jira.RestClient.ExecuteRequestAsync(Method.GET, resourceUrl, null, token)
                .ConfigureAwait(false);

            return response.ToString();
        }


        public async Task<List<IssueStatus>> GetIssueTypeStatusesAsync(string projKey, string issueType, CancellationToken token = default(CancellationToken))
        {

            var ret = new List<IssueStatus>();


            var resourceUrl = String.Format("rest/api/3/project/{0}/statuses", projKey);
            var serializerSettings = _jira.RestClient.Settings.JsonSerializerSettings;
            Newtonsoft.Json.Linq.JToken response = await _jira.RestClient.ExecuteRequestAsync(Method.GET, resourceUrl, null, token)
                .ConfigureAwait(false);

            foreach (var parent in response)
            {
                if (parent["name"].ToString() == issueType)
                {
                    var items = parent["statuses"].Select(a => JsonConvert.DeserializeObject<IssueStatus>(a.ToString(), serializerSettings));
                    ret.AddRange(items);
                }
            }

            return ret;
        }

        public List<Issue> GetSubTasksAsList(Issue issue)
        {
            return GetSubTasksAsync(issue).GetAwaiter().GetResult().ToList();
        }

        public async Task<List<Issue>> GetSubTasksAsync(Issue issue, CancellationToken token = default(CancellationToken))
        {
            List<Issue> result = new List<Issue>();

            int incr = 0;
            int total = 0;


            do
            {

                IPagedQueryResult<Issue> response = await issue.GetSubTasksAsync(10, incr, token).ConfigureAwait(false);

                total = response.TotalItems;

                incr += response.Count();

                result.AddRange(response);
            }
            while (incr < total);

            return result;
        }

        public async Task<IssueLabelCollection> GetIssueLabelsAsync(string issueKey, CancellationToken token = default(CancellationToken)) 
        {
            IssueLabelCollection result = default(IssueLabelCollection);

                //GET /rest/api/3/issue/ABC-123?fields=labels
                var resourceUrl = String.Format("rest/api/3/issue/{0}?fields=labels", issueKey);
                var serializerSettings = _jira.RestClient.Settings.JsonSerializerSettings;
                var response = await _jira.RestClient.ExecuteRequestAsync(Method.GET, resourceUrl, null, token)
                    .ConfigureAwait(false);

                JToken issueLabels = response["fields"][key:"labels"];
//                JToken totalChangeLogs = response["total"];

                if (issueLabels != null)
                {
                    result = JsonConvert.DeserializeObject<IssueLabelCollection>(issueLabels.ToString(), serializerSettings);
                }


            // var resourceUrl = String.Format("rest/api/3/status");
            // var response = await _jira.RestClient.ExecuteRequestAsync(Method.GET, resourceUrl, null, token)
            //     .ConfigureAwait(false);

            // return response.ToString();


            return result;

        }

        public async Task AddChangeLogsAsync(JIssue jIssue, ProgressTask progress, CancellationToken token = default(CancellationToken))
        {
            List<IssueChangeLog> changeLogs = new List<IssueChangeLog>();
            progress.Increment(1);
            
            changeLogs = await GetChangeLogsAsync(jIssue.Key, progress, token).ConfigureAwait(true);

            jIssue.AddChangeLogs(changeLogs);
            progress.Increment(1);
            // if (changeLogs.Count > 0)
            // {
            //     jIssue.ChangeLogs.AddRange((IEnumerable<JIssueChangeLog>)changeLogs);                
            // }
        }
        public async Task AddChangeLogsAsync(JIssue jIssue, CancellationToken token = default(CancellationToken))
        {
//            List<IssueChangeLog> changeLogs = new List<IssueChangeLog>();
            
            jIssue.AddChangeLogs(await GetChangeLogsAsync(jIssue.Key, token).ConfigureAwait(true));

            // jIssue.AddChangeLogs(changeLogs);
            // progress.Increment(1);
            // if (changeLogs.Count > 0)
            // {
            //     jIssue.ChangeLogs.AddRange((IEnumerable<JIssueChangeLog>)changeLogs);                
            // }
        }

        public async Task<List<IssueChangeLog>> GetChangeLogsAsync(string issueKey,ProgressTask progress,  CancellationToken token = default(CancellationToken))
        {
            List<IssueChangeLog> result = new List<IssueChangeLog>();

            int incr = 0;
            int total = 0;



            do
            {
                var resourceUrl = String.Format("rest/api/3/issue/{0}/changelog?maxResults=100&startAt={1}", issueKey, incr);
                var serializerSettings = _jira.RestClient.Settings.JsonSerializerSettings;
                var response = await _jira.RestClient.ExecuteRequestAsync(Method.GET, resourceUrl, null, token)
                    .ConfigureAwait(true);

                JToken changeLogs = response["values"];
                JToken totalChangeLogs = response["total"];

                if (totalChangeLogs != null)
                {
                    total = JsonConvert.DeserializeObject<Int32>(totalChangeLogs.ToString(), serializerSettings);
                    // if (progress.MaxValue != total)
                    // {
                    //     progress.MaxValue = total;
                    // }
                }

                if (changeLogs != null)
                {
                    var items = changeLogs.Select(cl => JsonConvert.DeserializeObject<IssueChangeLog>(cl.ToString(), serializerSettings));

                    incr += items.Count();

                    // progress.Increment(items.Count());

                    result.AddRange(items);
                }
            }
            while (incr < total);

            return result;
        }        

        public async Task<List<IssueChangeLog>> GetChangeLogsAsync(string issueKey, CancellationToken token = default(CancellationToken))
        {
            List<IssueChangeLog> result = new List<IssueChangeLog>();

            int incr = 0;
            int total = 0;

            do
            {
                var resourceUrl = String.Format("rest/api/3/issue/{0}/changelog?maxResults=100&startAt={1}", issueKey, incr);
                var serializerSettings = _jira.RestClient.Settings.JsonSerializerSettings;
                var response = await _jira.RestClient.ExecuteRequestAsync(Method.GET, resourceUrl, null, token)
                    .ConfigureAwait(false);

                JToken changeLogs = response["values"];
                JToken totalChangeLogs = response["total"];

                if (totalChangeLogs != null)
                {
                    total = JsonConvert.DeserializeObject<Int32>(totalChangeLogs.ToString(), serializerSettings);
                }

                if (changeLogs != null)
                {
                    var items = changeLogs.Select(cl => JsonConvert.DeserializeObject<IssueChangeLog>(cl.ToString(), serializerSettings));

                    incr += items.Count();

                    result.AddRange(items);
                }
            }
            while (incr < total);

            return result;
        }

        public List<Issue> GetIssuesSummary(string projectKey, params string[] fieldNames)
        {
            List<Issue> issues = new List<Issue>();

            int incr = 0;
            int total = 0;

            IssueSearchOptions searchOptions = new IssueSearchOptions($"project={projectKey}");
            searchOptions.AdditionalFields = fieldNames;
            // searchOptions.FetchBasicFields = basicFields;

            searchOptions.MaxIssuesPerRequest = _jira.Issues.MaxIssuesPerRequest;

            do
            {
                searchOptions.StartAt = incr;

                Task<IPagedQueryResult<Issue>> results = _jira.Issues.GetIssuesFromJqlAsync(searchOptions);
                results.Wait();

                total = results.Result.TotalItems;

                foreach (Issue i in results.Result)
                {
                    issues.Add((Issue)i);
                }

                incr += results.Result.Count();
            }
            while (incr < total);

            return issues;

        }


        public List<Issue> GetIssues(string jql, bool basicFields, params string[] additionalFields)
        {
            List<Issue> issues = new List<Issue>();

            int incr = 0;
            int total = 0;

            IssueSearchOptions searchOptions = new IssueSearchOptions(jql);
            searchOptions.FetchBasicFields = basicFields;
            

            searchOptions.MaxIssuesPerRequest = _jira.Issues.MaxIssuesPerRequest;

            if (additionalFields == null)
            {
                additionalFields = new string[] { "Flagged" };
            }
            else 
            {
                if (!additionalFields.ToList().Contains("Flagged"))
                {
                    var tmpAddlFields = additionalFields.ToList<string>();
                    tmpAddlFields.Add("Flagged");

                    additionalFields = tmpAddlFields.ToArray();
                }
            }

            if (additionalFields == null)
            {
                if (!string.IsNullOrWhiteSpace(_epicLinkFieldKey))
                {
                    additionalFields = new string[] { _epicLinkFieldKey };
                }
            }

            if (additionalFields != null)
            {
                var fldList = additionalFields.ToList();
                if (!fldList.Contains(_epicLinkFieldKey) && !string.IsNullOrWhiteSpace(_epicLinkFieldKey))
                {
                    fldList.Add(_epicLinkFieldKey);
                }
                searchOptions.AdditionalFields = fldList;
            }


            do
            {
                searchOptions.StartAt = incr;

                Task<IPagedQueryResult<Issue>> results = _jira.Issues.GetIssuesFromJqlAsync(searchOptions);
                results.Wait();

                total = results.Result.TotalItems;

                foreach (Issue i in results.Result)
                {
                    issues.Add((Issue)i);
                }

                incr += results.Result.Count();
            }
            while (incr < total);

            return issues;

        }

        public List<Issue> GetIssues(string jql)
        {
            if (!string.IsNullOrWhiteSpace(_epicLinkFieldKey))
            {
                return GetIssues(jql, true, _epicLinkFieldKey);
            }
            else
            {
                return GetIssues(jql, true);
            }
        }

        /// <summary>
        /// Get Issues (Slow) - faster to use GetIssues(string jql)
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public List<Issue> GetIssues(params string[] keys)
        {
            List<Issue> issues = new List<Issue>();
            if (keys != null && keys.Length > 0)
            {
                for (int i = 0; i < keys.Count(); i ++)
                {
                    var issue = GetIssue(keys[i]);
                    if (issue != null)
                    {
                        issues.Add(issue);
                    }
                        
                }
            }
            return issues;
        }

        public Issue? GetIssue(string key)
        {
            try 
            {
                return GetIssues(string.Format("key={0}", key)).FirstOrDefault();
            }
            catch
            {
                ConsoleUtil.WriteError("ERROR GETTING ISSUE: " + key);
                return null;
            }


            ////IssueSearchOptions options = new IssueSearchOptions(string.Format("project={0}", config.jiraProjectKey));
            //IssueSearchOptions options = new IssueSearchOptions(string.Format("Key = {0}", key));
            //options.MaxIssuesPerRequest = 50; //this is wishful thinking on my part -- client has this set at 20 -- unless you're a Jira admin, got to live with it.
            //options.FetchBasicFields = true;            

            //return GetJira().Issues.Queryable.Where(x=>x.Key == key).FirstOrDefault();

        }

        public List<IssueType> GetIssueTypes(string projectKey)
        {
            var proj = GetProject(projectKey);

            if (proj != null)
            {
                return proj.GetIssueTypesAsync().Result.ToList();
            }

            return null;

        }

        public List<ProjectComponent> GetProjectComponents(string projectKey)
        {
            return GetProject(projectKey).GetComponentsAsync().Result.ToList();
        }


        public List<IssueStatus> GetIssueStatuses()
        {

            return _jira.Statuses.GetStatusesAsync().Result.ToList();

        }

    }



    public class JField
    {   
        
         
        public JField()
        {
            ID = string.Empty;
            Key = string.Empty;
            Name  = string.Empty;
        }

        public JField(string key, string name): this()
        {
            Key = key;
            Name = name;
        }

        public string ID { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public bool Custom { get; set; }
        
    }
}
