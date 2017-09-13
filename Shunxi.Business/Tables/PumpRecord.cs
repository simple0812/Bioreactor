using System;

namespace Shunxi.Business.Tables
{
    public class PumpRecord
    {
        public int Id { get; set; }
        public int DeviceId { get; set; }
        public bool IsManual { get; set; }
        public double FlowRate { get; set; }
        public double Volume { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
