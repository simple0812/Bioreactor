using System.Runtime.Serialization;

namespace Shunxi.Business.Enums
{
    public enum IdleDesc
    {

        [EnumMember(Value = "start")]
        Start = 0,

        [EnumMember(Value = "completed")]
        Completed = 1,

        [EnumMember(Value = "paused")]
        Paused = 2,
    }
}
