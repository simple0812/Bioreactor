using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using Shunxi.Business.Models.devices;
using Shunxi.Business.Tables;
using Shunxi.Common.Log;

namespace Shunxi.DataAccess
{
    /*
       存储所有培养周期设置的数据
       该数据库唯一
    */
    public class IotContext : DbContext
    {
        public static int RequiredDatabaseVersion = 1;
        public DbSet<AppException> AppExceptions { get; set; }
        public DbSet<CellCultivation> CellCultivations { get; set; }
        public DbSet<Pump> Pumps { get; set; }
        public DbSet<Rocker> Rockers { get; set; }
        public DbSet<Gas> Gases { get; set; }
        public DbSet<TemperatureGauge> TemperatureGauges { get; set; }

        public DbSet<TemperatureRecord> TemperatureRecords { get; set; }
        public DbSet<PumpRecord> PumpRecords { get; set; }
        public DbSet<GasRecord> GasRecords { get; set; }
        public DbSet<SchemaInfo> SchemaInfoes { get; set; }

        public void Initialize()
        {
            using (IotContext courseraContext = new IotContext())
            {
                
                int currentVersion = 0;
                try
                {
                    if (courseraContext.SchemaInfoes.Any())
                        currentVersion = courseraContext.SchemaInfoes.Max(x => x.Version);
                }
                catch (Exception e)
                {
                    LogFactory.Create().Warnning(e.Message);
                }
                var mmSqliteHelper = new ContextHelper();

                while (currentVersion < RequiredDatabaseVersion)
                {
                    currentVersion++;
                    foreach (string migration in mmSqliteHelper.Migrations[currentVersion])
                    {
                        courseraContext.Database.ExecuteSqlCommand(migration);
                    }
                    courseraContext.SchemaInfoes.Add(new SchemaInfo() { Version = currentVersion });
                    courseraContext.SaveChanges();
                }
            }
        }
    }

    public class SchemaInfo
    {
        public Int64 Id { get; set; }
        public int Version { get; set; }
    }

}
