using System.ComponentModel.DataAnnotations;

namespace DungeonFarming.Controllers.ReqResModel;

public class PackagePurchaseRequest : RequestBase
{
    [Required] public string purchaseToken { get; set; }
    [Required] public short packageCode { get; set; }
}

public class PackagePurchaseResponse : ResponseBase
{
    public short packageListId { get; set; }
}
