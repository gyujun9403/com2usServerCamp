namespace DungeonFarming.DataBase.GameDb
{
    public class Inventory : IItemList
    {
        Int64 user_id { get; set; }
        Int16 currency0_code { get; set; }
        Int64 currency0_count { get; set; }
        Int16 item0_code { get; set; }
        Int64 item0_count { get; set; }
        Int16 item1_code { get; set; }
        Int64 item1_count { get; set; }
        Int16 item2_code { get; set; }
        Int64 item2_count { get; set; }
        Int16 item3_code { get; set; }
        Int64 item3_count { get; set; }
        Int16 item4_code { get; set; }
        Int64 item4_count { get; set; }
        Int16 item5_code { get; set; }
        Int64 item5_count { get; set; }
        Int16 item6_code { get; set; }
        Int64 item6_count { get; set; }
        Int16 item7_code { get; set; }
        Int64 item7_count { get; set; }
        Int16 item8_code { get; set; }
        Int64 item8_count { get; set; }
        Int16 item9_code { get; set; }
        Int64 item9_count { get; set; }
        public List<ItemInfo> getCurrencyList()
        {
            List<ItemInfo> currencyList = new List<ItemInfo>
            {
                new ItemInfo { itemId = currency0_code, itemNum = currency0_count }
            };
            return currencyList;
        }

        public List<ItemInfo> getItemList()
        {
            var itemList = new List<ItemInfo>
            {
                new ItemInfo { itemId = item0_code, itemNum = item0_count },
                new ItemInfo { itemId = item1_code, itemNum = item1_count },
                new ItemInfo { itemId = item2_code, itemNum = item2_count },
                new ItemInfo { itemId = item3_code, itemNum = item3_count },
                new ItemInfo { itemId = item4_code, itemNum = item4_count },
                new ItemInfo { itemId = item5_code, itemNum = item5_count },
                new ItemInfo { itemId = item6_code, itemNum = item6_count },
                new ItemInfo { itemId = item7_code, itemNum = item7_count },
                new ItemInfo { itemId = item8_code, itemNum = item8_count },
                new ItemInfo { itemId = item9_code, itemNum = item9_count }
            };
            return itemList;
        }
    }
}
