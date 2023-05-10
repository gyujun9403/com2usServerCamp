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
        readonly Int64 _userId;
        public DailyLoginRewardController(IHttpContextAccessor httpContextAccessor, IGameDb gameDb, ILogger<DailyLoginRewardController> logger)
        {
            _gameDb = gameDb;
            _logger = logger;
            _userId = httpContextAccessor.HttpContext.Items["userId"] as Int64? ?? -1;
        }
        [HttpPost("LoginRewardStack")]
        public async Task<LoginRewardStackResponse> LoginStack(LoginRewardStackRequst request)
        {
            LoginRewardStackResponse response = new LoginRewardStackResponse();
            var (errorCode, loginlog) = await _gameDb.GetLoginLog(_userId);
            if (loginlog == null)
            {
                response.errorCode = ErrorCode.ServerError;
                _logger.ZLogErrorWithPayload(LogEventId.DailyLoginReward, new { pkId = _userId }, "login log get FAIL");
                return response;
            }
            response.loginRewardStack = loginlog.consecutive_login_count;
            _logger.ZLogErrorWithPayload(LogEventId.DailyLoginReward, new { pkId = _userId }, "login reward stack send SUCCESS");
            return response;
        }
    }
}
