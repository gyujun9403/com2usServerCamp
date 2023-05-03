namespace DungeonFarming.DataBase.GameDb.GameUserDataORM
{
    public class LoginLogForReword
    {
        public long user_id { get; set; }
        public short consecutive_login_count { get; set; }
        public short missed_login_count { get; set; }
        public short today_login { get; set; }
    }
}
