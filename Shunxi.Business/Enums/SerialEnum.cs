using System;
using System.Runtime.Serialization;

namespace Shunxi.Business.Enums
{
    public enum SerialEnum
    {
        [EnumMember(Value = "Unknowns")]
        Unknown = 0,
        [EnumMember(Value = "LowerComputer")]
        LowerComputer,
        [EnumMember(Value = "Sim")]
        Sim,
        [EnumMember(Value = "QrCode")]
        QrCode
    }
}

public class EnumMember : Attribute
{
   public string Value { get; set; }
}
