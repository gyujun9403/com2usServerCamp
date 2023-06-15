namespace DungeonFarming.DataBaseServices.GameDb.MasterDataModel;


public class StageInfo
{
    public Int32 stage_code { get; set; }
    public String stage_name { get; set; }
    public Int32 required_user_level { get; set; }
}

public class StageTotalInfo
{
    public Int32 stageId { get; set; }

}
