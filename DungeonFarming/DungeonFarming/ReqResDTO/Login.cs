using DungeonFarming.DataBase.GameDb.GameUserDataORM;
using System;
using System.Collections.Generic;
public class LoginRequest
{
    public String userId { get; set; }
    public String password { get; set; }
    public String clientVersion { get; set; }
    public String masterDataVersion { get; set; }
}

[Serializable]
public class LoginResponse : ResponseBase
{
    public String token { get; set; }
    public List<UserItem> userItems { get; set; } = new();
}
