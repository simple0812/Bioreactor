using System;
using System.Threading.Tasks;
using Shunxi.Business.Protocols.Helper;
using Shunxi.Common.Log;

namespace Shunxi.Business.Protocols.SimDirectives
{
    public abstract class CompositeDirective
    {
        private int _retryTimes = 0;
        public abstract SimDirectiveType DirectiveType { get; }
        public Action<SimDirectiveResult> SuccessHandler { get; set; }

        public async Task Handle(SimDirectiveResult result)
        {
            if (result.IsExecOk && result.Status)
            {
                SuccessHandler?.Invoke(result);
            }
            else if (_retryTimes < 5)
            {
                LogFactory.Create().Warnning("sim resend times:" + _retryTimes + "," + result.Message);
                this._retryTimes++;
                await Task.Delay(500);
                SimWorker.Instance.Enqueue(this);
            }
            else
            {
                LogFactory.Create().Error("sim resend failed");
            }
        }
    }
}
