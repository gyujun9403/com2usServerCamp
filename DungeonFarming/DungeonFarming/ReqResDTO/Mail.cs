public class MailPreviewRequest : RequestBase
{
    public Int16 page { get; set; }
}

public class GetMailRequest : RequestBase
{
    public Int64 mailId { get; set; }
}

public class RecvMailItemsRequest : RequestBase
{
    public Int64 mailId { get; set; }
}

public class DeleteMailRequest : RequestBase
{
    public Int64 mailId { get; set; }
}

public class MailPreviewResponse : ResponseBase
{
    public List<MailPreview>? mailDataList { get; set; }
}

public class GetMailResponse : ResponseBase
{
    public Mail? mail { get; set; }
}

public class RecvMailItemsResponse : ResponseBase
{
}

public class DeleteMailResponse : ResponseBase
{
}
