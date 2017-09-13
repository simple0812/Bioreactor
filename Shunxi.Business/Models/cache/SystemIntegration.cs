using System.Collections.Generic;
using Shunxi.Business.Models.devices;

namespace Shunxi.Business.Models.cache
{
    public class SystemIntegration
    {
        //培养信息
        public CellCultivation CellCultivation { get; set; }

        public Pump PumpIn { get; set; }

        public Pump PumpOut { get; set; }

        public Rocker Rocker { get; set; }

        public Gas Gas { get; set; }

        public TemperatureGauge TemperatureGauge { get; set; }

        public PhDevice Ph { get; set; }

        public DoDevice Do { get; set; }

        public IList<BaseDevice> GetDevices()
        {
            IList<BaseDevice> Devices = new List<BaseDevice>()
            {
                PumpIn,
                PumpOut,
                TemperatureGauge,
                Rocker,
                Gas,
                Ph,
                Do
            };

            return Devices;
        }

        public bool Check(out string errMsg)
        {
            errMsg = "";
            return true; //PumpIn.Check(out errMsg) && Out.Check(out errMsg);
        }
    }
}
