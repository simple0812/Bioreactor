using System;
using System.Collections.Generic;
using System.Linq;
using Shunxi.Business.Models.devices;
using Shunxi.Common.Log;

namespace Shunxi.Business.Logic.Cultivations
{
    public abstract class BaseCultivation
    {
        public BaseCultivation(Pump pump)
        {
            Device = pump;
        }

        public Pump Device { get; set; }

        #region 重构
        public abstract void CalcSchedules();
        //获取下一次运行的时间、速度、体积
        public abstract Tuple<DateTime, double, double> GetNextRunParams(bool isFirst);

        //点击开始后 先计算并缓存所有的排期
        //点击暂停后记录暂停时间A  重新开始后记录开始时间B  将排期中大于A的数据增加B-A
        public IList<DateTime> Schedules = new List<DateTime>();

        public virtual void AdjustTimeForPause(DateTime stopTime)
        {
            LogFactory.Create().Info($"AdjustTimeForPause stopTime {stopTime:yyyy-MM-dd HH:mm:ss}");
            var now = DateTime.Now;
            var span = now - stopTime;//暂停时长

            for (var i = 0; i < Schedules.Count; i++)
            {
                //暂停只影响还未执行的排期
                if (Schedules[i] > stopTime)
                {
                    Schedules[i] = Schedules[i].Add(span);
                }
            }
        }

        public virtual void AdjustStartTimeWhenFirstRun(TimeSpan span)
        {
            LogFactory.Create().Info($"Device{Device.DeviceId} AdjustStartTimeWhenFirstRun {span.TotalMinutes:##.###} minutes");

            for (var i = 0; i < Schedules.Count; i++)
            {
                Schedules[i] = Schedules[i].Add(span);
            }
        }


        public bool hasNearStartTime(DateTime now)
        {
            return Schedules.Any(each => Math.Abs((each -now).TotalSeconds) < 5);
        }
        #endregion
    }
}
