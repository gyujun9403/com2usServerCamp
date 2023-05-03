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
            _mailsPerPage = config.GetSection("GameConfigs").GetValue<Int16>("Mails_per_Page");
        }
       
        [HttpPost("Preview")]
        public async Task<MailPreviewResponse> GetMailPreviet(MailPreviewRequest request)
        {
            MailPreviewResponse response = new MailPreviewResponse();
            if (request.page < 0)
            {
                response.errorCode = ErrorCode.InvalidMailPage;
                return response;
            }
            var (errorCodeSession, sessionData) = await _gameSessionDb.GetUserInfoSession(request.userId);
            if (errorCodeSession != ErrorCode.None)
            {
                // TODO : Logger
                response.errorCode = errorCodeSession;
                return response;
            }
            else if (sessionData == null)
            {
                // TODO : Logger
                response.errorCode = ErrorCode.GameDbError;
                return response;
            }
            var (errorCode, mailPreviewList) = await _gameDb.GetMailPreviewList(sessionData.pkId, request.page * _mailsPerPage, _mailsPerPage);
            if (errorCode != ErrorCode.None)
            {
                // TODO : Logger
                response.errorCode = errorCode;
                return response;
            }
            else if (mailPreviewList == null)
            {
                // TODO : Logger
                response.errorCode = ErrorCode.GameDbError;
                return response;
            }
            response.errorCode = ErrorCode.None;
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
                return response;
            }
            var (errorCodeSession, sessionData) = await _gameSessionDb.GetUserInfoSession(request.userId);
            if (errorCodeSession != ErrorCode.None)
            {
                // TODO : Logger
                response.errorCode = errorCodeSession;
                return response;
            }
            else if (sessionData == null)
            {
                // TODO : Logger
                response.errorCode = ErrorCode.GameDbError;
                return response;
            }
            // TODO : mailId가 유저의 메일인지 확인
            (response.errorCode, response.mail) = await _gameDb.GetMail(sessionData.pkId, request.mailId);
            return response;
        }

        [HttpPost("RecvMailItems")]
        public async Task<RecvMailItemsResponse> RecvMailItems(RecvMailItemsRequest request)
        {
            RecvMailItemsResponse response = new RecvMailItemsResponse();
            if (request.mailId < 0)
            {
                response.errorCode = ErrorCode.InvalidMailId;
                return response;
            }
            var (errorCodeSession, sessionData) = await _gameSessionDb.GetUserInfoSession(request.userId);
            if (errorCodeSession != ErrorCode.None)
            {
                // TODO : Logger
                response.errorCode = errorCodeSession;
                return response;
            }
            else if (sessionData == null)
            {
                // TODO : Logger
                response.errorCode = ErrorCode.GameDbError;
                return response;
            }
            response.errorCode = await _gameDb.RecvMailItem(sessionData.pkId, request.mailId);
            return response;
        }

        [HttpPost("DeleteMail")]
        public async Task<DeleteMailResponse> DeleteMail(DeleteMailRequest request)
        {
            DeleteMailResponse response = new DeleteMailResponse();
            if (request.mailId < 0)
            {
                response.errorCode = ErrorCode.InvalidMailId;
                return response;
            }
            var (errorCodeSession, sessionData) = await _gameSessionDb.GetUserInfoSession(request.userId);
            if (errorCodeSession != ErrorCode.None)
            {
                // TODO : Logger
                response.errorCode = errorCodeSession;
                return response;
            }
            else if (sessionData == null)
            {
                // TODO : Logger
                response.errorCode = ErrorCode.GameDbError;
                return response;
            }
            response.errorCode = await _gameDb.DeleteMail(sessionData.pkId, request.mailId);
            return response;
        }
    }
}
