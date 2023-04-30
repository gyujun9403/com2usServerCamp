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
            var connString = config.GetConnectionString("Mysql_Account");
            var connection = new MySqlConnection(connString);
            var compiler = new MySqlCompiler();
            _db = new QueryFactory(connection, compiler);
            _logger = logger;
        }

        private ErrorCode MysqlExceptionHandle(String user_id, MySqlException ex)
        {
            if (ex.Number == 1062) //duplicated id exception
            {
                _logger.ZLogError($"[GetAccountInfo] Error : {user_id} duplicate key exception");
                return ErrorCode.DuplicatedId;
            }
            _logger.ZLogError($"[GetAccountInfo] Error : {user_id} duplicate key exception");
            return ErrorCode.AccountDbError;
        }

        public async Task<ErrorCode> DeleteAccount(string userId)
        {
            try
            {
                if (await _db.Query("user_accounts").Where("user_id", userId).ExistsAsync())
                {
                    await _db.Query("user_accounts").Where("user_id", userId).DeleteAsync();
                    _logger.ZLogInformation($"[DeleteAccount] Info : {userId}");
                    return ErrorCode.None;
                }
                _logger.ZLogError($"[DeleteAccount] Error : {userId} Invalid Id");

                return ErrorCode.InvalidId;
            }
            catch (MySqlException ex)
            {
                return MysqlExceptionHandle(userId, ex);
            }
            catch (Exception ex)
            {
                _logger.ZLogError($"[DeleteAccount] Error : {userId} {ex.Message}");
                return ErrorCode.AccountDbError;
            }
        }

        public async Task<(ErrorCode, UserAccountsTuple?)> GetAccountInfo(string userId)
        {
            try
            {
                var rt = await _db.Query("user_accounts")
                    .Select("pk_id", "user_id", "salt", "hashed_password")
                    .Where("user_id", userId).FirstAsync<UserAccountsTuple>();
                if (rt.pk_id == null || rt.user_id == "" 
                    || rt.salt.Length == 0 || rt.hashed_password.Length == 0)
                {
                    _logger.ZLogError($"[GetAccountInfo] Error : {userId} Invalid Id");
                    return (ErrorCode.InvalidId, null);
                }
                _logger.ZLogInformation($"[GetAccountInfo] Info : {userId}");

                return (ErrorCode.None, rt);
            }
            catch (MySqlException ex)
            {
                return (MysqlExceptionHandle(userId, ex), null);
            }
            catch (Exception ex)
            {
                _logger.ZLogError($"[GetAccountInfo] Error : {userId} {ex.Message}");
                return (ErrorCode.AccountDbError, null);
            }
        }

        public async Task<(ErrorCode, Int16)> RegisteUser(UserAccountsTuple model)
        {
            try
            {
                await _db.Query("user_accounts").InsertAsync(new
                {
                    user_id = model.user_id,
                    salt = model.salt,
                    hashed_password = model.hashed_password,
                });
                Int16 id = await _db.Query("user_accounts").Select("pk_id").Where("user_id", model.user_id).FirstOrDefaultAsync<Int16>();
                _logger.ZLogInformation($"[GetAccountInfo] Info : {model.user_id}");
                return (ErrorCode.None, id);
            }
            catch (MySqlException ex)
            {
                return (MysqlExceptionHandle(model.user_id, ex), -1);
            }
            catch (Exception ex)
            {
                _logger.ZLogError($"[GetAccountInfo] Error : {model.user_id} {ex.Message}");
                return (ErrorCode.AccountDbError, -1);
            }

        }
    }
}
