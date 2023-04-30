namespace DungeonFarming.DataBase.GameDb
{
    public interface IItemList
    {
        public List<(Int16, Int64)> getCurrencyList();
        public List<(Int16, Int64)> getItemList();
    }
}
