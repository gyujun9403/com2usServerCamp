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
        readonly String _clientVersion;
        readonly String _masterDataVersion;
        public LoginController(IConfiguration config, IGameSessionDb gameSessionDb, IAccountDb accountDb)
        {
            _accountDb = accountDb;
            _gameSessionDb = gameSessionDb;
            _clientVersion = config.GetSection("Versions")["Client"];
            _masterDataVersion = config.GetSection("Versions")["Master_Data"];
        }
        // POST: api/Login
        [HttpPost]
        public async Task<LoginResponse> Login(LoginRequest request)
        {
            LoginResponse response = new LoginResponse();
            // 버전 정보 확인
            if (request.clientVersion != _clientVersion)
            {
                response.errorCode = ErrorCode.WorngClientVersion;
                return response;
            }
            if (request.masterDataVersion != _masterDataVersion)
            {
                response.errorCode = ErrorCode.WorngMasterDataVersion;
                return response;
            }

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
