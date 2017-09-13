using System;
using Shunxi.Business.Models.devices;

namespace Shunxi.Business.Logic.Cultivations
{
    public class FixedCultivation : ContinualCultivation
    {

        public FixedCultivation(Pump pump) : base(pump)
        {
        }


        //如果是第一次运行 如果Schedules[0] < datetime.now 重新调整开始时间
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

            for (var i = 0; i < Schedules.Count; i++)
            {
                if (Schedules[i] >= now)
                {
                    var nextTime = Schedules[i];
                    var flowrate = i == 0 ? Device.InitialFlowRate : Device.FlowRate;
                    var volume = i == 0 ? Device.InitialVolume : Device.Volume;
                    return new Tuple<DateTime, double, double>(nextTime, flowrate, volume);
                }
            }

            return null;

        }

    }
}
