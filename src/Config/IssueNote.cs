using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace JTIS.Config
{
    [JsonObject]
    public class IssueNotes
    {
        private SortedDictionary<string,IssueNote>? _issueNotes;
        public SortedDictionary<string,IssueNote>? issueNotes {
            get
            {
                if (_issueNotes == null) {_issueNotes = new SortedDictionary<string, IssueNote>();}
                return _issueNotes;
            }
            set
            {
                _issueNotes = value;
            }
        }
        public bool HasNote(string issueKey)
        {
            return issueNotes.ContainsKey(issueKey.ToUpper().Trim());
        }

        public int Count
        {
            get{
                return issueNotes.Count();
            }
        }
        public IssueNotes CreateNote(string issueKey, string note)
        {
            issueKey=issueKey.ToUpper().Trim();
            issueNotes[issueKey]=IssueNote.CreateNote(issueKey,note);
            return this;
        }
        public IssueNotes DeleteNote(string issueKey)
        {
            issueKey=issueKey.ToUpper().Trim();
            if (HasNote(issueKey))
            {
                issueNotes.Remove(issueKey);
            }
            return this;
        }

        public IReadOnlyList<IssueNote> Notes
        {
            get{
                return issueNotes.Values.ToList();
            } 
        }
        public string GetNote(string issueKey)
        {
            issueKey=issueKey.ToUpper().Trim();
            if (HasNote(issueKey))
            {
                return issueNotes[issueKey].Value;
            }
            else 
            {
                return string.Empty;
            }
        }

    }
    [JsonObject]
    public class IssueNote
    {
        public string ID
        {
            get {return Key;}
            set {Key = value;}
        }
        public string Key {get;set;}
        public string Value {get;set;}
        public DateTime LastEdit {get;set;}

        public static IssueNote CreateNote(string issueKey, string note)
        {
            var issNote = new IssueNote();
            issNote.AddValue(issueKey.ToUpper().Trim(), note);
            return issNote;
        }
        public IssueNote()
        {
            Key = string.Empty;
            Value = string.Empty;
            LastEdit = DateTime.Now;
        }

        public void AddValue(string key, string value)
        {
            Key=key;
            Value=value;
            LastEdit = DateTime.Now;
        }

        public override string ToString()
        {
            return $"({LastEdit.ToString()}) {Value}";
        }

        public string ToStringExtended(char delimeter =',')
        {
            return $"{Key}{delimeter} {LastEdit}{delimeter} \"{Value}\"";
        }
    }
}