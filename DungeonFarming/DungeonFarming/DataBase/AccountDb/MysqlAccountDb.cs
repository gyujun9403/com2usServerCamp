using SqlKata.Execution;
using SqlKata.Compilers;
using MySqlConnector;
using ZLogger;
using System.Reflection;

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

        public async Task<ErrorCode> DeleteAccount(string userId)
        {
            try
            {
                // 현재 없는 유저인경우, 예외를 던지지 않는다. 필요시 수정.
                await _db.Query("account").Where("user_id", userId).DeleteAsync();
                _logger.ZLogInformation($"[DeleteAccount] Info : {userId}");
                return ErrorCode.ErrorNone;
            }
            catch(Exception ex)
            {
                // 없는 유저일 경우 ERROR_CODE.INVALID_ID를 던지게 추가
                _logger.ZLogError($"[DeleteAccount] Error : {userId} {ex.Message}");
                return ErrorCode.AccountDbError;
            }
        }

        public async Task<(ErrorCode, AccountDbModel?)> GetAccountInfo(string userId)
        {
            AccountDbModel rt;
            try
            {
                rt = await _db.Query("account")
                    .Select("pk_id", "user_id", "salt", "hashed_password")
                    .Where("user_id", userId).FirstAsync<AccountDbModel>();
                if (rt.pk_id == null || rt.user_id == "" 
                    || rt.salt.Length == 0 || rt.hashed_password.Length == 0)
                {
                    _logger.ZLogError($"[GetAccountInfo] Error : {userId} Invalid Id");
                    return (ErrorCode.InvalidId, null);
                }
                _logger.ZLogInformation($"[GetAccountInfo] Info : {userId}");
                return (ErrorCode.ErrorNone, rt);
            }
            catch(Exception ex)
            {
                _logger.ZLogError($"[GetAccountInfo] Error : {userId} {ex.Message}");
                return (ErrorCode.AccountDbError, null);
            }
        }

        public async Task<ErrorCode> RegisteUser(AccountDbModel model)
        {
            try
            {
                await _db.Query("account").InsertAsync(new
                {
                    user_id = model.user_id,
                    salt = model.salt,
                    hashed_password = model.hashed_password,
                });
                _logger.ZLogInformation($"[GetAccountInfo] Info : {model.user_id}");
                return ErrorCode.ErrorNone;
            }
            catch (Exception ex)
            {
                // 중복인경우 ERROR_CODE.DUPLICATED_ID를 던지게 추가
                _logger.ZLogError($"[GetAccountInfo] Error : {model.user_id} {ex.Message}");
                return ErrorCode.AccountDbError;
            }
        }
    }
}
