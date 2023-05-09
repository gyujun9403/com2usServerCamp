using DungeonFarming.DataBase.GameDb.GameUserDataORM;
using DungeonFarming.DataBase.GameDb.MasterData;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Hosting;
using MySqlConnector;
using SqlKata;
using SqlKata.Compilers;
using SqlKata.Execution;
using ZLogger;

namespace DungeonFarming.DataBase.GameDb
{
    public class MysqlGameDb : IGameDb
    {
        IMasterDataOffer _masterDataOffer;
        ILogger<MysqlGameDb> _logger;
        QueryFactory _db;
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
           클래스 내부 Util 함수들
        --------------------------*/
        String? generateItemsInsertQuery(Int64 userId, List<ItemBundle> itemBundles)
        {
            String query = "INSERT INTO user_items (user_id, item_code, item_count, attack, defence, magic, enhance_count) VALUES ";
            foreach (ItemBundle itemBundle in itemBundles)
            {
                var itemDefine = _masterDataOffer.getItemDefine(itemBundle.itemCode);
                if (itemDefine == null)
                {
                    return null;
                }
                query += "(" + userId.ToString() + ", " +
                    itemBundle.itemCode.ToString() + ", " +
                    itemBundle.itemCount.ToString() + ", " +
                    itemDefine.attack.ToString() + ", " +
                    itemDefine.defence.ToString() + ", " +
                    itemDefine.magic.ToString() + ", " +
                    "0), ";
            }
            return query.TrimEnd(new char[] { ',', ' ' });
        }
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
            }
            return new LoginLog
            {
                user_id = log.user_id,
                consecutive_login_count = renewalLastLoginCount,
                last_login_date = DateTime.Now
            };
        }
        object getMailItemDefaultObj()
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


        /*-------------------------
         유저 등록/접속 관리 메서드들
        --------------------------*/
        public async Task<ErrorCode> RegistGameUser(Int64 userId)
        {
            try
            {
                await _db.Query("login_log").InsertAsync(new { user_id = userId });
                // TODO: Logger
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
            try
            {
                LoginLog log = await _db.Query("login_log")
                    .Where("user_id", userId)
                    .Select("*")
                    .FirstOrDefaultAsync<LoginLog>();
                renewalLoginLog = GenerateLoginLog(log);
                await _db.Query("login_log")
                    .Where("user_id", userId)
                    .UpdateAsync(renewalLoginLog);
                // TODO: Logger
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


        /*-------------------------
          유저 아이템 관리 메서드들
        --------------------------*/
        public async Task<ErrorCode> InsertUserItemsByItemBundles(Int64 userId, List<ItemBundle> itemBundles)
        {
            String? query = generateItemsInsertQuery(userId, itemBundles);
            if (query == null)
            {
                return ErrorCode.GameDbError;
            }
            try
            {
                await _db.StatementAsync(query);
                return ErrorCode.None;
            }
            catch (Exception ex)
            {
                _logger.ZLogErrorWithPayload(LogEventId.GameDb, ex, new { userId = userId }, "SetUserItemsByItemBundles EXCEPTION");
                return ErrorCode.GameDbError;
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

        // 로직상 stackable한 아이템들을 update가능함이 보장된다. 
        async Task<ErrorCode> UpdateItemCount(Int64 userId, List<ItemBundle> stackableItemBundle)
        {
            try
            {
                foreach (var bundle in stackableItemBundle)
                {
                    // TODO: 맨 첫번째 쿼리에만 적용되게?
                    var rowCnt = await _db.Query("user_items")
                        .Where("user_id", userId)
                        .Where("item_code", bundle.itemCode)
                        .Limit(1)
                        .UpdateAsync(new { item_count = bundle.itemCount });
                }
                return ErrorCode.None;
            }
            catch (MySqlException ex)
            {
                _logger.ZLogErrorWithPayload(LogEventId.GameDb, ex, new { userId = userId, ItemBundle = stackableItemBundle }, "UpdateItemCount MysqlEXCEPTION");
                return ErrorCode.GameDbError;
            }
            catch (Exception ex)
            {
                _logger.ZLogErrorWithPayload(LogEventId.GameDb, ex, new { userId = userId, ItemBundle = stackableItemBundle }, "UpdateItemCount EXCEPTION");
                return ErrorCode.GameDbError;
            }
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
        public async Task<(ErrorCode, List<MailPreview>?)> GetMailPreviewList(Int64 userId, Int32 startIndex, Int32 mailCount)
        {
            IEnumerable<Mail> mails;
            try
            {
                mails = await _db.Query("mailbox")
                    .Select("*")
                    .Where("user_id", userId)
                    .Where("is_deleted", 0)
                    .Where(q =>
                        q.Where("expiration_date", ">", DateTime.Now)
                        .OrWhere("expiration_date", null)
                    )
                    .Limit(mailCount).Offset(startIndex).OrderBy("recieve_date")
                    .GetAsync<Mail>();
                    //.ForPage()?
            }
            catch (Exception ex)
            {
                _logger.ZLogErrorWithPayload(LogEventId.GameDb, ex, new { userId = userId, startIndex = startIndex, mailCount = mailCount }, "GetMailPreviewList EXCEPTION");
                return (ErrorCode.GameDbError, null);
            }
            if (mails != null)
            {
                List<MailPreview> previews = new List<MailPreview>();
                foreach (Mail mail in mails)
                {
                    previews.Add(new MailPreview {
                        mail_id = mail.mail_id,
                        item0_code = mail.item0_code,
                        item0_count = mail.item0_count,
                        mail_title = mail.mail_title,
                        expiration_date = mail.expiration_date
                    });
                }
                return (ErrorCode.None, previews);
            }
            _logger.ZLogErrorWithPayload(LogEventId.GameDb, new { userId = userId, startIndex = startIndex, mailCount = mailCount }, "GetMailPreviewList mails null");
            return (ErrorCode.None, null);

        }
        public async Task<(ErrorCode, Mail?)> GetMail(Int64 userId, Int64 mailId)
        {
            try
            {
                Mail? mail = await _db.Query("mailbox")
                    .Select("*")
                    .Where("mail_id", mailId)
                    .Where("user_id", userId)
                    .Where("is_deleted", 0)
                    .Where( q => 
                        q.Where("expiration_date", ">", DateTime.Now)
                        .OrWhere("expiration_date", null)
                    )
                    .FirstOrDefaultAsync<Mail>();
                if (mail == null)
                {
                    _logger.ZLogErrorWithPayload(LogEventId.GameDb, new { userId = userId, mailId = mailId }, "GetMailPreviewList invalid mailId");
                    return (ErrorCode.InvalidMailId, null);
                }
                return (ErrorCode.None, mail);
            }
            catch (Exception ex)
            {
                _logger.ZLogErrorWithPayload(LogEventId.GameDb, ex, new { userId = userId, mailId = mailId }, "GetMail EXCEPTION");
                return (ErrorCode.GameDbError, null);
            }
        }

        // TODO: 좀 더 보편적으로 바꾸기. item list를 받아서 바꾸게.
        async Task<(ErrorCode, List<ItemBundle>?, List<ItemBundle>?)> SortStakableBundle(Int64 userId, Mail mail)
        {
            List<ItemBundle>? stackableBundle = new List<ItemBundle>();
            List<ItemBundle>? nonStackableBundle = new List<ItemBundle>();
            try
            {
                if (mail.item0_code != -1 || mail.item0_count != -1)
                { 
                    if( _masterDataOffer.getItemDefine(mail.item0_code).max_stack > 1)
                    {
                        Int32? stackedItemCount = await _db.Query("user_items")
                            .Select("item_count")
                            .Where("user_id", userId)
                            .Where("item_code", mail.item0_code)
                            .FirstOrDefaultAsync<Int32?>();
                        if (stackedItemCount != null)
                        { 
                            if (stackedItemCount + mail.item0_count > _masterDataOffer.getItemDefine(mail.item0_code).max_stack)
                            {
                                _logger.ZLogErrorWithPayload(LogEventId.GameDb, new { userId = userId, mailId = mail.mail_id }, "RecvMailItems item0 stack overflow");
                                return (ErrorCode.ItemCountExceeded, null, null);
                            }
                            stackableBundle.Add(new ItemBundle { itemCode = mail.item0_code, itemCount = stackedItemCount.Value + mail.item0_count });
                        }
                        else
                        {
                            nonStackableBundle.Add(new ItemBundle { itemCode = mail.item0_code, itemCount = mail.item0_count });
                        }
                    }
                    else
                    {
                        nonStackableBundle.Add(new ItemBundle { itemCode = mail.item0_code, itemCount = mail.item0_count });
                    }
                }
                if (mail.item1_code != -1 || mail.item1_count != -1)
                {
                    if (_masterDataOffer.getItemDefine(mail.item1_code).max_stack > 1)
                    {
                        Int32? stackedItemCount = await _db.Query("user_items")
                            .Select("item_count")
                            .Where("user_id", userId)
                            .Where("item_code", mail.item1_code)
                            .FirstOrDefaultAsync<Int32?>();
                        if (stackedItemCount != null)
                        {
                            if (stackedItemCount + mail.item1_count > _masterDataOffer.getItemDefine(mail.item1_code).max_stack)
                            {
                                _logger.ZLogErrorWithPayload(LogEventId.GameDb, new { userId = userId, mailId = mail.mail_id }, "RecvMailItems item1 stack overflow");
                                return (ErrorCode.ItemCountExceeded, null, null);
                            }
                            stackableBundle.Add(new ItemBundle { itemCode = mail.item1_code, itemCount = stackedItemCount.Value + mail.item1_count });
                        }
                        else
                        {
                            nonStackableBundle.Add(new ItemBundle { itemCode = mail.item1_code, itemCount = mail.item1_count });
                        }
                    }
                    else
                    {
                        nonStackableBundle.Add(new ItemBundle { itemCode = mail.item1_code, itemCount = mail.item1_count });
                    }
                }
                if (mail.item2_code != -1 || mail.item2_count != -1)
                {
                    if (_masterDataOffer.getItemDefine(mail.item2_code).max_stack > 1)
                    {
                        Int32? stackedItemCount = await _db.Query("user_items")
                            .Select("item_count")
                            .Where("user_id", userId)
                            .Where("item_code", mail.item2_code)
                            .FirstOrDefaultAsync<Int32?>();
                        if (stackedItemCount != null)
                        {
                            if (stackedItemCount + mail.item2_count > _masterDataOffer.getItemDefine(mail.item2_code).max_stack)
                            {
                                _logger.ZLogErrorWithPayload(LogEventId.GameDb, new { userId = userId, mailId = mail.mail_id }, "RecvMailItems item2 stack overflow");
                                return (ErrorCode.ItemCountExceeded, null, null);
                            }
                            stackableBundle.Add(new ItemBundle { itemCode = mail.item2_code, itemCount = stackedItemCount.Value + mail.item2_count });
                        }
                        else
                        {
                            nonStackableBundle.Add(new ItemBundle { itemCode = mail.item2_code, itemCount = mail.item2_count });
                        }
                    }
                    else
                    {
                        nonStackableBundle.Add(new ItemBundle { itemCode = mail.item2_code, itemCount = mail.item2_count });
                    }
                }
                if (mail.item3_code != -1 || mail.item3_count != -1)
                {
                    if (_masterDataOffer.getItemDefine(mail.item3_code).max_stack > 1)
                    {
                        Int32? stackedItemCount = await _db.Query("user_items")
                            .Select("item_count")
                            .Where("user_id", userId)
                            .Where("item_code", mail.item3_code)
                            .FirstOrDefaultAsync<Int32?>();
                        if (stackedItemCount != null)
                        {
                            if (stackedItemCount + mail.item3_count > _masterDataOffer.getItemDefine(mail.item3_code).max_stack)
                            {
                                _logger.ZLogErrorWithPayload(LogEventId.GameDb, new { userId = userId, mailId = mail.mail_id }, "RecvMailItems item3 stack overflow");
                                return (ErrorCode.ItemCountExceeded, null, null);
                            }
                            stackableBundle.Add(new ItemBundle { itemCode = mail.item3_code, itemCount = stackedItemCount.Value + mail.item3_count });
                        }
                        else
                        {
                            nonStackableBundle.Add(new ItemBundle { itemCode = mail.item3_code, itemCount = mail.item3_count });
                        }
                    }
                    else
                    {
                        nonStackableBundle.Add(new ItemBundle { itemCode = mail.item3_code, itemCount = mail.item3_count });
                    }
                }
                if (stackableBundle.Count == 0)
                {
                    stackableBundle = null;
                }
                if (nonStackableBundle.Count == 0)
                {
                    nonStackableBundle = null;
                }
                return (ErrorCode.None, stackableBundle, nonStackableBundle);
            }
            catch (MySqlException ex)
            {
                _logger.ZLogErrorWithPayload(LogEventId.GameDb, ex, new { itemId = userId, mail = mail }, "CheckMailItemUpdatable MysqlEXCEPTION");
                return (ErrorCode.GameDbError, null, null);
            }
            catch (Exception ex)
            {
                _logger.ZLogErrorWithPayload(LogEventId.GameDb, ex, new { itemId = userId, mail = mail }, "CheckMailItemUpdatable EXCEPTION");
                return (ErrorCode.GameDbError, null, null);
            }
        }

        // TODO: 코드 개선 필요...
        public async Task<ErrorCode> RecvMailItems(Int64 userId, Int64 mailId)
        {
            Mail? mail;
            try
            {
                // 메일을 읽어온다.
                mail = await _db.Query("mailbox")
                    .Select("*")
                    .Where("mail_id", mailId)
                    .Where("user_id", userId)
                    .Where("is_deleted", 0)
                    .Where(q =>
                        q.Where("expiration_date", ">", DateTime.Now)
                        .OrWhere("expiration_date", null))
                    .FirstOrDefaultAsync<Mail>();
                if (mail == null)
                {
                    _logger.ZLogWarningWithPayload(LogEventId.GameDb, new { userId = userId, mailId = mailId }, "RecvMailItems invalid mailId");
                    return ErrorCode.InvalidMailId;
                }

                // Insert할 번들과 Update할 번들을 나누며, 유저가 우편을 받을 수 있는지 확인한다.
                var (rtErrorCode, stackableBundle, nonStackableBundle ) = await SortStakableBundle(userId, mail);
                if (rtErrorCode == ErrorCode.ItemCountExceeded) // 초가되는 품목이 있다면 우편을 냅둔다.
                {
                    _logger.ZLogErrorWithPayload(LogEventId.GameDb, new { userId = userId, mail = mail }, "RecvMailItems item stack overflow");
                    return ErrorCode.ItemCountExceeded;
                }
                if (stackableBundle == null && nonStackableBundle == null) // 아무 아이템도 없는 경우.
                {
                    return ErrorCode.Noitems;
                }
                if (rtErrorCode != ErrorCode.None) // 초과 이외 DB쿼리 문제.
                {
                    _logger.ZLogErrorWithPayload(LogEventId.GameDb, new { userId = userId, mail = mail }, "RecvMailItems EXCEPTION");
                    return rtErrorCode;
                }

                // 메일을 받을 수 있다면 우선 유저 메일의 아이템을 싹 지운다.
                await _db.Query("mailbox")
                    .Where("mail_id", mailId)
                    .Where("user_id", userId)
                    .Where("is_deleted", 0)
                    .Where(q =>
                        q.Where("expiration_date", ">", DateTime.Now)
                        .OrWhere("expiration_date", null))
                    .UpdateAsync( getMailItemDefaultObj() );

                // 이후, 누적 불가한 아이템은 Insert, 누적 가능한 아이템은 Update한다.
                if (nonStackableBundle != null)
                {
                    rtErrorCode = await InsertUserItemsByItemBundles(userId, nonStackableBundle);
                    if (rtErrorCode != ErrorCode.None)
                    {
                        _logger.ZLogErrorWithPayload(LogEventId.GameDb, new { userId = userId, mail = mail }, "RecvMailItems insert nonStackableBundle EXCEPTION");
                        return rtErrorCode;
                    }
                }
                if (stackableBundle != null)
                {
                    rtErrorCode = await UpdateItemCount(userId, stackableBundle);
                    if (rtErrorCode != ErrorCode.None)
                    {
                        _logger.ZLogErrorWithPayload(LogEventId.GameDb, new { userId = userId, mail = mail }, "RecvMailItems update stackableBundle EXCEPTION");
                        return rtErrorCode;
                    }
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
                    .Where(q =>
                        q.Where("expiration_date", ">", DateTime.Now)
                        .OrWhere("expiration_date", null)
                    )
                    .UpdateAsync(new { is_deleted = 1 });
                return ErrorCode.None;
            }
            catch (Exception ex)
            {
                _logger.ZLogErrorWithPayload(LogEventId.GameDb, ex, new { userId = userId, mailId = mailId }, "DeleteMail EXCEPTION");
                return ErrorCode.GameDbError;
            }
        }
    }
}
