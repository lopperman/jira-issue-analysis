using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace JTIS
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

    }
}
