using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shunxi.Business.Enums;

namespace Shunxi.Business.Models
{
    public class IoStatusChangeEventArgs : EventArgs
    {
        public TargetDeviceTypeEnum DeviceType { get; set; }
        public int DeviceId { get; set; }
        public DeviceStatusEnum IoStatus { get; set; }
        public DirectiveData Feedback { get; set; }
        public double Delta { get; set; } //体积改变的增量
    }
}
