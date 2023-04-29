namespace DungeonFarming.BodyData
{
    public class LoginRequest : RequestBaseData
    {
        public String password { get; set; }
        public String clientVersion { get; set; }
        public String masterDataVersion { get; set; }
    }
    public class LoginResponse : ResponseBaseData
    {
        public String token { get; set; }
    }
}
