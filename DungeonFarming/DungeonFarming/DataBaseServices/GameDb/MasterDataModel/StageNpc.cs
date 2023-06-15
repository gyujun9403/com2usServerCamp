namespace DungeonFarming.DataBase.GameDb.GameDbModel;

public class StageNpc
{
    public Int32 pk_id { get; set; }
    public Int32 stage_code { get; set; }
    public Int32 npc_code { get; set; }
    public Int32 npc_count { get; set; }
    public Int32 exp_per_npc { get; set; }
}

public class StageNpcInfo
{
    public Int32 npcCode { get; set; }
    public Int32 npcCount { get; set; }
    public Int32 expPerNpc { get; set; }
}

public class StageNpcList
{
    public Int32 stageCode { get; set; }
    public List<StageNpcInfo> npcList { get; set; }
}
