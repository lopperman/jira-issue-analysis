using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace JiraCon
{
    public class FileUtil
    {
        public FileUtil()
        {
        }

        public static void SaveToJSON(List<JIssue> list, string path)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                string data = JsonConvert.SerializeObject(list,Formatting.None,settings);

                using (StreamWriter writer = new StreamWriter(path, false))
                {
                    writer.Write(data);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Obsolete("Need to figure out how to deserialize Atlassian.Jira.Issue",true)]
        public static List<JIssue> LoadFromJSON(string path)
        {
            List<JIssue> list = null;

            JsonSerializerSettings settings = new JsonSerializerSettings();

            try
            {
                string data = string.Empty;
                using (StreamReader reader = new StreamReader(path))
                {
                    data = reader.ReadToEnd();
                }

                list = JsonConvert.DeserializeObject<List<JIssue>>(data,settings);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return list;
        }
    }
}
