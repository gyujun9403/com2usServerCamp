namespace DungeonFarming.DataBase.PurchaseDb
{
    public interface IPurchaseDb
    {
        Task<ErrorCode> CheckPurchaseDuplicated(string purchaseToken);
        Task<ErrorCode> WritePurchase(long userId, string purchaseToken, short packageCode);
    }


}
