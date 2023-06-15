using System.Reflection.Metadata.Ecma335;
using System.Dynamic;
using System.IO;
using System;
using System.Text.Json.Serialization;

namespace JiraCon
{
    public class JTISConfig
    {
        // private const string CFG_FIELD_ID = "configId";
        // private const string CFG_FIELD_NAME = "configName";
        // private const string CFG_FIELD_USERNAME = "username";
        // private const string CFG_FIELD_APITOKEN = "apitoken";
        // private const string CFG_FIELD_BASEURL = "jiraurl";
        // private const string CFG_FIELD_PROJECT = "project";
        private bool _validConn = false;

        public JTISConfig()
        {
            SavedJQL = new List<JQLConfig>();
            StatusConfigs = new List<StatusConfig>();
        }

        public JTISConfig(int cfgId, string cfgName, string loginName, string authToken, string url, string project): this()
        {
            configId = cfgId;
            configName = cfgName;
            userName=loginName;
            apiToken=authToken;
            baseUrl=url;
            defaultProject=project;
        }
        [JsonPropertyName("cfgId")]
        public int? configId {get; set;}
        [JsonPropertyName("cfgName")]
        public string? configName {get; set;}
        [JsonPropertyName("loginName")]
        public string? userName {get; set;}
        [JsonPropertyName("securityToken")]
        public string? apiToken {get; set;}
        [JsonPropertyName("jiraBaseUrl")]
        public string? baseUrl {get; set;}
        [JsonPropertyName("projectKey")]
        public string? defaultProject {get;set;}
        [JsonPropertyName("savedJQL")]
        public List<JQLConfig> SavedJQL {get; set;}
        public List<StatusConfig> StatusConfigs {get; set;}
        

        [JsonIgnore]
        public int SavedJQLCount
        {
            get
            {
                return SavedJQL.Count();
            }
        }

        [JsonIgnore]
        public bool ValidConfig 
        {
            get
            {

                bool tmpValid = true;

                if (userName == null || userName.Length == 0)
                {
                    tmpValid = false;
                }
                if (apiToken==null || apiToken.Length==0)
                {
                    tmpValid = false;
                }
                if (baseUrl == null || baseUrl.Length==0)
                {
                    tmpValid = false;
                }
                if (defaultProject==null || defaultProject.Length==0)
                {
                    tmpValid = false;
                }
                if (configName == null || configName.Length==0)
                {
                    tmpValid = false;
                }
                if (configId == null || configId.Value <=0)
                {
                    tmpValid = false;
                }
                if (tmpValid == true && _validConn == false )
                {                    
                    if (JiraUtil.CreateRestClient(this)==true)
                    {
                        tmpValid = true;
                        _validConn = true ;
                    }
                    else 
                    {
                        tmpValid = false;
                    }
                }
                return tmpValid;
            }
        }

        public void AddJQL(JQLConfig jc)
        {
            jc.jqlId = SavedJQLCount + 1;
            SavedJQL.Add(jc);
        }
        public void AddJQL(string shortName, string saveJql )
        {
            JQLConfig cfg = new JQLConfig(shortName,saveJql);
            AddJQL(cfg);
        }
        public void DeleteJQL(JQLConfig cfg)
        {
            SavedJQL.Remove(cfg);
            if (SavedJQLCount > 0)
            {
                for (int i = 0; i < SavedJQLCount; i ++)
                {
                    SavedJQL[i].jqlId = i + 1;
                }
            }
        }
        public JQLConfig? GetJQLConfig(int jqlId)
        {
            return SavedJQL.FirstOrDefault<JQLConfig>(x=>x.jqlId == jqlId);
        }
        public JQLConfig? GetJQLConfig(string cfgName)
        {
            return SavedJQL.FirstOrDefault<JQLConfig>(x=>x.jqlName == cfgName);
        }

        public List<string> JQLNames()
        {
            List<string> retNames = new List<string>();
            if (SavedJQLCount > 0)
            {
                for (int i = 0; i < SavedJQLCount; i ++)
                {
                    retNames.Add(string.Format("{0:00} - {1}",SavedJQL[i].jqlId,SavedJQL[i].jqlName));
                }
            }
            return retNames;
        }


    }
}
