namespace DungeonFarming.ReqResDTO
{
    public class UserAchivementRequest : RequestBase
    {
    }

    public class UserAchivementResponse : ResponseBase 
    {
        public int userLevel { get; set; }
        public long userExp { get; set; }
        public long highestClearedStageId { get; set; }
    }
}
