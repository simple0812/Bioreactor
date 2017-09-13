namespace Shunxi.Business.Protocols.SimDirectives
{
    public class CengWriteDirective: BaseSimDirective
    {
        public override string DirectiveText => "AT+CENG=3";
    }
}
