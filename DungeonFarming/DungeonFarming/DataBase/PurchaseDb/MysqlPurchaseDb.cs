using DungeonFarming.DataBase.AccountDb;
using DungeonFarming.DataBase.GameDb;
using DungeonFarming.DataBase.PurchaseDb.PurchaseORM;
using MySqlConnector;
using SqlKata.Compilers;
using SqlKata.Execution;
using ZLogger;

namespace DungeonFarming.DataBase.PurchaseDb
{
    public class MysqlPurchaseDb : IPurchaseDb
    {
        readonly ILogger<MysqlPurchaseDb> _logger;
        IMasterDataOffer _masterDataOffer;
        QueryFactory _db;
        public MysqlPurchaseDb(IConfiguration config, IMasterDataOffer masterDataOffer, ILogger<MysqlPurchaseDb> logger)
        {
            var connString = config.GetConnectionString("Mysql_Purchase");
            var connection = new MySqlConnection(connString);
            var compiler = new MySqlCompiler();
            _db = new QueryFactory(connection, compiler);
            _logger = logger;
            _masterDataOffer = masterDataOffer;
        }
        public async Task<ErrorCode> CheckPurchaseDuplicated(string purchaseToken)
        {
            try
            {
                var isDuplicated = await _db.Query("purchase_histories")
                    .Where("purchase_token", purchaseToken)
                    .ExistsAsync();
                if (isDuplicated == true) 
                {
                    return ErrorCode.DuplicatedPurchaseToken;
                }
                return ErrorCode.None;
            }
            catch (MySqlException ex)
            {
                _logger.ZLogErrorWithPayload(LogEventId.PurchaseDb, ex, new { purchaseToken = purchaseToken }, "CheckPurchaseDuplicated MySqlException");
                return ErrorCode.purchaseDbError;
            }
            catch (Exception ex)
            {
                _logger.ZLogErrorWithPayload(LogEventId.PurchaseDb, ex, new { purchaseToken = purchaseToken }, "CheckPurchaseDuplicated MySqlException");
                return ErrorCode.purchaseDbError;
            }
        }
        public async Task<ErrorCode> WritePurchase(long userId, string purchaseToken, short packageCode)
        {
            try
            {
                await _db.Query("purchase_histories")
                    .InsertAsync( new { 
                        user_id = userId,
                        purchase_token = purchaseToken,
                        package_code = packageCode,
                        purchase_date = DateTime.Now
                    });
                return ErrorCode.None;
            }
            catch (MySqlException ex)
            {
                _logger.ZLogErrorWithPayload(LogEventId.PurchaseDb, ex, new { purchaseToken = purchaseToken }, "WritePurchase MySqlException");
                return ErrorCode.purchaseDbError;
            }
            catch (Exception ex)
            {
                _logger.ZLogErrorWithPayload(LogEventId.PurchaseDb, ex, new { purchaseToken = purchaseToken }, "WritePurchase MySqlException");
                return ErrorCode.purchaseDbError;
            }
        }
    }
}
