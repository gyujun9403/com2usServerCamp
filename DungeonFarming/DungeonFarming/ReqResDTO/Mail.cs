using System.ComponentModel.DataAnnotations;

public class MailPreviewRequest : RequestBase
{
    [Required]
    [Range(0, Int16.MaxValue, ErrorMessage = "Invalid page number")]
    public Int16 page { get; set; }
}

public class GetMailRequest : RequestBase
{
    [Required]
    [Range(0, Int64.MaxValue, ErrorMessage = "Invalid mail id")]
    public Int64 mailId { get; set; }
}

public class RecvMailItemsRequest : RequestBase
{
    [Required]
    [Range(0, Int64.MaxValue, ErrorMessage = "Invalid mail id")]
    public Int64 mailId { get; set; }
}

public class DeleteMailRequest : RequestBase
{
    [Required]
    [Range(0, Int64.MaxValue, ErrorMessage = "Invalid mail id")]
    public Int64 mailId { get; set; }
}

public class MailPreviewResponse : ResponseBase
{
    public List<MailListElem>? mailDataList { get; set; }
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
