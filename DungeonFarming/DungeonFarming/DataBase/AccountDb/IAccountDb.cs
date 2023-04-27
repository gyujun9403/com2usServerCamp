namespace DungeonFarming.DataBase.AccountDb
{
    public interface IAccountDb
    {
        // 유저 등록
        Task<ErrorCode> RegisteUser(AccountDbModel model);
        // 유저 정보 찾아오기
        Task<(ErrorCode, AccountDbModel?)> GetAccountInfo(String accountId);
    }
}
