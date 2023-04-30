namespace DungeonFarming.DataBase.GameSessionDb
{
    public interface IGameSessionDb
    {
        // token functions
        Task<ErrorCode> SetUserInfoSession(GameSessionData mokdel);
        Task<(ErrorCode, GameSessionData?)> GetUserInfoSession(String userId);
        Task<ErrorCode> DeleteUserInfoSession(String userId);
        Task<String> GetNotice();

        // GameSession : 유저별 게임의 진행상태 / 직전 상태 에 대한 정보
    }

}
