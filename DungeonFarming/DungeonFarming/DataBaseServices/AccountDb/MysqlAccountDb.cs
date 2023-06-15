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
                    user_assigned_id = model.user_assigned_id,
                    salt = model.salt,
                    hashed_password = model.hashed_password,
                });
                Int16 id = await _db.Query("user_accounts")
                    .Select("user_id")
                    .Where("user_assigned_id", model.user_assigned_id)
                    .FirstOrDefaultAsync<Int16>();
                _logger.ZLogInformationWithPayload(LogEventId.AccountDb, new { userAssignedId = model.user_assigned_id }, "RegisteUser SUCCESS");
                return (ErrorCode.None, id);
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062) //duplicated id exception
                {
                    _logger.ZLogErrorWithPayload(LogEventId.AccountDb, ex, new { userAssignedId = model.user_assigned_id }, "RegisteUser duplicated id EXCEPTION");
                    return (ErrorCode.DuplicatedId, -1);
                }
                _logger.ZLogErrorWithPayload(LogEventId.AccountDb, ex, new { userAssignedId = model.user_assigned_id }, "RegisteUser MYSQL_EXCEPTION");
                return (ErrorCode.AccountDbError, -1);
            }
            catch (Exception ex)
            {
                _logger.ZLogErrorWithPayload(LogEventId.AccountDb, ex, new { userAssignedId = model.user_assigned_id }, "RegisteUser EXCEPTION");
                return (ErrorCode.AccountDbError, -1);
            }
        }
        public async Task<(ErrorCode, UserAccountDto?)> GetAccountInfo(string userAssignedId)
        {
            try
            {
                var rt = await _db.Query("user_accounts")
                    .Select("user_id", "user_assigned_id", "salt", "hashed_password")
                    .Where("user_assigned_id", userAssignedId)
                    .FirstAsync<UserAccountDto?>();
                if (rt == null)
                {
                    _logger.ZLogWarningWithPayload(LogEventId.AccountDb, new { userAssignedId = userAssignedId }, "GetAccountInfo Invalid Id FAIL");
                    return (ErrorCode.InvalidId, null);
                }

                return (ErrorCode.None, rt);
            }
            catch (MySqlException ex)
            {
                _logger.ZLogErrorWithPayload(LogEventId.AccountDb, ex, new { userAssignedId = userAssignedId }, "GetAccountInfo MYSQL_EXCEPTION");
                return (ErrorCode.AccountDbError, null);
            }
            catch (Exception ex)
            {
                _logger.ZLogErrorWithPayload(LogEventId.AccountDb, ex, new { userAssignedId = userAssignedId }, "GetAccountInfo EXCEPTION");
                return (ErrorCode.AccountDbError, null);
            }
        }

        public async Task<ErrorCode> DeleteAccount(string userAssignedId)
        {
            try
            {
                if (await _db.Query("user_accounts").Where("user_assigned_id", userAssignedId).ExistsAsync())
                {
                    await _db.Query("user_accounts").Where("user_assigned_id", userAssignedId).DeleteAsync();
                    _logger.ZLogInformationWithPayload(LogEventId.AccountDb, new { userAssignedId = userAssignedId }, "DeleteAccount EXCEPTION");
                    return ErrorCode.None;
                }
                _logger.ZLogErrorWithPayload(LogEventId.AccountDb, new { userAssignedId = userAssignedId }, "DeleteAccount EXCEPTION");

                return ErrorCode.InvalidId;
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062) //duplicated id exception
                {
                    _logger.ZLogErrorWithPayload(LogEventId.AccountDb, ex, new { userAssignedId = userAssignedId }, "DeleteAccount duplicated id EXCEPTION");
                    return ErrorCode.DuplicatedId;
                }
                _logger.ZLogErrorWithPayload(LogEventId.AccountDb, ex, new { userAssignedId = userAssignedId }, "DeleteAccount MYSQL_EXCEPTION");
                return ErrorCode.AccountDbError;
            }
            catch (Exception ex)
            {
                _logger.ZLogErrorWithPayload(LogEventId.AccountDb, ex, new { userAssignedId = userAssignedId }, "DeleteAccount EXCEPTION");
                return ErrorCode.AccountDbError;
            }
        }
    }
}
