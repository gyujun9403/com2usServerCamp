using DungeonFarming.DataBase.GameDb.MasterData;
using DungeonFarming.DataBaseServices.GameDb.MasterDataDTO;

namespace DungeonFarming.DataBase.GameDb
{
    public interface IMasterDataOffer
    {
        public bool LoadMasterData();
        public DailyLoginReward? getDailyLoginReward(Int32 dayCount);
        public List<ItemBundle>? getDailyLoginRewardItemBundles(Int32 dayCount);
        public ItemAttrubute? getItemAttrubute(Int16 attribute);
        public List<PackageItem>? getPackage(Int16 packageCode);
        public List<ItemBundle>? getPackageItemBundles(Int16 packageCode);
        public DefaultItems? getDefaultItems(Int16 listCode);
        public List<ItemBundle>? getDefaultItemBundles(Int16 listCode);
        public ItemDefine? getItemDefine(Int64 itemCode);
        public Int64? getMaxExpOfLevel(Int32 level);
        public List<ItemBundle>? getStageItemBundle(Int64 stageCode);
        public List<NpcBundle>? getStageNpcBundle(Int64 stageCode);
        public StageInfo? getStageInfo(Int64 stageCode);
    }
}
