using DungeonFarming.DataBase.AccountDb;
using DungeonFarming.DataBase.GameDb.GameUserDataORM;
using DungeonFarming.DataBase.GameDb.MasterData;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Hosting;
using MySqlConnector;
using SqlKata;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        private ErrorCode MysqlExceptionHandle(String configString, MySqlException ex)
        {
            if (ex.Number == 1062) //duplicated id exception
            {
                _logger.ZLogError($"[GetAccountInfo] Error : {configString} duplicate key exception");
                return ErrorCode.DuplicatedId;
            }
            // 다른 예외처리 들을 추가.
            _logger.ZLogError($"[GetAccountInfo] Error : {configString} duplicate key exception");
            return ErrorCode.AccountDbError;
        }
        public List<ItemBundle>? GetItemsFromDefaultItem(DefaultItems defaultItems)
        {
            List<ItemBundle> rt = new List<ItemBundle>();
            if (defaultItems.item0_code != -1 && defaultItems.item0_count != -1)
            {
                rt.Add(new ItemBundle {
                    itemCode = defaultItems.item0_code,
                    itemCount = defaultItems.item0_count
                });
            }
            if (defaultItems.item1_code != -1 && defaultItems.item1_count != -1)
            {
                rt.Add(new ItemBundle
                {
                    itemCode = defaultItems.item1_code,
                    itemCount = defaultItems.item1_count
                });
            }
            if (defaultItems.item2_code != -1 && defaultItems.item2_count != -1)
            {
                rt.Add(new ItemBundle
                {
                    itemCode = defaultItems.item2_code,
                    itemCount = defaultItems.item2_count
                });
            }
            if (defaultItems.item3_code != -1 && defaultItems.item3_count != -1)
            {
                rt.Add(new ItemBundle
                {
                    itemCode = defaultItems.item3_code,
                    itemCount = defaultItems.item3_count
                });
            }
            if (rt.Count == 0)
            {
                return null;
            }
            return rt;
        }

        public async Task<(ErrorCode, List<ItemBundle>?)> GetDefaultItemBundle(Int16 listId)
        {
            try
            {
                DefaultItems? rt = await _db.Query("mt_default_items_list")
                    .Select("*").Where("list_id", listId)
                    .FirstOrDefaultAsync<DefaultItems>();
                if (rt == null)
                {
                    return (ErrorCode.GameDbError, null);
                }
                List<ItemBundle>? itemBundles = GetItemsFromDefaultItem(rt);
                if (itemBundles == null)
                {
                    return (ErrorCode.GameDbError, null);
                }
                // TODO: Logger
                return (ErrorCode.None, itemBundles);
            }
            catch (MySqlException ex)
            {
                return (MysqlExceptionHandle(listId.ToString(), ex), null);
            }
        }

        public async Task<ErrorCode> RegistUserInGame(Int64 userId)
        {
            try
            {
                await _db.Query("game_user").InsertAsync(new { user_id = userId });
                // TODO: Logger
                return ErrorCode.None;
            }
            catch (MySqlException ex)
            {
                return MysqlExceptionHandle(userId.ToString(), ex);
            }
        }

        public String? generateItemsInsertQuery(Int64 userId, List<ItemBundle> itemBundles)
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

        public async Task<ErrorCode> SetUserItemsByItemBundles(Int64 userId, List<ItemBundle> itemBundles)
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
            catch (MySqlException ex)
            {
                return MysqlExceptionHandle(userId.ToString(), ex);
            }
        }

        private LoginLog GenerateLoginLog(LoginLog log)
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

        // 로그인 갱신시에만 LoginLog를 던짐.
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
            catch (MySqlException ex)
            {
                return (MysqlExceptionHandle(userId.ToString(), ex), renewalLoginLog);
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
            catch (MySqlException ex)
            {
                return (MysqlExceptionHandle(userId.ToString(), ex), null);
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
            catch (MySqlException ex)
            {
                return (MysqlExceptionHandle(userId.ToString(), ex), null);
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
            catch (MySqlException ex)
            {
                return (MysqlExceptionHandle(userId.ToString(), ex), null);
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
            catch (MySqlException ex)
            {
                return MysqlExceptionHandle(newItem.item_id.ToString(), ex);
            }
        }
        public async Task<ErrorCode> SendMail(Mail mail)
        {

            try
            {
                await _db.Query("mailbox").InsertAsync(mail);
                return ErrorCode.None;
            }
            catch (MySqlException ex)
            {
                return MysqlExceptionHandle(mail.user_id.ToString(), ex);
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
                    //.ForPage()
                    .Limit(mailCount).Offset(startIndex).OrderBy("recieve_date")
                    .GetAsync<Mail>();
            }
            catch (MySqlException ex)
            {
                return (MysqlExceptionHandle(userId.ToString(), ex), null);
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
                // TODO: Logger
                return (ErrorCode.None, previews);
            }
            // TODO: Logger
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
                    return (ErrorCode.InvalidMailId, null);
                }
                return (ErrorCode.None, mail);
            }
            catch (MySqlException ex)
            {
                return (MysqlExceptionHandle(mailId.ToString(), ex), null);
            }
        }

        private String? MailItemsQuery(Mail mail)
        {
            List<(ItemDefine, Int64)> itemDefines = new List<(ItemDefine, Int64)>();
            ItemDefine? tempDefine = _masterDataOffer.getItemDefine(mail.item0_code);
            if (tempDefine != null)
            {
                itemDefines.Add((tempDefine, mail.item0_count));
            }
            tempDefine = _masterDataOffer.getItemDefine(mail.item1_code);
            if (tempDefine != null)
            {
                itemDefines.Add((tempDefine, mail.item1_count));
            }
            tempDefine = _masterDataOffer.getItemDefine(mail.item2_code);
            if (tempDefine != null)
            {
                itemDefines.Add((tempDefine, mail.item2_count));
            }
            tempDefine = _masterDataOffer.getItemDefine(mail.item3_code);
            if (tempDefine != null)
            {
                itemDefines.Add((tempDefine, mail.item3_count));
            }
            if (itemDefines.Count == 0)
            {
                return null;
            }
            String query = "INSERT INTO user_items VALUES ";
            foreach (var itemDefine in itemDefines)
            {
                query += "(" + mail.user_id + ", "
                    + itemDefine.Item1.item_code.ToString() + ", "
                    + itemDefine.Item2.ToString() + ", "
                    + itemDefine.Item1.attack.ToString() + ", "
                    + itemDefine.Item1.defence.ToString() + ", "
                    + itemDefine.Item1.magic.ToString() + ", "
                    + "0 ), ";
            }
            return query.TrimEnd(new char[] { ',', ' ' });
        }

        private object getMailItemDefaultObj()
        {
            return new {
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

        public async Task<ErrorCode> RecvMailItem(Int64 userId, Int64 mailId)
        {
            Mail? mail;
            try
            {
                mail = await _db.Query("mailbox")
                    .Select("*")
                    .Where("mail_id", mailId)
                    .Where("user_id", userId)
                    .Where("is_deleted", 0)
                    .Where(q =>
                        q.Where("expiration_date", ">", DateTime.Now)
                        .OrWhere("expiration_date", null)
                    )
                    .FirstOrDefaultAsync<Mail>();
                if (mail == null)
                {
                    return ErrorCode.InvalidMailId;
                }
                await _db.Query("mailbox")
                    .Where("mail_id", mailId)
                    .Where("user_id", userId)
                    .Where("is_deleted", 0)
                    .Where("expiration_date", ">", DateTime.Now)
                    .OrWhere("expiration_date", null)
                    .UpdateAsync( getMailItemDefaultObj() );
            }       
            catch (MySqlException ex)
            {
                return MysqlExceptionHandle(mailId.ToString(), null);
            }
            var query = MailItemsQuery(mail);
            if (query == null)
            {
                return ErrorCode.None;
            }
            try
            {
                await _db.StatementAsync(query);
                return ErrorCode.None;
            }
            catch (MySqlException ex)
            {
                // TODO : 실패시 다시 우편함에 집어 넣기
                return MysqlExceptionHandle(mailId.ToString(), null);
            }
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
            catch (MySqlException ex)
            {
                return MysqlExceptionHandle(mailId.ToString(), null);
            }
        }

        
    }
}
