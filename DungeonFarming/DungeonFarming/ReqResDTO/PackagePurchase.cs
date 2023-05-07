namespace DungeonFarming.DTO
{
    public class PackagePurchaseRequest : RequestBase
    {
        public String purchaseToken { get; set; }
        public Int16 packageCode { get; set; }
    }

    public class PackagePurchaseResponse : ResponseBase
    {
        public Int16 packageListId { get; set;}
    }
}
