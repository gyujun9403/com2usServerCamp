namespace DungeonFarming.DataBase.GameDb.GameDbModel;

public class LoginLog
{
    public Int64 user_id { get; set; }
    public Int16 consecutive_login_count { get; set; }
    public DateTime last_login_date { get; set; }
}
