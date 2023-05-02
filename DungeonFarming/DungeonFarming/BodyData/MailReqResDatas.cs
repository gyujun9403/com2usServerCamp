public class MailRequest : RequestBaseData
{
    public Int16 page { get; set; }
}

// 최대 20개 까지
public class MailResponse : ResponseBaseData
{
    public List<MailData> mailDataList { get; set; } = new();
}
