﻿namespace DungeonFarming.Controllers.ReqResModel;

public class NoticeRequest : RequestBase
{
}

public class NoticeResponse : ResponseBase
{
    public String? notice { get; set; }
}
