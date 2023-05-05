namespace DungeonFarming.DataBase.GameDb.GameUserDataORM
{

    public class UserItem
    {
        public Int64 item_id { get; set; }
        public Int64 user_id { get; set; }
        public Int16 item_code { get; set; }
        public Int64 item_counte { get; set; }
        public Int32 attack { get; set; }
        public Int32 defence { get; set; }
        public Int32 magic { get; set; }
        public Int16 enhance_count { get; set; }
    }
}
