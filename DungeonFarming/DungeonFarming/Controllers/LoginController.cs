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
            var (rtErrorCode, userAccountTuple) = await _accountDb.GetAccountInfo(request.userId);
            if (rtErrorCode != ErrorCode.None)
            {
                response.errorCode = rtErrorCode;
                _logger.ZLogErrorWithPayload(LogEventId.Login, new { userId = request.userId, errorCode = rtErrorCode }, "Account db FAIL");
                return response;
            }
            else if (userAccountTuple == null)
            {
                response.errorCode = ErrorCode.ServerError;
                _logger.ZLogErrorWithPayload(LogEventId.Login, new { userId = request.userId}, "Account db userAccountTuple null FAIL");
                return response;
            }
            else if (!userAccountTuple.pk_id.HasValue)
            {
                response.errorCode = ErrorCode.ServerError;
                _logger.ZLogErrorWithPayload(LogEventId.Login, new { userId = request.userId }, "Account db pk id null FAIL");
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
                // TODO: 게임 기능이 추가되면, 스테이지/몬스터 등의 게임 상태도 추가 할 것.
            });
            if (response.errorCode != ErrorCode.None)
            {
                _logger.ZLogErrorWithPayload(LogEventId.Login, new { userId = request.userId , errorCode = response.errorCode }, "Account Session write FAIL");
                return response;
            }
            response.token = token;

            // 2. 유저 장비 정보를 가져옴
            (response.errorCode, var userItems) = await _gameDb.GetUserItemList(userAccountTuple.pk_id.Value);
            if (response.errorCode != ErrorCode.None)
            {
                response.errorCode = ErrorCode.ServerError;
                _logger.ZLogErrorWithPayload(LogEventId.Login, new { userId = request.userId, errorCode = response.errorCode }, "Loginlog GetUserItemList FAIL");
                return response;
            }
            response.userItems = userItems;
            _logger.ZLogInformationWithPayload(LogEventId.Login, new { userId = request.userId}, "Login SUCCESS");
            return response;
        }
    }
}
