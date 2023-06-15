using DungeonFarming.DataBase.GameDb.GameDbModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class LoginRequest
{
    [Required] public String userAssignedId { get; set; }
    [Required] public String password { get; set; }
    [Required] public String clientVersion { get; set; }
    [Required] public String masterDataVersion { get; set; }
}

[Serializable]
public class LoginResponse : ResponseBase
{
    public String token { get; set; }
    public List<UserItem>? userItems { get; set; } = new();
}
