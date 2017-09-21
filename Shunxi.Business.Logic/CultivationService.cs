using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shunxi.Business.Enums;
using Shunxi.Business.Logic.Cultivations;
using Shunxi.Business.Models.cache;
using Shunxi.Business.Models.devices;
using Shunxi.Common.Log;
using Shunxi.DataAccess;
using Shunxi.Infrastructure.Common.Configuration;

namespace Shunxi.Business.Logic
{
    public static class CultivationService
    {
        public static void Remove(int id)
        {
            using (var db = new IotContext())
            {
                db.Database.ExecuteSqlCommand(@"DELETE FROM CellCultivations WHERE Id=?;", id);
            }
        }

        public static string GetLastDbName()
        {
            using (var db = new IotContext())
            {
                var p = db.CellCultivations.OrderByDescending(each => each.CreatedAt).FirstOrDefault();
                if (p == null) return string.Empty;
                return p.CreatedAt + ".db";
            }
        }

        public static int GetLastCultivationId()
        {
            var id = CurrentContext.SysCache?.System?.CellCultivation?.Id ?? 0;
            if (id > 0) return id;

            using (var db = new IotContext())
            {
                var p = db.CellCultivations.OrderByDescending(doc => doc.Id).FirstOrDefault();
                return p?.Id ?? 0;
            }
        }

        public static void RemovePumpCultivationsByCultivationId(int id)
        {
            using (var db = new IotContext())
            {
                db.Database.ExecuteSqlCommand(@"DELETE FROM TPumpCultivation WHERE CultivationId=?;", id);
            }
        }

        private static Pump ConvertToPump(string content)
        {
            return JsonConvert.DeserializeObject<Pump>(content);
        }

        private static BaseCultivation ConvertCultivation(string content)
        {
            var obj = JObject.Parse(content);
            ProcessModeEnum mode;
            Enum.TryParse(obj["ProcessMode"].ToString(), out mode);
            BaseCultivation cultivation = null;
            switch (mode)
            {
                case ProcessModeEnum.SingleMode:
                    cultivation = JsonConvert.DeserializeObject<SingleCultivation>(content);
                    break;
                case ProcessModeEnum.FixedVolumeMode:
                    cultivation = JsonConvert.DeserializeObject<FixedCultivation>(content);
                    break;
                case ProcessModeEnum.VariantVolumeMode:
                    cultivation = JsonConvert.DeserializeObject<VariantCultivation>(content);
                    break;
                default:
                    break;
            }

            return cultivation;
        }

        public static SystemIntegration GetCultivations(string content)
        {
            if (string.IsNullOrEmpty(content)) return null;

            var obj = JObject.Parse(content);

            var desc = obj["Description"].ToString();
            var name = obj["Name"].ToString();
            var cell = obj["Cell"].ToString();
            var userName = obj["UserName"].ToString();
            var rocker = obj["Rocker"]?.ToString();
            var gas = obj["Gas"]?.ToString();
            var xin = obj["In"].ToString();
            var xout = obj["Out"].ToString();
            var isTemperatureEnabled = obj["IsTemperatureEnabled"].ToString();

            return new SystemIntegration()
            {
                CellCultivation = new CellCultivation() {
                    Description = desc,
                    Name = name,
                    UserName = userName,
                    Cell = cell,
                },
                TemperatureGauge = new TemperatureGauge() { IsEnabled = Convert.ToBoolean(isTemperatureEnabled) },
                Rocker = JsonConvert.DeserializeObject<Rocker>(rocker),
                Gas = JsonConvert.DeserializeObject<Gas>(gas),
                PumpIn = ConvertToPump(xin),
                PumpOut = ConvertToPump(xout)
            };
        }

        public static IList<CellCultivation> GetCultivationsFromDb()
        {
            using (var db = new IotContext())
            {
                return db.CellCultivations.ToList();
            }
        }

        public static SystemIntegration GetDefaultCultivation()
        {
            var pumpIn = new Pump()
            {
                DeviceId = Config.Pump1Id,
                Name = Config.Pump1Name,
                StartTime = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                EndTime = DateTime.Now.AddDays(10),
                Direction = DirectionEnum.Anticlockwise,
                InitialVolume = 10.0D,
                InitialFlowRate = 10.0D,
                IsEnabled = true,
                Icon = "/Assets/Filters/Products/Automation.png",
                ProcessMode = ProcessModeEnum.SingleMode
            };

            var pumpOut = new Pump()
            {
                DeviceId = Config.Pump3Id,
                Name = Config.Pump3Name,
                StartTime = DateTime.Parse(DateTime.Now.AddMinutes(1).ToString("yyyy-MM-dd HH:mm:ss")),
                EndTime = DateTime.Now.AddDays(10),
                Direction = DirectionEnum.Clockwise,
                InitialVolume = 10,
                InitialFlowRate = 10,
                IsEnabled = true,
                Icon = "/Assets/Filters/Products/Automation.png",
                ProcessMode = ProcessModeEnum.SingleMode
            };
            var tempe = new TemperatureGauge()
            {
                DeviceId = Config.TemperatureId,
                IsEnabled = true,
                Temperature = 38.0D,
                Icon = "/Assets/Filters/Products/Automation.png",
                Name = "Temperature"
            };
            var rocker = new Rocker()
            {
                DeviceId = Config.RockerId,
                Angle = Config.DefaultAngle,
                Speed = Config.DefaultSpeed,
                IsEnabled = true,
                Icon = "/Assets/Filters/Products/Automation.png",
                Name = "Rocker"
            };
            var gas = new Gas()
            {
                DeviceId = Config.GasId,
                Concentration = 5.0D,
                FlowRate = 400.0D,
                IsEnabled = true,
                Icon = "/Assets/Filters/Products/Automation.png",
                Name = "Gas"
            };
            var ph = new PhDevice()
            {
                Icon = "/Assets/Filters/Products/Automation.png",
                DeviceId = Config.PhId, PH = 7.0, IsEnabled = true, Name = "Ph"
            };
            var xdo = new DoDevice()
            {
                DeviceId = Config.DoId, DO =1.0D, IsEnabled = true, Name = "Do",
                Icon = "/Assets/Filters/Products/Automation.png"
            };
            return new SystemIntegration
            {
                TemperatureGauge = tempe,
                Rocker = rocker,
                Gas = gas,
                PumpIn = pumpIn,
                PumpOut = pumpOut,
                Ph = ph,
                Do = xdo,
                CellCultivation = new CellCultivation()
                {
                    Description = "",
                    Name = $"date{DateTime.Now:yyyyMMddHHmmss}",
                    UserName = "",
                    Cell = ""
                }
            };
        }

        public static SystemIntegration SaveCultivations(SystemIntegration schedule)
        {
            if (schedule == null) return null;
            DeviceService.InitData();

            var tSchedule = schedule.CellCultivation;
            tSchedule.BatchNumber = CurrentContext.BatchNumber;
            tSchedule.CreatedAt = Common.Utility.Common.ToUnixTime(DateTime.Now);

            using (var db = new IotContext())
            {
                db.CellCultivations.Add(tSchedule);
                db.SaveChanges();
            }

            using (var db = new IotContext())
            {
                schedule.Rocker.CultivationId = tSchedule.Id;
                db.Rockers.Add(schedule.Rocker);

                schedule.Gas.CultivationId = tSchedule.Id;
                db.Gases.Add(schedule.Gas);

                schedule.TemperatureGauge.DeviceId = tSchedule.Id;
                db.TemperatureGauges.Add(schedule.TemperatureGauge);

                var p = new List<Pump>() { schedule.PumpIn, schedule.PumpOut };

                foreach (var each in p)
                {
                    each.CultivationId = tSchedule.Id;
                    db.Pumps.Add(each);
                }

                db.SaveChanges();
            }

            return schedule;
        }
    }
}
