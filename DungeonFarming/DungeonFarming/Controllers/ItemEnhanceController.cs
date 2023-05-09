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
        ILogger<ItemEnhanceController> _logger;
        IGameDb _gameDb;
        IMasterDataOffer _masterDataOffer;

        public ItemEnhanceController(ILogger<ItemEnhanceController> logger, IGameDb gameDb, IMasterDataOffer masterDataOffer)
        {
            _logger = logger;
            _gameDb = gameDb;
            _masterDataOffer = masterDataOffer;
        }

        private Int64 GetUserPkId()
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

        private ErrorCode CheckItemEnhancable(Int16 reqEnhancementCnt, UserItem userItem)
        {
            // 강화 가능한 속성인지 확인
            var itemDefine = _masterDataOffer.getItemDefine(userItem.item_code);
            if (itemDefine == null)
            {
                return ErrorCode.InvalidItemId;
            }
            var attrubute = _masterDataOffer.getItemAttrubute(itemDefine.attribute);
            if (attrubute == null)
            {
                return ErrorCode.InvalidItemId;
            }
            // TODO: 최대 강화 횟수를 보고 판단 할 것.
            if (attrubute.enhancementable != 1)
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
                // 강화시 수치를 떨구면 여기서
                return (ErrorCode.EnhancementFail, userItem);
            }
            userItem.enhance_count++;
            userItem.attack = (Int32)((double)userItem.attack + userItem.attack * 0.1);
            userItem.magic = (Int32)((double)userItem.magic + userItem.magic * 0.1);
            userItem.defence = (Int32)((double)userItem.defence + userItem.defence * 0.1);
            return (ErrorCode.EnhancementSucess, userItem);
        }

        [HttpPost()]
        public async Task<ItemEnhancementResponse> ItemEnhancement(ItemEnhancementRequest request)
        {
            ItemEnhancementResponse response = new ItemEnhancementResponse();
            Int64 userPkId = GetUserPkId();
            if (userPkId < 0)
            {
                response.errorCode = ErrorCode.ServerError;
                _logger.ZLogErrorWithPayload(LogEventId.ItemEnhance, new { userId = request.userId }, "pk id get FAIL");
                return response;
            }
            // 요청한 아이템 가져옴
            var (rtErrorCode, userItem) = await _gameDb.GetUserItem(userPkId, request.itemId);
            if (rtErrorCode != ErrorCode.None || userItem == null)
            {
                response.errorCode = rtErrorCode;
                // 인터페이스 대로 따라가는 클라에서 잘못된 id번호를 요청하는 것도 쉽게 발생 할 수 없는 에러이므로
                _logger.ZLogErrorWithPayload(LogEventId.ItemEnhance, new { userPkId = userPkId, ErrorCode = response.errorCode }, "userItem get FAIL");
                return response;
            }
            // 아이템이 강화 가능한지, 현재 강화 스택에 맞는지 확인
            response.errorCode = CheckItemEnhancable(request.enhancementCount, userItem);
            if (response.errorCode != ErrorCode.None)
            {
                _logger.ZLogInformationWithPayload(LogEventId.ItemEnhance, new { userPkId = userPkId, ErrorCode = response.errorCode }, "CAN'T enhance");
                return response;
            }
            // 확률 계산을 함
            (rtErrorCode, userItem) = DoItemEnhancement(userItem);
            response.errorCode = rtErrorCode;
            // 계산 후 결과를 DB에 저장
            rtErrorCode = await _gameDb.UpdateUserItem(userItem);
            if (rtErrorCode != ErrorCode.None)
            {
                response.errorCode = rtErrorCode;
                _logger.ZLogErrorWithPayload(LogEventId.ItemEnhance, new { userPkId = userPkId, userItem = userItem, ErrorCode = response.errorCode }, "userItem update FAIL");
                return response;
            }
            response.itemId = userItem.item_id;
            response.userItems = userItem;
            // 결과를 클라에 전송.
            _logger.ZLogInformationWithPayload(LogEventId.ItemEnhance, new { userPkId = userPkId, isSuccess = response.errorCode}, "item enhancement try complete");
            return response;
        }
    }
}
