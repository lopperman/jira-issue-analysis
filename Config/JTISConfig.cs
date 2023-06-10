using System.Reflection.Metadata.Ecma335;
using System.Dynamic;
using System.IO;
using System;
using System.Text.Json.Serialization;

namespace JiraCon
{
    public class JTISConfig
    {
        [JsonIgnore]
        const string configFileName = "JiraTISConfig.txt";
        [JsonIgnore]
        const string configFolderName = "JiraTIS";
        private const string CFG_FIELD_USERNAME = "username";
        private const string CFG_FIELD_APITOKEN = "apitoken";
        private const string CFG_FIELD_BASEURL = "jiraurl";
        private const string CFG_FIELD_PROJECT = "project";

        public JTISConfig()
        {
            if (!Directory.Exists(Path.Combine(ConfigFolderPath)))
            {
                Directory.CreateDirectory(Path.Combine(ConfigFolderPath ));
            }
            PopulateFromFile();
        }

        public JTISConfig(bool emptyConfig)
        {
            userName=string.Empty;
            apiToken=string.Empty;
            baseUrl=string.Empty;
            defaultProject=string.Empty;
        }

        public JTISConfig(string loginName, string authToken, string url, string project): this()
        {
            userName=loginName;
            apiToken=authToken;
            baseUrl=url;
            defaultProject=project;
        }
        [JsonPropertyName("loginName")]
        public string? userName {get; set;}
        [JsonPropertyName("securityToken")]
        public string? apiToken {get; set;}
        [JsonPropertyName("jiraBaseUrl")]
        public string? baseUrl {get; set;}
        [JsonPropertyName("projectKey")]
        public string? defaultProject {get;set;}

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
                return tmpValid;
            }
        }

        [JsonIgnore]
        public static int ConfigItemRequiredCount
        {
            get
            {
                return 4;
            }
        }

        [JsonIgnore]
        public string ConfigFolderPath
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal),"Library","Application Support",configFolderName );
            }
        }
        [JsonIgnore]
        public string ConfigFilePath
        {
            get
            {
                return Path.Combine(ConfigFolderPath,configFileName);
            }
        }

        public bool SetConfigFromArgs(string[] progArgs, bool? saveToFile = false)
        {
            //TODO:  IMPLEMENT

            userName = null;
            apiToken = null;
            baseUrl = null;
            defaultProject = null;

            if  (progArgs.Length != ConfigItemRequiredCount  )
            {
                throw new ArgumentException("SetConfigArgs must have: " + ConfigItemRequiredCount.ToString() + " arguments.","progArgs");
            }

            for (int i = 0; i<progArgs.Length; i ++)
            {
                string tmpArg = progArgs[i];
                if (tmpArg.Contains(string.Format("{0}=",CFG_FIELD_USERNAME )))
                {
                    userName = tmpArg.Split("=")[1];
                }
                else if (tmpArg.Contains(string.Format("{0}=",CFG_FIELD_APITOKEN )))
                {
                    apiToken = tmpArg.Split("=")[1];
                }
                else if (tmpArg.Contains(string.Format("{0}=",CFG_FIELD_BASEURL )))
                {
                    baseUrl = tmpArg.Split("=")[1];
                }
                else if (tmpArg.Contains(string.Format("{0}=",CFG_FIELD_PROJECT )))
                {
                    defaultProject = tmpArg.Split("=")[1];
                }
            }
            if (ValidConfig==true && saveToFile != null && saveToFile.Value == true)
            {
                SaveToFile(ConfigFilePath,1);
            }

            return ValidConfig;
        }

        public void SaveToFile(string filePath, int? configNumber)
        {
            if (ValidConfig==true)
            {
                if (configNumber==null){configNumber = 1;}
                using (StreamWriter writer = new StreamWriter(filePath,false))
                {
                    writer.WriteLine(String.Format("## BEGIN JTIS_CONFIG_{0:00}",configNumber.Value));
                    writer.WriteLine(String.Format("{0}={1}",CFG_FIELD_USERNAME,userName));
                    writer.WriteLine(String.Format("{0}={1}",CFG_FIELD_APITOKEN,apiToken));
                    writer.WriteLine(String.Format("{0}={1}",CFG_FIELD_BASEURL,baseUrl));
                    writer.WriteLine(String.Format("{0}={1}",CFG_FIELD_PROJECT,defaultProject));
                    writer.WriteLine(String.Format("## END JTIS_CONFIG_{0:00}",configNumber.Value));
                }
            }
            
        }

        private bool PopulateFromFile()
        {
            userName = null;
            apiToken = null;
            baseUrl = null;
            defaultProject = null;

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
                                if (lineArr[0].ToLower() == CFG_FIELD_USERNAME)
                                {
                                    userName = lineArr[1];
                                } else if (lineArr[0].ToLower() == CFG_FIELD_APITOKEN)
                                {
                                    apiToken = lineArr[1];
                                }else if (lineArr[0].ToLower() == CFG_FIELD_BASEURL)
                                {
                                    baseUrl = lineArr[1];
                                }else if (lineArr[0].ToLower() == CFG_FIELD_PROJECT)
                                {
                                    defaultProject = lineArr[1];
                                } 
                            }
                        }
                    }
                    return ValidConfig;
                }    
            } else 
            {
                File.Create(Path.Combine(ConfigFolderPath,configFileName));
                return false;
            }
        }

    }
}
