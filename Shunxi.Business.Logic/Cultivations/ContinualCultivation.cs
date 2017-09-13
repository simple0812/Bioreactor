using System;
using System.Linq;
using Shunxi.Business.Models.devices;
using Shunxi.Common.Log;

namespace Shunxi.Business.Logic.Cultivations
{
    public class ContinualCultivation : BaseCultivation
    {
        public ContinualCultivation(Pump pump) : base(pump)
        {
           
        }

        public virtual int GetFirstSpan()
        {
            return (Device.FirstSpan <= 0 ? Device.Period : Device.FirstSpan) * (int)Device.TimeType;
        }

        public override void CalcSchedules()
        {
            Schedules.Clear();
            
            Schedules.Add(Device.StartTime);
            var nextTime = Device.StartTime.AddMinutes(GetFirstSpan());

            //如下一次开始时间 < endtime,且够一个周期的时间
            if (nextTime.AddMinutes(Device.Period * (int)Device.TimeType) <= Device.EndTime)
            {
                Schedules.Add(nextTime);

                while (true)
                {
                    nextTime = nextTime.AddMinutes(Device.Period * (int)Device.TimeType);
                    if (nextTime.AddMinutes(Device.Period * (int)Device.TimeType) > Device.EndTime) break;
                    Schedules.Add(nextTime);
                }
            }

            LogFactory.Create().Info($"DEVICE{Device.DeviceId} CalcSchedules ==>{Schedules.Count}, first{Schedules.FirstOrDefault():yyyy-MM-dd HH:mm:ss}, last{Schedules.LastOrDefault():yyyy-MM-dd HH:mm:ss}");
        }

        public override Tuple<DateTime, double, double> GetNextRunParams(bool isFirst)
        {
            throw new NotImplementedException();
        }
    }
}
