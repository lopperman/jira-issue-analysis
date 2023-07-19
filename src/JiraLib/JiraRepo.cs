using Atlassian.Jira;
using Newtonsoft.Json;
using RestSharp;
using Newtonsoft.Json.Linq;
using JTIS.Console;
using JTIS.Data;
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

                if (issueLabels != null)
                {
                    result = JsonConvert.DeserializeObject<IssueLabelCollection>(issueLabels.ToString(), serializerSettings);
                }

            return result;

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
