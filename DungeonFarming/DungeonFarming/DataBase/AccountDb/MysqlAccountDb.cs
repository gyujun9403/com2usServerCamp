using SqlKata.Execution;
using SqlKata.Compilers;
using MySqlConnector;
using ZLogger;

namespace DungeonFarming.DataBase.AccountDb
{
    public class MysqlAccountDb : IAccountDb
    {
        private ILogger<MysqlAccountDb> _logger;
        QueryFactory _db;
        public MysqlAccountDb(IConfiguration config, ILogger<MysqlAccountDb> logger)
        {
            String connString = config.GetConnectionString("Mysql_Account");
            var connection = new MySqlConnection(connString);
            var compiler = new MySqlCompiler();
            _db = new QueryFactory(connection, compiler);
            _logger = logger;
        }

        public async Task<(ErrorCode, AccountDbModel?)> GetAccountInfo(string accountId)
        {
            AccountDbModel rt;
            try
            {
                rt = await _db.Query("account")
                    .Select("pk_id", "account_id", "salt", "hashed_password")
                    .Where("user_id", accountId).FirstAsync<AccountDbModel>();
                if (rt.pk_id == null || rt.account_id == "" 
                    || rt.salt.Length == 0 || rt.hashed_password.Length == 0)
                {
                    _logger.ZLogError($"[GetAccountInfo] Error : {accountId} Invalid Id");
                    return (ErrorCode.InvalidId, null);
                }
                _logger.ZLogInformation($"[GetAccountInfo] Info : {accountId}");
                return (ErrorCode.ErrorNone, rt);
            }
            catch(Exception e)
            {
                _logger.ZLogError($"[GetAccountInfo] Error : {accountId} Account Db Error");
                return (ErrorCode.AccountDbError, null);
            }
        }

        public async Task<ErrorCode> RegisteUser(AccountDbModel model)
        {
            try
            {
                await _db.Query("account").InsertAsync(new
                {
                    user_id = model.account_id,
                    salt = model.salt,
                    hashed_password = model.hashed_password,
                });
                _logger.ZLogInformation($"[GetAccountInfo] Info : {model.account_id}");
            }
            catch (Exception ex)
            {
                // 중복인경우 ERROR_CODE.DUPLICATED_ID를 던지게 추가
                _logger.ZLogError($"[GetAccountInfo] Error : {model.account_id} Account Db Error");
                return ErrorCode.AccountDbError;
            }
            return ErrorCode.ErrorNone;
        }
    }
}
