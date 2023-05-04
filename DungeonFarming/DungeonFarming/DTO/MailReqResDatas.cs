public class MailPreviewRequest : RequestBaseData
{
    public Int16 page { get; set; }
}

public class GetMailRequest : RequestBaseData
{
    public Int64 mailId { get; set; }
}

public class RecvMailItemsRequest : RequestBaseData
{
    public Int64 mailId { get; set; }
}

public class DeleteMailRequest : RequestBaseData
{
    public Int64 mailId { get; set; }
}

// 최대 20개 까지
public class MailPreviewResponse : ResponseBaseData
{
    public List<MailPreview>? mailDataList { get; set; }
}

public class GetMailResponse : ResponseBaseData
{
    public Mail? mail { get; set; }
}

public class RecvMailItemsResponse : ResponseBaseData
{
}

public class DeleteMailResponse : ResponseBaseData
{
}
