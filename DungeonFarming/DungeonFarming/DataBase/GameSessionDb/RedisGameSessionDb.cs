using CloudStructures;
using CloudStructures.Structures;
using System.Reflection;
using ZLogger;

namespace DungeonFarming.DataBase.GameSessionDb
{
    public class RedisGameSessionDb : IGameSessionDb
    {
        RedisConfig _redisConfig;
        RedisConnection _redisConnection;
        ILogger<RedisGameSessionDb> _logger;
        public RedisGameSessionDb(IConfiguration config, ILogger<RedisGameSessionDb> logger)
        {
            string _connectionString = config.GetConnectionString("Redis_GameSession");
            _redisConfig = new CloudStructures.RedisConfig("test", _connectionString);
            _redisConnection = new RedisConnection(_redisConfig);
            _logger = logger;
        }

        public async Task<ErrorCode> deleteToken(string accountId)
        {
            try
            {
                RedisString<string> redisString = new RedisString<string>(_redisConnection, "token:" + accountId, null);
                CloudStructures.RedisResult<string> result = await redisString.GetAndDeleteAsync();
                if (result.HasValue == false)
                {
                    _logger.ZLogError($"[deleteToken] Error : {accountId}, Invalid Id");
                    return ErrorCode.ErrorNone;
                }
                _logger.ZLogInformation($"[deleteToken] Info : {accountId}");
                return ErrorCode.ErrorNone;
            }
            catch (Exception ex)
            {
                _logger.ZLogError($"[deleteToken] Error : {accountId} {ex.Message}");
                return ErrorCode.GameSessionDbError;
            }
        }

        public async Task<(ErrorCode, string?)> getToken(string accountId)
        {
            try
            {
                RedisString<string> redisString = new RedisString<string>(_redisConnection, "token:" + accountId, TimeSpan.FromHours(1));
                CloudStructures.RedisResult<string> result = await redisString.GetAsync();
                if (result.HasValue == false)
                {
                    _logger.ZLogError($"[getToken] Error : {accountId}, Invalid Id");
                    return (ErrorCode.InvalidId, null);
                }
                _logger.ZLogInformation($"[setToken] Info : {accountId}");
                return (ErrorCode.InvalidId, result.Value);
            }
            catch(Exception ex)
            {
                _logger.ZLogError($"[getToken] Error : {accountId} {ex.Message}");
                return (ErrorCode.GameSessionDbError, null);
            }
        }

        public async Task<ErrorCode> setToken(AuthCheckModel model)
        {
            try
            {
                RedisString<string> redisString = new RedisString<string>(_redisConnection, "token:" + model.user_id, TimeSpan.FromHours(1));
                await redisString.SetAsync(model.token, TimeSpan.FromHours(1));
                _logger.ZLogInformation($"[setToken] Info : {model.user_id}");
            }
            catch (Exception ex)
            {
                _logger.ZLogError($"[setToken] Error : {model.user_id} {ex.Message}");
                return ErrorCode.GameSessionDbError;
            }
            return ErrorCode.ErrorNone;
        }
    }
}
