namespace DungeonFarming.DataBaseServices.GameDb.GameUserDataDTO
{
    public class UserAchievement
    {
        public long user_id { get; set; }
        public int user_level { get; set; }
        public long user_exp { get; set; }
        public long max_cleared_stage_id { get; set; }
    }
}
