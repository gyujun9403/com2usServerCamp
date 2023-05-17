using DungeonFarming.DataBase.AccountDb;
using DungeonFarming.DataBase.GameDb;
using DungeonFarming.DataBase.GameSessionDb;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ZLogger;

namespace DungeonFarming.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MailController : ControllerBase
    {
        readonly Int16 _mailsPerPage;
        readonly ILogger<MailController> _logger;
        readonly IGameDb _gameDb;
        readonly GameSessionData _gameSessionData;

        public MailController(IHttpContextAccessor httpContextAccessor, IConfiguration config, ILogger<MailController> logger, IGameDb gameDb)
        {
            _logger = logger;
            _gameDb = gameDb;
            _mailsPerPage = config.GetSection("GameConfigs").GetValue<Int16>("Mails_per_Page");
            _gameSessionData = httpContextAccessor.HttpContext.Items["gameSessionData"] as GameSessionData;
        }

        [HttpPost("ListUpMail")]
        public async Task<MailPreviewResponse> ListUpMail(MailPreviewRequest request)
        {
            MailPreviewResponse response = new MailPreviewResponse();

            (response.errorCode, var mailPreviewList) = await _gameDb.GetMailListUp(_gameSessionData.userId, request.page * _mailsPerPage, _mailsPerPage);
            if ((response.errorCode != ErrorCode.None && response.errorCode != ErrorCode.NoMail)
                || response.errorCode != ErrorCode.NoMail && mailPreviewList == null)
            {
                _logger.ZLogErrorWithPayload(LogEventId.Mail,
                    new {
                        errorCode = response.errorCode,
                        userId = request.userId,
                        startIndex = request.page * _mailsPerPage,
                        mailCount = _mailsPerPage,
                        mailPreviewList = mailPreviewList },
                    "GetMailPreview mail preview list get FAIL");
                return response;
            }
            response.mailDataList = mailPreviewList;
            return response;
        }

        [HttpPost("OpenMail")]
        public async Task<GetMailResponse> OpenMail(GetMailRequest request)
        {
            GetMailResponse response = new GetMailResponse();

            (response.errorCode, response.mail) = await _gameDb.OpenMail(_gameSessionData.userId, request.mailId);
            if (response.errorCode != ErrorCode.None && response.errorCode != ErrorCode.NoMail)
            {
                _logger.ZLogErrorWithPayload(LogEventId.Mail,
                    new { errorCode = response.errorCode, userId = request.userId, mailId = request.mailId, mail = response.mail }, 
                    "mail get FAIL");
                return response;
            }
            _logger.ZLogInformationWithPayload( LogEventId.Mail,
                        new { userId = request.userId, mailId = request.mailId },
                        "GetMail mail get SUCCESS");

            return response;
        }

        [HttpPost("RecvMailItems")]
        public async Task<RecvMailItemsResponse> RecvMailItems(RecvMailItemsRequest request)
        {
            RecvMailItemsResponse response = new RecvMailItemsResponse();

            response.errorCode = await _gameDb.RecvMailItems(_gameSessionData.userId, request.mailId);
            if (response.errorCode != ErrorCode.None && response.errorCode != ErrorCode.Noitems)
            {
                _logger.ZLogErrorWithPayload(LogEventId.Mail, 
                    new { errorCode = response.errorCode, userId = request.userId, mailId = request.mailId }, 
                    "RecvMailItems mail get FAIL");
                return response;
            }
            return response;
        }

        [HttpPost("DeleteMail")]
        public async Task<DeleteMailResponse> DeleteMail(DeleteMailRequest request)
        {
            DeleteMailResponse response = new DeleteMailResponse();

            response.errorCode = await _gameDb.DeleteMail(_gameSessionData.userId, request.mailId);
            if (response.errorCode != ErrorCode.None && response.errorCode != ErrorCode.NoMail)
            {
                _logger.ZLogErrorWithPayload(LogEventId.Mail,
                    new { errorCode = response.errorCode, userId = request.userId, mailId = request.mailId },
                    "mail get FAIL");
                return response;
            }
            return response;
        }
    }
}
