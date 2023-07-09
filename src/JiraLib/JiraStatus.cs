

using System.Text.Json.Serialization;
using JTIS.Analysis;
using JTIS.Config;
using Newtonsoft.Json.Linq;

namespace JTIS 
{
    public class JiraStatus 
    {

        // var id = j["id"].Value<string>();
        // var name = j["name"].Value<string>();
        // var catToken = j["statusCategory"].Value<JToken>();
        // var catKey = catToken["key"].Value<string>();
        // var catName = catToken["name"].Value<string>();

        public int StatusId {get;set;}
        public string StatusName {get;set;}
        public string CategoryKey {get;set;}
        public string CategoryName {get;set;}
        public bool DefaultInUse {get;set;}

        public JiraStatus()
        {
            StatusName = string.Empty;
            CategoryKey = string.Empty;
            CategoryName = string.Empty;
        }

        public JiraStatus(int id, string name, string catKey, string catName, bool inDefaultPrj ):this()
        {
            StatusId = id;
            StatusName = name;
            CategoryKey = catKey;
            CategoryName = catName;    
            DefaultInUse = inDefaultPrj ;
        }

        public JiraStatus(JToken token):this()
        {
            var id = token["id"].Value<string>();
            var name = token["name"].Value<string>();
            var catToken = token["statusCategory"].Value<JToken>();
            var catKey = catToken["key"].Value<string>();
            var catName = catToken["name"].Value<string>();

            StatusId = int.Parse(id);
            StatusName = name ?? string.Empty;
            CategoryKey = catKey ?? string.Empty;
            CategoryName = catName ?? string.Empty;
        }

        [JsonIgnore]
        public StatusType Type 
        {
            get
            {
                switch (CategoryName.ToLower())
                {
                    case "done":
                        return StatusType.stEnd;;
                    case "in progress":
                        return StatusType.stActiveState;
                    case "to do":
                        return StatusType.stPassiveState;       
                    case "ignore":
                        return StatusType.stIgnoreState;                 
                    default:
                        return StatusType.stUnknown;;
                }

            }
            
        }
    }
}