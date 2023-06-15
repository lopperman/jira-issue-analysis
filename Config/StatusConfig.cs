using System.Reflection.Metadata.Ecma335;
using System.Dynamic;
using System.IO;
using System;
using System.Text.Json.Serialization;

namespace JiraCon
{
    public enum StatusType
    {
        stActiveState = 1, 
        stPassiveState = 2, 
        stIgnoreState = 3
    }
    public class StatusConfig
    {
        public StatusConfig()
        {
        }

        public StatusConfig( string stName, StatusType stType): this()
        {
            this.StatusName = stName;
            this.StatusType = stType;
        }

        [JsonPropertyName("StatusName")]
        public string? StatusName {get; set;}
        [JsonPropertyName("StatusType")]
        public StatusType StatusType {get; set;}


        [JsonIgnore]
        public bool ValidStatusConfig
        {
            get
            {
                return (StatusName != null && StatusName.Length > 0 && (int)StatusType >0);
            }
        }

    }
}
