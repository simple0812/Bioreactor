using System;
using Shunxi.Business.Models;
using Shunxi.Business.Protocols.Directives;
using Shunxi.Business.Protocols.Enums;

namespace Shunxi.Business.Protocols
{
    [VersionList(new Type[] { typeof(V485_1.V485_1) })]
    public interface IProtocol
    {
        byte[] GenerateDirectiveBuffer(BaseDirective directive);
        DirectiveResult ResolveFeedback(byte[] directive);
    }
}
