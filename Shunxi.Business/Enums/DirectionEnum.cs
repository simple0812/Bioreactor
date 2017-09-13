using System.Runtime.Serialization;

namespace Shunxi.Business.Enums
{
    public enum DirectionEnum
    {
        [EnumMember(Value = "In")]
        In = 0,
        [EnumMember(Value = "Out")]
        Out
    }
}
