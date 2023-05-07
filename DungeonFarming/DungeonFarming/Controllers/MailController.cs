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
        public async Task<MailPreviewResponse> GetMailPreviet(MailPreviewRequest request)
        {
            MailPreviewResponse response = new MailPreviewResponse();
            var userPkId = GetUserPkId();
            if (userPkId < 0)
            {
                response.errorCode = ErrorCode.ServerError;
                return response;
            }
            if (request.page < 0)
            {
                response.errorCode = ErrorCode.InvalidMailPage;
                return response;
            }
            var (errorCode, mailPreviewList) = await _gameDb.GetMailPreviewList(userPkId, request.page * _mailsPerPage, _mailsPerPage);
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
            Int64 userPkId = -1;
            if (HttpContext.Request.Headers.TryGetValue("UserPkId", out var userPkIdStr))
            {
                if (long.TryParse(userPkIdStr, out userPkId) == false)
                {
                    response.errorCode = ErrorCode.GameDbError;
                    return response;
                }
            }
            if (request.mailId < 0)
            {
                response.errorCode = ErrorCode.InvalidMailId;
                return response;
            }
            (response.errorCode, response.mail) = await _gameDb.GetMail(userPkId, request.mailId);
            return response;
        }

        [HttpPost("RecvMailItems")]
        public async Task<RecvMailItemsResponse> RecvMailItems(RecvMailItemsRequest request)
        {
            RecvMailItemsResponse response = new RecvMailItemsResponse();
            Int64 userPkId = -1;
            if (HttpContext.Request.Headers.TryGetValue("UserPkId", out var userPkIdStr))
            {
                if (long.TryParse(userPkIdStr, out userPkId) == false)
                {
                    response.errorCode = ErrorCode.GameDbError;
                    return response;
                }
            }
            if (request.mailId < 0)
            {
                response.errorCode = ErrorCode.InvalidMailId;
                return response;
            }
            response.errorCode = await _gameDb.RecvMailItems(userPkId, request.mailId);
            return response;
        }

        [HttpPost("DeleteMail")]
        public async Task<DeleteMailResponse> DeleteMail(DeleteMailRequest request)
        {
            DeleteMailResponse response = new DeleteMailResponse();
            Int64 userPkId = -1;
            if (HttpContext.Request.Headers.TryGetValue("UserPkId", out var userPkIdStr))
            {
                if (long.TryParse(userPkIdStr, out userPkId) == false)
                {
                    response.errorCode = ErrorCode.GameDbError;
                    return response;
                }
            }
            if (request.mailId < 0)
            {
                response.errorCode = ErrorCode.InvalidMailId;
                return response;
            }
            response.errorCode = await _gameDb.DeleteMail(userPkId, request.mailId);
            return response;
        }
    }
}
