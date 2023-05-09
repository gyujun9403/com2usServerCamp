namespace DungeonFarming
{
    public class LogEventId
    {
        // Controller 이벤트 id
        public static readonly EventId Regist = new EventId(1, "Regist");
        public static readonly EventId Login = new EventId(2, "Login");
        public static readonly EventId Logout = new EventId(3, "Logout");
        public static readonly EventId DailyLoginReward = new EventId(4, "DailyLoginReward");
        public static readonly EventId ItemEnhance = new EventId(5, "ItemEnhance");
        public static readonly EventId Mail = new EventId(6, "Mail");
        public static readonly EventId Notice = new EventId(7, "Notice");
        public static readonly EventId PackagePurchase = new EventId(8, "PackagePurchase");


        // Db 이벤트 id
        public static readonly EventId AccountDb = new EventId(101, "AccountDb");
        public static readonly EventId GameSessionDb = new EventId(102, "GameSessionDb");
        public static readonly EventId GameDb = new EventId(103, "GameDb");
        public static readonly EventId MasterDataOffer = new EventId(104, "MasterDataOffer");
        public static readonly EventId PurchaseDb = new EventId(105, "PurchaseDb");
    }
}
