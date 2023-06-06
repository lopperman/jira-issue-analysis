using System;
using System.Collections.Generic;

namespace JiraCon
{
    public class JTISConfig
    {

        public JTISConfig(string filePath)
        {

        }
        public JTISConfig(string loginName, string authToken, string url, string? project)
        {
            userName=loginName;
            apiToken=authToken;
            baseUrl=url;
            if (project != null)
            {
                defaultProject=project;
            }
        }
        public string? userName {get;set;}
        public string? apiToken {get;set;}
        public string? baseUrl {get;set;}
        public string? defaultProject {get;set;}

        public void SaveToFile(string filePath)
        {
            
        }

    }
}
