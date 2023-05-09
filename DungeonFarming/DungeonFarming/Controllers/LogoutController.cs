using DungeonFarming.DataBase.GameSessionDb;
using Microsoft.AspNetCore.Mvc;
using ZLogger;

namespace DungeonFarming.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LogoutController : ControllerBase
    {
        readonly IGameSessionDb _gameSessionDb;
        readonly ILogger<LogoutController> _logger;
        public LogoutController(IGameSessionDb gameSessionDb, ILogger<LogoutController> logger)
        {
            _gameSessionDb = gameSessionDb;
            _logger = logger;
        }

        [HttpPost]
        public async Task<LogoutResponse> Logout(LogoutRequest request)
        {
            LogoutResponse response = new LogoutResponse();
            response.errorCode = await _gameSessionDb.DeleteUserInfoSession(request.userId);
            if (response.errorCode != ErrorCode.None)
            {
                _logger.ZLogErrorWithPayload(LogEventId.Logout, new { userid = request.userId, ErrorCode = response.errorCode }, "user logout FAIL");
            }
            else
            {
                _logger.ZLogInformationWithPayload(LogEventId.Logout, new { userid = request.userId }, "user logout Success");
            }

            return response;
        }

    }
}
