using DungeonFarming.DataBase.AccountDb;
using DungeonFarming.DataBase.GameDb;
using DungeonFarming.DataBase.GameDb.GameUserDataORM;
using DungeonFarming.DataBase.GameSessionDb;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

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
        public LoginController(IConfiguration config, IGameSessionDb gameSessionDb, IGameDb gameDb, IAccountDb accountDb, IMasterDataOffer masterDataOffer)
        {
            _accountDb = accountDb;
            _gameSessionDb = gameSessionDb;
            _gameDb = gameDb;
            _masterDataOffer = masterDataOffer;
        }

        private Mail ReGenerateLoginwardMail(LoginLog log, List<ItemBundle>? itemBundle) 
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
            (ErrorCode errorCode, UserAccountsTuple? userAccountTuple) rt = await _accountDb.GetAccountInfo(request.userId);
            if (rt.errorCode != ErrorCode.None)
            {
                response.errorCode = rt.errorCode;
                // Todo:logger
                return response;
            }
            if (rt.userAccountTuple == null)
            {
                response.errorCode = ErrorCode.ServerError;
                // Todo:logger
                return response;
            }
            if (!rt.userAccountTuple.pk_id.HasValue)
            {
                response.errorCode = ErrorCode.ServerError;
                // Todo:logger
                return response;
            }

            // 비밀번호 확인
            if (Security.VerifyHashedPassword(request.password, rt.userAccountTuple.salt,
                    rt.userAccountTuple.hashed_password) == false)
            {
                response.errorCode = ErrorCode.WorngPassword;
                return response;
            }

            // 토큰 가져오고 검증.
            String token = Security.GenerateToken();
            response.errorCode = await _gameSessionDb.SetUserInfoSession(new GameSessionData
            {
                userId = request.userId,
                pkId = rt.userAccountTuple.pk_id.Value,
                token = token,
                userStatus = UserStatus.Login
                // TODO: 게임 기능이 추가되면, 스테이지/몬스터 등의 게임 상태도 추가 할 것.
            });
            if (response.errorCode != ErrorCode.None)
            {
                // TODO: 토큰 입력 실패시 account db에서 유저 정보 삭제 하는 기능 필요할듯?
            }
            response.token = token;

            // 게임 DB
            // 1. 유저 접속 기록을 갱신
            (response.errorCode, var loginLog) = await _gameDb.UpdateAndGetLoginLog(rt.userAccountTuple.pk_id.Value);
            if (response.errorCode != ErrorCode.None && response.errorCode != ErrorCode.AreadyLogin) 
            {
                // TODO: logger
                response.errorCode = ErrorCode.ServerError;
                return response;
            }
            if (loginLog == null)
            {
                // TODO: logger
                response.errorCode = ErrorCode.ServerError;
                return response;
            }
            // 로그인 보너스 지급
            if (response.errorCode == ErrorCode.None)
            {
                List<ItemBundle>? reward = _masterDataOffer.getDailyLoginRewardItemBundles(loginLog.consecutive_login_count);
                response.errorCode = await _gameDb.SendMail(ReGenerateLoginwardMail(loginLog, reward));
            }
            // 2. 유저 장비 정보를 가져옴
            var (errorCode, userItems) = await _gameDb.GetUserItemList(rt.userAccountTuple.pk_id.Value);
            if (errorCode != ErrorCode.None)
            {
                // TOOD : log
                response.errorCode = ErrorCode.ServerError;
            }
            response.userItems = userItems;
 
            // 3. 유저의 인벤토리 정보를 전송.
            //            List<UserItem> userItems
            return response;
        }
    }
}
