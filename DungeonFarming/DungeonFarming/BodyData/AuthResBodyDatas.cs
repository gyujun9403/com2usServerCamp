namespace DungeonFarming.BodyData
{
    public class BaseAuthResData
    {
        public ErrorCode errorCode { get; set; }
    }

    public class RegisterResData : BaseAuthResData
    {
        
    }
    public class LoginResData : BaseAuthResData
    {
        public string token { get; set; }
    }

    public class LogoutResData : BaseAuthResData
    {
    }

    public class DeleteAccountResData : BaseAuthResData
    {
    }
}
