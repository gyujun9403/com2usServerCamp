using DungeonFarming.DataBase.GameDb.GameDbModel;
using System.ComponentModel.DataAnnotations;

namespace DungeonFarming.DTO
{
    public class ItemEnhancementRequest : RequestBase
    {
        [Range(0, Int64.MaxValue, ErrorMessage = "Invalid itemID")]
        public Int64 itemId { get; set; }

        [Range(0, Int16.MaxValue, ErrorMessage = "Invalid enhancementCount")]
        public Int16 enhancementCount { get; set; }
    }
    public class ItemEnhancementResponse : ResponseBase
    {
        public Int64 itemId { get; set; } = -1;
        public UserItem? userItems { get; set; } = new();
    }
}
