using DungeonFarming.DataBase.AccountDb;
using DungeonFarming.DataBase.GameDb;
using DungeonFarming.DataBase.GameDb.GameUserDataORM;
using DungeonFarming.DataBase.GameSessionDb;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using ZLogger;

namespace DungeonFarming.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoginController : ControllerBase
    {
        readonly IAccountDb _accountDb;
        readonly IGameSessionDb _gameSessionDb;
        readonly IGameDb _gameDb;
        readonly IMasterDataOffer _masterDataOffer;
        readonly ILogger<LoginController> _logger;

        public LoginController(ILogger<LoginController> logger, IMasterDataOffer masterDataOffer,
            IGameSessionDb gameSessionDb, IGameDb gameDb, IAccountDb accountDb)
        {
            _logger = logger;
            _masterDataOffer = masterDataOffer;
            _gameSessionDb = gameSessionDb;
            _gameDb = gameDb;
            _accountDb = accountDb;
        }

        [HttpPost]
        public async Task<LoginResponse> Login(LoginRequest request)
        {
            LoginResponse response = new LoginResponse();

            // 계정 정보 가져오고 확인
            (response.errorCode, var userAccountTuple) = await _accountDb.GetAccountInfo(request.userId);
            if (response.errorCode != ErrorCode.None)
            {
                return response;
            }
            else if (userAccountTuple == null || !userAccountTuple.pk_id.HasValue)
            {
                response.errorCode = ErrorCode.ServerError;
                return response;
            }

            // 비밀번호 확인
            if (Security.VerifyHashedPassword(request.password, userAccountTuple.salt,
                    userAccountTuple.hashed_password) == false)
            {
                response.errorCode = ErrorCode.WorngPassword;
                _logger.ZLogInformationWithPayload(LogEventId.Login, new { userId = request.userId}, "login password FAIL");
                return response;
            }

            // 토큰 발급하고 세션 생성.
            String token = Security.GenerateToken();
            response.errorCode = await _gameSessionDb.SetUserInfoSession(new GameSessionData
            {
                userStringId = request.userId,
                userId = userAccountTuple.pk_id.Value,
                token = token,
                userStatus = UserStatus.Login
            });
            if (response.errorCode != ErrorCode.None)
            {
                return response;
            }
            response.token = token;

            // 2. 유저 장비 정보를 가져옴
            (response.errorCode, var userItems) = await _gameDb.GetUserItemList(userAccountTuple.pk_id.Value);
            if (response.errorCode != ErrorCode.None)
            {
                response.errorCode = ErrorCode.ServerError;
                return response;
            }
            response.userItems = userItems;
            _logger.ZLogInformationWithPayload(LogEventId.Login, new { userId = request.userId}, "Login SUCCESS");
            return response;
        }
    }
}
