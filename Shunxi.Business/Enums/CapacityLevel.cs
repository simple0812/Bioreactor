using System;

namespace Shunxi.Business.Enums
{
    public enum CapacityLevel
    {
        L400 = 1,
        L500 = 2,
        L600 = 3,
        L700 = 4,
        L800 = 5,
        L900 = 6,
        L1000 = 7
    }

    public static class CapacityLevelHelper
    {
        public static CapacityLevel GetCapacityLevel(double capacity)
        {
            var lv = Math.Round(capacity / 100, 0, MidpointRounding.AwayFromZero) - 3;
            if (lv < 1) lv = 1;
            if (lv > 7) lv = 7;

            return (CapacityLevel)lv;
        }
    }
}
