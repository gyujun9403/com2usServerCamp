using DungeonFarming.DataBase.AccountDb;
using DungeonFarming.DataBase.GameDb;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using SqlKata.Execution;
using ZLogger;

namespace DungeonFarming.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RegistController : ControllerBase
    {
        readonly IAccountDb _accountDb;
        readonly IGameDb _gameDb;
        readonly IMasterDataOffer _masterDataOffer;
        readonly ILogger<RegistController> _logger;
        public RegistController(IAccountDb accountDb, IGameDb gameDb, IMasterDataOffer masterDataOffer, ILogger<RegistController> logger)
        {
            _accountDb = accountDb;
            _gameDb = gameDb;
            _masterDataOffer = masterDataOffer;
            _logger = logger;
        }

        //TODO:void RollBackUserAccount

        [HttpPost()]
        public async Task<RegisterResponse> Registration(RegisteRequest request)
        {
            RegisterResponse response = new RegisterResponse();
            (byte[] saltBytes, byte[] hashedPasswordBytes) rt = Security.GetSaltAndHashedPassword(request.password);
            var (errorCode, pkId) = await _accountDb.RegisteUser(new UserAccountDto
            {
                pk_id = null,
                user_id = request.userId,
                salt = rt.saltBytes,
                hashed_password = rt.hashedPasswordBytes
            });
            response.errorCode = errorCode;
            if (response.errorCode != ErrorCode.None)
            {
                _logger.ZLogErrorWithPayload(LogEventId.Regist, new { userId = request.userId , ErrorCode = response.errorCode }, "Registration FAIL");
                return response;
            }

            // 0. GameDb에 유저를 등록한다.
            if (await _gameDb.RegistGameUser(pkId) != ErrorCode.None)
            {
                //_logger.ZLogError($"[Registration] Error : {request.userId} - RegistUserInGame");
                _logger.ZLogErrorWithPayload(LogEventId.Regist, new { userId = request.userId }, "GameDb regist FAIL");
                response.errorCode = ErrorCode.ServerError;
                return response;
            }

            // 1. 기본 지급 아이템 목록을 가져온다
            // TODO: 0을 상수화 해서 사용 -> config에서 읽어오게 한다던가..
            var itemBundle = _masterDataOffer.getDefaultItemBundles(0);
            if (itemBundle == null)
            {
                _logger.ZLogErrorWithPayload(LogEventId.Regist, new { listCode = 0 }, "default item bundle load FAIL");
                response.errorCode = ErrorCode.ServerError;
                return response;
            }
            // 2. 유저의 인벤토리에 기본 지급 아이템들을 지급한다.
            if (await _gameDb.InsertUserItemsByItemBundles(pkId, itemBundle) != ErrorCode.None)
            {
                _logger.ZLogErrorWithPayload(LogEventId.Regist, new { userId = request.userId }, "User Regist Item setting FAIL");
                response.errorCode = ErrorCode.ServerError;
                return response;
            }
            _logger.ZLogInformationWithPayload(LogEventId.Regist, new { userId = request.userId }, "User Regist SUCCESS");
            return response;
        }
    }
}
