using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Dynamic;
using System.IO;
using System;
using System.Text.Json.Serialization;

namespace JTIS
{
    public class JQLConfig
    {
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

        public bool JQLSyntax 
        {
            
            get{
                bool isjql = false;                
                if (jql.Contains('='))
                {
                    isjql = true;
                }
                else 
                {
                    string[] tmp = jql.Split(' ',StringSplitOptions.RemoveEmptyEntries );
                    if (tmp.Length > 0)
                    {
                        string[] jOper = JQLUtil.JQLOperators;
                        for (int i = 0 ;i < jOper.Length; i ++)
                        {
                            var tmpO = jOper[i].ToLower();
                            if (jql.ToLower().Contains(tmpO,StringComparison.OrdinalIgnoreCase))
                            {
                                isjql = true;
                                break;
                            }
                        }
                    }
                }
                return isjql;
            }
        }

        public override string ToString()
        {
            return $"Id: {jqlId:#00},  jqlName: {jqlName}{Environment.NewLine}\tJQL: {jql}";
        }

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
