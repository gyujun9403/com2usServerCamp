using DungeonFarming.BodyData;
using DungeonFarming.DataBase.AccountDb;
using DungeonFarming.DataBase.GameSessionDb;
using DungeonFarming.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZLogger;

namespace DungeonFarming.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DeleteAccountController : ControllerBase
    {
        IAccountDb _accountDb;
        IGameSessionDb _gameSessionDb;
        ILogger<DeleteAccountController> _logger;
        
        public DeleteAccountController(IAccountDb accountDb, IGameSessionDb gameSessionDb, ILogger<DeleteAccountController> logger)
        {
            _accountDb = accountDb;
            _gameSessionDb = gameSessionDb;
            _logger = logger;
        }

        // POST: DeleteAccount
        [HttpPost]
        public async Task<DeleteAccountResData> Post(DeleteAccountBodyData body)
        {
            DeleteAccountResData deleteAccountResData = new DeleteAccountResData();
            deleteAccountResData.errorCode = await _gameSessionDb.deleteToken(body.user_id);
            if (deleteAccountResData.errorCode != ErrorCode.ErrorNone)
            {
                _logger.ZLogError($"[DeleteAccount] Error : {body.user_id} - {deleteAccountResData.errorCode}");
                return deleteAccountResData;
            }
            deleteAccountResData.errorCode = await _accountDb.DeleteAccount(body.user_id);
            if (deleteAccountResData.errorCode != ErrorCode.ErrorNone)
            {
                _logger.ZLogError($"[DeleteAccount] Error : {body.user_id} - {deleteAccountResData.errorCode}");
                return deleteAccountResData;
            }

            _logger.ZLogInformation($"[DeleteAccount] Info : {body.user_id} - DeleteAccount");
            return deleteAccountResData;
        }
    }
}
