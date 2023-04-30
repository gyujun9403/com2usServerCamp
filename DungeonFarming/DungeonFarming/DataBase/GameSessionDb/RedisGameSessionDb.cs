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
            var _connectionString = config.GetConnectionString("Redis_GameSession");
            _redisConfig = new CloudStructures.RedisConfig("test", _connectionString);
            _redisConnection = new RedisConnection(_redisConfig);
            _logger = logger;
        }

        String GenerateUserInfoSessionKey(String userId)
        {
            return "userInfo:" + userId;
        }

        public async Task<ErrorCode> DeleteUserInfoSession(String userId)
        {
            try
            {
                var keyString = GenerateUserInfoSessionKey(userId);
                var redisString = new RedisString<GameSessionData>(_redisConnection, keyString, null);
                var result = await redisString.GetAndDeleteAsync();
                if (result.HasValue == false)
                {
                    _logger.ZLogError($"[delete UserInfo Session] Error : {userId}, Invalid Id");
                    return ErrorCode.InvalidToken;
                }
                _logger.ZLogInformation($"[delete UserInfo Session] Info : {userId}");

                return ErrorCode.None;
            }
            catch (Exception ex)
            {
                _logger.ZLogError($"[delete UserInfo Session] Error : {userId} {ex.Message}");
                return ErrorCode.GameSessionDbError;
            }
        }

        public async Task<(ErrorCode, GameSessionData?)> GetUserInfoSession(String userId)
        {
            try
            {
                var keyString = GenerateUserInfoSessionKey(userId);
                var redisString = new RedisString<GameSessionData>(_redisConnection, keyString, TimeSpan.FromHours(1));
                var result = await redisString.GetAsync();
                if (result.HasValue == false)
                {
                    _logger.ZLogError($"[get UserInfo Session] Error : {userId}, Invalid Id");
                    return (ErrorCode.InvalidId, null);
                }
                _logger.ZLogInformation($"[set UserInfo Session] Info : {userId}");

                return (ErrorCode.None, result.Value);
            }
            catch(Exception ex)
            {
                _logger.ZLogError($"[get UserInfo Session] Error : {userId} {ex.Message}");
                return (ErrorCode.GameSessionDbError, null);
            }
        }

        public async Task<ErrorCode> SetUserInfoSession(GameSessionData userInfo)
        {
            try
            {
                var keyString = GenerateUserInfoSessionKey(userInfo.userId);
                var redisString = new RedisString<GameSessionData>(_redisConnection, keyString, TimeSpan.FromHours(1));
                await redisString.SetAsync(userInfo, TimeSpan.FromHours(1));
                _logger.ZLogInformation($"[set UserInfo Session] Info : {userInfo.userId}");
                return ErrorCode.None;
            }
            catch (Exception ex)
            {
                _logger.ZLogError($"[set UserInfo Session] Error : {userInfo.userId} {ex.Message}");
                return ErrorCode.GameSessionDbError;
            }
        }

        public async Task<string> GetNotice()
        {
            var redisString = new RedisString<String>(_redisConnection, "notice", null);
            var result = await redisString.GetAsync();
            return result.Value;
        }
    }
}
