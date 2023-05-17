using DungeonFarming.DataBase.GameDb.GameUserDataORM;
using DungeonFarming.DataBaseServices.GameDb.GameUserDataDTO;
using MySqlConnector;
using SqlKata;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Reflection.PortableExecutable;
using System.Xml;
using ZLogger;

namespace DungeonFarming.DataBase.GameDb;

public class MysqlGameDb : IGameDb
{
    readonly IMasterDataOffer _masterDataOffer;
    readonly ILogger<MysqlGameDb> _logger;
    readonly QueryFactory _db;
    public MysqlGameDb(IConfiguration config, IMasterDataOffer masterDataOffer, ILogger<MysqlGameDb> logger)
    {
        var connString = config.GetConnectionString("Mysql_Game");
        var connection = new MySqlConnection(connString);
        var compiler = new MySqlCompiler();
        _db = new QueryFactory(connection, compiler);
        _logger = logger;
        _masterDataOffer = masterDataOffer;
    }

    /*-------------------------
     유저 등록/접속 관리 메서드들
    --------------------------*/
    public async Task<ErrorCode> RegisUserLoginLog(Int64 userId)
    {
        try
        {
            await _db.Query("login_log").InsertAsync(new { user_id = userId });
            return ErrorCode.None;
        }
        catch (MySqlException ex)
        {
            if (ex.Number == 1062) //duplicated id exception
            {
                _logger.ZLogErrorWithPayload(LogEventId.GameDb, ex, new { userId = userId }, "RegistGameUser duplicated id EXCEPTION");
                return ErrorCode.DuplicatedId;
            }
            _logger.ZLogErrorWithPayload(LogEventId.GameDb, ex, new { userId = userId }, "RegistGameUser MYSQL_EXCEPTION");
            return ErrorCode.GameDbError;
        }
        catch (Exception ex)
        {
            _logger.ZLogErrorWithPayload(LogEventId.GameDb, ex, new { userId = userId }, "RegistGameUser EXCEPTION");
            return ErrorCode.GameDbError;
        }
    }
    public async Task<(ErrorCode, LoginLog?)> UpdateAndGetLoginLog(Int64 userId)
    {
        LoginLog? renewalLoginLog = null;
        Query loginLogQuery = _db.Query("login_log").Where("user_id", userId);
        try
        {
            LoginLog? log = await loginLogQuery.Select("*").FirstOrDefaultAsync<LoginLog?>();
            if (log == null)
            {
                return (ErrorCode.InvalidUserData, null);
            }
            renewalLoginLog = GenerateLoginLog(log);
            await loginLogQuery.UpdateAsync(renewalLoginLog);
            if (log.consecutive_login_count == renewalLoginLog.consecutive_login_count)
            {
                return (ErrorCode.AreadyLogin, renewalLoginLog);
            }
            return (ErrorCode.None, renewalLoginLog);
        }
        catch (Exception ex)
        {
            _logger.ZLogErrorWithPayload(LogEventId.GameDb, ex, new { userId = userId }, "RegistGameUser EXCEPTION");
            return (ErrorCode.GameDbError, null);
        }
    }
    public async Task<(ErrorCode, LoginLog?)> GetLoginLog(Int64 userId)
    {
        try
        {
            LoginLog? log = await _db.Query("login_log")
                .Where("user_id", userId)
                .Select("*")
                .FirstOrDefaultAsync<LoginLog>();
            if (log == null)
            {
                return (ErrorCode.GameDbError, null);
            }
            return (ErrorCode.None, log);
        }
        catch (Exception ex)
        {
            _logger.ZLogErrorWithPayload(LogEventId.GameDb, ex, new { userId = userId }, "RegistGameUser EXCEPTION");
            return (ErrorCode.GameDbError, null);
        }
    }
    public async Task<ErrorCode> DeleteLoginLog(Int64 userId)
    {
        try
        {
            await _db.Query("login_log").Where("user_id", userId).DeleteAsync();
            return ErrorCode.None;
        }
        catch (MySqlException ex)
        {
            _logger.ZLogErrorWithPayload(LogEventId.GameDb, ex, new { userId = userId }, "DeleteLoginLog MYSQL_EXCEPTION");
            return ErrorCode.GameDbError;
        }
        catch (Exception ex)
        {
            _logger.ZLogErrorWithPayload(LogEventId.GameDb, ex, new { userId = userId }, "DeleteLoginLog EXCEPTION");
            return ErrorCode.GameDbError;
        }
    }


    /*-------------------------
      유저 아이템 관리 메서드들
    --------------------------*/
    public async Task<(ErrorCode, List<Int64>? insertedKeys)> InsertUserItemsByItemBundles(Int64 userId, List<ItemBundle> itemBundleList)
    {
        List<Int64> insertedItemId = new List<Int64>();
        try
        {
            foreach (var bundle in itemBundleList)
            {
                var itemDefine = _masterDataOffer.getItemDefine(bundle.itemCode);
                if (itemDefine == null)
                {
                    throw new Exception("Item code not defined");
                }
                var key = await _db.Query("user_items")
                    .InsertGetIdAsync<Int64>(new
                    {
                        user_id = userId,
                        item_code = bundle.itemCode,
                        item_count = bundle.itemCount,
                        attack = itemDefine.attack,
                        defence = itemDefine.defence,
                        magic = itemDefine.magic,
                        enhance_count = 0
                    });

                if (key < 0)
                {
                    throw new Exception("Invalid key return");
                }
                insertedItemId.Add(key);
            }
            return (ErrorCode.None, insertedItemId);
        }
        catch (Exception ex)
        {
            foreach (var itemId in insertedItemId)
            {
                await _db.Query("user_Items").Where("item_id", itemId).DeleteAsync();
            }
            _logger.ZLogErrorWithPayload(LogEventId.GameDb, ex, new { userId = userId }, "SetUserItemsByItemBundles EXCEPTION");
            return (ErrorCode.GameDbError, null);
        }
    }
    public async Task<(ErrorCode, List<UserItem>?)> GetUserItemList(Int64 userId)
    {
        try
        {
            List<UserItem> userItems = new List<UserItem>();
            IEnumerable<UserItem> itemEnumList = await _db.Query("user_items")
                .Select("*").Where("user_id", userId).GetAsync<UserItem>();
            foreach (UserItem item in itemEnumList)
            {
                userItems.Add(item);
            }
            return (ErrorCode.None, userItems);
        }
        catch (Exception ex)
        {
            _logger.ZLogErrorWithPayload(LogEventId.GameDb, ex, new { userId = userId }, "GetUserItemList EXCEPTION");
            return (ErrorCode.GameDbError, null);
        }
    }
    public async Task<(ErrorCode, UserItem?)> GetUserItem(Int64 userId, Int64 itemId)
    {
        try
        {
            UserItem? userItems = await _db.Query("user_items")
                .Select("*")
                .Where("user_id", userId)
                .Where("item_id", itemId)
                .FirstOrDefaultAsync<UserItem?>();
            if (userItems == null)
            {
                return (ErrorCode.InvalidItemId, null);
            }
            return (ErrorCode.None, userItems);
        }
        catch (Exception ex)
        {
            _logger.ZLogErrorWithPayload(LogEventId.GameDb, ex, new { userId = userId, itemId = itemId }, "GetUserItem EXCEPTION");
            return (ErrorCode.GameDbError, null);
        }
    }
    public async Task<ErrorCode> UpdateUserItem(UserItem newItem)
    {
        try
        {
            Int32 effectedRow = await _db.Query("user_items")
                .Where("user_id", newItem.user_id)
                .Where("item_id", newItem.item_id)
                .UpdateAsync(newItem);
            if (effectedRow == 0)
            {
                return ErrorCode.InvalidItemId;
            }
            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            _logger.ZLogErrorWithPayload(LogEventId.GameDb, ex, new { userId = newItem.user_id, UserItem = newItem }, "UpdateUserItem EXCEPTION");
            return ErrorCode.GameDbError;
        }
    }

    // 누적 아이템의 카운트는, 위에서 알아서 검증 할 것.
    async Task<ErrorCode> IncreaseUserItemCount(Int64 userId, List<(ItemBundle IncreaseData, UserItem backupData)> updateItemBundleWithBackup)
    {
        List<UserItem> backupUserItemList = new List<UserItem>();
        try
        {
            foreach (var elem in updateItemBundleWithBackup)
            {
                var rowCnt = await _db.Query("user_items")
                    .Where("item_id", elem.backupData.item_id)
                    .UpdateAsync(new { item_count = elem.IncreaseData.itemCount + elem.backupData.item_count });
                if (rowCnt == 0)
                {
                    throw new Exception("Increase Fail " + elem.ToString());
                }
                backupUserItemList.Add(elem.backupData);
            }
            return ErrorCode.None;
        }
        catch (MySqlException ex)
        {
            _logger.ZLogErrorWithPayload(LogEventId.GameDb, ex, new { userId = userId }, "IncreaseUserItemCount MysqlEXCEPTION");

        }
        catch (Exception ex)
        {
            _logger.ZLogErrorWithPayload(LogEventId.GameDb, ex, new { userId = userId }, "IncreaseUserItemCount EXCEPTION");
        }

        try
        {
            foreach (var backupElem in backupUserItemList)
            {
                await _db.Query("user_items")
                    .Where("item_id", backupElem.item_id)
                    .UpdateAsync(backupElem);
            }
        }
        catch (Exception ex)
        {
            _logger.ZLogErrorWithPayload(LogEventId.GameDb, ex, new { userId = userId }, "IncreaseUserItemCount BACKUP FAIL");
        }
        return ErrorCode.GameDbError;
    }


    /*-------------------------
         메일 조작 메서드들
    --------------------------*/
    public async Task<ErrorCode> SendMail(Mail mail)
    {
        try
        {
            await _db.Query("mailbox").InsertAsync(mail);
            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            _logger.ZLogErrorWithPayload(LogEventId.GameDb, ex, new { userId = mail.user_id, Mail = mail }, "SendMail EXCEPTION");
            return ErrorCode.GameDbError;
        }
    }
    public async Task<(ErrorCode, List<MailListElem>?)> GetMailListUp(Int64 userId, Int32 startIndex, Int32 mailCount)
    {
        IEnumerable<Mail> mails;
        try
        {
            mails = await _db.Query("mailbox")
                .Select("*")
                .Where("user_id", userId)
                .Where("is_deleted", 0)
                .Where("expiration_date", ">", DateTime.Now)
                .Limit(mailCount).Offset(startIndex).OrderBy("recieve_date")
                .GetAsync<Mail>();
        }
        catch (Exception ex)
        {
            _logger.ZLogErrorWithPayload(LogEventId.GameDb, ex, new { userId = userId, startIndex = startIndex, mailCount = mailCount }, "GetMailPreviewList EXCEPTION");
            return (ErrorCode.GameDbError, null);
        }
        if (mails != null)
        {
            List<MailListElem> previews = new List<MailListElem>();
            foreach (Mail mail in mails)
            {
                previews.Add(new MailListElem {
                    mail_id = mail.mail_id,
                    item0_code = mail.item0_code, item0_count = mail.item0_count,
                    item1_code = mail.item1_code, item1_count = mail.item1_count,
                    item2_code = mail.item2_code, item2_count = mail.item2_count,
                    item3_code = mail.item3_code, item3_count = mail.item3_count,
                    mail_title = mail.mail_title,
                    read_date = mail.read_date,
                    expiration_date = mail.expiration_date
                });
            }
            return (ErrorCode.None, previews);
        }
        _logger.ZLogErrorWithPayload(LogEventId.GameDb, new { userId = userId, startIndex = startIndex, mailCount = mailCount }, "GetMailPreviewList mails null");
        return (ErrorCode.None, null);

    }
    public async Task<(ErrorCode, Mail?)> OpenMail(Int64 userId, Int64 mailId)
    {
        try
        {
            Query MailQuery = _db.Query("mailbox")
                .Where("mail_id", mailId)
                .Where("user_id", userId)
                .Where("is_deleted", 0)
                .Where("expiration_date", ">", DateTime.Now);
            Mail? mail = await MailQuery.Select("*").FirstAsync<Mail>();
            if (mail == null)
            {
                _logger.ZLogErrorWithPayload(LogEventId.GameDb, new { userId = userId, mailId = mailId }, "OpenMail invalid mailId");
                return (ErrorCode.InvalidMailId, null);
            }
            if (mail.read_date == new DateTime(9999, 12, 31, 23, 59, 59))
            {
                DateTime nowTime = DateTime.Now;
                await MailQuery.UpdateAsync(new { read_date = nowTime });
                mail.read_date = nowTime;
            }
            return (ErrorCode.None, mail);
        }
        catch (Exception ex)
        {
            _logger.ZLogErrorWithPayload(LogEventId.GameDb, ex, new { userId = userId, mailId = mailId }, "OpenMail EXCEPTION");
            return (ErrorCode.GameDbError, null);
        }
    }

    public async Task<ErrorCode> DeleteUserItem(Int64 userId, Int64 itemId)
    {
        try
        {
            await _db.Query("user_items")
                .Where("user_id", userId)
                .Where("item_id", itemId)
                .DeleteAsync();
            return ErrorCode.None;
        }
        catch (MySqlException ex)
        {
            _logger.ZLogErrorWithPayload(LogEventId.GameDb, ex, new { userId = userId, itemId = itemId }, "DeleteUserItem MysqlEXCEPTION");
            return ErrorCode.GameDbError;
        }
        catch (Exception ex)
        {
            _logger.ZLogErrorWithPayload(LogEventId.GameDb, ex, new { userId = userId, itemId = itemId }, "DeleteUserItem EXCEPTION");
            return ErrorCode.GameDbError;
        }
    }

    public async Task<ErrorCode> RecvMailItems(Int64 userId, Int64 mailId)
    {
        try
        {
            Mail? mail;
            Query mailQuery = _db.Query("mailbox")
                    .Where("mail_id", mailId)
                    .Where("user_id", userId)
                    .Where("is_deleted", 0)
                    .Where("expiration_date", ">", DateTime.Now);
            mail = await mailQuery.Select("*").FirstOrDefaultAsync<Mail>();
            if (mail == null)
            {
                _logger.ZLogWarningWithPayload(LogEventId.GameDb, new { userId = userId, mailId = mailId }, "RecvMailItems invalid mailId");
                return ErrorCode.InvalidMailId;
            }
            await mailQuery.UpdateAsync( GetMailItemDefaultObj() );

            var attachedItemBundle = mail.AttachedItemBundle();
            if (attachedItemBundle == null)
            {
                await mailQuery.UpdateAsync(mail);
                return ErrorCode.Noitems;
            }

            var rtErrorCode = await GiveUserItems(userId, attachedItemBundle);
            if (rtErrorCode != ErrorCode.None)
            {
                await mailQuery.UpdateAsync(mail);
                return rtErrorCode;
            }
        }
        catch (Exception ex)
        {
            _logger.ZLogErrorWithPayload(LogEventId.GameDb, ex, new { userId = userId, mailId = mailId }, "RecvMailItems get mail EXCEPTION");
            return ErrorCode.GameDbError;
        }
        return ErrorCode.None;
    }
    public async Task<ErrorCode> DeleteMail(Int64 userId, Int64 mailId)
    {
        try
        {
            await _db.Query("mailbox")
                .Where("mail_id", mailId)
                .Where("user_id", userId)
                .Where("is_deleted", 0)
                .Where("expiration_date", ">", DateTime.Now)
                .UpdateAsync(new { is_deleted = 1 });
            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            _logger.ZLogErrorWithPayload(LogEventId.GameDb, ex, new { userId = userId, mailId = mailId }, "DeleteMail EXCEPTION");
            return ErrorCode.GameDbError;
        }
    }

    // Dungeon

    public async Task<ErrorCode> RegistUserAchivement(long userId)
    {
        try
        {
            await _db.Query("user_achievement")
                .InsertAsync(new UserAchievement {
                    user_id = userId,
                    user_level = 1,
                    user_exp = 0,
                    highest_cleared_stage_id = -1
                });
            return ErrorCode.None;
        }
        catch (MySqlException ex)
        {
            if (ex.Number == 1062) //duplicated id exception
            {
                _logger.ZLogErrorWithPayload(LogEventId.GameDb, ex, new { userId = userId }, "RegistUserAchivement duplicated id MySqlException");
                return ErrorCode.DuplicatedId;
            }
            _logger.ZLogErrorWithPayload(LogEventId.GameDb, ex, new { userId = userId }, "RegistUserAchivement MySqlException");
            return ErrorCode.GameDbError;
        }
        catch (Exception ex)
        {
            _logger.ZLogErrorWithPayload(LogEventId.GameDb, ex, new { userId = userId }, "RegistGameUser EXCEPTION");
            return ErrorCode.GameDbError;
        }
    }

    public async Task<(ErrorCode, UserAchievement?)> GetUserAchivement(Int64 userId)
    {
        try
        {
            UserAchievement userAchievement = await _db.Query("user_achievement")
                .Where("user_id", userId)
                .FirstOrDefaultAsync<UserAchievement>();
            if (userAchievement == null || userAchievement.user_id == -1)
            {
                _logger.ZLogErrorWithPayload(LogEventId.GameDb, new { userId = userId }, "GetUserAchivement invalid userId");
                return (ErrorCode.InvalidId, null);
            }
            return (ErrorCode.None, userAchievement);
        }
        catch (MySqlException ex)
        {
            _logger.ZLogErrorWithPayload(LogEventId.GameDb, ex, new { userId = userId }, "GetUserAchivement MysqlEXCEPTION");
            return (ErrorCode.GameDbError, null);
        }
        catch (Exception ex)
        {
            _logger.ZLogErrorWithPayload(LogEventId.GameDb, ex, new { userId = userId }, "GetUserAchivement EXCEPTION");
            return (ErrorCode.GameDbError, null);
        }
    }
    public async Task<ErrorCode> UpdateUserAchivement(UserAchievement userAchievement)
    {
        try
        {
            var raw = await _db.Query("user_achievement")
                .Where("user_id", userAchievement.user_id)
                .Limit(1)
                .UpdateAsync(userAchievement);
            if (raw < 1)
            {
                _logger.ZLogErrorWithPayload(LogEventId.GameDb, new { achivement = userAchievement }, "UpdateUserAchivement invalid userId");
                return ErrorCode.InvalidId;
            }
            return ErrorCode.None;
        }
        catch (MySqlException ex)
        {
            _logger.ZLogErrorWithPayload(LogEventId.GameDb, ex, new { achivement = userAchievement }, "UpdateUserAchivement MysqlEXCEPTION");
            return ErrorCode.GameDbError;
        }
        catch (Exception ex)
        {
            _logger.ZLogErrorWithPayload(LogEventId.GameDb, ex, new { achivement = userAchievement }, "UpdateUserAchivement EXCEPTION");
            return ErrorCode.GameDbError;
        }
    }

    public async Task<ErrorCode> GiveUserItems(Int64 userId, List<ItemBundle> itemBundleList)
    {
        var (stackableBundle, unstackableBundle) = SortStackableUnstackableItemBundleList(itemBundleList);
        if (stackableBundle == null && unstackableBundle == null)
        {
            return ErrorCode.InvalidItemId;
        }

        List<Int64>? insertedKeyList = null;
        var (rtErrorCode, insertBundle, increaseTuple) = await SetInsertUpdateList(userId, stackableBundle, unstackableBundle);
        if (rtErrorCode != ErrorCode.None)
        {
            return rtErrorCode;
        }

        if (insertBundle != null)
        {
            (rtErrorCode, insertedKeyList) = await InsertUserItemsByItemBundles(userId, insertBundle);
            if (rtErrorCode != ErrorCode.None)
            {
                _logger.ZLogErrorWithPayload(LogEventId.GameDb, new { userId = userId, itemBundleList = itemBundleList }, "GiveUserItems insert nonStackableBundle EXCEPTION");
                return rtErrorCode;
            }
        }

        if (increaseTuple != null)
        {
            rtErrorCode = await IncreaseUserItemCount(userId, increaseTuple);
            if (rtErrorCode != ErrorCode.None)
            {
                if (insertedKeyList != null)
                {
                    foreach (var key in insertedKeyList)
                    {
                        await DeleteUserItem(userId, key);
                    }
                }
                _logger.ZLogErrorWithPayload(LogEventId.GameDb, new { userId = userId, itemBundleList = itemBundleList }, "GiveUserItems update stackableBundle EXCEPTION");
                return rtErrorCode;
            }
        }
        return ErrorCode.None;
    }

    /*-------------------------
        클래스 내부 Util 함수들
    --------------------------*/
    LoginLog GenerateLoginLog(LoginLog log)
    {
        Int16 renewalLastLoginCount = log.consecutive_login_count;
        // 어제 새벽 6시 이전이 마지막 로그인인 경우(출석 스택 초기화)
        if (log.last_login_date < DateTime.Today.AddDays(-1).AddHours(6))
        {
            renewalLastLoginCount = 1;
        }
        else if (log.last_login_date < DateTime.Today.AddHours(6))
        {
            renewalLastLoginCount += 1;
            if (renewalLastLoginCount > 30)
            {
                renewalLastLoginCount = 1;
            }
        }
        return new LoginLog
        {
            user_id = log.user_id,
            consecutive_login_count = renewalLastLoginCount,
            last_login_date = DateTime.Now
        };
    }
    object GetMailItemDefaultObj()
    {
        return new
        {
            item0_code = -1,
            item0_count = -1,
            item1_code = -1,
            item1_count = -1,
            item2_code = -1,
            item2_count = -1,
            item3_code = -1,
            item3_count = -1,
        };
    }
    (List<ItemBundle>? stackable, List<ItemBundle>? unstackable) SortStackableUnstackableItemBundleList(List<ItemBundle> raw)
    {
        var stackable = new List<ItemBundle>();
        var unstackable = new List<ItemBundle>();

        foreach (var bundle in raw)
        {
            var itemDefine = _masterDataOffer.getItemDefine(bundle.itemCode);
            if (itemDefine == null)
            {
                return (null, null);
            }
            else if (itemDefine.max_stack > 1)
            {
                stackable.Add(bundle);
            }
            else
            {
                unstackable.Add(bundle);
            }
        }
        if (stackable.Count == 0)
        {
            stackable = null;
        }
        if (unstackable.Count == 0)
        {
            unstackable = null;
        }
        return (stackable, unstackable);
    }

    //stackable한 아이템들의 userItem을 받아옴
    async Task<(ErrorCode, UserItem?)> ReadItemBundleFromUserItems(Int64 userId, ItemBundle itemBundle)
    {
        try
        {
            List<UserItem> rtList = new List<UserItem>();
            UserItem userItem = await _db.Query("user_items")
                .Select("*")
                .Where("user_id", userId)
                .Where("item_code", itemBundle.itemCode)
                .FirstOrDefaultAsync<UserItem>();
            if (userItem.item_id == -1)
            {
                return (ErrorCode.None, null);
            }
            else
            {
                return (ErrorCode.None, userItem);
            }
        }
        catch (MySqlException ex)
        {
            _logger.ZLogErrorWithPayload(LogEventId.GameDb, ex, new { userId = userId }, "RegistGameUser MYSQL_EXCEPTION");
            return (ErrorCode.GameDbError, null);
        }
        catch (Exception ex)
        {
            _logger.ZLogErrorWithPayload(LogEventId.GameDb, ex, new { userId = userId }, "RegistGameUser EXCEPTION");
            return (ErrorCode.GameDbError, null);
        }
    }

    async Task<(ErrorCode, List<ItemBundle>? insert, List<(ItemBundle bundle, UserItem backup)>? increaseTuple)> SetInsertUpdateList(Int64 userId, List<ItemBundle>? stackableItemBundle, List<ItemBundle>? unstackableItemBundle)
    {
        List<ItemBundle>? insertList = unstackableItemBundle;
        List<(ItemBundle bundle, UserItem backup)>? updateList = new List<(ItemBundle bundle, UserItem backup)>();

        if (stackableItemBundle == null)
        {
            return (ErrorCode.None, insertList, null);
        }

        foreach (var bundle in stackableItemBundle)
        {
            var (rtErrorCode, userItem) = await ReadItemBundleFromUserItems(userId, bundle);
            if (rtErrorCode != ErrorCode.None || (rtErrorCode == ErrorCode.None && userItem == null))
            {

            }
            else if (userItem == null)
            {
                insertList.Add(bundle);
            }
            else
            {
                var itemDefine = _masterDataOffer.getItemDefine(bundle.itemCode);
                if (itemDefine == null)
                {
                    return (ErrorCode.InvalidItemId, null, null);
                }
                else if (bundle.itemCount + userItem.item_count > itemDefine.max_stack)
                {
                    return (ErrorCode.ItemCountExceeded, null, null);
                }
                else
                {
                    updateList.Add((bundle, userItem));
                }
            }
        }
        if (updateList.Count == 0)
        {
            return (ErrorCode.None, insertList, null);
        }
        return (ErrorCode.None, insertList, updateList);
    }
}
