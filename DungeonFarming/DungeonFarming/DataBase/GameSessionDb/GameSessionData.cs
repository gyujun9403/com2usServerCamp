namespace DungeonFarming.DataBase.GameSessionDb
{
    public enum UserStatus : Int16
    {
        Login = 0,
        Gaming = 1
    }
    public class GameSessionData
    {
        public String userId { get; set; }
        public String token { get; set; }
        //
        public UserStatus userStatus { get; set; }
    }
}
