[![.NET](https://github.com/lopperman/jiraTimeInStatus/actions/workflows/dotnet.yml/badge.svg)](https://github.com/lopperman/jiraTimeInStatus/actions/workflows/dotnet.yml)
---
# jiraTimeInStatus
---
#### (Will update help files soon -- feel free to [comment and ask questions](https://github.com/lopperman/jiraTimeInStatus/discussions))
---
[Alpha Release](https://github.com/lopperman/jiraTimeInStatus/releases/tag/0.1.0-alpha)
---
Utility to determine amount of time a jira issue has spend in any status
![](https://github.com/lopperman/jiraTimeInStatus/blob/master/images/mainMenu.png?raw=true)
![](https://github.com/lopperman/jiraTimeInStatus/blob/master/images/configMenu.png?raw=true)

## Mac and PC Compatible

### Console App to interact with Jira Cloud
#### Primary Purpose - retrieve ALL issueLogs for stories in order to find and analyze about of calendar time, and 'In Progress' time a Jira Issue has spent in any particular state.
 - Retrieve Issue Status mapping from Jira to determine if any given issue status is a 'TO DO', 'IN PROGRESS', or 'DONE' -- this issue is written to a local file, but can be overriden
 - Outputs to csv file with the following columns: key,type,created,featureTeam,summary,epicKey,parentIssueKey,currentStatus,labels,start,end,status,activeWork,calendarWork,totalBusinessDays,totalBusinessHours_8HourDay,transitionAfterHours,exclude,reason


## Configuration
 - Will create a local config file that contains userName, jira api token, base jira url, default project code

## Misc
 - Supports extracting Json from any Jira issue
 - Supports selecting issues for Time Analysis by entering list of issue keys, or by entering JQL, or by entering an Epic Key (will analyze all children of an Epic)
 - Provides views/extracts of various Jira configurations, including list of status configurations for a project, list of issue types, etc

![](https://github.com/lopperman/jiraTimeInStatus/blob/master/images/showChangeHistory_1.png?raw=true)
![](https://github.com/lopperman/jiraTimeInStatus/blob/master/images/showChangeHistory_2.png?raw=true)
![](https://github.com/lopperman/jiraTimeInStatus/blob/master/images/showChangeHistory_3.png?raw=true)
![](https://github.com/lopperman/jiraTimeInStatus/blob/master/images/showChangeHistory_4.png?raw=true)
---
![](https://github.com/lopperman/jiraTimeInStatus/blob/master/images/showJson.png?raw=true)
---
![](https://github.com/lopperman/jiraTimeInStatus/blob/master/images/itemStatusValues.png?raw=true)
---
![](https://github.com/lopperman/jiraTimeInStatus/blob/master/images/createExtractFiles.png?raw=true)
---
![](https://github.com/lopperman/jiraTimeInStatus/blob/master/images/createWorkmetricsAnalysis_1.png?raw=true)
![](https://github.com/lopperman/jiraTimeInStatus/blob/master/images/createWorkmetricsAnalysis_2.png?raw=true)
![](https://github.com/lopperman/jiraTimeInStatus/blob/master/images/createWorkmetricsAnalysis_3.png?raw=true)
![](https://github.com/lopperman/jiraTimeInStatus/blob/master/images/createWorkmetricsAnalysis_4.png?raw=true)
![](https://github.com/lopperman/jiraTimeInStatus/blob/master/images/createWorkmetricsAnalysis_5.png?raw=true)
![](https://github.com/lopperman/jiraTimeInStatus/blob/master/images/createWorkmetricsAnalysis_6.png?raw=true)




