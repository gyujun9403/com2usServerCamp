public class DeleteAccountResponse : ResponseBaseData
{
}

public class DeleteAccountRequest : RequestBaseData
{
    public string password { get; set; }
    public string token { get; set; }
}
