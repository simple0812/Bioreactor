using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shunxi.Business.Models
{
    public class DeviceIOResult
    {
        public DeviceIOResult()
        {
            Status = false;
            Code = "CANCEL";
        }

        public DeviceIOResult(bool status)
        {
            Status = status;
        }

        public DeviceIOResult(bool status, string code)
        {
            Status = status;
            Code = code;
        }

        public bool Status { get; set; }
        public string Code { get; set; }

    }
}
