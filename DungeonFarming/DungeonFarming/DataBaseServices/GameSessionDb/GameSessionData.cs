namespace DungeonFarming.DataBase.GameSessionDb;

public enum UserStatus : Int16
{
    Login = 0,
    Gaming = 1
}

public class GameSessionData
{
    public String userAssignedId { get; set; }

    public Int64 userId { get; set; }
    public String token { get; set; }
    public UserStatus userStatus { get; set; }
    public Int32 stageCode { get; set; }
    public Dictionary<Int32, Int64> killedNpcs { get; set; } = new Dictionary<Int32, Int64>();
    public Dictionary<Int32, Int64> FarmedItems { get; set; } = new Dictionary<Int32, Int64>();

    public GameSessionData GetResetSession()
    {
        return new GameSessionData
        {
            userAssignedId = this.userAssignedId,
            userId = this.userId,
            token = this.token,
            userStatus = UserStatus.Login
        };
    }

    public void ResetGameData()
    {
        userStatus = UserStatus.Login;
        stageCode = 0;
        killedNpcs.Clear();
        FarmedItems.Clear();
    }

    public GameSessionData Clone()
    {
        return new GameSessionData
        {
            userAssignedId = this.userAssignedId,
            userId = this.userId,
            token = this.token,
            stageCode = this.stageCode,
            killedNpcs = new Dictionary<Int32, Int64>(this.killedNpcs),
            FarmedItems = new Dictionary<Int32, Int64>(this.FarmedItems)
        };
    }
}
