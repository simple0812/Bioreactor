namespace Shunxi.Business.Protocols.SimDirectives
{
    public class HttpBearerCloseDirective : BaseSimDirective
    {
        public override string DirectiveText => "AT+SAPBR=0,1";
    }
}
