using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shunxi.Business.Enums;
using Shunxi.Business.Models.devices;

namespace Shunxi.Business.Models.cache
{
    public class SystemRealTimeStatus
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public SysStatusEnum CurrStatus { get; set; }

        public double CurrVolume { get; set; }
        public DateTime CurrTime { get; set; }

        public TemperatureGauge Temperature { get; set; }
        public Rocker Rocker { get; set; }
        public Gas Gas { get; set; }
        public PhDevice Ph { get; set; }
        public DoDevice Do { get; set; }
        public PumpRealTimeStatus In { get; set; }
        public PumpRealTimeStatus Out { get; set; }

        public SystemRealTimeStatus(SystemIntegration system)
        {
            In = new PumpRealTimeStatus()
            {
                DeviceId = system.PumpIn.DeviceId,
                IsEnabled = system.PumpIn.IsEnabled,
                Icon = system.PumpIn.Icon,
                Name = system.PumpIn.Name,
                Device = system.PumpIn
            };

            Out = new PumpRealTimeStatus()
            {
                DeviceId = system.PumpOut.DeviceId,
                IsEnabled = system.PumpOut.IsEnabled,
                Icon = system.PumpOut.Icon,
                Name = system.PumpOut.Name,
                Device = system.PumpOut
            };
            
            Rocker = new Rocker()
            {
                DeviceId = system.Rocker.DeviceId,
                IsEnabled = system.Rocker.IsEnabled,
                Icon = system.Rocker.Icon,
                Name = system.Rocker.Name,
            };
            Temperature = new TemperatureGauge()
            {
                DeviceId = system.TemperatureGauge.DeviceId,
                IsEnabled = system.TemperatureGauge.IsEnabled,
                Icon = system.TemperatureGauge.Icon,
                Name = system.TemperatureGauge.Name,
            };
            Gas = new Gas()
            {
                DeviceId = system.Gas.DeviceId,
                IsEnabled = system.Gas.IsEnabled,
                Icon = system.Gas.Icon,
                Name = system.Gas.Name,
            };
            Ph = new PhDevice()
            {
                DeviceId = system.Ph.DeviceId,
                IsEnabled = system.Ph.IsEnabled,
                Icon = system.Ph.Icon,
                Name = system.Ph.Name,
            };
            Do = new DoDevice()
            {
                DeviceId = system.Do.DeviceId,
                IsEnabled = system.Do.IsEnabled,
                Icon = system.Do.Icon,
                Name = system.Do.Name,
            };
        }

        //列表会用于修改页面，必须防止其他线程更改该列表的值
        public IList<BaseDevice> GetDevices()
        {
            return new List<BaseDevice>()
            {
                In.CloneProperties(),
                Out.CloneProperties(),
                Temperature.CloneProperties(),
                Rocker.CloneProperties(),
                Gas.CloneProperties(),
                Ph.CloneProperties(),
                Do.CloneProperties()
            };
        }

        public void Update(int id, bool isRunning, double volume, double flowRate, int runtimes, int allTimes, DateTime ftime, DateTime ltime, DateTime stime, DateTime etime, DateTime ntime)
        {
            var pump = In.DeviceId == id ? In : Out;
            pump.RealTimeFlowRate = flowRate;
            pump.CurrVolume = volume;
            pump.IsRunning = isRunning;
            pump.RunTimes = runtimes;
            pump.FirstTime = ftime;
            pump.LastTime = ltime;
            pump.TheStartTime = stime;
            pump.TheEndTime = etime;
            pump.NextTime = ntime;
            pump.AllRunTimes = allTimes;
        }

        public void Update(int id, bool isRunning, double volume, double flowRate, int runtimes)
        {
            var pump = In.DeviceId == id ? In : Out;
            pump.IsRunning = isRunning;
            pump.RunTimes = runtimes;
            pump.RealTimeFlowRate = flowRate;
            pump.CurrVolume = volume;
        }
    }
}
