public class DeleteAccountResponse : ResponseBase
{
}

public class DeleteAccountRequest : RequestBase
{
    public string password { get; set; }
    public string token { get; set; }
}
