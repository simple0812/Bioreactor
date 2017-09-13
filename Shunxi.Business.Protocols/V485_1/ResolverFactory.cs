using Shunxi.Business.Enums;

namespace Shunxi.Business.Protocols.V485_1
{
    internal static class ResolverFactory
    {
        public static IFeedbackResolver Create(TargetDeviceTypeEnum deviceType)
        {
            IFeedbackResolver resolver = new PumpFeedbackResolver();
            switch (deviceType)
            {
                case TargetDeviceTypeEnum.Pump:
                    resolver = new PumpFeedbackResolver();
                    break;
                case TargetDeviceTypeEnum.Rocker:
                    resolver = new RockerFeedbackResolver();
                    break;
                case TargetDeviceTypeEnum.Temperature:
                    resolver = new ThemometerFeedbackResolver();
                    break;
                case TargetDeviceTypeEnum.Gas:
                    resolver = new GasFeedbackResolver();
                    break;
                default:
                    break;
            }

            return resolver;
        }

        public static IFeedbackResolver Create(int deviceId)
        {
            IFeedbackResolver resolver = new PumpFeedbackResolver();
            switch (deviceId)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                    resolver = new PumpFeedbackResolver();
                    break;
                case 0x80:
                    resolver = new RockerFeedbackResolver();
                    break;
                case 0x90:
                case 0xa0:
                case 0xa1:
                    resolver = new ThemometerFeedbackResolver();
                    break;
                case 0x91:
                case 0x92:
                    resolver = new GasFeedbackResolver();
                    break;
                default:
                    break;
            }

            return resolver;
        }
    }
}
