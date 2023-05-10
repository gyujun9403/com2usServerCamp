using SqlKata.Execution;
using SqlKata.Compilers;
using MySqlConnector;
using ZLogger;
using System.Reflection;

namespace DungeonFarming.DataBase.AccountDb
{
    public class MysqlAccountDb : IAccountDb
    {
        readonly ILogger<MysqlAccountDb> _logger;
        readonly QueryFactory _db;
        public MysqlAccountDb(IConfiguration config, ILogger<MysqlAccountDb> logger)
        {
            var connString = config.GetConnectionString("Mysql_Account");
            var connection = new MySqlConnection(connString);
            var compiler = new MySqlCompiler();
            _db = new QueryFactory(connection, compiler);
            _logger = logger;
        }
        public async Task<(ErrorCode, Int16)> RegisteUser(UserAccountDto model)
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
                //_logger.ZLogInformation($"[GetAccountInfo] Info : {model.user_id}");
                _logger.ZLogInformationWithPayload(LogEventId.AccountDb, new { userId = model.user_id }, "RegisteUser SUCCESS");
                return (ErrorCode.None, id);
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062) //duplicated id exception
                {
                    _logger.ZLogErrorWithPayload(LogEventId.AccountDb, ex, new { userId = model.user_id }, "RegisteUser duplicated id EXCEPTION");
                    return (ErrorCode.DuplicatedId, -1);
                }
                _logger.ZLogErrorWithPayload(LogEventId.AccountDb, ex, new { userId = model.user_id }, "RegisteUser MYSQL_EXCEPTION");
                return (ErrorCode.AccountDbError, -1);
            }
            catch (Exception ex)
            {
                _logger.ZLogErrorWithPayload(LogEventId.AccountDb, ex, new { userId = model.user_id }, "RegisteUser EXCEPTION");
                return (ErrorCode.AccountDbError, -1);
            }
        }
        public async Task<(ErrorCode, UserAccountDto?)> GetAccountInfo(string userId)
        {
            try
            {
                var rt = await _db.Query("user_accounts")
                    .Select("pk_id", "user_id", "salt", "hashed_password")
                    .Where("user_id", userId).FirstAsync<UserAccountDto>();
                if (rt.pk_id == null || rt.user_id == ""
                    || rt.salt.Length == 0 || rt.hashed_password.Length == 0)
                {
                    //_logger.ZLogError($"[GetAccountInfo] Error : {userId} Invalid Id");
                    _logger.ZLogWarningWithPayload(LogEventId.AccountDb, new { userId = userId }, "GetAccountInfo Invalid Id FAIL");
                    return (ErrorCode.InvalidId, null);
                }
                //_logger.ZLogInformation($"[GetAccountInfo] Info : {userId}");

                return (ErrorCode.None, rt);
            }
            catch (MySqlException ex)
            {
                _logger.ZLogErrorWithPayload(LogEventId.AccountDb, ex, new { userId = userId }, "GetAccountInfo MYSQL_EXCEPTION");
                return (ErrorCode.AccountDbError, null);
            }
            catch (Exception ex)
            {
                _logger.ZLogErrorWithPayload(LogEventId.AccountDb, ex, new { userId = userId }, "GetAccountInfo EXCEPTION");
                return (ErrorCode.AccountDbError, null);
            }
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
                if (ex.Number == 1062) //duplicated id exception
                {
                    _logger.ZLogErrorWithPayload(LogEventId.AccountDb, ex, new { userId = userId }, "DeleteAccount duplicated id EXCEPTION");
                    return ErrorCode.DuplicatedId;
                }
                _logger.ZLogErrorWithPayload(LogEventId.AccountDb, ex, new { userId = userId }, "DeleteAccount MYSQL_EXCEPTION");
                return ErrorCode.AccountDbError;
            }
            catch (Exception ex)
            {
                _logger.ZLogErrorWithPayload(LogEventId.AccountDb, ex, new { userId = userId }, "DeleteAccount EXCEPTION");
                return ErrorCode.AccountDbError;
            }
        }
    }
}
