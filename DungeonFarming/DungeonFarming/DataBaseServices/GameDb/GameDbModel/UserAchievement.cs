namespace DungeonFarming.DataBase.GameDb.GameDbModel;

public class UserAchievement
{
    public Int64 user_id { get; set; } = -1;
    public Int32 user_level { get; set; }
    public Int64 user_exp { get; set; }
    public Int32 highest_cleared_stage_id { get; set; }

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
