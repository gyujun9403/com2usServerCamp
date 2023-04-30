namespace DungeonFarming.DataBase.GameDb
{
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

        public List<(Int16, Int64)> getCurrencyList()
        {
            List<(Int16, Int64)> rt = new List<(Int16, Int64)>();
            rt.Add((currency_code, currency_count));
            return rt;
        }

        public List<(Int16, Int64)> getItemList()
        {
            List<(Int16, Int64)> rt = new List<(Int16, Int64)>();
            rt.Add((item0_code, item0_count));
            rt.Add((item1_code, item1_count));
            rt.Add((item2_code, item2_count));
            return rt;
        }
    }
}
