namespace DungeonFarming.DataBase.AccountDb
{
    public class AccountDbModel
    {
        public Int64? pk_id { get; set; }
        public String account_id { get; set;}
        public byte[] salt { get; set;}
        public byte[] hashed_password { get; set;}
    }
}
