using System.Linq;
using Atlassian.Jira;


namespace JTIS.Data
{

    public class JFields
    {
        private Jira _jira;
        public JFields(Jira jira)
        {
            _jira = jira;
        }
        public static JFields Create(Jira jira)
        {
            return new JFields(jira);
        }

        // public async void Load()
        // {
        //     var jCustomFields =  _jira.Fields.GetCustomFieldsForProjectAsync(JTISConfigHelper).GetAwaiter().GetResult();

        //     //return GetSubTasksAsync(issue).GetAwaiter().GetResult().ToList();
        //     //
        // }
    }
}