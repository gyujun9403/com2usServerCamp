namespace DungeonFarming.DTO
{
    public class PackagePurchaseRequest : RequestBaseData
    {
        public String purchaseToken { get; set; }
        public Int16 packageCode { get; set; }
    }

    public class PackagePurchaseResponse : ResponseBaseData
    {
        public Int16 packageListId { get; set;}
    }
}
