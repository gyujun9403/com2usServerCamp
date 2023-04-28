namespace DungeonFarming.DataBase.GameSessionDb
{
    public interface IGameSessionDb
    {
        // token functions
        Task<ErrorCode> setToken(AuthCheckModel mokdel);
        Task<(ErrorCode, String?)> getToken(String userId);
        Task<ErrorCode> deleteToken(String userId);

        // GameSession : 유저별 게임의 진행상태 / 직전 상태 에 대한 정보
    }

}
