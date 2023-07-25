

using System.Text.Json.Serialization;
using JTIS.Analysis;
using Newtonsoft.Json.Linq;

namespace JTIS
{
    public class JiraStatus 
    {

        public int ProgressOrder {get;set;}
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

        public static string ToMarkup(JiraStatus js)
        {
            return $"{js.ProgressOrder:00} - [bold]{js.StatusName.ToUpper()}[/] - ([dim]Category: [underline]{js.CategoryName}[/], Id: {js.StatusId}[/])";
        }

        public override string ToString()
        {
            return $"{ProgressOrder:00} - {StatusName} - (Id: {StatusId}, Category: {CategoryName})";
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