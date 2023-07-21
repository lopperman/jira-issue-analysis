namespace JTIS.Analysis;

using JTIS.Console;
using JTIS.Data;
using JTIS.Extensions;
using JTIS.Menu;
using Spectre.Console;

internal class Overview1
{
    jtisFilterItems<string> _issueTypeFilter = new jtisFilterItems<string>();
    private FetchOptions fetchOptions = FetchOptions.DefaultFetchOptions;
    private List<jtisIssue>?  _filteredIssues = new List<jtisIssue>();

    private jtisIssueData? _jtisIssueData = null;

    private Overview1(FetchOptions options)
    {
        fetchOptions = options;
    }

    public static void Create(FetchOptions options)
    {        
        var vs = new Overview1(options.AllowCachedSelection().CacheResults().IncludeChangeLogs());
        vs.Build();
    }

    private void CheckIssueTypeFilter()
    {        
        _issueTypeFilter.Clear();
        foreach (var kvp in _jtisIssueData.IssueTypesCount)
        {   
            var tKey = kvp.Key;
            var tVal = kvp.Value;

            _issueTypeFilter.AddFilterItem(tKey, $"Count: {tVal.ToString()}");
        }
        if (_issueTypeFilter.Count > 1)
        {
            if (ConsoleUtil.Confirm($"Filter which of the {_issueTypeFilter.Count} issue types get displayed?",true))
            {
                var response = MenuManager.MultiSelect<jtisFilterItem<string>>($"Choose items to include. [dim](To select all items, press ENTER[/])",_issueTypeFilter.Items.ToList());
                if (response != null && response.Count() > 0)
                {
                    _issueTypeFilter.Clear();
                    _issueTypeFilter.AddFilterItems(response); 
                }
            }
        }

        UpdateFilteredIssueList();
    }

    private void UpdateFilteredIssueList()
    {
        _filteredIssues.Clear();
        if (_issueTypeFilter.Count==0)
        {
            _filteredIssues = _jtisIssueData.jtisIssuesList;
        }
        else 
        {
            foreach (var issType in _issueTypeFilter.Items)
            {
                _filteredIssues.AddRange(_jtisIssueData.jtisIssuesList.Where(x=>x.jIssue.IssueType.StringsMatch(issType.Value)));
            }
        }
    }

    public void Build()
    {
        fetchOptions.IncludeChangeLogs().AllowCachedSelection().CacheResults();
        _jtisIssueData = IssueFetcher.FetchIssues(fetchOptions);
        if (fetchOptions.Cancelled) {return;}

        if (_jtisIssueData != null && _jtisIssueData.jtisIssueCount > 0)
        {
            CheckIssueTypeFilter();
            foreach (var item in _filteredIssues)
            {
                AnsiConsole.MarkupLine($"[bold]{item.jIssue.IssueType}[/], {item.jIssue.Key}, {item.jIssue.StatusName}");
            }
        }

        ConsoleUtil.PressAnyKeyToContinue();
    }

    private void Summarize()
    {
        //https://graphiant.atlassian.net/rest/api/3/search?jql=project=WWT&fields=issueType,status,key,priority,flagged&expand=names
        //var issues = JiraUtil.JiraRepo.GetIssues
    }
}
