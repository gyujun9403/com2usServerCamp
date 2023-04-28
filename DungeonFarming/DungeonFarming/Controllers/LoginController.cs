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
        IAccountDb _accountDb;
        IGameSessionDb _gameSessionDb;
        public LoginController(IGameSessionDb gameSessionDb, IAccountDb accountDb)
        {
            _accountDb = accountDb;
            _gameSessionDb = gameSessionDb;
        }
        // POST: api/Login
        [HttpPost]
        public async Task<LoginResData> Post(LoginReqBodyData body)
        {
            LoginResData loginResData = new LoginResData();
            (ErrorCode, AccountDbModel?) rt = await _accountDb.GetAccountInfo(body.user_id);
            if (rt.Item1 != ErrorCode.ErrorNone)
            {
                loginResData.errorCode = rt.Item1;
                return loginResData;
            }
            String token = Security.GenerateToken();
            loginResData.errorCode = await _gameSessionDb.setToken(new AuthCheckModel
            {
                user_id = body.user_id,
                token = token
            });
            if (loginResData.errorCode != ErrorCode.ErrorNone)
            {
                // 토큰 입력 실패시 account db에서 유저 정보 삭제 하는 기능 필요할듯?
            }
            loginResData.token = token;
            return loginResData;
        }
    }
}
