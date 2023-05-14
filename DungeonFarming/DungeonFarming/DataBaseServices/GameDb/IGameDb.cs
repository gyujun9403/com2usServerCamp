using DungeonFarming.DataBase.GameDb.GameUserDataORM;
using DungeonFarming.DataBase.GameDb.MasterData;
using DungeonFarming.DataBaseServices.GameDb.GameUserDataDTO;

namespace DungeonFarming.DataBase.GameDb
{
    public interface IGameDb
    {
        // regist and Login (Auth)
        Task<ErrorCode> RegisUserLoginLog(Int64 userId);
        Task<(ErrorCode, LoginLog?)> UpdateAndGetLoginLog(Int64 userId);
        Task<(ErrorCode, LoginLog?)> GetLoginLog(Int64 userId);
        Task<ErrorCode> DeleteLoginLog(Int64 userId);
        // UserItem
        Task<ErrorCode> InsertUserItemsByItemBundles(Int64 userId, List<ItemBundle> itemBundles);
        Task<(ErrorCode, List<UserItem>?)> GetUserItemList(Int64 userId);
        Task<(ErrorCode, UserItem?)> GetUserItem(Int64 userId, Int64 itemId);
        Task<ErrorCode> UpdateUserItem(UserItem newItem);
        Task<ErrorCode> DeleteUserItem(Int64 userId, Int64 itemId);
        // Mail
        Task<ErrorCode> SendMail(Mail mail);
        Task<(ErrorCode, List<MailPreview>?)> GetMailPreviewList(Int64 userId, Int32 startIndex, Int32 mailCount);
        Task<(ErrorCode, Mail?)> GetMail(Int64 userId, Int64 mailId);
        Task<ErrorCode> RecvMailItems(Int64 userId, Int64 mailId);
        Task<ErrorCode> DeleteMail(Int64 userId, Int64 mailId);
        // DungeonInfo
        Task<ErrorCode> RegistUserAchivement(Int64 userId);
        Task<(ErrorCode, UserAchievement)> GetUserAchivement(Int64 userId);
        Task<ErrorCode> UpdateUserAchivement(UserAchievement userAchievement);
    }
}
