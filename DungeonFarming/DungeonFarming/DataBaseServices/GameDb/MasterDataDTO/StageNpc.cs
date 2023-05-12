namespace DungeonFarming.DataBaseServices.GameDb.MasterDataDTO
{

    public class StageNpc
    {
        public Int64 pk_id { get; set; }
        public Int64 stage_code { get; set; }
        public Int64 npc_code { get; set; }
        public Int64 npc_count { get; set; }
        public Int64 exp_per_npc { get; set; }
    }

    public class NpcBundle
    {
        public Int64 npcCode { get; set; }
        public Int64 npcCount { get; set; }
        public Int64 expPerNpc { get; set; }
    }

    public class StageNpcList
    {
        public Int64 stageCode { get; set; }
        public List<NpcBundle> npcList { get; set; }
    }
}
