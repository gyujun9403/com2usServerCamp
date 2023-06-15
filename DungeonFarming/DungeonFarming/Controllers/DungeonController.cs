using DungeonFarming.DataBase.GameDb;
using DungeonFarming.DataBase.GameSessionDb;
using DungeonFarming.DataBase.GameDb.GameDbModel;
using Microsoft.AspNetCore.Mvc;
using ZLogger;
using DungeonFarming.DataBaseServices.GameDb.MasterDataModel;
using DungeonFarming.Controllers.ReqResModel;

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
            _gameSessionData = httpContextAccessor.HttpContext.Items["userSession"] as GameSessionData;
        }

        [HttpPost("LastClearedStage")]
        public async Task<LastClearedStageResponse> LastClearedStage(AttendanceGetStackRequst request)
        {
            LastClearedStageResponse response = new LastClearedStageResponse();

            (response.errorCode, var userAchievement) = await _gameDb.GetUserAchivement(_gameSessionData.userId);
            if (response.errorCode != ErrorCode.None)
            {
                return response;
            }
            
            response.maxClearedStageCode = userAchievement.highest_cleared_stage_id;            
            return response;
        }

        [HttpPost("EnterStage")]
        public async Task<EnterStageResponse> EnterStage(EnterStageRequest request)
        {
            EnterStageResponse response = new EnterStageResponse();

            var stageInfo = _masterDataOffer.getStageInfo(request.stageId);
            if (stageInfo == null)
            {
                response.errorCode = ErrorCode.InvalidStageCode;
                return response;
            }

            (response.errorCode, var userAchievement) = await _gameDb.GetUserAchivement(_gameSessionData.userId);
            if (response.errorCode != ErrorCode.None)
            {
                return response;
            }
            
            response.errorCode = CheckEnterable(stageInfo, userAchievement);
            if (response.errorCode != ErrorCode.None)
            {
                return response;
            }

            GameSessionData newSessionData = _gameSessionData;
            response.stageId = request.stageId;
            var stageNpcList = _masterDataOffer.getStageNpcInfoList(request.stageId);
            var stageItemList = _masterDataOffer.getStageItemInfoList(request.stageId);
            if (stageNpcList == null || stageItemList == null)
            {
                response.errorCode = ErrorCode.InvalidStageCode;
                _logger.ZLogWarningWithPayload(LogEventId.Dungeon, new { userAssignedId = request.userAssignedId, stageId = request.stageId }, "Invalid stageId requested");
                response.stageId = request.stageId;
            }

            newSessionData.stageCode = response.stageId;
            newSessionData.userStatus = UserStatus.Gaming;
            await _gameSessionDb.SetUserInfoSession(newSessionData);
            response.isEnterable = true;
            response.npcBundles = stageNpcList;
            response.rewardItemBundles = new List<ItemBundle>();
            foreach (var item in stageItemList)
            {
                response.rewardItemBundles.Add(new ItemBundle { itemCode = item.itemCode, itemCount = item.itemCount });
            }

            return response;
        }

        [HttpPost("KillNpcs")]
        public async Task<KillNpcResponse> KillNpc(KillNpcRequest request)
        {
            var response = new KillNpcResponse();
            response.errorCode = ErrorCode.None;
            if (_gameSessionData.userStatus != UserStatus.Gaming)
            {
                response.errorCode = ErrorCode.InvalidUserStatus;
                return response;
            }

            var stageNpcDic = _masterDataOffer.getStageNpcInfoDic(_gameSessionData.stageCode);
            if (stageNpcDic == null)
            {
                response.errorCode = ErrorCode.InvalidNpcCode;
                _logger.ZLogWarningWithPayload(LogEventId.Dungeon, new { userAssignedId = request.userAssignedId, stageId = _gameSessionData.stageCode }, "Invalid stageId requested");
                return response;
            }

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
                    _logger.ZLogWarningWithPayload(LogEventId.Dungeon, new { userAssignedId = request.userAssignedId, stageId = request.killedNpcList }, "Invalid NpcId requested");
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

        [HttpPost("ParmingItems")]
        public async Task<ParmingItemsResponse> ParmingItems(ParmingItemsRequst request)
        {
            var response = new ParmingItemsResponse();
            response.errorCode = ErrorCode.None;
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
            foreach (var itemBundel in request.FarmedItemList)
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
                        _logger.ZLogWarningWithPayload(LogEventId.Dungeon, new { userAssignedId = request.userAssignedId, stageId = request.FarmedItemList }, "Invalid ItemId requested");
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

        [HttpPost("StageClear")]
        public async Task<StageClearResponse> StageClear(StageClearRequest request)
        {
            var response = new StageClearResponse();
            GameSessionData resetSessionData = _gameSessionData.GetResetSession();

            if (_gameSessionData.userStatus != UserStatus.Gaming)
            {
                response.errorCode = ErrorCode.InvalidUserStatus;
                return response;
            }

            response.errorCode = await _gameSessionDb.SetUserInfoSession(resetSessionData);
            if (response.errorCode != ErrorCode.None)
            {
                await _gameSessionDb.SetUserInfoSession(_gameSessionData);
                return response;
            }

            response.errorCode = CheckItemNpcCount();
            if ( response.errorCode != ErrorCode.None)
            {
                // 필요시 MasterData와 일치 하지 않으면 세션을 초기화(게임 무효화).
                return response; 
            }

            Int64 achivedExp = 0;
            var stageNpcDic = _masterDataOffer.getStageNpcInfoDic(_gameSessionData.stageCode);
            foreach (var npc in _gameSessionData.killedNpcs)
            {
                achivedExp += stageNpcDic[npc.Key].expPerNpc * npc.Value;
            }
            (response.errorCode, var oldUserAchivement) = await _gameDb.GetUserAchivement(_gameSessionData.userId);
            if (response.errorCode != ErrorCode.None)
            {
                await _gameSessionDb.SetUserInfoSession(_gameSessionData);
                return response;
            }
            response.errorCode = await IncreaseUserExp(achivedExp, oldUserAchivement);
            if (response.errorCode != ErrorCode.None)
            {
                await _gameSessionDb.SetUserInfoSession(_gameSessionData);
                return response;
            }

            List<ItemBundle> farmedItemBundle = new List<ItemBundle>();
            foreach (var item in _gameSessionData.FarmedItems)
            {
                farmedItemBundle.Add(new ItemBundle { itemCode = item.Key, itemCount = item.Value });
            }
            if (farmedItemBundle.Count() > 0)
            {
                response.errorCode = await _gameDb.GiveUserItems(_gameSessionData.userId, farmedItemBundle);
                if (response.errorCode != ErrorCode.None)
                {
                    await _gameSessionDb.SetUserInfoSession(_gameSessionData);
                    await _gameDb.UpdateUserAchivement(oldUserAchivement);
                    return response;
                }
            }

            response.rewardItemBundles = farmedItemBundle;
            response.achivedExp = achivedExp;
            _logger.ZLogInformationWithPayload(LogEventId.Dungeon, new { sessionData = _gameSessionData }, "Stage Clear SUCCESS");
            return response;
        }

        ErrorCode CheckEnterable(StageInfo stageInfo, UserAchievement userAchievement)
        {
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

        ErrorCode CheckItemNpcCount()
        {
            var stageItemDic = _masterDataOffer.getStageItemInfoDic(_gameSessionData.stageCode);
            if (stageItemDic == null)
            {
                return ErrorCode.InvalidStageCode;
            }
            if (CheckFarmingItems(stageItemDic, _gameSessionData.FarmedItems) == false)
            {
                _logger.ZLogErrorWithPayload(LogEventId.Dungeon,
                    new
                    {
                        userAssignedId = _gameSessionData.userAssignedId,
                        stageItems = stageItemDic.Values.ToArray(),
                        userFarmedItems = _gameSessionData.FarmedItems.Values.ToArray()
                    }, "Too Much Item Farmed");

                return ErrorCode.TooMuchItemFarmed;
            }

            var stageNpcDic = _masterDataOffer.getStageNpcInfoDic(_gameSessionData.stageCode);
            if (stageNpcDic == null)
            {
                return ErrorCode.InvalidStageCode;
            }
            if (CheckKilledNpcs(stageNpcDic, _gameSessionData.killedNpcs) == false)
            {
                _logger.ZLogErrorWithPayload(LogEventId.Dungeon,
                    new
                    {
                        userAssignedId = _gameSessionData.userAssignedId,
                        stageNpc = stageNpcDic.Values.ToArray(),
                        userKilledNpcs = _gameSessionData.killedNpcs.Values.ToArray()
                    }, "Not Enough Npc Kill");
                return ErrorCode.NotEnoughNpcKillCount;
            }
            return ErrorCode.None;
        }
        bool CheckFarmingItems(Dictionary<Int64, ItemBundle> stageItemsDic, Dictionary<Int32, Int64> farmingItems)
        {
            foreach (var farmedItem in farmingItems)
            {
                if (!stageItemsDic.TryGetValue(farmedItem.Key, out var stageItems) || stageItems.itemCount < farmedItem.Value)
                {
                    return false;
                }
            }
            return true;
        }
        bool CheckKilledNpcs(Dictionary<Int64, StageNpcInfo> stageNpcsDic, Dictionary<Int32, Int64> killedNpcs)
        {
            foreach (var killedNpc in killedNpcs)
            {
                if (!stageNpcsDic.TryGetValue(killedNpc.Key, out var stageNpc) || stageNpc.npcCount > killedNpc.Value)
                {
                    return false;
                }
            }
            return true;
        }

        async Task<ErrorCode> IncreaseUserExp(Int64 achivedExp, UserAchievement userAchivement)
        {

            var newUserAchivement = userAchivement.Clone();
            var maxExpOfLevel = _masterDataOffer.getMaxExpOfLevel(newUserAchivement.user_level);
            if (maxExpOfLevel == null)
            {
                _logger.ZLogErrorWithPayload(LogEventId.Dungeon, new { newUserAchivement = newUserAchivement }, "Max Exp get Fail");
                return ErrorCode.GameDbError;
            }
            if (newUserAchivement.user_exp + achivedExp > maxExpOfLevel)
            {
                if (_masterDataOffer.getMaxExpOfLevel(newUserAchivement.user_level + 1) == null)
                {
                    long? maxExp = maxExpOfLevel;
                    newUserAchivement.user_exp = maxExp ?? newUserAchivement.user_exp;
                }
                else
                {
                    newUserAchivement.user_level += 1;
                    newUserAchivement.user_exp = newUserAchivement.user_exp + achivedExp - maxExpOfLevel.Value;
                }
            }
            if (await _gameDb.UpdateUserAchivement(newUserAchivement) != ErrorCode.None)
            {
                return ErrorCode.GameDbError;
            }
            return ErrorCode.None;
        }

    }
}
