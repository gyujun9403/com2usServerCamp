using DungeonFarming.BodyData;
using DungeonFarming.DataBase.AccountDb;
using DungeonFarming.DTOs;
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
        readonly ILogger<RegisteController> _logger;
        public RegisteController(IAccountDb accountDb, ILogger<RegisteController> logger)
        {
            _accountDb = accountDb;
            _logger = logger;
        }

        [HttpPost()]
        public async Task<RegisterResData> Registration(RegisteReqBodyData bodyData)
        {
            RegisterResData registerResData = new RegisterResData();
            byte[] saltBytes, hashedPasswordBytes;
            Security.Hashing(bodyData.password, out saltBytes, out hashedPasswordBytes);
            registerResData.errorCode = await _accountDb.RegisteUser(new AccountDbModel
            {
                pk_id = null,
                user_id = bodyData.user_id,
                salt = saltBytes,
                hashed_password = hashedPasswordBytes
            });
            if (registerResData.errorCode == ErrorCode.ErrorNone)
            {
                _logger.ZLogInformation($"[Registration] Info : {bodyData.user_id} - Regist");
            }
            else
            {
                _logger.ZLogError($"[Registration] Error : {bodyData.user_id} - {registerResData.errorCode}");
            }
            return registerResData;
        }
    }
}
