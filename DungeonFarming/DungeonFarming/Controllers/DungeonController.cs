using DungeonFarming.DataBase.GameDb;
using DungeonFarming.DataBase.GameSessionDb;
using DungeonFarming.DataBaseServices.GameDb.GameUserDataDTO;
using DungeonFarming.DataBaseServices.GameDb.MasterDataDTO;
using DungeonFarming.DTO;
using DungeonFarming.ReqResDTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using ZLogger;

namespace DungeonFarming.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DungeonController : ControllerBase
    {
        readonly IGameDb _gameDb;
        readonly IGameSessionDb _gameSessionDb;
        readonly IMasterDataOffer _masterDataOffer;
        readonly GameSessionData _gameSessionData;
        readonly ILogger<DungeonController> _logger;

        public DungeonController(IHttpContextAccessor httpContextAccessor, IGameDb gameDb, IGameSessionDb gameSessionDb, 
            ILogger<DungeonController> logger, IMasterDataOffer masterDataOffer)
        {
            _gameDb = gameDb;
            _gameSessionDb = gameSessionDb;
            _masterDataOffer = masterDataOffer;
            _logger = logger;
            _gameSessionData = httpContextAccessor.HttpContext.Items["gameSessionData"] as GameSessionData;
        }

        [HttpPost("LastClearedStage")]
        public async Task<LastClearedStageResponse> LastClearedStage(AttendanceGetStackRequst request)
        {
            LastClearedStageResponse response = new LastClearedStageResponse();
            // context에서 userId를 가져온다
            // 마스터 데이터에서 던전 정보(List)를 받아온다
            (response.errorCode, var userAchievement) = await _gameDb.GetUserAchivement(_gameSessionData.userId);
            if (response.errorCode != ErrorCode.None)
            {
                _logger.ZLogErrorWithPayload(LogEventId.Dungeon, new { userId = request.userId, errorCode = response.errorCode }, "GetUserAchivement FAIL");
                return response;
            }
            response.maxClearedStageCode = userAchievement.highest_cleared_stage_id;
            return response;
            // 유저 데이터를 받아오고, 클리어 정보 제공.
        }

        ErrorCode CheckEnterable(StageInfo stageInfo, UserAchievement userAchievement)
        {

            // 레벨 체크
            if (stageInfo.required_user_level > userAchievement.user_level)
            {
                return ErrorCode.LowLevel;
            }
            else if (stageInfo.stage_code > userAchievement.highest_cleared_stage_id + 1)
            {
                return ErrorCode.UnreachableStage;
            }

            return ErrorCode.None;
        }

        // 던전 입장
        [HttpPost("EnterStage")]
        public async Task<EnterStageResponse> EnterStage(EnterStageRequest request)
        {
            EnterStageResponse response = new EnterStageResponse();
            // 요청된 스테이지 아이디로 masterdate에서 스테이지 정보를 꺼내옴
            var stageInfo = _masterDataOffer.getStageInfo(request.stageId);
            var (rtErrorCode, userAchievement) = await _gameDb.GetUserAchivement(_gameSessionData.userId);
            if (rtErrorCode != ErrorCode.None)
            {
                return response;
            }
            // 현재 레벨에 접근 가능한지, 그리고 이전 스테이지만 클리어 되어 있지 않은지 확인 -> 이전스테이지의 조건은 스테이지의 아이디가 작으면.
            response.errorCode = CheckEnterable(stageInfo, userAchievement);
            if (response.errorCode != ErrorCode.None)
            {
                return response;
            }
            response.stageId = request.stageId;
            var stageNpcList = _masterDataOffer.getStageNpcInfoList(request.stageId);
            var stageItemList = _masterDataOffer.getStageNpcInfoList(request.stageId);
            // redis에 session의 데이터에 스테이지의 정보를 넣고, 게이밍 상태로 변경
            GameSessionData newSessionData = _gameSessionData;
            newSessionData.stageCode = response.stageId;
            newSessionData.userStatus = UserStatus.Gaming;
            // 업데이트
            await _gameSessionDb.SetUserInfoSession(newSessionData);
            // 응답에 반환. 
            return response;
        }

        // 던전에서 몬스터를 하나 잡음
        public async Task<KillNpcResponse> KillNpc(KillNpcRequest request)
        {
            var response = new KillNpcResponse();
            if (_gameSessionData.userStatus != UserStatus.Gaming)
            {
                response.errorCode = ErrorCode.InvalidUserStatus;
                return response;
            }
            // 세션에 저장된 몬스터 코드가 있는지 확인
            var stageNpcDic = _masterDataOffer.getStageNpcInfoDic(_gameSessionData.stageCode);
            if (stageNpcDic == null)
            {
                return response;
            }
            response.errorCode = ErrorCode.None;
            foreach (var npcBundel in request.killedNpcList)
            {
                if (stageNpcDic.ContainsKey(npcBundel.npcCode))
                {
                    if (_gameSessionData.killedNpcs.ContainsKey(npcBundel.npcCode))
                    {
                        _gameSessionData.killedNpcs[npcBundel.npcCode] += npcBundel.npcCount;
                    }
                    else
                    {
                        _gameSessionData.killedNpcs.Add(npcBundel.npcCode, npcBundel.npcCount);
                    }
                }
                else
                {
                    response.errorCode = ErrorCode.InvalidNpcCode;
                    _gameSessionData.ResetGameData();
                    break;
                }
            }

            var rtErrorCode = await _gameSessionDb.SetUserInfoSession(_gameSessionData);
            if (rtErrorCode != ErrorCode.None)
            {
                response.errorCode = ErrorCode.GameSessionDbError;
            }
            return response; 
            // 에러코드 반환
        }

        // 던전에서 아이템을 얻음
        public async Task<ParmingItemsResponse> ParmingItems(ParmingItemsRequst request)
        {
            //세션에 저장된 아이템 정보가 있는지 확인 -> 필터로 빼기
            var response = new ParmingItemsResponse();
            if (_gameSessionData.userStatus != UserStatus.Gaming)
            {
                response.errorCode = ErrorCode.InvalidUserStatus;
                return response;
            }
            // 세션에 저장된 아이템 코드가 있는지 확인
            var stageItemDic = _masterDataOffer.getStageItemInfoDic(_gameSessionData.stageCode);
            if (stageItemDic == null)
            {
                return response;
            }
            response.errorCode = ErrorCode.None;
            foreach (var itemBundel in request.FarmedItemBundle)
            {
                if (stageItemDic.ContainsKey(itemBundel.itemCode))
                {
                    if (_gameSessionData.FarmedItems.ContainsKey(itemBundel.itemCode))
                    {
                        _gameSessionData.FarmedItems[itemBundel.itemCode] += itemBundel.itemCount;
                    }
                    else
                    {
                        _gameSessionData.FarmedItems.Add(itemBundel.itemCode, itemBundel.itemCount);
                    }
                }
                else
                {
                    response.errorCode = ErrorCode.InvalidItemId;
                    _gameSessionData.ResetGameData();
                    break;
                }
            }

            var rtErrorCode = await _gameSessionDb.SetUserInfoSession(_gameSessionData);
            if (rtErrorCode != ErrorCode.None)
            {
                response.errorCode = ErrorCode.GameSessionDbError;
            }
            return response;
        }


        // 던전 클리어 요청
        public async Task<StageClearResponse> StageClear(StageClearRequest request)
        {
            var response = new StageClearResponse();

            if (_gameSessionData.userStatus != UserStatus.Gaming)
            {
                response.errorCode = ErrorCode.InvalidUserStatus;
                return response;
            }
            
            var stageItemDic = _masterDataOffer.getStageItemInfoDic(_gameSessionData.stageCode);
            if (stageItemDic == null)
            {
                return response;
            }
            if (CheckFarmingItems(stageItemDic, _gameSessionData.FarmedItems) == false)
            {
                response.errorCode = ErrorCode.TooMuchItemFarmed;
                _logger.ZLogErrorWithPayload(LogEventId.Dungeon,
                    new
                    {
                        userId = _gameSessionData.userId,
                        stageItems = stageItemDic.Values.ToArray(),
                        userFarmedItems = _gameSessionData.FarmedItems.Values.ToArray()
                    }, "Too Much Item Farmed");
                return response;
            }

            var stageNpcDic = _masterDataOffer.getStageNpcInfoDic(_gameSessionData.stageCode);
            if (stageNpcDic == null)
            {
                return response;
            }
            if (CheckKilledNpcs(stageNpcDic, _gameSessionData.killedNpcs) == false)
            {
                response.errorCode = ErrorCode.NotEnoughNpcKillCount;
                _logger.ZLogErrorWithPayload(LogEventId.Dungeon,
                    new
                    {
                        userId = _gameSessionData.userId,
                        stageNpc = stageNpcDic.Values.ToArray(),
                        userKilledNpcs = _gameSessionData.killedNpcs.Values.ToArray()
                    }, "Not Enough Npc Kill");
                return response;
            }

            GameSessionData backupSessionData = _gameSessionData;
            _gameSessionData.ResetGameData();
            response.errorCode = await _gameSessionDb.SetUserInfoSession(_gameSessionData);
            if (response.errorCode != ErrorCode.None)
            {
                await _gameSessionDb.SetUserInfoSession(backupSessionData);
            }
            // TODO: 일단 집어넣게 하고, 메일꺼랑 같이 연동 되게 고치기.
            var itemList = _gameSessionData.FarmedItems.Select(kv => new ItemBundle { itemCode = kv.Key, itemCount = kv.Value }).ToList();
            response.errorCode = await _gameDb.InsertUserItemsByItemBundles(_gameSessionData.userId, itemList);
            if (response.errorCode != ErrorCode.None)
            {
                await _gameSessionDb.SetUserInfoSession(backupSessionData);
            }

            // 반환
            response.rewardItemBundles = itemList;
            foreach ( var npc in _gameSessionData.killedNpcs)
            {
                response.achivedExp += stageNpcDic[npc.Key].expPerNpc * npc.Value;
            }
            return response;
        }
        bool CheckFarmingItems(Dictionary<Int64, ItemBundle> stageItemsDic, Dictionary<Int64, Int64> farmingItems)
        {
            return farmingItems.All(reqItem => stageItemsDic.TryGetValue(reqItem.Key, out var stageItems) && stageItems.itemCount >= reqItem.Value);
        }

        bool CheckKilledNpcs(Dictionary<Int64, StageNpcInfo> stageNpcsDic, Dictionary<Int64, Int64> killedNpcs)
        {
            return killedNpcs.All(reqNpc => stageNpcsDic.TryGetValue(reqNpc.Key, out var stageNpc) && stageNpc.npcCount <= reqNpc.Value);
        }
    }
}
