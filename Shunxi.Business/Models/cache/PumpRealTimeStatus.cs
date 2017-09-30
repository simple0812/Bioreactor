using System;
using Shunxi.Business.Models.devices;

namespace Shunxi.Business.Models.cache
{
    public class PumpRealTimeStatus :BaseDevice
    {
        private double _RealTimeFlowRate;
        public double RealTimeFlowRate
        {
            get => _RealTimeFlowRate;
            set
            {
                _RealTimeFlowRate = value;
                OnPropertyChanged();
            }
        }

        private double _CurrVolume;
        public double CurrVolume
        {
            get => _CurrVolume;
            set
            {
                _CurrVolume = value;
                OnPropertyChanged();
            }
        }

        private int _RunTimes;
        public int RunTimes
        {
            get => _RunTimes;
            set
            {
                _RunTimes = value;
                OnPropertyChanged();
            }
        }

        private int _AllRunTimes;
        public int AllRunTimes
        {
            get => _AllRunTimes;
            set
            {
                _AllRunTimes = value;
                OnPropertyChanged();
            }
        }

        //泵第一次开始运行时间
        private DateTime _FirstTime = DateTime.MinValue;
        public DateTime FirstTime
        {
            get => _FirstTime;
            set
            {
                _FirstTime = value;
                OnPropertyChanged();
            }
        }

        //泵第一次开始运行时间
        private DateTime _LastTime = DateTime.MinValue;
        public DateTime LastTime
        {
            get => _LastTime;
            set
            {
                _LastTime = value;
                OnPropertyChanged();
            }
        }

        //本周期 泵开始时间
        private DateTime _TheStartTime = DateTime.MinValue;
        public DateTime TheStartTime
        {
            get => _TheStartTime;
            set
            {
                _TheStartTime = value;
                OnPropertyChanged();
            }
        }

        //本周期 泵预计结束时间
        private DateTime _TheEndTime = DateTime.MinValue;
        public DateTime TheEndTime
        {
            get => _TheEndTime;
            set
            {
                _TheEndTime = value;
                OnPropertyChanged();
            }
        }

        //泵预计下一周期开始时间
        private DateTime _NextTime = DateTime.MinValue;
        public DateTime NextTime
        {
            get => _NextTime;
            set
            {
                _NextTime = value;
                OnPropertyChanged();
            }
        }
    }
}
