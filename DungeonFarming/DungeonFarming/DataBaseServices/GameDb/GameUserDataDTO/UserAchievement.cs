namespace DungeonFarming.DataBaseServices.GameDb.GameUserDataDTO
{
    public class UserAchievement
    {
        public long user_id { get; set; } = -1;
        public int user_level { get; set; }
        public long user_exp { get; set; }
        public long highest_cleared_stage_id { get; set; }

        public UserAchievement Clone()
        {
            return new UserAchievement
            {
                user_id = user_id,
                user_level = user_level,
                user_exp = user_exp,
                highest_cleared_stage_id = highest_cleared_stage_id
            };
        }
    }
}
