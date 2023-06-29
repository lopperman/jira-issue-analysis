using JTIS.Console;

namespace JTIS.Config
{
    public static class JTISTimeZone
    {
        private static TimeZoneInfo? _timeZoneInfo = null;
        private static int? cfgId;

        public static TimeZoneInfo DisplayTimeZone
        {
            get
            {
                if (_timeZoneInfo != null)
                {
                    return _timeZoneInfo ;
                }
                else 
                {
                    return TimeZoneInfo.Local;
                }
            }
        }

        public static bool DefaultTimeZone
        {
            get{
                return DisplayTimeZone.Id.Equals(TimeZoneInfo.Local.Id,StringComparison.OrdinalIgnoreCase);
            }
        }
        public static void Reset()
        {
            _timeZoneInfo = null;
            cfgId = 0;
        }
        public static void SetJTISTimeZone(JTISConfig cfg)
        {
            cfgId = cfg.configId;
            _timeZoneInfo = null;
            var lookupTZ = FindTimeZone(cfg.TimeZoneId);
            if (lookupTZ != null)
            {
                _timeZoneInfo = lookupTZ;
            }
        }
        private static TimeZoneInfo? FindTimeZone(string? id)
        {
            TimeZoneInfo? retTZ = null;

            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }
            else 
            {
                try 
                {
                    var findTZ = TimeZoneInfo.FindSystemTimeZoneById(id);
                    if (findTZ != null)
                    {
                        retTZ = findTZ;
                    }
                }
                catch (Exception exObj)
                {
                    ConsoleUtil.WriteError(string.Format("Error parsing TimeZone Id '{0}'",id),false,ex:exObj,pause:true);
                }
            }
            return retTZ;
        }
    }
}