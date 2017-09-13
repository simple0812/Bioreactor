using System;
using Shunxi.Business.Models.devices;

namespace Shunxi.Business.Logic.Cultivations
{
    public class SingleCultivation : BaseCultivation
    {

        public override void CalcSchedules()
        {
            Schedules.Clear();
            Schedules.Add(Device.StartTime);
        }

        public override Tuple<DateTime, double, double> GetNextRunParams(bool isFirst)
        {

            var now = DateTime.Now;
            if (isFirst)
            {
                if (Schedules[0] < now)
                {
                    AdjustStartTimeWhenFirstRun(now - Schedules[0]);
                }

                return new Tuple<DateTime, double, double>(Schedules[0], Device.InitialFlowRate, Device.InitialVolume);
            }

            if (Schedules[0] < DateTime.Now )
            {
                return null;
            }

            return new Tuple<DateTime, double, double>(Schedules[0], Device.InitialFlowRate, Device.InitialVolume);
        }

        public SingleCultivation(Pump pump) : base(pump)
        {
        }
    }
}
