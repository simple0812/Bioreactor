using System;
using Shunxi.Business.Enums;
using Shunxi.Business.Models.devices;
using Shunxi.Common.Log;
using Shunxi.Infrastructure.Common.Configuration;

namespace Shunxi.Business.Logic.Cultivations
{
    public class VariantCultivation : ContinualCultivation
    {
        //Period表示翻倍时间
        public VariantCultivation(Pump pump) : base(pump)
        {
        }

        //
        //如果输入泵与输出泵绑定 则输出泵每次输出量为输入泵上一次输入量的80% 
        //输出每次翻倍的时候运行一次  
        //变量模式下 输入泵输出间隔1小时 输出泵输出间隔为设置的翻倍间隔
        //times为运行次数
        public double GetTotalVolumeForTotalTimeDurationWhenBind(int runTimes)
        {
            var totalVolume = 0D;
            if (Device.Period == 0)
            {
                return 0;
            }

            if (runTimes == 0)
            {
                return Device.Direction == DirectionEnum.Anticlockwise ? Device.InitialVolume : Device.InitialVolume * 0.8;
            }

            if (Device.Direction == DirectionEnum.Anticlockwise)
            {
                var multiple = (int)Math.Ceiling((double)runTimes / (Device.Period * (int)Device.TimeType));
                totalVolume = Device.InitialVolume * Math.Pow(2, multiple - 1) * 1.8;
            }
            else
            {
                totalVolume = Device.InitialVolume * Math.Pow(2, runTimes) * 0.8;
            }

            return totalVolume;
        }


        //times为运行次数
        public double GetTotalVolumeForTotalTimeDurationWhenNoBind(int runTimes)
        {
            var totalVolume = 0D;
            if (Device.Period == 0)
            {
                return 0;
            }

            if (runTimes == 0)
            {
                return Device.InitialVolume;
            }
            var multiple = (int)Math.Ceiling((decimal)runTimes / (Device.Period * (int)Device.TimeType));
            totalVolume = Device.InitialVolume * Math.Pow(2, multiple);

            return totalVolume;
        }

        //通过添加的次数获取当前周期需要添加的总的体积
        public double GetTotalVolumeForTotalTimeDuration(int times)
        {
          return  GetTotalVolumeForTotalTimeDurationWhenNoBind(times);
        }

        public double GetVolumePerTime(int times)
        {
            var total = GetTotalVolumeForTotalTimeDuration(times);

            if (Device.Period == 0) return total;

            if (Device.Direction == DirectionEnum.Anticlockwise)
                return times == 0 ? total : total / (Device.Period * (int)Device.TimeType);

            return GetTotalVolumeForTotalTimeDuration(times);
        }

        public int GetAllTimes()
        {
            if (Device.Period == 0) return 1;

            var startTime = Device.StartTime;
            var endTime = Device.EndTime;

            var total = (endTime - startTime).TotalMinutes;

            return (int)Math.Floor(total / (Device.Period *(int)Device.TimeType));
        }

        //通过翻倍的次数获取当前周期需要添加的总的体积
        public double GetTotalVolumeByDoubleTimes(int doubleTimes)
        {
            var totalVolume = 0.0D;
            if (Device.Direction == DirectionEnum.Anticlockwise)
            {
                totalVolume = Device.InitialVolume * Math.Pow(2, doubleTimes - 1) * 1.8;
            }
            else
            {
                totalVolume = Device.InitialVolume * Math.Pow(2, doubleTimes) * 0.8;
            }

            return totalVolume;

        }

        public bool Check(out string errMsg)
        {
            errMsg = "";
            var time = GetAllTimes();
            LogFactory.Create().Info($"{time}");
            if (time > Config.MAX_VARIANT_TIMES)
            {
                errMsg = $"pump{Device.DeviceId}变量模式最多能翻倍为{Config.MAX_VARIANT_TIMES}次";
                return false;
            }

            var input = GetTotalVolumeByDoubleTimes(time);

            LogFactory.Create().Info($"{input} {time}");

            if (input > Config.MAX_VARIANT_VOLUME)
            {
                errMsg = $"pump{Device.DeviceId}变量模式最大输入量为{Config.MAX_VARIANT_VOLUME}ml";
                return false;
            }

            var perTime = GetVolumePerTime(time) / Device.InitialFlowRate;
            if (perTime > Config.MAX_VARIANT_SINGLE_RUNTIME)
            {
                errMsg = $"pump{Device.DeviceId}变量模式单次最大输入时间为{Config.MAX_VARIANT_SINGLE_RUNTIME}min";
                return false;
            }

            return true;
        }

       
    }
}
