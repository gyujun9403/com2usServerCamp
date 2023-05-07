using DungeonFarming.DataBase.GameDb.GameUserDataORM;

namespace DungeonFarming.DTO
{
    public class ItemEnhancementRequest : RequestBase
    {
        public Int64 itemId { get; set; }
        public Int16 enhancementCount { get; set; }
    }
    public class ItemEnhancementResponse : ResponseBase
    {
        public Int64 itemId { get; set; } = -1;
        public UserItem? userItems { get; set; } = new();
    }
}
