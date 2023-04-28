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
                var redisString = new RedisString<String>(_redisConnection, keyString, null);
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

        public async Task<(ErrorCode, String?)> GetUserInfoSession(String userId)
        {
            try
            {
                var keyString = GenerateUserInfoSessionKey(userId);
                var redisString = new RedisString<String>(_redisConnection, keyString, TimeSpan.FromHours(1));
                var result = await redisString.GetAsync();
                if (result.HasValue == false)
                {
                    _logger.ZLogError($"[get UserInfo Session] Error : {userId}, Invalid Id");
                    return (ErrorCode.InvalidId, null);
                }
                _logger.ZLogInformation($"[set UserInfo Session] Info : {userId}");

                return (ErrorCode.InvalidId, result.Value);
            }
            catch(Exception ex)
            {
                _logger.ZLogError($"[get UserInfo Session] Error : {userId} {ex.Message}");
                return (ErrorCode.GameSessionDbError, null);
            }
        }

        public async Task<ErrorCode> SetUserInfoSession(UserInfoSessionData userInfo)
        {
            try
            {
                var keyString = GenerateUserInfoSessionKey(userInfo.user_id);
                // TODO: GameDB 정의 후 token이외에 필요한 정보 추가할 수 있게 UserInfoSessionData수정.
                var redisString = new RedisString<String>(_redisConnection, keyString, TimeSpan.FromHours(1));
                await redisString.SetAsync(userInfo.token, TimeSpan.FromHours(1));
                _logger.ZLogInformation($"[set UserInfo Session] Info : {userInfo.user_id}");
                return ErrorCode.None;
            }
            catch (Exception ex)
            {
                _logger.ZLogError($"[set UserInfo Session] Error : {userInfo.user_id} {ex.Message}");
                return ErrorCode.GameSessionDbError;
            }
        }
    }
}
