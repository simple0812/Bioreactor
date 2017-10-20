using System.Runtime.Serialization;

namespace Shunxi.Business.Enums
{
    public enum ProcessModeEnum
    {
        [EnumMember(Value = "SingleMode")]
        SingleMode = 0,

        [EnumMember(Value = "FixedVolumeMode")]
        FixedVolumeMode = 1,

//        [EnumMember(Value = "VariantVolumeMode")]
//        VariantVolumeMode = 2,
    }
}
