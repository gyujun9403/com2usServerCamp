﻿namespace DungeonFarming.DataBase.AccountDb
{
    public interface IAccountDb
    {
        // 유저 등록
        Task<(ErrorCode, Int16)> RegisteUser(UserAccountsTuple model);
        // 유저 정보 찾아오기
        Task<(ErrorCode, UserAccountsTuple?)> GetAccountInfo(String userId);

        Task<ErrorCode> DeleteAccount(String userId);
    }
}
