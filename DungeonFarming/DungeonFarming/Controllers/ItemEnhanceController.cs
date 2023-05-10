using DungeonFarming.DataBase.GameDb;
using DungeonFarming.DataBase.GameDb.GameUserDataORM;
using DungeonFarming.DataBase.GameDb.MasterData;
using DungeonFarming.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZLogger;

namespace DungeonFarming.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ItemEnhanceController : ControllerBase
    {
        readonly ILogger<ItemEnhanceController> _logger;
        readonly IGameDb _gameDb;
        readonly IMasterDataOffer _masterDataOffer;
        readonly Int64 _userId;

        public ItemEnhanceController(IHttpContextAccessor httpContextAccessor, ILogger<ItemEnhanceController> logger, 
            IGameDb gameDb, IMasterDataOffer masterDataOffer)
        {
            _logger = logger;
            _gameDb = gameDb;
            _masterDataOffer = masterDataOffer;
            _userId = httpContextAccessor.HttpContext.Items["userId"] as Int64? ?? -1;
        }

        private ErrorCode CheckItemEnhancable(Int16 reqEnhancementCnt, UserItem userItem)
        {
            // 강화 가능한 속성인지 확인
            var itemDefine = _masterDataOffer.getItemDefine(userItem.item_code);
            if (itemDefine == null)
            {
                return ErrorCode.InvalidItemId;
            }
            if (itemDefine.enhance_max_count == 0)
            {
                return ErrorCode.EnhancementUnavailable;
            }
            if (userItem.enhance_count >= itemDefine.enhance_max_count)
            {
                return ErrorCode.MaxEnhancementLevelExceeded;
            }
            if (reqEnhancementCnt != userItem.enhance_count + 1)
            {
                return ErrorCode.InvalidEnhancementCount;
            }
            return ErrorCode.None;
        }

        // successRate : 0 ~ 100 %
        private bool DetermineSuccess(int successRate)
        {
            successRate *= 1000; // 100퍼센트 : 100000, 0.001퍼센트 : 1
            Random rand = new Random();
            int roll = rand.Next(0, 100000); // 0부터 99999까지의 값을 무작위로 선택
            return roll <= successRate; // 성공률보다 작거나 같으면 true를 반환
        }

        private (ErrorCode, UserItem) DoItemEnhancement(UserItem userItem)
        {
            if (DetermineSuccess(30) == false)
            {
                // 강화시 수치를 떨구면 여기서 작성 할 것.
                return (ErrorCode.EnhancementFail, userItem);
            }
            userItem.enhance_count++;
            //TODO: 강화 증가 비율도 표에서 가지고 오게?
            userItem.attack += Math.Max(1, (int)Math.Ceiling(userItem.attack * 0.1));
            userItem.magic += Math.Max(1, (int)Math.Ceiling(userItem.magic * 0.1));
            userItem.defence += Math.Max(1, (int)Math.Ceiling(userItem.defence * 0.1));
            return (ErrorCode.EnhancementSucess, userItem);
        }

        [HttpPost()]
        public async Task<ItemEnhancementResponse> ItemEnhancement(ItemEnhancementRequest request)
        {
            ItemEnhancementResponse response = new ItemEnhancementResponse();
            var (rtErrorCode, userItem) = await _gameDb.GetUserItem(_userId, request.itemId);
            if (rtErrorCode != ErrorCode.None || userItem == null)
            {
                response.errorCode = rtErrorCode;
                _logger.ZLogWarningWithPayload(LogEventId.ItemEnhance, new { userPkId = _userId, ErrorCode = response.errorCode }, "userItem get FAIL");
                return response;
            }

            response.errorCode = CheckItemEnhancable(request.enhancementCount, userItem);
            if (response.errorCode != ErrorCode.None)
            {
                _logger.ZLogInformationWithPayload(LogEventId.ItemEnhance, new { userPkId = _userId, ErrorCode = response.errorCode }, "CAN'T enhance");
                return response;
            }
            (response.errorCode, userItem) = DoItemEnhancement(userItem);
            if (response.errorCode == ErrorCode.EnhancementSucess)
            {
                rtErrorCode = await _gameDb.UpdateUserItem(userItem);
                if (rtErrorCode != ErrorCode.None)
                {
                    response.errorCode = rtErrorCode;
                    _logger.ZLogErrorWithPayload(LogEventId.ItemEnhance, new { userPkId = _userId, userItem = userItem, ErrorCode = response.errorCode }, "userItem update FAIL");
                    return response;
                }
                response.itemId = userItem.item_id;
                response.userItems = userItem;
            }
            else //response.errorCode == ErrorCode.EnhancementFail
            {
                rtErrorCode = await _gameDb.DeleteUserItem(_userId, userItem.item_id);
                if (rtErrorCode != ErrorCode.None)
                {
                    response.errorCode = rtErrorCode;
                    _logger.ZLogErrorWithPayload(LogEventId.ItemEnhance, new { userPkId = _userId, userItem = userItem, ErrorCode = response.errorCode }, "userItem update FAIL");
                    return response;
                }
            }
            _logger.ZLogInformationWithPayload(LogEventId.ItemEnhance, new { userPkId = _userId, isSuccess = response.errorCode}, "item enhancement try complete");
            return response;
        }
    }
}
