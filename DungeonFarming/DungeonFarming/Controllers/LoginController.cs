using DungeonFarming.DataBase.AccountDb;
using DungeonFarming.DataBase.GameDb;
using DungeonFarming.DataBase.GameDb.GameUserDataORM;
using DungeonFarming.DataBase.GameSessionDb;
using Microsoft.AspNetCore.Mvc;

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
        readonly String _clientVersion;
        readonly String _masterDataVersion;
        public LoginController(IConfiguration config, IGameSessionDb gameSessionDb, IGameDb gameDb, IAccountDb accountDb)
        {
            _accountDb = accountDb;
            _gameSessionDb = gameSessionDb;
            _gameDb = gameDb;
            _clientVersion = config.GetSection("Versions")["Client"];
            _masterDataVersion = config.GetSection("Versions")["Master_Data"];

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
            response.errorCode = await _gameDb.UpdateUserConnectDate(rt.userAccountTuple.pk_id.Value);
            if (response.errorCode != ErrorCode.None) 
            {
                // TODO: logger
                response.errorCode = ErrorCode.ServerError;
                return response;
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
