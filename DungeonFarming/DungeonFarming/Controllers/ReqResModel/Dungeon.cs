using DungeonFarming.DataBase.GameDb;
using DungeonFarming.DataBase.GameDb.GameDbModel;
using System.ComponentModel.DataAnnotations;

namespace DungeonFarming.Controllers.ReqResModel;

public class LastClearedStage : RequestBase
{
}

public class NpcBundle
{
    public int npcCode { get; set; }
    public int npcCount { get; set; }
}

public class LastClearedStageResponse : ResponseBase
{
    public int maxClearedStageCode { get; set; }
}

public class EnterStageRequest : RequestBase
{
    [Required] public int stageId { get; set; }
}

public class EnterStageResponse : ResponseBase
{
    public int stageId { get; set; }
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
    public long achivedExp { get; set; }
}
