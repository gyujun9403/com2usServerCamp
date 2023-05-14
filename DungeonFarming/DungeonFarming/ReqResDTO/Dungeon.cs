using DungeonFarming.DataBase.GameDb;
using DungeonFarming.DataBaseServices.GameDb.MasterDataDTO;
using System.ComponentModel.DataAnnotations;

namespace DungeonFarming.ReqResDTO
{
    public class LastClearedStage : RequestBase
    {
    }

    public class NpcBundle
    {
        public Int64 npcCode { get; set; }
        public Int64 npcCount { get; set; }
    }
    //public class DunGeonDetailedInfo
    //{
    //    public Int64 dungeonId { get; set; }
    //    public bool isClear { get; set; }
    //    public List<ItemBundle>? rewardItemBundles { get; set; }
    //}

    public class LastClearedStageResponse : ResponseBase
    {
        public Int64 maxClearedStageCode { get; set; }
    }

    public class EnterStageRequest : RequestBase
    {
        [Required] public Int64 stageId { get; set; }
    }

    public class EnterStageResponse : ResponseBase
    {
        public Int64 stageId { get; set; }
        public bool isEnterable { get; set; } = false;
        public List<StageNpcInfo>? npcBundles { get; set; }
        public List<ItemBundle>? rewardItemBundles { get; set; }
    }

    public class KillNpcRequest : RequestBase
    {
        [Required] public List<NpcBundle> killedNpcList { get; set; }
    }

    public class KillNpcResponse : ResponseBase
    {
    }

    public class ParmingItemsRequst : RequestBase
    {
        [Required] public List<ItemBundle> FarmedItemBundle { get; set; }
    }

    public class ParmingItemsResponse : ResponseBase
    {
    }

    public class StageClearRequest : RequestBase
    {
    }

    public class StageClearResponse : ResponseBase
    {
        public List<ItemBundle>? rewardItemBundles { get; set; }
        public Int64 achivedExp { get; set; }
    }
}
