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

        Mail ReGenerateLoginRewardMail(LoginLog log, List<ItemBundle>? itemBundle) 
        {
            var mail = new Mail();
            mail.user_id = log.user_id;
            if (itemBundle != null && itemBundle.Count > 0)
            {
                mail.item0_code = itemBundle[0].itemCode;
                mail.item0_count = itemBundle[0].itemCount;
                if (itemBundle.Count > 1)
                {
                    mail.item1_code = itemBundle[1].itemCode;
                    mail.item1_count = itemBundle[1].itemCount;
                    if (itemBundle.Count > 2)
                    {
                        mail.item2_code = itemBundle[2].itemCode;
                        mail.item2_count = itemBundle[2].itemCount;
                    }
                    if (itemBundle.Count > 3)
                    {
                        mail.item3_code = itemBundle[3].itemCode;
                        mail.item3_count = itemBundle[3].itemCount;
                    }
                }
            }
            mail.mail_title = $"로그인 보상 {log.consecutive_login_count}일 차";
            mail.mail_text = $"로그인 보상 {log.consecutive_login_count}일 차!!!";
            mail.recieve_date = DateTime.Now;
            mail.expiration_date = DateTime.Now.AddDays(15);
            return mail;
        }
        // POST: api/Login
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
                userId = request.userId,
                pkId = userAccountTuple.pk_id.Value,
                token = token,
                userStatus = UserStatus.Login
                // TODO: 게임 기능이 추가되면, 스테이지/몬스터 등의 게임 상태도 추가 할 것.
            });
            if (response.errorCode != ErrorCode.None)
            {
                // TODO: 토큰 입력 실패시 account db에서 유저 정보 삭제 하는 기능 필요할듯?
                _logger.ZLogErrorWithPayload(LogEventId.Login, new { userId = request.userId , errorCode = response.errorCode }, "Account Session write FAIL");
            }
            response.token = token;

            // 게임 DB
            // 1. 유저 접속 기록을 갱신
            // 만약 기록이 없는 경우 새로 생성?
            (response.errorCode, var loginLog) = await _gameDb.UpdateAndGetLoginLog(userAccountTuple.pk_id.Value);
            if (response.errorCode != ErrorCode.None && response.errorCode != ErrorCode.AreadyLogin) 
            {
                _logger.ZLogErrorWithPayload(LogEventId.Login, new { userId = request.userId, errorCode = response.errorCode }, "Loginlog Update and Get FAIL");
                return response;
            }
            else if (response.errorCode == ErrorCode.AreadyLogin)
            {
                _logger.ZLogInformationWithPayload(LogEventId.Login, new { userId = request.userId, errorCode = response.errorCode }, "User Login request AGAIN");
            }
            if (loginLog == null)
            {
                response.errorCode = ErrorCode.ServerError;
                _logger.ZLogErrorWithPayload(LogEventId.Login, new { userId = request.userId, errorCode = response.errorCode }, "Loginlog is NULL");
                return response;
            }
            // 로그인 보너스 지급
            if (response.errorCode == ErrorCode.None)
            {
                List<ItemBundle>? reward = _masterDataOffer.getDailyLoginRewardItemBundles(loginLog.consecutive_login_count);
                response.errorCode = await _gameDb.SendMail(ReGenerateLoginRewardMail(loginLog, reward));
                if (response.errorCode != ErrorCode.None)
                {
                    _logger.ZLogErrorWithPayload(LogEventId.Login, new { userId = request.userId, loginLog = loginLog, reward = reward, errorCode = response.errorCode }, "Loginlog SendMail FAIL");
                    return response; ;
                }
            }
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
