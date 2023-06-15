using DungeonFarming.DataBase.GameDb.GameDbModel;
using DungeonFarming.DataBase.GameDb.MasterDataModel;
using DungeonFarming.DataBaseServices.GameDb.MasterDataModel;

namespace DungeonFarming.DataBase.GameDb;

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
    public List<ItemBundle>? getStageItemInfoList(Int32 stageCode);
    public Dictionary<Int64, ItemBundle>? getStageItemInfoDic(Int32 stageCode);
    public List<StageNpcInfo>? getStageNpcInfoList(Int32 stageCode);
    public Dictionary<Int64, StageNpcInfo>? getStageNpcInfoDic(Int32 stageCode);
    public StageInfo? getStageInfo(Int32 stageCode);
}
