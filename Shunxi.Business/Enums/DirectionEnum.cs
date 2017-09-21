using System.Runtime.Serialization;

namespace Shunxi.Business.Enums
{
    public enum DirectionEnum
    {
        [EnumMember(Value = "In")]
        Anticlockwise = 0,
        [EnumMember(Value = "Out")]
        Clockwise
    }
}
