using Shunxi.Business.Enums;
using Shunxi.Business.Models.devices;

namespace Shunxi.Business.Logic.Cultivations
{
    public static class CultivationFactory
    {
        public static BaseCultivation GetCultivation(Pump pump)
        {
            switch (pump.ProcessMode)
            {
                case ProcessModeEnum.SingleMode: return new SingleCultivation(pump);
                case ProcessModeEnum.FixedVolumeMode: return new FixedCultivation(pump);
                case ProcessModeEnum.VariantVolumeMode: return new VariantCultivation(pump);
            }

            return null;
        }
    }
}
