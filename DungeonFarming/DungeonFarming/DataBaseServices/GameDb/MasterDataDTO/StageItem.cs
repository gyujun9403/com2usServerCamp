using DungeonFarming.DataBase.GameDb;

namespace DungeonFarming.DataBaseServices.GameDb.MasterDataDTO
{
    public class StageItem
    {
        public Int64 pk_id { get; set; }
        public Int64 stage_code { get; set; }
        public Int64 item_code { get; set; }
    }

    public class StageItemList
    {
        public Int64 stageCode { get; set; }
        public List<ItemBundle>? itemList { get; set; }
    }
}
