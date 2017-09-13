
namespace Shunxi.Business.Enums
{
    public enum DeviceStatusEnum
    {
        Idle = 0,
        PreStart,
        Startting, // 收到TryStart反馈
        Running,
        PrePause,
        Pausing, // 收到TryPause反馈
        AllFinished,//完成全部流程
        Error
    }
}
