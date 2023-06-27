namespace JiraCon
{
    internal class VisualSnapshot
    {
        public VisualSnapshotType SnapshotType {get;private set;}
        public VisualSnapshot()
        {
        }
        public VisualSnapshot(VisualSnapshotType snapshotType):this()
        {
            SnapshotType = snapshotType;                        
            Summarize();
        }
        private void Summarize()
        {
            //https://graphiant.atlassian.net/rest/api/3/search?jql=project=WWT&fields=issueType,status,key,priority,flagged&expand=names
            //var issues = JiraUtil.JiraRepo.GetIssues
        }
    }
}