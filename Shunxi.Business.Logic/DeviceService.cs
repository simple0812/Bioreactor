using System;
using Shunxi.Business.Logic.Controllers;
using Shunxi.Business.Models.cache;
using Shunxi.Business.Tables;
using Shunxi.DataAccess;

namespace Shunxi.Business.Logic
{
    public static class DeviceService
    {
        public static void InitData()
        {
            CurrentContext.BatchNumber = Guid.NewGuid().ToString();
        }

        public static string GetBatchNumber()
        {
            return CurrentContext.BatchNumber;
        }

        public static int GetPumpRunTimesById(int id)
        {
            var ctrl = ControlCenter.Instance.GetControllerById(id);
            if (ctrl == null) return 0;
            return ctrl.AlreadyRunTimes;
        }

        public static void SaveTemperatureRecord(TemperatureRecord record)
        {
            using (var ctx = new IotContext())
            {
                ctx.TemperatureRecords.Add(record);
                ctx.SaveChanges();
            }
        }

        public static void SavePumpRecord(PumpRecord record)
        {
            using (var ctx = new IotContext())
            {
                ctx.PumpRecords.Add(record);
                ctx.SaveChanges();
            }
        }

        public static void SaveGasRecord(GasRecord record)
        {
            using (var ctx = new IotContext())
            {
                ctx.GasRecords.Add(record);
                ctx.SaveChanges();
            }
        }
    }
}
