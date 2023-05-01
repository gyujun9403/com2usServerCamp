using DungeonFarming.DataBase.AccountDb;
using DungeonFarming.DataBase.GameDb;
using DungeonFarming.DataBase.GameSessionDb;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace DungeonFarming.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MailController : ControllerBase
    {
        readonly Int16 _mailsPerPage;
        readonly ILogger<MailController> _logger;
        readonly IGameDb _gameDb;
        readonly IGameSessionDb _gameSessionDb;

        public MailController(IConfiguration config, ILogger<MailController> logger, IGameDb gameDb, IGameSessionDb gameSessionDb)
        {
            _logger = logger;
            _gameDb = gameDb;
            _gameSessionDb = gameSessionDb;
            _mailsPerPage = config.GetSection("GameConfigs").GetValue<short>("Mails_per_Page");
        }

        [HttpPost]
        public async Task<MailResponse> Mail(MailRequest request)
        {
            MailResponse response = new MailResponse();
            var (errorCodeSession, sessionData) = await _gameSessionDb.GetUserInfoSession(request.userId);
            if (errorCodeSession != ErrorCode.None || sessionData == null)
            {
                // TODO : Logger
                response.errorCode = ErrorCode.GameDbError;
                return response;
            }
            // TODO: 우편개수 별도로 define하기.
            var (errorCodeGame, mailDatas) = await _gameDb.GetMails(sessionData.pkId, request.page * _mailsPerPage, _mailsPerPage);
            if (errorCodeGame != ErrorCode.None || mailDatas == null)
            {
                // TODO : Logger
                response.errorCode = ErrorCode.GameDbError;
                return response;
            }
            response.errorCode = ErrorCode.None;
            response.mailDataList = mailDatas;
            return response;
        }
    }
}
