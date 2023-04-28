using DungeonFarming.BodyData;
using DungeonFarming.DataBase.AccountDb;
using DungeonFarming.DataBase.GameSessionDb;
using DungeonFarming.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DungeonFarming.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        readonly IAccountDb _accountDb;
        readonly IGameSessionDb _gameSessionDb;
        public LoginController(IGameSessionDb gameSessionDb, IAccountDb accountDb)
        {
            _accountDb = accountDb;
            _gameSessionDb = gameSessionDb;
        }
        // POST: api/Login
        [HttpPost]
        public async Task<LoginResponse> Login(LoginRequest request)
        {
            LoginResponse response = new LoginResponse();
            (ErrorCode errorCode, UserAccountsTuple? userAccountTuple) rt = await _accountDb.GetAccountInfo(request.user_id);
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

            if (Security.VerifyHashedPassword(request.password, rt.userAccountTuple.salt,
                    rt.userAccountTuple.hashed_password) == false)
            {
                response.errorCode = ErrorCode.WorngPassword;
                return response;
            }

            String token = Security.GenerateToken();
            response.errorCode = await _gameSessionDb.SetUserInfoSession(new UserInfoSessionData
            {
                user_id = request.user_id,
                token = token
            });
            if (response.errorCode != ErrorCode.None)
            {
                // 토큰 입력 실패시 account db에서 유저 정보 삭제 하는 기능 필요할듯?
            }
            response.token = token;

            return response;
        }
    }
}
