using System.Reflection;
using System.Runtime.Versioning;
using System.Diagnostics;
using System.Threading;
using System;
using System.Collections.Generic;

namespace JiraCon
{
    public class JTISConfig
    {
        const string configFileName = "JiraTISConfig.txt";
        const string configFolderName = "JiraTIS";

        public JTISConfig()
        {
            if (!Directory.Exists(Path.Combine(ConfigFolderPath)))
            {
                Directory.CreateDirectory(Path.Combine(ConfigFolderPath ));
            }
            if (PopulateFromFile()==true)
            {
                ValidConfig = true;
            }
        }

        public JTISConfig(string loginName, string authToken, string url, string? project): this()
        {
            userName=loginName;
            apiToken=authToken;
            baseUrl=url;
            if (project != null)
            {
                defaultProject=project;
            }
        }
        public string? userName {get;private set;}
        public string? apiToken {get;private set;}
        public string? baseUrl {get;private set;}
        public string? defaultProject {get;set;}

        public bool ValidConfig {get;private set;}

        public string ConfigFolderPath
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal),"Library","Application Support",configFolderName );
            }
        }
        public string ConfigFilePath
        {
            get
            {
                return Path.Combine(ConfigFolderPath,configFileName);
            }
        }



        public void SaveToFile(string filePath)
        {
            
        }

        private bool PopulateFromFile()
        {
            bool validFileConfig = false;

            if (File.Exists(Path.Combine(ConfigFolderPath,configFileName)))
            {
                using (StreamReader reader = new StreamReader(Path.Combine(ConfigFolderPath,configFileName)))
                {
                    string line;
                    string[] lineArr;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.StartsWith(value: "#"))
                        {
                            continue;
                        }
                        else 
                        {
                            lineArr = line.Split("=");
                            if (lineArr.Length==2)
                            {
                                if (lineArr[0].ToLower() == "username")
                                {
                                    userName = lineArr[1];
                                } else if (lineArr[0].ToLower() == "apitoken")
                                {
                                    apiToken = lineArr[1];
                                }else if (lineArr[0].ToLower() == "jiraurl")
                                {
                                    baseUrl = lineArr[1];
                                }else if (lineArr[0].ToLower() == "project")
                                {
                                    defaultProject = lineArr[1];
                                } 
                            }
                        }
                    }
                    validFileConfig = true;
                    if (userName == null || baseUrl == null || apiToken == null || defaultProject == null)
                    {
                        validFileConfig = false;
                    }
                }    
            } else 
            {
                File.Create(Path.Combine(ConfigFolderPath,configFileName));
                //TODO:  populate file
            }



            return validFileConfig ;
        }

    }
}
