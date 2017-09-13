using System;
using Shunxi.Business.Enums;

namespace Shunxi.Business.Models.devices
{
    public class Pump :BaseDevice
    {

        public override TargetDeviceTypeEnum DeviceType => TargetDeviceTypeEnum.Pump;
        #region 属性

        private DirectionEnum _direction;
        public DirectionEnum Direction
        {
            get { return _direction; }
            set
            {
                _direction = value;
                OnPropertyChanged();
            }
        }

        private int _period = 3;
        public int Period
        {

            get { return _period; }
            set
            {
                _period = value;
                OnPropertyChanged();
            }
        }

        private int _firstSpan = 0;
        public int FirstSpan
        {

            get { return _firstSpan; }
            set
            {
                _firstSpan = value;
                OnPropertyChanged();
            }
        }

        private DateTime _startTime = DateTime.Now;
        public DateTime StartTime
        {
            get { return _startTime; }
            set
            {
                _startTime = value;
                OnPropertyChanged();
            }
        }

        private DateTime _endTime = DateTime.Now;
        public DateTime EndTime
        {
            get { return _endTime; }
            set
            {
                _endTime = value;
                OnPropertyChanged();
            }
        }

        private double _initialVolume;
        public double InitialVolume
        {
            get { return _initialVolume; }
            set
            {
                _initialVolume = value;
                OnPropertyChanged();
            }
        }

        private double initialFlowRate = 0;
        public double InitialFlowRate
        {
            get { return initialFlowRate; }
            set
            {
                initialFlowRate = value;
                OnPropertyChanged();
            }
        }

        private double _volume = 5;
        public double Volume
        {
            get { return _volume; }
            set
            {
                _volume = value;
                OnPropertyChanged();
            }
        }

        private double _flowRate = 5;
        public double FlowRate
        {
            get { return _flowRate; }
            set
            {
                _flowRate = value;
                OnPropertyChanged();
            }
        }

        private ProcessModeEnum _processMode;
        public ProcessModeEnum ProcessMode
        {
            get { return _processMode; }
            set
            {
                _processMode = value;
                OnPropertyChanged();
            }
        }

        private TimeType _timeType = TimeType.Minute;
        public TimeType TimeType
        {
            get { return _timeType; }
            set
            {
                _timeType = value;
                OnPropertyChanged();
            }
        }

        #endregion


        public override bool Validate(ref string errMsg)
        {
            if (InitialVolume <= 0 || InitialVolume >= 5000)
            {
                errMsg = "加液量取值范围为0~5000";
                return false;
            }

            if (InitialFlowRate <= 0 || InitialFlowRate >= 100)
            {
                errMsg = "流速取值范围为0~100";
                return false;
            }

            if (StartTime >= EndTime)
            {
                errMsg = "开始时间必须小于结束时间";
                return false;
            }

            if (ProcessMode == ProcessModeEnum.FixedVolumeMode)
            {
                var ret = ValidateForFixedMode(ref errMsg);
                if (!ret) return false;
            }

            return base.Validate(ref errMsg);
        }

        private bool ValidateForFixedMode(ref string errMsg)
        {
            var firstSpan = InitialVolume / InitialFlowRate; //min
            var span = Volume / FlowRate;

            if (FlowRate <= 0 || FlowRate >= 100)
            {
                errMsg = "流速取值范围为0~100";
                return false;
            }

            if (Period != 0)
            {
                if (span >= Period * (int)TimeType || firstSpan >= Period * (int)TimeType)
                {
                    errMsg = $"每次运行时间大于Period";
                    return false;
                }
            }

            if ((FirstSpan == 0 && firstSpan > Period * (int)TimeType) || (FirstSpan != 0 && firstSpan > FirstSpan * (int)TimeType))
            {
                errMsg = $"首次运行时间大于FirstSpan";
                return false;
            }

            return true;
        }

    }
}
