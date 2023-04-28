using DungeonFarming.BodyData;
using DungeonFarming.DataBase.GameSessionDb;
using DungeonFarming.DTOs;
using Microsoft.AspNetCore.Http;
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

        // POST: Logout
        [HttpPost]
        public async Task<LogoutResData> Post(LogoutReqBodyData body)
        {
            LogoutResData logoutResData = new LogoutResData();
            logoutResData.errorCode = await _gameSessionDb.deleteToken(body.user_id);
            // TODO: 게임 데이터 삭제하는 로직도 필요.
            if (logoutResData.errorCode == ErrorCode.ErrorNone)
            {
                _logger.ZLogInformation($"[Logout] Info : body.user_id - Logout");
            }
            else
            {
                _logger.ZLogError($"[Logout] Error : body.user_id - {logoutResData.errorCode}");
            }
            return logoutResData;
        }

    }
}
