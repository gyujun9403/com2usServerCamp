using DungeonFarming.DataBase.GameDb.GameDbModel;
using DungeonFarming.DataBase.GameDb.MasterDataModel;

namespace DungeonFarming.DataBase.GameDb
{
    public interface IGameDb
    {
        // User Login
        Task<ErrorCode> RegisUserLoginLog(Int64 userId);
        Task<(ErrorCode, LoginLog?)> UpdateAndGetLoginLog(Int64 userId);
        Task<(ErrorCode, LoginLog?)> GetLoginLog(Int64 userId);
        Task<ErrorCode> DeleteLoginLog(Int64 userId);
        // User Items
        Task<(ErrorCode, List<Int64>? insertedKeys)> InsertUserItemsByItemBundles(Int64 userId, List<ItemBundle> itemBundleList);
        Task<(ErrorCode, List<UserItem>?)> GetUserItemList(Int64 userId);
        Task<(ErrorCode, UserItem?)> GetUserItem(Int64 userId, Int64 itemId);
        Task<ErrorCode> UpdateUserItem(UserItem newItem);
        Task<ErrorCode> DeleteUserItem(Int64 userId, Int64 itemId);
        Task<ErrorCode> GiveUserItems(Int64 userId, List<ItemBundle> itemBundleList);
        // Mail
        Task<ErrorCode> SendMail(Mail mail);
        Task<(ErrorCode, List<MailListElem>?)> GetMailListUp(Int64 userId, Int32 startIndex, Int32 mailCount);
        Task<(ErrorCode, Mail?)> OpenMail(Int64 userId, Int64 mailId);
        Task<(ErrorCode, List<ItemBundle>? attachedItemList)> RecvMailItems(Int64 userId, Int64 mailId);
        Task<ErrorCode> DeleteMail(Int64 userId, Int64 mailId);
        // DungeonInfo
        Task<ErrorCode> RegistUserAchivement(Int64 userId);
        Task<(ErrorCode, UserAchievement)> GetUserAchivement(Int64 userId);
        Task<ErrorCode> UpdateUserAchivement(UserAchievement userAchievement);


    }
}
