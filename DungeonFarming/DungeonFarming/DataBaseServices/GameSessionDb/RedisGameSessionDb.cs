using CloudStructures;
using CloudStructures.Structures;
using System.Reflection;
using ZLogger;

namespace DungeonFarming.DataBase.GameSessionDb;

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

    String GenerateUserInfoSessionKey(String userAssignedId)
    {
        return "userInfo:" + userAssignedId;
    }

    public async Task<ErrorCode> DeleteUserInfoSession(String userAssignedId)
    {
        try
        {
            var keyString = GenerateUserInfoSessionKey(userAssignedId);
            var redisString = new RedisString<GameSessionData>(_redisConnection, keyString, null);
            var result = await redisString.GetAndDeleteAsync();
            if (result.HasValue == false)
            {
                _logger.ZLogErrorWithPayload(LogEventId.GameSessionDb, new { userAssignedId = userAssignedId }, "DeleteSession Invalid Id FAIL");
                return ErrorCode.InvalidToken;
            }
            return ErrorCode.None;
        }
        catch (Exception ex)
        {
            _logger.ZLogErrorWithPayload(LogEventId.GameSessionDb, ex, new { userAssignedId = userAssignedId }, "DeleteSession EXCEPTION");
            return ErrorCode.GameSessionDbError;
        }
    }

    public async Task<(ErrorCode, GameSessionData?)> GetUserInfoSession(String userAssignedId)
    {
        try
        {
            var keyString = GenerateUserInfoSessionKey(userAssignedId);
            var redisString = new RedisString<GameSessionData>(_redisConnection, keyString, TimeSpan.FromHours(1));
            var result = await redisString.GetAsync();
            if (result.HasValue == false)
            {
                _logger.ZLogErrorWithPayload(LogEventId.GameSessionDb, new { userAssignedId = userAssignedId }, "GetUserInfoSession Invalid Id FAIL");
                return (ErrorCode.InvalidId, null);
            }
            return (ErrorCode.None, result.Value);
        }
        catch(Exception ex)
        {
            _logger.ZLogErrorWithPayload(LogEventId.GameSessionDb, ex, new { userAssignedId = userAssignedId }, "GetUserInfoSession EXCEPTION");
            return (ErrorCode.GameSessionDbError, null);
        }
    }

    public async Task<ErrorCode> SetUserInfoSession(GameSessionData userInfo)
    {
        try
        {
            var keyString = GenerateUserInfoSessionKey(userInfo.userAssignedId);
            var redisString = new RedisString<GameSessionData>(_redisConnection, keyString, TimeSpan.FromHours(1));
            if (await redisString.SetAsync(userInfo, TimeSpan.FromHours(1)) == true)
            {
                return ErrorCode.None;
            }
            else
            {
                _logger.ZLogErrorWithPayload(LogEventId.GameSessionDb, new { session = userInfo }, "SetUserInfoSession Session Set FAIL");
                return ErrorCode.GameSessionDbError;
            }
        }
        catch (Exception ex)
        {
            _logger.ZLogErrorWithPayload(LogEventId.GameSessionDb, ex, new { session = userInfo }, "SetUserInfoSession EXCEPTION");
            return ErrorCode.GameSessionDbError;
        }
    }

    public async Task<(ErrorCode, string?)> GetNotice()
    {
        String? result = null;
        try
        {
            var redisString = new RedisString<String>(_redisConnection, "notice", null);
            var residResult = await redisString.GetAsync();
            if (residResult.HasValue == false)
            {
                _logger.ZLogErrorWithPayload(LogEventId.GameSessionDb, new { }, "GetNotice FAIL");
                return (ErrorCode.GameSessionDbError, null);
            }
            result = residResult.Value;
        }
        catch (Exception ex)
        {
            _logger.ZLogErrorWithPayload(LogEventId.GameSessionDb, ex, new { }, "GetNotice EXCEPTION");
            return (ErrorCode.GameSessionDbError, null);
        }
        return (ErrorCode.None, result);
    }
}
