using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shunxi.Business.Enums;

namespace Shunxi.Business.Models
{
    public class CommunicationEventArgs : EventArgs
    {
        public DeviceStatusEnum DeviceStatus { get; set; }
        public byte[] Command { get; set; }
        public TargetDeviceTypeEnum DeviceType { get; set; }
        public int DeviceId { get; set; }
        public int DirectiveId { get; set; }
        public DirectiveData Data { get; set; }

        public string Description { get; set; }


    }
}
