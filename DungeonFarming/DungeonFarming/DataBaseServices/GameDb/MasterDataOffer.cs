using DungeonFarming.DataBase.AccountDb;
using DungeonFarming.DataBase.GameDb.MasterData;
using DungeonFarming.DataBaseServices.GameDb.MasterDataDTO;
using MySqlConnector;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Reflection.Emit;
using System.Security.Principal;
using ZLogger;

namespace DungeonFarming.DataBase.GameDb
{
    public class MasterDataOffer : IMasterDataOffer
    {
        ILogger<MysqlAccountDb> _logger;
        QueryFactory _db;
        Dictionary<Int32, DailyLoginReward> _dailyLoginRewards = new Dictionary<Int32, DailyLoginReward>();
        Dictionary<Int16, ItemAttrubute> _itemAttributes = new Dictionary<Int16, ItemAttrubute>();
        Dictionary<Int16, List<PackageItem> > _packages = new Dictionary<Int16, List<PackageItem> >();
        Dictionary<Int16, DefaultItems> _defaultItemsList = new Dictionary<Int16, DefaultItems>();
        Dictionary<Int64, ItemDefine> _itemDefines = new Dictionary<Int64, ItemDefine>();
        Dictionary<Int32, Int64> _expPerUserLevel = new Dictionary<Int32, Int64>();
        //Dictionary<Int64, List<ItemBundle> > _stageItemLists = new Dictionary<Int64, List<ItemBundle>>();
        Dictionary<Int64, Dictionary<Int64, ItemBundle>> _stageItemDics = new Dictionary<Int64, Dictionary<Int64, ItemBundle>>();
        //Dictionary<Int64, List<StageNpcInfo>> _stageNpcLists = new Dictionary<Int64, List<StageNpcInfo>>();
        Dictionary<Int64, Dictionary<Int64, StageNpcInfo>> _stageNpcDics = new Dictionary<Int64, Dictionary<Int64, StageNpcInfo>>();
        Dictionary<Int64, StageInfo> _stageInfos = new Dictionary<Int64, StageInfo>();

        public MasterDataOffer(IConfiguration config, ILogger<MysqlAccountDb> logger)
        {
            var connString = config.GetConnectionString("Mysql_Game");
            var connection = new MySqlConnection(connString);
            var compiler = new MySqlCompiler();
            _db = new QueryFactory(connection, compiler);
            _logger = logger;
        }


        /*-------------------------
             마스터 데이터 로드
        --------------------------*/
        public bool LoadMasterData()
        {
            return (LoadDailyLoginRewards() && LoadItemAttributes()
                && LoadPackage() && LoadDefaultItemLists() && LoadItemDefines())
                && LoadUserLevelExp() && LoadStageItems() && LoadStageNpcs() && LoadStageInfos();
        }


        /*-------------------------
         마스터 데이터 데이터 getter
        --------------------------*/
        public DailyLoginReward? getDailyLoginReward(Int32 dayCount)
        {
            if (_dailyLoginRewards.ContainsKey(dayCount))
            {
                return _dailyLoginRewards[dayCount];
            }
            return null;
        }
        public List<ItemBundle>? getDailyLoginRewardItemBundles(Int32 dayCount)
        {
            if (_dailyLoginRewards.ContainsKey(dayCount))
            {
                List<ItemBundle> itemBundles = new List<ItemBundle>();
                DailyLoginReward dailyLoginReward = _dailyLoginRewards[dayCount];
                if (dailyLoginReward.item_code != -1 && dailyLoginReward.item_count != -1)
                {
                    itemBundles.Add(new ItemBundle
                    {
                        itemCode = dailyLoginReward.item_code,
                        itemCount = dailyLoginReward.item_count
                    });
                }
                return itemBundles;
            }
            return null;
        }
        public ItemAttrubute? getItemAttrubute(Int16 attribute)
        {
            
            if (_itemAttributes.ContainsKey(attribute))
            {
                return _itemAttributes[attribute];
            }
            return null;
        }
        public List<PackageItem>? getPackage(Int16 packageCode)
        {
            if (_packages.ContainsKey(packageCode))
            {
                return _packages[packageCode];
            }
            return null;
        }
        public List<ItemBundle>? getPackageItemBundles(Int16 packageCode)
        {
            if (_packages.ContainsKey(packageCode))
            {
                var items = _packages[packageCode];
                var itemBundles = new List<ItemBundle>();
                foreach (var item in items)
                {
                    if (item.item_code != -1 && item.item_count != -1)
                    {
                        itemBundles.Add(new ItemBundle
                        {
                            itemCode = item.item_code,
                            itemCount = item.item_count
                        });
                    }
                }
                return itemBundles;
            }
            return null;
        }
        public DefaultItems? getDefaultItems(Int16 listCode)
        {
            if (_defaultItemsList.ContainsKey(listCode))
            {
                return _defaultItemsList[listCode];
            }
            return null;
        }
        public List<ItemBundle>? getDefaultItemBundles(Int16 listCode)
        {
            if (_defaultItemsList.ContainsKey(listCode))
            {
                List<ItemBundle> itemBundles = new List<ItemBundle>();
                DefaultItems defaultItems = _defaultItemsList[listCode];
                if (defaultItems.item0_code != -1 && defaultItems.item0_count != -1)
                {
                    itemBundles.Add(new ItemBundle
                    {
                        itemCode = defaultItems.item0_code,
                        itemCount = defaultItems.item0_count
                    });
                }
                if (defaultItems.item1_code != -1 && defaultItems.item1_count != -1)
                {
                    itemBundles.Add(new ItemBundle
                    {
                        itemCode = defaultItems.item1_code,
                        itemCount = defaultItems.item1_count
                    });
                }
                if (defaultItems.item2_code != -1 && defaultItems.item2_count != -1)
                {
                    itemBundles.Add(new ItemBundle
                    {
                        itemCode = defaultItems.item2_code,
                        itemCount = defaultItems.item2_count
                    });
                }
                if (defaultItems.item3_code != -1 && defaultItems.item3_count != -1)
                {
                    itemBundles.Add(new ItemBundle
                    {
                        itemCode = defaultItems.item3_code,
                        itemCount = defaultItems.item3_count
                    });
                }
                return itemBundles;
            }
            return null;
        }
        public ItemDefine? getItemDefine(Int64 itemCode)
        {
            if (_itemDefines.ContainsKey(itemCode))
            {
                return _itemDefines[itemCode];
            }
            return null;
        }
        public Int64? getMaxExpOfLevel(Int32 level)
        {
            if (_expPerUserLevel.ContainsKey(level))
            {
                return _expPerUserLevel[level];
            }
            return null;
        }

        public List<ItemBundle>? getStageItemInfoList(Int64 stageCode)
        {
            if (_stageItemDics.ContainsKey(stageCode))
            {
                return _stageItemDics[stageCode].Values.ToList();
            }
            return null;
        }

        public Dictionary<Int64, ItemBundle>? getStageItemInfoDic(Int64 stageCode)
        {
            if (_stageItemDics.ContainsKey(stageCode))
            {
                return _stageItemDics[stageCode];
            }
            return null;
        }

        public List<StageNpcInfo>? getStageNpcInfoList(Int64 stageCode)
        {
            if (_stageNpcDics.ContainsKey(stageCode))
            {
                return _stageNpcDics[stageCode].Values.ToList();
            }
            return null;
        }

        public Dictionary<Int64, StageNpcInfo>? getStageNpcInfoDic(Int64 stageCode)
        {
            if (_stageNpcDics.ContainsKey(stageCode))
            {
                return _stageNpcDics[stageCode];
            }
            return null;
        }

        public StageInfo? getStageInfo(Int64 stageCode)
        {
            if (_stageInfos.ContainsKey(stageCode))
            {
                return _stageInfos[stageCode];
            }
            return null;
        }


        /*-------------------------
         마스터 데이터 각 테이블 로드
        --------------------------*/
        bool LoadDailyLoginRewards()
        {
            try
            {
                IEnumerable<DailyLoginReward> masterData = _db.Query("mt_daily_login_rewards")
                    .Select("*").Get<DailyLoginReward>();
                foreach (DailyLoginReward rewardElem in masterData)
                {
                    _dailyLoginRewards.Add(rewardElem.day_count, rewardElem);
                }
                return true;
            }
            catch (MySqlException ex)
            {
                _logger.ZLogCriticalWithPayload(LogEventId.MasterDataOffer, ex, new { }, "LoadDailyLoginRewards MySqlException");
                return false;
            }
            catch (Exception ex)
            {
                _logger.ZLogCriticalWithPayload(LogEventId.MasterDataOffer, ex, new { }, "LoadDailyLoginRewards Exception");
                return false;
            }
        }
        bool LoadItemAttributes()
        {
            try
            {
                IEnumerable<ItemAttrubute> masterData = _db.Query("mt_item_attributes")
                    .Select("*").Get<ItemAttrubute>();
                foreach (ItemAttrubute attributeElem in masterData)
                {
                    _itemAttributes.Add(attributeElem.attribute, attributeElem);
                }
                return true;
            }
            catch (MySqlException ex)
            {
                _logger.ZLogCriticalWithPayload(LogEventId.MasterDataOffer, ex, new { }, "LoadItemAttributes MySqlException");
                return false;
            }
            catch (Exception ex)
            {
                _logger.ZLogCriticalWithPayload(LogEventId.MasterDataOffer, ex, new { }, "LoadItemAttributes Exception");
                return false;
            }
        }
        bool LoadPackage()
        {
            try
            {
                IEnumerable<PackageItem> masterData = _db.Query("mt_package")
                    .Select("*").Get<PackageItem>();
                foreach (PackageItem itemElem in masterData)
                {
                    if (_packages.ContainsKey(itemElem.package_code) == true)
                    {
                        _packages[itemElem.package_code].Add(itemElem);
                    }
                    else
                    {
                        _packages.Add(itemElem.package_code, new List<PackageItem> { itemElem });
                    }
                }
                return true;
            }
            catch (MySqlException ex)
            {
                _logger.ZLogCriticalWithPayload(LogEventId.MasterDataOffer, ex, new { }, "LoadPackage MySqlException");
                return false;
            }
            catch (Exception ex)
            {
                _logger.ZLogCriticalWithPayload(LogEventId.MasterDataOffer, ex, new { }, "LoadPackage Exception");
                return false;
            }
        }
        bool LoadDefaultItemLists()
        {
            try
            {
                IEnumerable<DefaultItems> masterData = _db.Query("mt_default_items_list")
                    .Select("*").Get<DefaultItems>();
                foreach (DefaultItems itemListElem in masterData)
                {
                    _defaultItemsList.Add(itemListElem.list_id, itemListElem);
                }
                return true;
            }
            catch (MySqlException ex)
            {
                _logger.ZLogCriticalWithPayload(LogEventId.MasterDataOffer, ex, new { }, "LoadDefaultItemLists MySqlException");
                return false;
            }
            catch (Exception ex)
            {
                _logger.ZLogCriticalWithPayload(LogEventId.MasterDataOffer, ex, new { }, "LoadDefaultItemLists Exception");
                return false;
            }
        }
        bool LoadItemDefines()
        {
            try
            {
                IEnumerable<ItemDefine> masterData = _db.Query("mt_item_defines")
                    .Select("*").Get<ItemDefine>();
                foreach (ItemDefine itemDefineElem in masterData)
                {
                    _itemDefines.Add(itemDefineElem.item_code, itemDefineElem);
                }
                return true;
            }
            catch (MySqlException ex)
            {
                _logger.ZLogCriticalWithPayload(LogEventId.MasterDataOffer, ex, new { }, "LoadItemDefines MySqlException");
                return false;
            }
            catch (Exception ex)
            {
                _logger.ZLogCriticalWithPayload(LogEventId.MasterDataOffer, ex, new { }, "LoadItemDefines Exception");
                return false;
            }
        }
        bool LoadUserLevelExp()
        {
            try
            {
                IEnumerable<UserLevelExp> masterData = _db.Query("mt_user_level_exps")
                    .Select("*").Get<UserLevelExp>();
                foreach (UserLevelExp expPerLevel in masterData)
                {
                    _expPerUserLevel.Add(expPerLevel.user_level, expPerLevel.max_exp);
                }
                return true;
            }
            catch (MySqlException ex)
            {
                _logger.ZLogCriticalWithPayload(LogEventId.MasterDataOffer, ex, new { }, "LoadUserLevelExp MySqlException");
                return false;
            }
            catch (Exception ex)
            {
                _logger.ZLogCriticalWithPayload(LogEventId.MasterDataOffer, ex, new { }, "LoadUserLevelExp Exception");
                return false;
            }
        }

        bool LoadStageItems()
        {
            try
            {
                Dictionary<Int64, List<Int64> > registered_items = new Dictionary<Int64, List<Int64> >();
                IEnumerable<StageItem> masterData = _db.Query("mt_stage_items")
                    .Select("*").Get<StageItem>();
                foreach (StageItem stageItem in masterData)
                {
                    if (registered_items.ContainsKey(stageItem.stage_code))
                    {
                        if (registered_items[stageItem.stage_code].Contains(stageItem.item_code) == true)
                        {
                            continue;
                        }
                    }

                    Int64 stageItemCount = _db.Query("mt_stage_items")
                        .Where("stage_code", stageItem.stage_code)
                        .Where("item_code", stageItem.item_code)
                        .Count<Int64>();
                    if (stageItemCount <= 0)
                    {

                    }

                    if (registered_items.ContainsKey(stageItem.stage_code))
                    {
                        registered_items[stageItem.stage_code].Add(stageItem.item_code);
                    }
                    else
                    {
                        registered_items.Add(stageItem.stage_code, new List<Int64> { stageItem.item_code });
                    }

                    if (_stageItemDics.ContainsKey(stageItem.stage_code) == true)
                    {
                        _stageItemDics[stageItem.stage_code].Add(stageItem.item_code, new ItemBundle
                        {
                            itemCode = stageItem.item_code,
                            itemCount = stageItemCount
                        });
                    }
                    else
                    {
                        _stageItemDics.Add(stageItem.stage_code, new Dictionary<Int64, ItemBundle> {
                            { stageItem.item_code, new ItemBundle { itemCode = stageItem.item_code, itemCount = stageItemCount } }
                            });
                    }
                }
                return true;
            }
            catch (MySqlException ex)
            {
                _logger.ZLogCriticalWithPayload(LogEventId.MasterDataOffer, ex, new { }, "LoadStageItems MySqlException");
                return false;
            }
            catch (Exception ex)
            {
                _logger.ZLogCriticalWithPayload(LogEventId.MasterDataOffer, ex, new { }, "LoadStageItems Exception");
                return false;
            }
        }

        bool LoadStageNpcs()
        {
            try
            {
                Dictionary<Int64, List<Int64>> registered_npcs = new Dictionary<Int64, List<Int64>>();
                IEnumerable<StageNpc> masterData = _db.Query("mt_stage_npcs")
                    .Select("*").Get<StageNpc>();
                foreach (StageNpc stageNpc in masterData)
                {
                    if (registered_npcs.ContainsKey(stageNpc.stage_code))
                    {
                        if (registered_npcs[stageNpc.stage_code].Contains(stageNpc.npc_code) == true)
                        {
                            continue;
                        }
                    }

                    if (registered_npcs.ContainsKey(stageNpc.stage_code))
                    {
                        registered_npcs[stageNpc.stage_code].Add(stageNpc.npc_code);
                    }
                    else
                    {
                        registered_npcs.Add(stageNpc.stage_code, new List<Int64> { stageNpc.npc_code });
                    }

                    if (_stageNpcDics.ContainsKey(stageNpc.stage_code) == true)
                    {
                        _stageNpcDics[stageNpc.stage_code].Add(stageNpc.npc_code, new StageNpcInfo
                        {
                            npcCode = stageNpc.npc_code,
                            npcCount = stageNpc.npc_count,
                            expPerNpc = stageNpc.exp_per_npc
                        });
                    }
                    else
                    {
                        _stageNpcDics.Add(stageNpc.stage_code, new Dictionary<Int64, StageNpcInfo> {
                            { stageNpc.npc_code, new StageNpcInfo { npcCode = stageNpc.npc_code, npcCount = stageNpc.npc_count, expPerNpc = stageNpc.exp_per_npc }}
                        });
                    }
                }
                return true;
            }
            catch (MySqlException ex)
            {
                _logger.ZLogCriticalWithPayload(LogEventId.MasterDataOffer, ex, new { }, "LoadStageNpcs MySqlException");
                return false;
            }
            catch (Exception ex)
            {
                _logger.ZLogCriticalWithPayload(LogEventId.MasterDataOffer, ex, new { }, "LoadStageNpcs Exception");
                return false;
            }
        }

        bool LoadStageInfos()
        {
            try
            {
                IEnumerable<StageInfo> masterData = _db.Query("mt_stage_infos")
                    .Select("*").Get<StageInfo>();
                foreach (StageInfo stageInfo in masterData)
                {
                    _stageInfos.Add(stageInfo.stage_code, stageInfo);
                }
                return true;
            }
            catch (MySqlException ex)
            {
                _logger.ZLogCriticalWithPayload(LogEventId.MasterDataOffer, ex, new { }, "LoadStageInfos MySqlException");
                return false;
            }
            catch (Exception ex)
            {
                _logger.ZLogCriticalWithPayload(LogEventId.MasterDataOffer, ex, new { }, "LoadStageInfos Exception");
                return false;
            }
        }

    }
}
