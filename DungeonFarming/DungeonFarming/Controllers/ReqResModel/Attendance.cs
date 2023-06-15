namespace DungeonFarming.Controllers.ReqResModel;

public class AttendanceRequest : RequestBase
{
}
public class AttendanceResponse : ResponseBase
{
    public short attendanceStack { get; set; }
}
public class AttendanceGetStackRequst : RequestBase
{
}

public class AttendanceGetStackResponse : ResponseBase
{
    public short attendanceStack { get; set; }
}
