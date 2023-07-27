namespace JTIS.Config;

public enum CfgEnum
{
    cfgUNKNOWN = 0, 
    cfgCTIngoreIfMissingStart = 1, 
    cfgAutoRecordAndSave = 2, 
    cfgCTWeeklyTwoWeeks = 3
}

public class CfgOptions
{
    public List<CfgOption>? items {get;set;}
    public CfgOptions()
    {
        if (items == null){
            items = new List<CfgOption>();
        }
    }

    public void AddDefaultsIfMissing()
    {
        if (items.Exists(x=>x.configOption==CfgEnum.cfgCTIngoreIfMissingStart)==false){
            SetOption(CfgEnum.cfgCTIngoreIfMissingStart,true,"Ignores issues in certain areas if start and end states are defined, and found start/end states are identical.");
        }
        if (items.Exists(x=>x.configOption==CfgEnum.cfgAutoRecordAndSave)==false){
        SetOption(CfgEnum.cfgAutoRecordAndSave,false,"If enabled, when manual sessions recording is not active, session recording will activate and save file for screen output from Issue Analysis areas");
        }
        if (items.Exists(x=>x.configOption==CfgEnum.cfgCTWeeklyTwoWeeks)==false){
        SetOption(CfgEnum.cfgCTWeeklyTwoWeeks,false,"If enabled, Cycle-Time Weekly Grouping in 2 week blocks, otherwise (False) will use 1 week blocks");
        }


        
    }
    public void SetOption(CfgEnum cfgEnum, bool enabled, string? desc=null)
    {
        CfgOption item = items.SingleOrDefault(x=>x.configOption==cfgEnum);
        if (item != null)
        {
            item.Enabled = enabled;
        }
        else 
        {
            item = new CfgOption(cfgEnum,enabled,desc);
            items.Add(item);
        }
        CfgManager.config.IsDirty = true;
    }    
}

public class CfgOption
{
    public CfgEnum configOption {get;set;}
    public string Description {get;set;} = string.Empty;
    public bool Enabled {get;set;}

    public CfgOption()
    {

    }

    public static string ToMarkup(CfgOption cfgOpt)
    {
        string desc = cfgOpt.Description.Length > 0 ? cfgOpt.Description : "n/a";
        return $"[dim]Option:[/] [bold]{cfgOpt.configOptionName}[/], [dim]Enabled: [/][bold]{cfgOpt.Enabled}[/] - [italic]{desc}[/]";

    }
    public CfgOption(CfgEnum cfgEnum, bool enabled, string? desc = null)
    {
        configOption = cfgEnum;
        Enabled = enabled;
        if (desc == null)
        {
            Description = "n/a";            
        }
        else 
        {
            Description = desc;
        }
    }
    public string configOptionName 
    {
        get {
            return Enum.GetName(typeof(CfgEnum),configOption);
        }
    }
}