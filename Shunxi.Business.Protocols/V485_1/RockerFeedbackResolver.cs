using System.Linq;
using Shunxi.Business.Enums;
using Shunxi.Business.Models;
using Shunxi.Business.Protocols.Helper;

namespace Shunxi.Business.Protocols.V485_1
{
    internal class RockerFeedbackResolver : IFeedbackResolver
    {
        public DirectiveResult ResolveFeedback(byte[] bytes)
        {
            if (bytes.Length <= 2) return null;
            var directiveType = (DirectiveTypeEnum)bytes[1];
            switch (directiveType)
            {
                case DirectiveTypeEnum.Idle:
                    return ParseIdleResultData(bytes);

                case DirectiveTypeEnum.TryStart:
                    return ParseTryStartResultData(bytes);

                case DirectiveTypeEnum.TryPause:
                    return ParseTryPauseResultData(bytes);

                case DirectiveTypeEnum.Close:
                    return ParseCloseResultData(bytes);

                case DirectiveTypeEnum.Running:
                    return ParseRunningResultData(bytes);

                case DirectiveTypeEnum.Pausing:
                    return ParsePausingResultData(bytes);

                default:
                    return null;
            }
        }

        private DirectiveResult ParseIdleResultData(byte[] bytes)
        {
            var ret = new DirectiveResult();
            if (!DirectiveHelper.IsValidationResult(bytes, ((DirectiveTypeEnum)bytes[1]).GetFeedbackLength()))
            {
                ret.Status = false;
                return ret;
            }

            ret.Status = true;
            var data = new RockerDirectiveData();

            data.DeviceId = bytes[0];
            data.DirectiveType = (DirectiveTypeEnum)bytes[1];
            data.Speed = DirectiveHelper.Parse2BytesToNumber(bytes.Skip(2).Take(2).ToArray());
            data.DirectiveId = (int)DirectiveHelper.Parse2BytesToNumber(bytes.Skip(4).Take(2).ToArray());
            data.DeviceType = TargetDeviceTypeEnum.Rocker;

            ret.Data = data;
            ret.SourceDirectiveType = DirectiveTypeEnum.Idle;

            return ret;
        }

        private DirectiveResult ParseTryStartResultData(byte[] bytes)
        {
            var ret = new DirectiveResult();
            if (!DirectiveHelper.IsValidationResult(bytes, ((DirectiveTypeEnum)bytes[1]).GetFeedbackLength()))
            {
                ret.Status = false;
                return ret;
            }

            ret.Status = true;
            var data = new RockerDirectiveData();

            data.DeviceId = bytes[0];
            data.DirectiveType = (DirectiveTypeEnum)bytes[1];
            data.DirectiveId = (int)DirectiveHelper.Parse2BytesToNumber(bytes.Skip(2).Take(2).ToArray());
            data.DeviceType = TargetDeviceTypeEnum.Rocker;

            ret.SourceDirectiveType = DirectiveTypeEnum.TryStart;
            ret.Data = data;

            return ret;
        }

        private DirectiveResult ParseTryPauseResultData(byte[] bytes)
        {
            var ret = new DirectiveResult();

            if (!DirectiveHelper.IsValidationResult(bytes, ((DirectiveTypeEnum)bytes[1]).GetFeedbackLength()))
            {
                ret.Status = false;
                return ret;
            }

            ret.Status = true;
            var data = new RockerDirectiveData();

            data.DeviceId = bytes[0];
            data.DirectiveType = (DirectiveTypeEnum)bytes[1];
            data.DirectiveId = (int)DirectiveHelper.Parse2BytesToNumber(bytes.Skip(2).Take(2).ToArray());
            data.DeviceType = TargetDeviceTypeEnum.Rocker;

            ret.SourceDirectiveType = DirectiveTypeEnum.TryPause;
            ret.Data = data;

            return ret;
        }

        private DirectiveResult ParseCloseResultData(byte[] bytes)
        {
            var ret = new DirectiveResult();
            if (!DirectiveHelper.IsValidationResult(bytes, ((DirectiveTypeEnum)bytes[1]).GetFeedbackLength()))
            {
                ret.Status = false;
                return ret;
            }

            ret.Status = true;
            var data = new RockerDirectiveData();

            data.DeviceId = bytes[0];
            data.DirectiveType = (DirectiveTypeEnum)bytes[1];
            data.DirectiveId = (int)DirectiveHelper.Parse2BytesToNumber(bytes.Skip(2).Take(2).ToArray());
            data.DeviceType = TargetDeviceTypeEnum.Rocker;

            ret.SourceDirectiveType = DirectiveTypeEnum.Close;
            ret.Data = data;

            return ret;
        }

        private DirectiveResult ParseRunningResultData(byte[] bytes)
        {
            var ret = new DirectiveResult();
            if (DirectiveHelper.IsValidationResult(bytes, ((DirectiveTypeEnum)bytes[1]).GetFeedbackLength() + 2))
            {
                return ParseLongRunningResultData(bytes);
            }

            if (DirectiveHelper.IsValidationResult(bytes, ((DirectiveTypeEnum) bytes[1]).GetFeedbackLength()))
            {
                return ParseNormalRunningResultData(bytes);
            }

            ret.Status = false;
            return ret;
        }

        private DirectiveResult ParseLongRunningResultData(byte[] bytes)
        {
            var ret = new DirectiveResult();

            ret.Status = true;
            var data = new RockerDirectiveData();

            data.DeviceId = bytes[0];
            data.DirectiveType = (DirectiveTypeEnum)bytes[1];
            data.CenterTemperature = DirectiveHelper.Parse2BytesToNumber(bytes.Skip(2).Take(2).ToArray()) / 10;
            data.Speed = DirectiveHelper.Parse2BytesToNumber(bytes.Skip(4).Take(2).ToArray());
            data.HeaterTemperature = DirectiveHelper.Parse2BytesToNumber(bytes.Skip(6).Take(2).ToArray()) / 10;
            data.EnvTemperature = DirectiveHelper.Parse2BytesToNumber(bytes.Skip(8).Take(2).ToArray()) / 10;
            data.DirectiveId = (int)DirectiveHelper.Parse2BytesToNumber(bytes.Skip(10).Take(2).ToArray());
            data.DeviceType = TargetDeviceTypeEnum.Rocker;

            ret.SourceDirectiveType = DirectiveTypeEnum.Running;
            ret.Data = data;

            return ret;
        }

        private DirectiveResult ParseNormalRunningResultData(byte[] bytes)
        {
            var ret = new DirectiveResult();

            ret.Status = true;
            var data = new RockerDirectiveData();

            data.DeviceId = bytes[0];
            data.DirectiveType = (DirectiveTypeEnum)bytes[1];
            data.Angle = (double)DirectiveHelper.Parse2BytesToNumber(bytes.Skip(2).Take(2).ToArray()) / 10;
            data.Speed = DirectiveHelper.Parse2BytesToNumber(bytes.Skip(4).Take(2).ToArray());
            data.DeviceStatus = bytes.Skip(6).Take(1).FirstOrDefault();
            data.RockMode = (RockEnum)bytes.Skip(7).Take(1).FirstOrDefault();
            data.DirectiveId = (int)DirectiveHelper.Parse2BytesToNumber(bytes.Skip(8).Take(2).ToArray());
            data.DeviceType = TargetDeviceTypeEnum.Rocker;

            ret.SourceDirectiveType = DirectiveTypeEnum.Running;
            ret.Data = data;

            return ret;
        }



        private DirectiveResult ParsePausingResultData(byte[] bytes)
        {
            var ret = new DirectiveResult();
            if (!DirectiveHelper.IsValidationResult(bytes, ((DirectiveTypeEnum)bytes[1]).GetFeedbackLength()))
            {
                ret.Status = false;
                return ret;
            }

            ret.Status = true;
            var data = new RockerDirectiveData();

            data.DeviceId = bytes[0];
            data.DirectiveType = (DirectiveTypeEnum)bytes[1];
            data.TimeInterval = (int)DirectiveHelper.Parse2BytesToNumber(bytes.Skip(2).Take(2).ToArray());
            data.Speed = DirectiveHelper.Parse2BytesToNumber(bytes.Skip(4).Take(2).ToArray());
            data.DeviceStatus = bytes.Skip(6).Take(1).FirstOrDefault();
            data.DirectiveId = (int)DirectiveHelper.Parse2BytesToNumber(bytes.Skip(7).Take(2).ToArray());
            data.DeviceType = TargetDeviceTypeEnum.Rocker;

            ret.SourceDirectiveType = DirectiveTypeEnum.Pausing;
            ret.Data = data;

            return ret;
        }
    }
}
