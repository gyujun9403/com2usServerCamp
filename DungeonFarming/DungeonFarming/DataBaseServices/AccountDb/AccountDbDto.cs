namespace DungeonFarming.DataBase.AccountDb
{
    public class UserAccountDto
    {
        public Int64? user_id { get; set; }
        public String user_assigned_id { get; set;}
        public byte[] salt { get; set;}
        public byte[] hashed_password { get; set;}
    }
}
