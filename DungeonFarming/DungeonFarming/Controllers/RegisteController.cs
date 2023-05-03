using DungeonFarming.DataBase.AccountDb;
using DungeonFarming.DataBase.GameDb;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using SqlKata.Execution;
using ZLogger;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DungeonFarming.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RegisteController : ControllerBase
    {
        readonly IAccountDb _accountDb;
        readonly IGameDb _gameDb;
        readonly ILogger<RegisteController> _logger;
        public RegisteController(IAccountDb accountDb, IGameDb gameDb, ILogger<RegisteController> logger)
        {
            _accountDb = accountDb;
            _gameDb = gameDb;
            _logger = logger;
        }

        [HttpPost()]
        public async Task<RegisterResponse> Registration(RegisteRequest request)
        {
            RegisterResponse response = new RegisterResponse();
            (byte[] saltBytes, byte[] hashedPasswordBytes) rt = Security.GetSaltAndHashedPassword(request.password);
            var (errorCode, pkId) = await _accountDb.RegisteUser(new UserAccountsTuple
            {
                pk_id = null,
                user_id = request.userId,
                salt = rt.saltBytes,
                hashed_password = rt.hashedPasswordBytes
            });
            response.errorCode = errorCode;
            if (response.errorCode != ErrorCode.None)
            {
                _logger.ZLogInformation($"[Registration] Info : {request.userId} - Regist");
                return response;
            }

            // 0. GameDb에 유저를 등록한다.
            if (await _gameDb.RegistUserInGame(pkId) != ErrorCode.None)
            {
                _logger.ZLogError($"[Registration] Error : {request.userId} - RegistUserInGame");
                response.errorCode = ErrorCode.ServerError;
                return response;
            }

            // 1. 기본 지급 아이템 목록을 가져온다
            // TODO: 0을 상수화 해서 사용 -> config에서 읽어오게 한다던가..
            var (ItemListErrorCode, itemList) = await _gameDb.GetDefaultItemBundle(0);
            if (ItemListErrorCode != ErrorCode.None || itemList == null)
            {
                _logger.ZLogError($"[Registration] Error : {request.userId} - GetDefaultItemList");
                response.errorCode = ErrorCode.ServerError;
                return response;
            }
            // 2. 유저의 인벤토리에 기본 지급 아이템들을 지급한다.
            if (await _gameDb.SetUserItemsByItemBundles(pkId, itemList) != ErrorCode.None)
            {
                _logger.ZLogError($"[Registration] Error : {request.userId} - SetItemListInUserInventory");
                response.errorCode = ErrorCode.ServerError;
                return response;
            }
            _logger.ZLogError($"[Registration] Error : {request.userId} - {response.errorCode}");
            return response;
        }
    }
}
