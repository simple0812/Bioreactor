using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shunxi.Business.Models.devices;

namespace Shunxi.App.CellMachine.Views.Devices
{
   public interface IEditDeviceView
    {
       bool ShowView(BaseDevice device);
   } 
}
