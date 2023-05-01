using DungeonFarming.DataBase.AccountDb;
using MySqlConnector;
using SqlKata;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Collections.Generic;
using ZLogger;

namespace DungeonFarming.DataBase.GameDb
{
    public class MysqlGameDb : IGameDb
    {
        private ILogger<MysqlAccountDb> _logger;
        QueryFactory _db;
        public MysqlGameDb(IConfiguration config, ILogger<MysqlAccountDb> logger)
        {
            var connString = config.GetConnectionString("Mysql_Game");
            var connection = new MySqlConnection(connString);
            var compiler = new MySqlCompiler();
            _db = new QueryFactory(connection, compiler);
            _logger = logger;
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

        public async Task<(ErrorCode, DefaultItemList?)> GetDefaultItemList(Int16 listId)
        {
            try
            {
                DefaultItemList rt = await _db.Query("default_item_list")
                    .Select("*").Where("list_id", listId)
                    .FirstOrDefaultAsync<DefaultItemList>();
                // TODO: Logger
                return (ErrorCode.None, rt);
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
                await _db.Query("game_user").InsertAsync( new { user_id = userId } );
                await _db.Query("inventory").InsertAsync( new { user_id = userId } );
                // TODO: Logger
                return ErrorCode.None;
            }
            catch (MySqlException ex)
            {
                return MysqlExceptionHandle(userId.ToString(), ex);
            }
        }

        private String makeInventoryInsertQuery(IItemList items)
        {
            List<ItemInfo> currentList = items.getCurrencyList();
            List<ItemInfo> itemList = items.getItemList();
            String query = "UPDATE inventory SET ";
            for (int i = 0; i < currentList.Count; i++)
            {
                query += "currency" + i + "_code = " + currentList[i].itemId.ToString() + ", " +
                    "currency" + i + "_count = " + currentList[i].itemNum.ToString() + ", ";
            }

            for (int i = 0; i < itemList.Count; i++)
            {
                query += "item" + i + "_code = " + itemList[i].itemId.ToString() + ", " +
                    "item" + i + "_count = " + itemList[i].itemNum.ToString() + ", ";
            }
            return query.TrimEnd(new char[] { ',', ' ' });
        }

        public async Task<ErrorCode> SetItemListInUserInventory(Int64 userId, IItemList items)
        {
            
            String query = makeInventoryInsertQuery(items) + " WHERE user_id=" + userId.ToString();
            try
            {
                await _db.StatementAsync(query);
                // TODO: Logger
                return ErrorCode.None;
            }
            catch (MySqlException ex)
            {
                return MysqlExceptionHandle(userId.ToString(), ex);
            }
        }

        public async Task<ErrorCode> UpdateUserConnectDate(Int64 userId)
        {
            try 
            {
                LoginLogForReword log = await _db.Query("game_user")
                    .Where("user_id", userId)
                    .Select("*")
                    .FirstOrDefaultAsync<LoginLogForReword>();
                if (log.today_login == 1)
                {
                    return ErrorCode.None;
                }
                await _db.Query("game_user")
                    .Where("user_id", userId)
                    .UpdateAsync( new {
                        consecutive_login_count = log.consecutive_login_count + 1,
                        missed_login_count = 0,
                        today_login = 1
                    });
                // TODO: Logger
                return ErrorCode.GameDbError;
            }
            catch (MySqlException ex)
            {
                return MysqlExceptionHandle(userId.ToString(), ex);
            }
        }

        public async Task<(ErrorCode, Inventory?)> GetInventory(Int64 user_id)
        {
            try
            {
                Inventory rt = await _db.Query("inventory")
                    .Select("*").Where("user_id", user_id)
                    .FirstOrDefaultAsync<Inventory>();
                // TODO: Logger
                return (ErrorCode.None, rt);
            }
            catch (MySqlException ex)
            {
                return (MysqlExceptionHandle(user_id.ToString(), ex), null);
            }
        }
    }
}
