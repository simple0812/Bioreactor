using Shunxi.Business.Models;

namespace Shunxi.Business.Protocols.V485_1
{
    internal interface IFeedbackResolver
    {
        DirectiveResult ResolveFeedback(byte[] bytes);
    }
}
