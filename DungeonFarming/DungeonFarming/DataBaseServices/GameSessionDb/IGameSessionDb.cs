namespace DungeonFarming.DataBase.GameSessionDb;

public interface IGameSessionDb
{
    Task<ErrorCode> SetUserInfoSession(GameSessionData mokdel);
    Task<(ErrorCode, GameSessionData?)> GetUserInfoSession(String userAssignedId);
    Task<ErrorCode> DeleteUserInfoSession(String userAssignedId);
    Task<(ErrorCode, string?)> GetNotice();
}
