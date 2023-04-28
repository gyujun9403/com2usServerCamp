namespace DungeonFarming.DTOs
{
    public class AuthBaseBodyData
    {
        public string user_id { get; set; }
    }
    public class RegisteReqBodyData : AuthBaseBodyData
    {
        public string password { get; set; }
    }

    public class LoginReqBodyData : AuthBaseBodyData
    {
        public string password { get; set; }
    }

    public class LogoutReqBodyData : AuthBaseBodyData
    {
        public string token { get; set; }
    }

    public class DeleteAccountBodyData : AuthBaseBodyData
    {
        public string password { get; set; }
        public string token { get; set; }
    }
}
