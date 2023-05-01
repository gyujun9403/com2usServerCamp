namespace DungeonFarming.DataBase.GameDb
{
    public class LoginLogForReword
    {
        public Int64 user_id { get; set; }
        public Int16 consecutive_login_count { get; set; }
        public Int16 missed_login_count { get; set; }
        public Int16 today_login { get; set; }
    }
}
