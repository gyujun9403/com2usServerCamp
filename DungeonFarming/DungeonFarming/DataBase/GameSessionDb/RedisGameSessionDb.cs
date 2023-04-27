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
        RedisGameSessionDb(IConfiguration config, ILogger<RedisGameSessionDb> logger)
        {
            string _connectionString = config.GetConnectionString("Redis_GameSession");
            _redisConfig = new CloudStructures.RedisConfig("test", _connectionString);
            _redisConnection = new RedisConnection(_redisConfig);
            _logger = logger;
        }
        public async Task<(ErrorCode, string?)> getToken(string accountId)
        {
            try
            {
                RedisString<string> redisString = new RedisString<string>(_redisConnection, "token:" + accountId, TimeSpan.FromHours(1));
                CloudStructures.RedisResult<string> result = await redisString.GetAsync();
                if (!result.HasValue)
                {
                    _logger.ZLogError($"[getToken] Error : {accountId}, Invalid Id");
                    return (ErrorCode.InvalidId, null);
                }
                _logger.ZLogInformation($"[setToken] Info : {accountId}");
                return (ErrorCode.InvalidId, result.Value);
            }
            catch(Exception e)
            {
                _logger.ZLogError($"[getToken] Error : {accountId} GameSession Db Error");
                return (ErrorCode.GameSessionDbError, null);
            }
        }

        public async Task<ErrorCode> setToken(AuthCheckModel model)
        {
            try
            {
                RedisString<string> redisString = new RedisString<string>(_redisConnection, "token:" + model.account_id, TimeSpan.FromHours(1));
                await redisString.SetAsync(model.token, TimeSpan.FromHours(1));
                _logger.ZLogInformation($"[setToken] Info : {model.account_id}");
            }
            catch (Exception e)
            {
                _logger.ZLogError($"[setToken] Error : {model.account_id} Game Session Db Error");
                return ErrorCode.GameSessionDbError;
            }
            return ErrorCode.ErrorNone;
        }
    }
}
