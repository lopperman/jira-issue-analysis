# jiraTimeInStatus
Utility to determine amount of time a jira issue has spend in any status

## Mac and PC Compatible

### Console App to interact with Jira Cloud
#### Primary Purpose - retrieve ALL issueLogs for stories in order to find and analyze about of calendar time, and 'In Progress' time a Jira Issue has spent in any particular state.
 - Retrieve Issue Status mapping from Jira to determine if any given issue status is a 'TO DO', 'IN PROGRESS', or 'DONE' -- this issue is written to a local file, but can be overriden
 - Outputs to csv file with the following columns: key,type,created,featureTeam,summary,epicKey,parentIssueKey,currentStatus,labels,start,end,status,activeWork,calendarWork,totalBusinessDays,totalBusinessHours_8HourDay,transitionAfterHours,exclude,reason
