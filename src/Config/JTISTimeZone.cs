using JTIS.Console;
using JTIS.Extensions;

namespace JTIS.Config
{
    public static class JTISTimeZone
    {
        private static TimeZoneInfo? _localTimeZone = TimeZoneInfo.Local;
        private static TimeZoneInfo? _targetTimeZone = null;
        private static Guid? ActiveConfig {get;set;} = null;
        public static TimeZoneInfo? TargetTimeZone {get;private set;} = null;

        public static DateTime CheckDate(DateTime input)
        {
            if (DefaultTimeZone)
            {
                return input;
            }
            else 
            {
                return TimeZoneInfo.ConvertTime(input,DisplayTimeZone);
                //DateTime dateTime_Eastern = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Eastern_Standard_Time);
            }
        }
        public static TimeZoneInfo DisplayTimeZone
        {
            get{
                if (_targetTimeZone == null && CfgManager.config.TimeZoneId != null)
                {
                    SetJTISTimeZone(CfgManager.config);
                }
                if (ActiveConfig == null || ActiveConfig.Value.Equals(CfgManager.config.Key.Value)==false)
                {
                    SetJTISTimeZone(CfgManager.config);
                }
                if (_targetTimeZone == null)
                {
                    return _localTimeZone;
                }
                else 
                {
                    return _targetTimeZone;
                }
            }
        }
        public static TimeZoneInfo LocalTimeZone
        {
            get{
                return _localTimeZone;
            }
        }
        public static bool DefaultTimeZone
        {
            get{
                return _targetTimeZone == null || _targetTimeZone.Id.StringsMatch(TimeZoneInfo.Local.Id);
            }
        }
        public static void Reset()
        {
            _targetTimeZone = null;
            ActiveConfig = null;
        }
        public static void SetJTISTimeZone(JTISConfig cfg)
        {
            Reset();
            ActiveConfig = cfg.Key.Value;
            if (cfg.TimeZoneId != null)
            {
                var lookupTZ = FindTimeZone(cfg.TimeZoneId);
                if (lookupTZ != null)
                {
                    _targetTimeZone = lookupTZ;
                }
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