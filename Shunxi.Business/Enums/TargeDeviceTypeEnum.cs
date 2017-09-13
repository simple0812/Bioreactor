using System.Runtime.Serialization;

namespace Shunxi.Business.Enums
{
    public enum TargetDeviceTypeEnum
    {
        [EnumMember(Value = "Unknown")] Unknown = -1,

        [EnumMember(Value = "Pump")] Pump = 0,

        [EnumMember(Value = "Rocker")] Rocker = 1,

        [EnumMember(Value = "Temperature")] Temperature = 2,

        [EnumMember(Value = "Gas")] Gas = 3,

        [EnumMember(Value = "Ph")] Ph = 4,

        [EnumMember(Value = "Do")] Do = 5
    }
}
