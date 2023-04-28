namespace DungeonFarming.BodyData
{
    public class LoginRequest : RequestBaseData
    {
        public string password { get; set; }
    }
    public class LoginResponse : ResponseBaseData
    {
        public string token { get; set; }
    }
}
