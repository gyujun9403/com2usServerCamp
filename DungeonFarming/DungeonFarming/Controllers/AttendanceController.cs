using DungeonFarming.DataBase.AccountDb;
using DungeonFarming.DataBase.GameDb;
using DungeonFarming.DataBase.GameDb.GameUserDataORM;
using DungeonFarming.DataBase.GameSessionDb;
using DungeonFarming.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZLogger;

namespace DungeonFarming.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        readonly IGameDb _gameDb;
        readonly ILogger<AttendanceController> _logger;
        readonly IMasterDataOffer _masterDataOffer;
        //readonly Int64 _userId;
        readonly GameSessionData _gameSessionData;
        public AttendanceController(IHttpContextAccessor httpContextAccessor, ILogger<AttendanceController> logger,
            IGameDb gameDb, IMasterDataOffer masterDataOffer)
        {
            _gameDb = gameDb;
            _logger = logger;
            _masterDataOffer = masterDataOffer;
            //_userId = httpContextAccessor.HttpContext.Items["userId"] as Int64? ?? -1;
            _gameSessionData = httpContextAccessor.HttpContext.Items["gameSessionData"] as GameSessionData;
        }

        [HttpPost()]
        public async Task<AttendanceResponse> Attendance(AttendanceRequest request)
        {
            AttendanceResponse response = new AttendanceResponse();

            (response.errorCode, var loginLog) = await _gameDb.UpdateAndGetLoginLog(_gameSessionData.userId);
            if (response.errorCode != ErrorCode.None && response.errorCode != ErrorCode.AreadyLogin)
            {
                _logger.ZLogErrorWithPayload(LogEventId.Login, new { userId = request.userId, errorCode = response.errorCode }, "Loginlog Update and Get FAIL");
                return response;
            }
            else if (response.errorCode == ErrorCode.AreadyLogin)
            {
                _logger.ZLogInformationWithPayload(LogEventId.Login, new { userId = request.userId, errorCode = response.errorCode }, "User Login request AGAIN");
            }
            else if (loginLog == null)
            {
                response.errorCode = ErrorCode.ServerError;
                _logger.ZLogErrorWithPayload(LogEventId.Login, new { userId = request.userId, errorCode = response.errorCode }, "Loginlog is NULL");
                return response;
            }

            List<ItemBundle>? reward = _masterDataOffer.getDailyLoginRewardItemBundles(loginLog.consecutive_login_count);
            Mail rewardMail = GenerateLoginRewardMail(loginLog, reward);
            response.errorCode = await _gameDb.SendMail(rewardMail);
            if (response.errorCode != ErrorCode.None)
            {
                _logger.ZLogErrorWithPayload(LogEventId.Login, new { userId = request.userId, loginLog = loginLog, reward = reward, errorCode = response.errorCode }, "Loginlog SendMail FAIL");
                return response; ;
            }
            response.attendanceStack = loginLog.consecutive_login_count;
            return response;
        }

        [HttpPost("GetStack")]
        public async Task<AttendanceGetStackResponse> AttendanceGetStack(AttendanceGetStackRequst request)
        {
            AttendanceGetStackResponse response = new AttendanceGetStackResponse();

            var (errorCode, loginlog) = await _gameDb.GetLoginLog(_gameSessionData.userId);
            if (loginlog == null)
            {
                response.errorCode = ErrorCode.ServerError;
                _logger.ZLogErrorWithPayload(LogEventId.DailyLoginReward, new { userId = _gameSessionData.userId }, "login log get FAIL");
                return response;
            }

            response.attendanceStack = loginlog.consecutive_login_count;
            _logger.ZLogErrorWithPayload(LogEventId.DailyLoginReward, new { userId = _gameSessionData.userId }, "login reward stack send SUCCESS");
            return response;
        }


        Mail GenerateLoginRewardMail(LoginLog log, List<ItemBundle>? itemBundle)
        {
            var mail = new Mail();
            mail.user_id = log.user_id;
            if (itemBundle != null && itemBundle.Count > 0)
            {
                mail.item0_code = itemBundle[0].itemCode;
                mail.item0_count = itemBundle[0].itemCount;
                if (itemBundle.Count > 1)
                {
                    mail.item1_code = itemBundle[1].itemCode;
                    mail.item1_count = itemBundle[1].itemCount;
                    if (itemBundle.Count > 2)
                    {
                        mail.item2_code = itemBundle[2].itemCode;
                        mail.item2_count = itemBundle[2].itemCount;
                    }
                    if (itemBundle.Count > 3)
                    {
                        mail.item3_code = itemBundle[3].itemCode;
                        mail.item3_count = itemBundle[3].itemCount;
                    }
                }
            }
            mail.mail_title = $"로그인 보상 {log.consecutive_login_count}일 차";
            mail.mail_text = $"로그인 보상 {log.consecutive_login_count}일 차!!!";
            mail.recieve_date = DateTime.Now;
            mail.expiration_date = DateTime.Now.AddDays(15);
            return mail;
        }
    }
}
