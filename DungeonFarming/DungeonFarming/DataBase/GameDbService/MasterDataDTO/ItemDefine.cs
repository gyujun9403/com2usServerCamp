namespace DungeonFarming.DataBase.GameDb.MasterData
{
    public class ItemDefine
    {
        public Int16 item_code { get; set; }
        public String item_name { get; set; }
        public Int16 attribute { get; set; }
        public Int64 sell { get; set; }
        public Int64 buy { get; set; }
        public Int16 use_lv { get; set; }
        public Int32 attack { get; set; }
        public Int32 defence { get; set; }
        public Int32 magic { get; set; }
        public Int16 enhance_max_count { get; set; }
        public Int64 max_stack { get; set; }
    }
}
