using DungeonFarming.DataBase.GameDb;
using DungeonFarming.DataBase.GameSessionDb;
using DungeonFarming.DataBaseServices.GameDb.GameUserDataDTO;
using DungeonFarming.ReqResDTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZLogger;

namespace DungeonFarming.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserAchivementController : ControllerBase
    {
        readonly IGameDb _gameDb;
        readonly ILogger<UserAchivementController> _logger;
        readonly GameSessionData _gameSessionData;

        public UserAchivementController(IHttpContextAccessor httpContextAccessor, IGameDb gameDb, ILogger<UserAchivementController> logger)
        {
            _gameDb = gameDb;
            _logger = logger;
            _gameSessionData = httpContextAccessor.HttpContext.Items["gameSessionData"] as GameSessionData;
        }

        [HttpPost]
        public async Task<UserAchivementResponse> UserAchievement(UserAchivementRequest request)
        {
            UserAchivementResponse response = new UserAchivementResponse();

            (response.errorCode, var UserAchievement) = await _gameDb.GetUserAchivement(_gameSessionData.userId);
            if (response.errorCode != ErrorCode.None)
            {
                return response;
            }

            response.userLevel = UserAchievement.user_level;
            response.userExp = UserAchievement.user_exp;
            response.highestClearedStageId = UserAchievement.highest_cleared_stage_id;
            return response;
        }
    }
}
