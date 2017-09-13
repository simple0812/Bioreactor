using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shunxi.Business.Enums;

namespace Shunxi.Business.Models
{
    public class RunStatusChangeEventArgs : EventArgs
    {
        public SysStatusEnum SysStatus { get; set; }
    }
}
