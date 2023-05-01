
public class ItemInfo
{
    public Int16 itemId { get; set; }
    public Int64 itemNum { get; set; }
}

public interface IItemList
{
    public List<ItemInfo> getCurrencyList();
    public List<ItemInfo> getItemList();
}
