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
            _gameSessionData = httpContextAccessor.HttpContext.Items["userSession"] as GameSessionData;
        }

        [HttpPost("ListUpMail")]
        public async Task<MailPreviewResponse> ListUpMail(MailPreviewRequest request)
        {
            MailPreviewResponse response = new MailPreviewResponse();

            (response.errorCode, var mailPreviewList) = await _gameDb.GetMailListUp(_gameSessionData.userId, request.page * _mailsPerPage, _mailsPerPage);
            if (response.errorCode != ErrorCode.None)
            {
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
            if (response.errorCode != ErrorCode.None)
            {
                return response;
            }
            
            _logger.ZLogInformationWithPayload( LogEventId.Mail, new { userAssignedId = _gameSessionData.userAssignedId, mailId = request.mailId }, "GetMail mail get SUCCESS");
            return response;
        }

        [HttpPost("RecvMailItems")]
        public async Task<RecvMailItemsResponse> RecvMailItems(RecvMailItemsRequest request)
        {
            RecvMailItemsResponse response = new RecvMailItemsResponse();

            (response.errorCode, var attachedItemList) = await _gameDb.RecvMailItems(_gameSessionData.userId, request.mailId);
            if (response.errorCode == ErrorCode.None)
            {
                _logger.ZLogInformationWithPayload(LogEventId.Mail,
                    new { userAssignedId = _gameSessionData.userAssignedId, mailId = request.mailId, itemList = attachedItemList }
                    , "user mailItems recv SUCCESS");
            }
            return response;
        }

        [HttpPost("DeleteMail")]
        public async Task<DeleteMailResponse> DeleteMail(DeleteMailRequest request)
        {
            DeleteMailResponse response = new DeleteMailResponse();

            response.errorCode = await _gameDb.DeleteMail(_gameSessionData.userId, request.mailId);
            if (response.errorCode == ErrorCode.None)
            {
                _logger.ZLogInformationWithPayload(LogEventId.Mail, new { userAssignedId = _gameSessionData.userAssignedId, mailId = request.mailId }, "mail delete SUCCESS");
            }
            return response;
        }
    }
}
