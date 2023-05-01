public class DefaultItemList : IItemList
{
    public Int16 list_id { get; set; }
    public Int16 currency_code { get; set; }
    public Int64 currency_count { get; set; }
    public Int16 item0_code { get; set; }
    public Int64 item0_count { get; set; }
    public Int16 item1_code { get; set; }
    public Int64 item1_count { get; set; }
    public Int16 item2_code { get; set; }
    public Int64 item2_count { get; set; }

    public List<ItemInfo> getCurrencyList()
    {
        List<ItemInfo> rt = new List<ItemInfo> {
            new ItemInfo { itemId = currency_code, itemNum = currency_count } 
        } ;
        return rt;
    }

    public List<ItemInfo> getItemList()
    {
        List<ItemInfo> rt = new List<ItemInfo>
        { 
            new ItemInfo { itemId = item0_code, itemNum = item0_count},
            new ItemInfo { itemId = item1_code, itemNum = item1_count},
            new ItemInfo { itemId = item2_code, itemNum = item2_count}
        };
        return rt;
    }
}
