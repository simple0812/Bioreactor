using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Shunxi.Business.Tables;

namespace Shunxi.DataAccess
{
    /*
       存储每个周期的反馈数据
       该数据库不唯一，每开始一个培养周期新建一个数据库(数据库名称：时间戳.db)
    */
    public class MyDbContext : DbContext
    {
        public DbSet<AppException> AppExceptions { get; set; }
        public DbSet<TemperatureRecord> TemperatureRecords { get; set; }
        public DbSet<PumpRecord> PumpRecords { get; set; }
        public DbSet<GasRecord> GasRecords { get; set; }

        private MyDbContext() { }

        public MyDbContext(string databaseName)
            : base(databaseName)
        {
        }

        public MyDbContext Init()
        {
            string cmdText = @"
                CREATE TABLE IF NOT EXISTS AppExceptions (
                    id INTEGER  primary key autoincrement, 
                    ModuleName varchar(50),
                    Priority INTEGER ,
                    CreatedAt TimeStamp NOT NULL DEFAULT (datetime('now','localtime')) ,
                    Message varchar(50) ,
                    StackTrace varchar(500) ,
                    Status INTEGER ,
                    BatchNumber varchar(100),
                    Type varchar(50) 
                );

                CREATE TABLE IF NOT EXISTS TemperatureRecords (
                    id INTEGER  primary key autoincrement, 
                    DeviceId INTEGER,
                    CreatedAt TimeStamp NOT NULL DEFAULT (datetime('now','localtime')) ,
                    HeaterTemperature double,
                    EnvTemperature double,
                    Temperature double
                );

                CREATE TABLE IF NOT EXISTS GasRecords (
                    id INTEGER  primary key autoincrement, 
                    DeviceId INTEGER,
                    CreatedAt TimeStamp NOT NULL DEFAULT (datetime('now','localtime')) ,
                    Concentration double,
                    FlowRate double
                );

                CREATE TABLE IF NOT EXISTS PumpRecords (
                    id INTEGER  primary key autoincrement, 
                    DeviceId INTEGER,
                    IsManual INTEGER,
                    StartTime TimeStamp NOT NULL DEFAULT (datetime('now','localtime')) ,
                    EndTime TimeStamp NOT NULL DEFAULT (datetime('now','localtime')) ,
                    Volume double,
                    FlowRate double
                );
            ";

            this.Database.ExecuteSqlCommand(cmdText);

            return this;
        }
    }
}
