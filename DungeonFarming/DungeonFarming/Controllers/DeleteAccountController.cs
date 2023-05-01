using DungeonFarming.DataBase.AccountDb;
using DungeonFarming.DataBase.GameSessionDb;
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
        public async Task<DeleteAccountResponse> DeleteAccount(DeleteAccountRequest request)
        {
            DeleteAccountResponse response = new DeleteAccountResponse();
            response.errorCode = await _gameSessionDb.DeleteUserInfoSession(request.userId);
            if (response.errorCode != ErrorCode.None)
            {
                _logger.ZLogError($"[DeleteAccount] Error : {request.userId} - {response.errorCode}");
                return response;
            }

            response.errorCode = await _accountDb.DeleteAccount(request.userId);
            if (response.errorCode != ErrorCode.None)
            {
                _logger.ZLogError($"[DeleteAccount] Error : {request.userId} - {response.errorCode}");
                return response;
            }

            _logger.ZLogInformation($"[DeleteAccount] Info : {request.userId} - DeleteAccount");
            return response;
        }
    }
}
