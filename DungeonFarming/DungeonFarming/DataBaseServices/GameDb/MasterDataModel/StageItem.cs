using DungeonFarming.DataBase.GameDb;
namespace DungeonFarming.DataBaseServices.GameDb.MasterDataModel;

public class StageItem
{
    public Int32 pk_id { get; set; }
    public Int32 stage_code { get; set; }
    public Int32 item_code { get; set; }
}

public class StageItemList
{
    public Int32 stageCode { get; set; }
    public List<ItemBundle>? itemList { get; set; }
}
