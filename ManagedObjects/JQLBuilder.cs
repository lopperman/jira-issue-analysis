

using System.Text;

namespace JiraCon
{
    public static class JQLBuilder
    {


        public static string BuildInList(string colName, string vals,char delimitChar = ' ', string? prependIfMissing=null)
        {
            string[]? valArr;
            valArr = vals.Split(delimitChar,StringSplitOptions.RemoveEmptyEntries);
            return BuildInList(colName,valArr,prependIfMissing);
        }

        public static string BuildInList(string colName, string[] vals,  string? prependIfMissing=null)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{colName} in (");
            bool added = false;
            if (vals.Length > 0)
            {
                for (int i = 0; i < vals.Length; i ++)
                {
                    var itemVal = vals[i];
                    itemVal = itemVal.Trim();
                    if (prependIfMissing != null)
                    {
                        if (!itemVal.StartsWith(prependIfMissing,StringComparison.CurrentCultureIgnoreCase))
                        {
                            itemVal = $"{prependIfMissing}{itemVal}";
                        }
                    }
                    if (itemVal.Contains(' '))
                    {
                        itemVal = $"'{itemVal}'";
                    }
                    if (!added)
                    {                        
                        sb.AppendFormat($"{itemVal}");
                        added=true;
                    }
                    else 
                    {
                        sb.AppendFormat($", {itemVal}");
                    }
                }
                sb.Append(")");
            }
            return sb.ToString();
        }

    }


}