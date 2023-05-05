using DungeonFarming.DataBase.GameDb.GameUserDataORM;

namespace DungeonFarming.DTO
{
    public class ItemEnhancementRequest : RequestBaseData
    {
        public Int64 itemId { get; set; }
        public Int16 enhancementCount { get; set; }
    }
    public class ItemEnhancementResponse : ResponseBaseData
    {
        public Int64 itemId { get; set; } = -1;
        public UserItem? userItems { get; set; } = new();
    }
}
