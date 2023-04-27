using DungeonFarming.BodyData;
using DungeonFarming.DataBase.AccountDb;
using DungeonFarming.DataBase.GameSessionDb;
using DungeonFarming.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DungeonFarming.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        IAccountDb _accountDb;
        IGameSessionDb _gameSessionDb;
        LoginController(IGameSessionDb gameSessionDb, IAccountDb accountDb)
        {
            _accountDb = accountDb;
            _gameSessionDb = gameSessionDb;
        }
        // POST: api/Login
        [HttpPost]
        public async Task<LoginResData> Post(LoginReqBodyData body)
        {
            LoginResData loginResData = new LoginResData();
            (ErrorCode, AccountDbModel?) rt = await _accountDb.GetAccountInfo(body.Account_id);
            if (rt.Item1 != ErrorCode.ErrorNone)
            {
                loginResData.errorCode = rt.Item1;
                return loginResData;
            }
            loginResData.errorCode = await _gameSessionDb.setToken(new AuthCheckModel
            {
                account_id = body.Account_id,
                token = Security.GenerateToken(body.Account_id)
            });
            if (loginResData.errorCode != ErrorCode.ErrorNone)
            {
                // 토큰 입력 실패시 account db에서 유저 정보 삭제 하는 기능 필요할듯?
            }
            return loginResData;
        }
    }
}
