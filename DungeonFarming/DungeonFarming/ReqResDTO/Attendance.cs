namespace DungeonFarming.DTO;

public class AttendanceRequest : RequestBase
{
}
public class AttendanceResponse : ResponseBase
{
    public Int16 attendanceStack { get; set; }
}
public class AttendanceGetStackRequst : RequestBase
{
}

public class AttendanceGetStackResponse : ResponseBase
{
    public Int16 attendanceStack { get; set; }
}
