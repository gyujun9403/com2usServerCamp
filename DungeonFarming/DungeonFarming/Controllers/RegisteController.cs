using DungeonFarming.BodyData;
using DungeonFarming.DataBase.AccountDb;
using DungeonFarming.DTOs;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using SqlKata.Execution;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DungeonFarming.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RegisteController : ControllerBase
    {
        IAccountDb _accountDb;
        public RegisteController(IAccountDb accountDb)
        {
            _accountDb = accountDb;
        }

        [HttpPost()]
        public async Task<RegisterResData> Registration(RegisteReqBodyData bodyData)
        {
            RegisterResData registerResData = new RegisterResData();
            byte[] saltBytes, hashedPasswordBytes;
            Security.Hashing(bodyData.Password, out saltBytes, out hashedPasswordBytes);
            registerResData.errorCode = await _accountDb.RegisteUser(new AccountDbModel
            {
                pk_id = null,
                account_id = bodyData.Account_id,
                salt = saltBytes,
                hashed_password = hashedPasswordBytes
            });
            return registerResData;
        }

        //[HttpPost("login")]
        //public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        //{
        //    return Ok();
        //}

        //[HttpDelete("delete")]
        //public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountDTO dto)
        //{
        //    return Ok();
        //}
    }
}
