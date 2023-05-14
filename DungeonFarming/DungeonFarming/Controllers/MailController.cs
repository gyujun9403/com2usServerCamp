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
        //readonly Int64 _userId;
        readonly GameSessionData _gameSessionData;
        public MailController(IHttpContextAccessor httpContextAccessor, IConfiguration config, ILogger<MailController> logger, IGameDb gameDb)
        {
            _logger = logger;
            _gameDb = gameDb;
            _mailsPerPage = config.GetSection("GameConfigs").GetValue<Int16>("Mails_per_Page");
            //_userId = (long)HttpContext.Items["userId"];
            //_userId = httpContextAccessor.HttpContext.Items["userId"] as long? ?? -1;
            _gameSessionData = httpContextAccessor.HttpContext.Items["gameSessionData"] as GameSessionData;
        }

        [HttpPost("Preview")]
        public async Task<MailPreviewResponse> GetMailPreview(MailPreviewRequest request)
        {
            MailPreviewResponse response = new MailPreviewResponse();
            if (request.page < 0)
            {
                response.errorCode = ErrorCode.InvalidMailPage;
                // 정해진 인터페이스대로 동작하는 게임이 음수 페이지를 요청할 일은 없을 것이다.
                _logger.ZLogErrorWithPayload(LogEventId.Mail, new { userId = request.userId, page = request.page }, "GetMailPreview invalid page");
                return response;
            }
            (response.errorCode, var mailPreviewList) = await _gameDb.GetMailPreviewList(_gameSessionData.userId, request.page * _mailsPerPage, _mailsPerPage);
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

        [HttpPost("GetMail")]
        public async Task<GetMailResponse> GetMail(GetMailRequest request)
        {
            GetMailResponse response = new GetMailResponse();
            if (request.mailId < 0)
            {
                response.errorCode = ErrorCode.InvalidMailId;
                _logger.ZLogErrorWithPayload(LogEventId.Mail, new { userId = request.userId, mailId = request.mailId }, "GetMail invalid mailId");
                return response;
            }
            (response.errorCode, response.mail) = await _gameDb.GetMail(_gameSessionData.userId, request.mailId);
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
            if (request.mailId < 0)
            {
                response.errorCode = ErrorCode.InvalidMailId;
                _logger.ZLogErrorWithPayload(LogEventId.Mail, 
                    new { userId = request.userId, mailId = request.mailId }, 
                    "RecvMailItems invalid mailId");
                return response;
            }
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
            if (request.mailId < 0)
            {
                response.errorCode = ErrorCode.InvalidMailId;
                _logger.ZLogErrorWithPayload(LogEventId.Mail,
                    new { userId = request.userId, mailId = request.mailId },
                    "DeleteMail invalid mailId");
                return response;
            }
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
