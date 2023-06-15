using System.ComponentModel.DataAnnotations;

namespace DungeonFarming.Controllers.ReqResModel;
public class RegisteRequest
{
    [Required]
    [MinLength(3, ErrorMessage = "Minimum  ID length is 3")]
    [MaxLength(20, ErrorMessage = "Maximum ID length is 20")]
    [RegularExpression("^[a-zA-Z0-9_-]+$", ErrorMessage = "Illegal characters in ID")]
    public String userAssignedId { get; set; }

    [Required]
    [MinLength(6, ErrorMessage = "Minimum Password length is 6")]
    [MaxLength(12, ErrorMessage = "Maximum Password length is 12")]
    [DataType(DataType.Password)]
    public String password { get; set; }
}
public class RegisterResponse : ResponseBase
{

}
