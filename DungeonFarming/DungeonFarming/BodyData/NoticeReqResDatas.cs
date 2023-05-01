public class NoticeRequest : RequestBaseData
{
    public string token { get; set; }
}

public class NoticeResponse : ResponseBaseData
{
    public String notice { get; set; }
}
