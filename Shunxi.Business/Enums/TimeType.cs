using System.Runtime.Serialization;

namespace Shunxi.Business.Enums
{
    public enum TimeType
    {
        [EnumMember(Value = "Minute")]
        Minute = 1,
        [EnumMember(Value = "Hour")]
        Hour = 60
    }
}
