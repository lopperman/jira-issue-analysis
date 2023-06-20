using System.Reflection.Metadata.Ecma335;
using System.Dynamic;
using System.IO;
using System;
using System.Text.Json.Serialization;

namespace JiraCon
{
    public class JQLConfig
    {
        // private const string JQL_ID = "jqlId";
        // private const string JQL_NAME = "jqlName";
        // private const string JQL_DATA = "jql";
        public JQLConfig()
        {
        }

        public JQLConfig( string jqlName, string jql): this()
        {
            this.jqlName = jqlName;
            this.jql = jql;
        }

        public JQLConfig(int jqlId, string jqlName, string jql): this()
        {
            this.jqlId = jqlId;
            this.jqlName = jqlName;
            this.jql = jql;
        }
        [JsonPropertyName("jqlId")]
        public int? jqlId {get; set;}
        [JsonPropertyName("jqlName")]
        public string? jqlName {get; set;}
        [JsonPropertyName("jql")]
        public string? jql {get; set;}


        [JsonIgnore]
        public bool ValidJQL
        {
            get
            {
                return (jqlName != null && jql != null && jqlName.Length > 0 && jql.Length > 0);
            }
        }

    }
}