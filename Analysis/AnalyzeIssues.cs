

namespace JiraCon
{

    public enum AnalysisType
    {
        _atUnknown = -1, 
        atJQL = 1, 
        atIssues = 2, 
        atEpics = 3
    }

    public class AnalyzeIssues
    {
        private AnalysisType _type = AnalysisType._atUnknown;
        private string searchData = string.Empty;

        public AnalyzeIssues()
        {

        }
        public AnalyzeIssues(AnalysisType analysisType): this()
        {
            _type = analysisType;
            string? data = string.Empty;
            ConsoleUtil.WriteStdLine("ENTER 1 OR MORE ISSUE KEYS (E.G. WWT-100 WWT-101) SEPARATED BY SPACES",StdLine.slResponse,false);
            ConsoleUtil.WriteStdLine("(IF YOU WISH TO CANCEL OR SELECT A SAVED LIST OF ISSUES, PRESS 'ENTER')",StdLine.slResponse,false);
            data = Console.ReadLine();
            if (data == null || data.Length == 0)
            {
                ConsoleUtil.WriteStdLine("PRESS 'Y' TO SELECT A SAVED LIST OF ISSUES - ANY OTHER KEY TO CANCEL",StdLine.slResponse,false);
                if (Console.ReadKey().Key == ConsoleKey.Y)
                {
                    data = SelectSavedJQL();
                }
            }
        }

        private string? SelectSavedJQL()
        {
            string title = string.Empty;
            string ret = string.Empty;
            switch(_type)
            {
                case AnalysisType.atIssues:
                    title = "SELECT SAVED LIST (SPACE-DELIMITED ISSUE KEYS) - ANALYSIS WILL RUN ON EACH ITEM IN THE LIST";
                    break;
                case AnalysisType.atEpics:
                    title = "SELECT SAVED LIST (SPACE-DELIMITED EPIC KEYS) - ANALYSIS WILL RUN ON ALL CHILDREN LINKED TO EPIC(S)";
                    break;
                case AnalysisType.atJQL:
                    title = "SELECT SAVED JQL QUERY (MUST BE VALID JQL) - ANALYSIS WILL RUN ON ISSUES RETURNED FROM QUERY";
                    break;
                default:
                    title = string.Empty;
                    break;
            }
            if (title.Length == 0)
            {
                return string.Empty;;
            }
            ret = JTISConfigHelper.GetSavedJQL(title);
            if (ret != null && ret.Length > 0)
            {
                ConsoleUtil.WriteStdLine("PRESS 'Y' TO USE THE FOLLOWING SAVED JQL/QUERY - ANY OTHER KEY TO CANCEL",StdLine.slResponse,false);
                if (Console.ReadKey().Key == ConsoleKey.Y)
                {
                    return ret;
                }
            }
            return String.Empty;
        }
    }
}