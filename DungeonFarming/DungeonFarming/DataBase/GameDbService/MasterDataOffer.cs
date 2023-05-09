using DungeonFarming.DataBase.AccountDb;
using DungeonFarming.DataBase.GameDb.MasterData;
using MySqlConnector;
using SqlKata.Compilers;
using SqlKata.Execution;
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
        Dictionary<Int16, ItemDefine> _itemDefines = new Dictionary<Int16, ItemDefine>();
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
        public bool LoadMasterDatas()
        {
            return (LoadDailyLoginRewards() && LoadItemAttributes() 
                && LoadPackage() && LoadDefaultItemLists() && LoadItemDefines());
        }


        /*-------------------------
         마스터 데이터 데이터 getter
        --------------------------*/
        public DailyLoginReward? getDailyLoginReward(Int32 dayCount)
        {
            if (_dailyLoginRewards.ContainsKey(dayCount))
                return _dailyLoginRewards[dayCount];
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
                return _itemAttributes[attribute];
            return null;
        }
        public List<PackageItem>? getPackage(Int16 packageCode)
        {
            if (_packages.ContainsKey(packageCode))
                return _packages[packageCode];
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
                return _defaultItemsList[listCode];
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
        public ItemDefine? getItemDefine(Int16 itemCode)
        {
            if (_itemDefines.ContainsKey(itemCode))
                return _itemDefines[itemCode];
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
    }
}
