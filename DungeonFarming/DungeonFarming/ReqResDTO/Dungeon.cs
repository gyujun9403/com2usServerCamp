using DungeonFarming.DataBase.GameDb;
using System.ComponentModel.DataAnnotations;

namespace DungeonFarming.ReqResDTO
{
    public class GetStageListRequst : RequestBase
    {
    }

    public class NpcBundle
    {
        public Int64 npcCode { get; set; }
        public Int64 npcCount { get; set; }
    }
    public class DunGeonDetailedInfo
    {
        public Int64 dungeonId { get; set; }
        public bool isClear { get; set; }
        public List<ItemBundle>? rewardItemBundles { get; set; }
    }

    public class GetStageListResponse : ResponseBase
    {
        public List<Int64>? clearedStageList;
    }

    public class SelectStageRequest : RequestBase
    {
        [Required] public Int64 stageId { get; set; }
    }

    public class SelectStageResponse : ResponseBase 
    {
        public Int64 stageId { get; set; } = -1;
        public bool isClear { get; set; } = false;
        public List<NpcBundle>? npcBundles { get; set; }
        public List<ItemBundle>? rewardItemBundles { get; set; }
    }

    public class KillNpcRequest : RequestBase
    {
        [Required] public NpcBundle killedNpcList { get; set; }
    }

    public class KillNpcResponse : ResponseBase
    {
    }

    public class ParmingItemsRequst : RequestBase
    {
        [Required] public ItemBundle FarmedItemBundle { get; set; }
    }

    public class ParmingItemsResponse : ResponseBase
    {
    }

    public class ClearStageRequest : RequestBase
    {
    }

    public class ClearStageResponse : ResponseBase
    {
        public List<ItemBundle>? rewardItemBundles { get; set; }
        public Int64 achivedExp { get; set; }
    }
}
