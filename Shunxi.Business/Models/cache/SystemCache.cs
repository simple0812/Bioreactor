using System;

namespace Shunxi.Business.Models.cache
{
    public class SystemCache
    {
        public SystemIntegration System { get; set; }
        public readonly SystemRealTimeStatus SystemRealTimeStatus;

        public SystemCache(SystemIntegration system)
        {
            System = system;
            SystemRealTimeStatus = new SystemRealTimeStatus(system);
        }
        public SystemRealTimeStatus SyncCurTime()
        {
            SystemRealTimeStatus.CurrTime = DateTime.Now;
            return SystemRealTimeStatus;
        }
    }
}
