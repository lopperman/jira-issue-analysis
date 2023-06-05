using System;
using System.Collections.Generic;
using System.Linq;

namespace JiraCon
{
    public class JiraCard
    {
        private List<JiraChangeLog> _changeLogList = new List<JiraChangeLog>();

        public JiraCard()
        {
            
        }

        public JiraCard(string id, string key, string status, string desc, DateTime created, DateTime updated, string type)
        {
            _id = id;
            Key = key;
            Description = desc;
            Created = created;
            Updated = updated;
            Status = status;
            CardType = type;
        }

        private static double GetBusinessDays(DateTime startD, DateTime endD)
        {
            double calcBusinessDays =
                1 + ((endD - startD).TotalDays * 5 -
                (startD.DayOfWeek - endD.DayOfWeek) * 2) / 7;

            if (endD.DayOfWeek == DayOfWeek.Saturday) calcBusinessDays--;
            if (startD.DayOfWeek == DayOfWeek.Sunday) calcBusinessDays--;

            return calcBusinessDays;
        }

        public double? CycleTime
        {
            get
            {
                double? ret = (double?)null;

                if (DevStartDt.HasValue && DevDoneDt.HasValue)
                {
                    ret = GetBusinessDays(DevStartDt.Value, DevDoneDt.Value);
                }


                return ret;

            }
        }

        /// <summary>
        /// If card status is done or ready for demo, returns the first In Development date in 2020 that isn't followed by a "ready for development date"
        /// Otherewise, return the first state change date after 1/1/20 that is not followed by a "ready for development date"
        /// </summary>
        public DateTime? DevStartDt
        {
            get
            {
                DateTime? ret = (DateTime?)null;
                

                if (IsDone && DevDoneDt.HasValue)
                {
                    DateTime? latestReadyForDevDt = LatestReadyForDevelopment;

                    JiraChangeLog changeLog = null;

                    if (latestReadyForDevDt.HasValue)
                    {
                        changeLog = _changeLogList.Where(x => x.ToValue != null && x.ToValue.ToLower().StartsWith("in dev") && x.ChangeLogDt >= latestReadyForDevDt.Value).OrderBy(x => x.ChangeLogDt).FirstOrDefault();
                        if (changeLog != null)
                        {
                            ret = changeLog.ChangeLogDt;
                        }
                    }
                    else
                    {
                        changeLog = _changeLogList.Where(x => x.ToValue != null && x.ToValue.ToLower().StartsWith("in dev")).OrderBy(x => x.ChangeLogDt).FirstOrDefault();
                        if (changeLog != null)
                        {
                            ret = changeLog.ChangeLogDt;
                        }

                    }

                }

                return ret;
            }
        }


        private DateTime? LatestReadyForDevelopment
        {
            get
            {
                DateTime? ret = (DateTime?)null;

                JiraChangeLog changeLog = _changeLogList.Where(x => x.ToValue != null && x.ToValue.ToLower().StartsWith("ready for dev")).OrderByDescending(x => x.ChangeLogDt).FirstOrDefault();

                if (changeLog != null)
                {
                    return changeLog.ChangeLogDt;
                }

                return ret;
            }
        }

        public DateTime? DevDoneDt
        {
            get
            {
                DateTime? ret = (DateTime?)null;


                if (IsDone)
                {
                    JiraChangeLog changeLog = _changeLogList.Where(x => x.ToValue != null && x.ToValue.ToLower().StartsWith("ready for demo")).OrderByDescending(x => x.ChangeLogDt).FirstOrDefault();

                    if (changeLog != null)
                    {
                        ret = changeLog.ChangeLogDt.Date;
                    }
                    else
                    {
                        changeLog = _changeLogList.Where(x => x.ToValue != null && x.ToValue.ToLower().StartsWith("done")).OrderByDescending(x => x.ChangeLogDt).FirstOrDefault();

                        if (changeLog != null)
                        {
                            ret = changeLog.ChangeLogDt.Date;
                        }
                    }
                }

                return ret;
            }
        }

        public bool IsDone
        {
            get
            {
                return Status.ToLower().StartsWith("done") || Status.ToLower().StartsWith("ready for demo");
            }
        }

        public void AddChangeLog(JiraChangeLog changeLog)
        {
            if (!_changeLogList.Exists(x=>x._id == changeLog._id))
            {
                _changeLogList.Add(changeLog);
            }
        }

        public void AddChangeLog(string id, DateTime changeDt, string fieldName, string fieldType, string fromId, string fromValue, string toId, string toValue)
        {
            if (!_changeLogList.Exists(x=>x._id == id))
            {
                _changeLogList.Add(JiraChangeLog.BuildJiraChangeLog(id, changeDt, fieldName, fieldType, fromId, fromValue, toId, toValue));
            }
        }

        public string _id { get; set; }
        public string Key { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public string CardType { get; set; }

        public List<JiraChangeLog> ChangeLogs
        {
            get
            {
                return _changeLogList;
            }
        }


    }

    public class JiraChangeLog
    {
        public string _id { get; set; }
        public DateTime ChangeLogDt { get; set; }
        public string FieldName { get; set; }
        public string FieldType { get; set; }
        public string FromId { get; set; }
        public string FromValue { get; set; }
        public string ToId { get; set; }
        public string ToValue { get; set; }



        public JiraChangeLog(string id, DateTime changeDt, string fieldName, string fieldType, string fromId, string fromValue, string toId, string toValue)
        {
            _id = id;
            ChangeLogDt = changeDt;
            FieldName = fieldName;
            FieldType = fieldType;
            FromId = fromId;
            FromValue = fromValue;
            ToId = toId;
            ToValue = toValue;
           
        }

        public static JiraChangeLog BuildJiraChangeLog(string id, DateTime changeDt, string fieldName, string fieldType, string fromId, string fromValue, string toId, string toValue)
        {
            return new JiraChangeLog(id,changeDt,fieldName,fieldType,fromId,fromValue,toId,toValue);
                        
        }



    }
}
