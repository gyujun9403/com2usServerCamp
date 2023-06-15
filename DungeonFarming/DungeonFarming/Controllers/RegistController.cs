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

        [HttpPost]
        public async Task<RegisterResponse> Registration(RegisteRequest request)
        {
            RegisterResponse response = new RegisterResponse();

            var (saltBytes, hashedPasswordBytes) = Security.GetSaltAndHashedPassword(request.password);
            (response.errorCode, var userId) = await _accountDb.RegisteUser(new UserAccountDto
                {
                    user_id = null,
                    user_assigned_id = request.userAssignedId,
                    salt = saltBytes,
                    hashed_password = hashedPasswordBytes
                });
            if (response.errorCode != ErrorCode.None)
            {
                _logger.ZLogErrorWithPayload(LogEventId.Regist, new { userAssignedId = request.userAssignedId , ErrorCode = response.errorCode }, "Registration FAIL");
                return response;
            }
            else if (response.errorCode == ErrorCode.DuplicatedId)
            {
                return response;
            }

            // 0. GameDb에 유저를 등록한다.
            response.errorCode = await _gameDb.RegisUserLoginLog(userId);
            if (response.errorCode != ErrorCode.None)
            {
                await _accountDb.DeleteAccount(request.userAssignedId);
                if (response.errorCode != ErrorCode.DuplicatedId)
                {
                    response.errorCode = ErrorCode.ServerError;
                }
                return response;
            }

            // 0. 유저의 게임 정보 세팅
            response.errorCode = await _gameDb.RegistUserAchivement(userId);
            if (response.errorCode != ErrorCode.None)
            {
                await _accountDb.DeleteAccount(request.userAssignedId);
                if (response.errorCode != ErrorCode.DuplicatedId)
                {
                    response.errorCode = ErrorCode.ServerError;
                }
                return response;
            }

            // 1. 기본 지급 아이템 목록을 가져온다
            // TODO: 0을 상수화 해서 사용 -> config에서 읽어오게 한다던가..
            var itemBundle = _masterDataOffer.getDefaultItemBundles(0);
            if (itemBundle == null)
            {
                await _accountDb.DeleteAccount(request.userAssignedId);
                await _gameDb.DeleteLoginLog(userId);
                response.errorCode = ErrorCode.ServerError;
                _logger.ZLogErrorWithPayload(LogEventId.Regist, new { userId = userId }, "getDefaultItemBundles Fail");
                return response;
            }

            // 2. 유저의 인벤토리에 기본 지급 아이템들을 지급한다.
            //지급 실패시 GameDb에 유저 제거
            var (rtErrorCode, insertedkey) = await _gameDb.InsertUserItemsByItemBundles(userId, itemBundle);
            if (rtErrorCode != ErrorCode.None)
            {
                await _accountDb.DeleteAccount(request.userAssignedId);
                await _gameDb.DeleteLoginLog(userId);
                response.errorCode = ErrorCode.ServerError;
                return response;
            }

            _logger.ZLogInformationWithPayload(LogEventId.Regist, new { userAssignedId = request.userAssignedId }, "User Regist SUCCESS");
            return response;
        }
    }
}
