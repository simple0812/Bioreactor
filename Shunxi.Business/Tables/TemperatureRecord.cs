using System;

namespace Shunxi.Business.Tables
{
    public class TemperatureRecord
    {
        public int Id { get; set; }
        public int CellCultivationId { get; set; }
        public int DeviceId { get; set; }
        public double HeaterTemperature { get; set; }
        public double EnvTemperature { get; set; }
        public DateTime CreatedAt { get; set; }
        public double Temperature { get; set; }
    }
}
