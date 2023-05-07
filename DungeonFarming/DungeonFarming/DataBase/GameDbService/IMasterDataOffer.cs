using DungeonFarming.DataBase.GameDb.MasterData;

namespace DungeonFarming.DataBase.GameDb
{
    public interface IMasterDataOffer
    {
        public bool LoadMasterDatas();
        public DailyLoginReward? getDailyLoginReward(Int32 dayCount);
        public List<ItemBundle>? getDailyLoginRewardItemBundles(Int32 dayCount);
        public ItemAttrubute? getItemAttrubute(Int16 attribute);
        public List<PackageItem>? getPackage(Int16 packageCode);
        public List<ItemBundle>? getPackageItemBundles(Int16 packageCode);
        public DefaultItems? getDefaultItems(Int16 listCode);
        public List<ItemBundle>? getDefaultItemBundles(Int16 listCode);
        public ItemDefine? getItemDefine(Int16 itemCode);
    }
}
