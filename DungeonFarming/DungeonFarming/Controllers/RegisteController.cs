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
        public async Task<RegisterResponse> Registration(RegisteRequest request)
        {
            RegisterResponse response = new RegisterResponse();
            (byte[] saltBytes, byte[] hashedPasswordBytes) rt = Security.GetSaltAndHashedPassword(request.password);
            response.errorCode = await _accountDb.RegisteUser(new UserAccountsTuple
            {
                pk_id = null,
                user_id = request.userId,
                salt = rt.saltBytes,
                hashed_password = rt.hashedPasswordBytes
            });
            if (response.errorCode == ErrorCode.None)
            {
                _logger.ZLogInformation($"[Registration] Info : {request.userId} - Regist");
            }
            else
            {
                _logger.ZLogError($"[Registration] Error : {request.userId} - {response.errorCode}");
            }

            return response;
        }
    }
}
