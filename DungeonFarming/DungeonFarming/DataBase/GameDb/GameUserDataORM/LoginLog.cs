namespace DungeonFarming.DataBase.GameDb.GameUserDataORM
{
    public class LoginLog
    {
        public long user_id { get; set; }
        public short consecutive_login_count { get; set; }
        public DateTime last_login_date { get; set; }
    }
}
