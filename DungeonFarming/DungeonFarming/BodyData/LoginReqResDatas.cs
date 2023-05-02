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
public class LoginResponse : ResponseBaseData
{
    public String token { get; set; }
    public List<ItemInfo> currencys { get; set; } = new();
    public List<ItemInfo> items { get; set; } = new();
}
