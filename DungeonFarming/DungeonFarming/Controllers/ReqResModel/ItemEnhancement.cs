using DungeonFarming.DataBase.GameDb.GameDbModel;
using System.ComponentModel.DataAnnotations;

namespace DungeonFarming.Controllers.ReqResModel;

public class ItemEnhancementRequest : RequestBase
{
    [Range(0, long.MaxValue, ErrorMessage = "Invalid itemID")]
    public long itemId { get; set; }

    [Range(0, short.MaxValue, ErrorMessage = "Invalid enhancementCount")]
    public short enhancementCount { get; set; }
}
public class ItemEnhancementResponse : ResponseBase
{
    public long itemId { get; set; } = -1;
    public UserItem? userItems { get; set; } = new();
}
