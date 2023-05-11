using System.ComponentModel.DataAnnotations;

public class RequestBase
{
    [Required] public String userId { get; set; }
    [Required] public String token { get; set; }
    [Required] public String clientVersion { get; set; }
    [Required] public String masterDataVersion { get; set; }
}
