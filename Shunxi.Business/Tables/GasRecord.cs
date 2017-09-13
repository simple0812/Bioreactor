using System;

namespace Shunxi.Business.Tables
{
    public class GasRecord
    {
        public int Id { get; set; }
        public int DeviceId { get; set; }
        public double FlowRate { get; set; }
        public double Concentration { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
