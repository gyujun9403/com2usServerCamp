using DungeonFarming.DataBase.AccountDb;
using DungeonFarming.DataBase.GameDb;
using DungeonFarming.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZLogger;

namespace DungeonFarming.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DailyLoginRewardController : ControllerBase
    {
        readonly IGameDb _gameDb;
        readonly ILogger<DailyLoginRewardController> _logger;
        public DailyLoginRewardController(IGameDb gameDb, ILogger<DailyLoginRewardController> logger)
        {
            _gameDb = gameDb;
            _logger = logger;
        }
        [HttpPost("LoginRewardStack")]
        public async Task<LoginRewardStackResponse> LoginStack(LoginRewardStackRequst request)
        {
            LoginRewardStackResponse response = new LoginRewardStackResponse();
            Int64 userPkId = -1;
            if (HttpContext.Request.Headers.TryGetValue("UserPkId", out var userPkIdStr))
            {
                if (long.TryParse(userPkIdStr, out userPkId) == false)
                {
                    response.errorCode = ErrorCode.GameDbError;
                    _logger.ZLogErrorWithPayload(LogEventId.DailyLoginReward, new { userId = request.userId }, "pk id get FAIL");
                    return response;
                }
            }
            // 토큰 가져오고 검증.
            var (errorCode, loginlog) = await _gameDb.GetLoginLog(userPkId);
            if (loginlog == null)
            {
                response.errorCode = ErrorCode.ServerError;
                _logger.ZLogErrorWithPayload(LogEventId.DailyLoginReward, new { pkId = userPkId }, "login log get FAIL");
                return response;
            }
            response.loginRewardStack = loginlog.consecutive_login_count;
            _logger.ZLogErrorWithPayload(LogEventId.DailyLoginReward, new { pkId = userPkId }, "login reward stack send SUCCESS");
            return response;
        }
    }
}
