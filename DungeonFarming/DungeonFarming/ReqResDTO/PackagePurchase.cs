using System.ComponentModel.DataAnnotations;

namespace DungeonFarming.DTO
{
    public class PackagePurchaseRequest : RequestBase
    {
        [Required] public String purchaseToken { get; set; }
        [Required] public Int16 packageCode { get; set; }
    }

    public class PackagePurchaseResponse : ResponseBase
    {
        public Int16 packageListId { get; set;}
    }
}
