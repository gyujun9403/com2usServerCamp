using DungeonFarming.DataBase.GameDb;
using DungeonFarming.DataBase.GameDb.GameDbModel;
using System.ComponentModel.DataAnnotations;

namespace DungeonFarming.ReqResDTO
{
    public class LastClearedStage : RequestBase
    {
    }

    public class NpcBundle
    {
        public Int32 npcCode { get; set; }
        public Int32 npcCount { get; set; }
    }

    public class LastClearedStageResponse : ResponseBase
    {
        public Int32 maxClearedStageCode { get; set; }
    }

    public class EnterStageRequest : RequestBase
    {
        [Required] public Int32 stageId { get; set; }
    }

    public class EnterStageResponse : ResponseBase
    {
        public Int32 stageId { get; set; }
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
        [Required] public List<ItemBundle> FarmedItemList { get; set; }
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
