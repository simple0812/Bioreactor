using Shunxi.Business.Enums;

namespace Shunxi.Business.Tables
{
    public class AppException 
    {
        public int Id { get; set; }
        public string ModuleName { get; set; }
        public ExceptionPriority Priority { get; set; }
        public int CreatedAt { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public int Status { get; set; }
        // 新的培养周期批号
        public int CellCultivationId { get; set; }
        public string BatchNumber { get; set; }
        public string Type { get; set; }
    }
}
