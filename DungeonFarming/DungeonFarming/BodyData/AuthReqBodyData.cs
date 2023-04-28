﻿namespace DungeonFarming.DTOs
{
    public class AuthBaseBodyData
    {
        public string user_id { get; set; }
        public string Password { get; set; }
    }
    public class RegisteReqBodyData : AuthBaseBodyData
    {
    }

    public class LoginReqBodyData : AuthBaseBodyData
    {
    }

    public class DeleteAccountBodyData : AuthBaseBodyData
    {
    }
}
