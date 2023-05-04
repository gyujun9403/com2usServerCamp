using DungeonFarming.DataBase.GameDb.GameUserDataORM;
using DungeonFarming.DataBase.GameDb.MasterData;

namespace DungeonFarming.DataBase.GameDb
{
    public interface IGameDb
    {
        // 유저를 등록 -> registe시
        Task<ErrorCode> RegistUserInGame(Int64 userId);
        // 유저 접속시 연접/미접 일수 갱신 -> 로그인시
        Task<(ErrorCode, LoginLog?)> UpdateAndGetLoginLog(Int64 userId);
        Task<(ErrorCode, LoginLog?)> GetLoginLog(Int64 userId);
        Task<(ErrorCode, List<ItemBundle>?)> GetDefaultItemBundle(Int16 listId);
        Task<ErrorCode> SetUserItemsByItemBundles(Int64 userId, List<ItemBundle> itemBundles);
        Task<ErrorCode> SendMail(Mail mail);
        Task<(ErrorCode, List<UserItem>?)> GetUserItemList(Int64 userId);

        Task<(ErrorCode, List<MailPreview>?)> GetMailPreviewList(Int64 userId, Int32 startIndex, Int32 mailCount);
        Task<(ErrorCode, Mail?)> GetMail(Int64 userId, Int64 mailId);

        Task<ErrorCode> RecvMailItem(Int64 userId, Int64 mailId);

        Task<ErrorCode> DeleteMail(Int64 userId, Int64 mailId);
    }
}
