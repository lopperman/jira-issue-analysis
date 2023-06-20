[![.NET](https://github.com/lopperman/jiraTimeInStatus/actions/workflows/dotnet.yml/badge.svg)](https://github.com/lopperman/jiraTimeInStatus/actions/workflows/dotnet.yml)
---
# jiraTimeInStatus
### Description

##### Retrieve Jira Issues via [Jira v3 REST API](https://developer.atlassian.com/cloud/jira/platform/rest/v3/intro/#version), and calculate amount of time issues have spent in any combination of defined Issues States.  All user-defined states resolve to _TO DO, IN PROGRESS, or DONE_.  A configuration file is created which maps issues statuses to 'Active' or 'Waiting', and is used to calculate total 'Active Days' and 'Calendar Days'.  
##### It can be difficult to obtain this information from a Jira Cloud Instance, as it requires accessing all the change logs for an issue.  This tool obtains all issue change logs (including where the number of change logs exceeds maximum changes logs per API request).
---
#### (Will update help files soon -- feel free to [comment and ask questions](https://github.com/lopperman/jiraTimeInStatus/discussions))
---
[Latest Release](https://github.com/lopperman/jiraTimeInStatus/releases)
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

![](https://github.com/lopperman/jiraTimeInStatus/blob/master/images/IssueStatusMapping.png?raw=true)
![](https://github.com/lopperman/jiraTimeInStatus/blob/master/images/showChangeHistory_1.png?raw=true)
![](https://github.com/lopperman/jiraTimeInStatus/blob/master/images/showChangeHistory_2.png?raw=true)
![](https://github.com/lopperman/jiraTimeInStatus/blob/master/images/showChangeHistory_3.png?raw=true)
![](https://github.com/lopperman/jiraTimeInStatus/blob/master/images/showChangeHistory_4.png?raw=true)
---
![](https://github.com/lopperman/jiraTimeInStatus/blob/master/images/SingleIssueSummary.png?raw=true)
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




