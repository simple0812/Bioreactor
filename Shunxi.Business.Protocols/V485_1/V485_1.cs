using System.Collections.Generic;
using System.Linq;
using Shunxi.Business.Enums;
using Shunxi.Business.Models;
using Shunxi.Business.Protocols.Directives;
using Shunxi.Business.Protocols.Enums;
using Shunxi.Business.Protocols.Helper;

namespace Shunxi.Business.Protocols.V485_1
{
    [Version(ProtocolVersion.V485_1)]
    public class V485_1 : IProtocol
    {
        public byte[] GenerateDirectiveBuffer(BaseDirective directive)
        {
            switch (directive.DirectiveType)
            {
                case DirectiveTypeEnum.Idle:
                    return GenerateDirectiveBuffer(directive as IdleDirective);

                case DirectiveTypeEnum.TryStart:
                    return GenerateDirectiveBuffer(directive as TryStartDirective);

                case DirectiveTypeEnum.TryPause:
                    return GenerateDirectiveBuffer(directive as TryPauseDirective);

                case DirectiveTypeEnum.Close:
                    return GenerateDirectiveBuffer(directive as CloseDirective);

                case DirectiveTypeEnum.Running:
                    return GenerateDirectiveBuffer(directive as RunningDirective);

                case DirectiveTypeEnum.Pausing:
                    return GenerateDirectiveBuffer(directive as PausingDirective);

                default:
                    return null;
            }
        }

        public DirectiveResult ResolveFeedback(byte[] bytes)
        {
            if (bytes == null || bytes.Length <= 3) return null;

            var directiveType = (TargetDeviceTypeEnum) bytes[bytes.Length - 3];
            var resolver = ResolverFactory.Create(bytes[0]);

            return resolver.ResolveFeedback(bytes);
        }

        private byte[] GenerateDirectiveBuffer(CloseDirective directive)
        {
            return GetCommonBufferData(directive);
        }

        private byte[] GenerateDirectiveBuffer(TryStartDirective directive)
        {
            if (null == directive)
                return new byte[0];

            IList<byte> list = new List<byte>();

            list.Add((byte)directive.TargetDeviceId);
            list.Add((byte)directive.DirectiveType);
            list = list.Concat(DirectiveHelper.ParseNumberTo2Bytes(directive.Param1)).ToList();
            list = list.Concat(DirectiveHelper.ParseNumberTo2Bytes(directive.Param2)).ToList();
            list.Add((byte)directive.Mode);
            list = list.Concat(DirectiveHelper.ParseNumberTo2Bytes(directive.DirectiveId)).ToList();
            list.Add((byte)directive.DeviceType);

            list = list.Concat(DirectiveHelper.GenerateCheckCode(list.ToArray())).ToList();

            return list.ToArray();
        }

        private byte[] GenerateDirectiveBuffer(IdleDirective directive)
        {
            return GetCommonBufferData(directive);
        }

        private byte[] GenerateDirectiveBuffer(TryPauseDirective directive)
        {
            return GetCommonBufferData(directive);
        }

        private byte[] GenerateDirectiveBuffer(RunningDirective directive)
        {
            return GetCommonBufferData(directive);
        }

        private byte[] GenerateDirectiveBuffer(PausingDirective directive)
        {
            return GetCommonBufferData(directive);
        }

        private static byte[] GetCommonBufferData(BaseDirective directive)
        {
            if (null == directive)
                return new byte[0];

            IList<byte> list = new List<byte>();

            list.Add((byte)directive.TargetDeviceId);
            list.Add((byte)directive.DirectiveType);
            list = list.Concat(DirectiveHelper.ParseNumberTo2Bytes(directive.DirectiveId)).ToList();
            list.Add((byte)directive.DeviceType);

            list = list.Concat(DirectiveHelper.GenerateCheckCode(list.ToArray())).ToList();

            return list.ToArray();
        }
        
    }
}
