namespace DungeonFarming.DataBase.AccountDb
{
    public class UserAccountsTuple
    {
        public Int64? pk_id { get; set; }
        public String user_id { get; set;}
        public byte[] salt { get; set;}
        public byte[] hashed_password { get; set;}
    }
}
