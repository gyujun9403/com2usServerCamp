namespace DungeonFarming.DataBaseServices.GameDb.MasterDataDTO
{

    public class StageNpc //TODO: 이름 변경
    {
        public Int64 pk_id { get; set; }
        public Int64 stage_code { get; set; }
        public Int64 npc_code { get; set; }
        public Int64 npc_count { get; set; }
        public Int64 exp_per_npc { get; set; }
    }

    public class StageNpcInfo
    {
        public Int64 npcCode { get; set; }
        public Int64 npcCount { get; set; }
        public Int64 expPerNpc { get; set; }
    }

    public class StageNpcList
    {
        public Int64 stageCode { get; set; }
        public List<StageNpcInfo> npcList { get; set; }
    }
}
