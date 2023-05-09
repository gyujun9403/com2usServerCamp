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
        public MailController(IConfiguration config, ILogger<MailController> logger, IGameDb gameDb)
        {
            _logger = logger;
            _gameDb = gameDb;
            _mailsPerPage = config.GetSection("GameConfigs").GetValue<Int16>("Mails_per_Page");
        }

        public Int64 GetUserPkId()
        {
            Int64 userPkId = -1;
            if (HttpContext.Request.Headers.TryGetValue("UserPkId", out var userPkIdStr))
            {
                if (long.TryParse(userPkIdStr, out userPkId) == false)
                {
                    return -1;
                }
            }
            return userPkId;
        }

        [HttpPost("Preview")]
        public async Task<MailPreviewResponse> GetMailPreview(MailPreviewRequest request)
        {
            MailPreviewResponse response = new MailPreviewResponse();
            var userPkId = GetUserPkId();
            if (userPkId < 0)
            {
                response.errorCode = ErrorCode.ServerError;
                _logger.ZLogErrorWithPayload(LogEventId.Mail, new { userId = request.userId }, "GetMailPreview pk id get FAIL");
                return response;
            }
            if (request.page < 0)
            {
                response.errorCode = ErrorCode.InvalidMailPage;
                // 정해진 인터페이스대로 동작하는 게임이 음수 페이지를 요청할 일은 없을 것이다.
                _logger.ZLogErrorWithPayload(LogEventId.Mail, new { userId = request.userId, page = request.page }, "GetMailPreview invalid page");
                return response;
            }
            (response.errorCode, var mailPreviewList) = await _gameDb.GetMailPreviewList(userPkId, request.page * _mailsPerPage, _mailsPerPage);
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
            var userPkId = GetUserPkId();
            if (userPkId < 0)
            {
                response.errorCode = ErrorCode.ServerError;
                _logger.ZLogErrorWithPayload(LogEventId.Mail, new { userId = request.userId }, "GetMail pk id get FAIL");
                return response;
            }
            if (request.mailId < 0)
            {
                response.errorCode = ErrorCode.InvalidMailId;
                _logger.ZLogErrorWithPayload(LogEventId.Mail, new { userId = request.userId, mailId = request.mailId }, "GetMail invalid mailId");
                return response;
            }
            (response.errorCode, response.mail) = await _gameDb.GetMail(userPkId, request.mailId);
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
            var userPkId = GetUserPkId();
            if (userPkId < 0)
            {
                response.errorCode = ErrorCode.ServerError;
                _logger.ZLogErrorWithPayload(LogEventId.Mail, new { userId = request.userId }, "RecvMailItems pk id get FAIL");
                return response;
            }
            if (request.mailId < 0)
            {
                response.errorCode = ErrorCode.InvalidMailId;
                _logger.ZLogErrorWithPayload(LogEventId.Mail, 
                    new { userId = request.userId, mailId = request.mailId }, 
                    "RecvMailItems invalid mailId");
                return response;
            }
            response.errorCode = await _gameDb.RecvMailItems(userPkId, request.mailId);
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
            var userPkId = GetUserPkId();
            if (userPkId < 0)
            {
                response.errorCode = ErrorCode.ServerError;
                _logger.ZLogErrorWithPayload(LogEventId.Mail, new { userId = request.userId }, "DeleteMail pk id get FAIL");
                return response;
            }
            if (request.mailId < 0)
            {
                response.errorCode = ErrorCode.InvalidMailId;
                _logger.ZLogErrorWithPayload(LogEventId.Mail,
                    new { userId = request.userId, mailId = request.mailId },
                    "DeleteMail invalid mailId");
                return response;
            }
            response.errorCode = await _gameDb.DeleteMail(userPkId, request.mailId);
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
