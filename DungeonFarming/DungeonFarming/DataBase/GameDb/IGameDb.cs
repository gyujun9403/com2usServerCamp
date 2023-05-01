namespace DungeonFarming.DataBase.GameDb
{
    public interface IGameDb
    {
        // 유저를 등록 -> registe시
        Task<ErrorCode> RegistUserInGame(Int64 userId);
        // 유저 접속시 연접/미접 일수 갱신 -> 로그인시
        Task<ErrorCode> UpdateUserConnectDate(Int64 userId);
        // 유저의 기본 지급 아이템 목록을 받아옴
        Task<(ErrorCode, DefaultItemList?)> GetDefaultItemList(Int16 listId);
        // 유저의 인벤토리에 기본 지급 아이템들을 지급
        Task<ErrorCode> SetItemListInUserInventory(Int64 userId, IItemList items);

        Task<(ErrorCode, Inventory?)> GetInventory(Int64 user_id);
    }
}
