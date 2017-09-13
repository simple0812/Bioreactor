using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.SQLite;
using System.Diagnostics;
using Shunxi.Business.Models.devices;
using Shunxi.Business.Tables;

namespace Shunxi.DataAccess
{
    /*
       存储所有培养周期设置的数据
       该数据库唯一
    */
    public class IotContext : DbContext
    {
        public DbSet<AppException> AppExceptions { get; set; }
        public DbSet<CellCultivation> CellCultivations { get; set; }
        public DbSet<Pump> Pumps { get; set; }
        public DbSet<Rocker> Rockers { get; set; }
        public DbSet<Gas> Gases { get; set; }
        public DbSet<TemperatureGauge> TemperatureGauges { get; set; }

        public DbSet<TemperatureRecord> TemperatureRecords { get; set; }
        public DbSet<PumpRecord> PumpRecords { get; set; }
        public DbSet<GasRecord> GasRecords { get; set; }

        public IotContext(string databaseName = "MyDatabase")
            : base(databaseName)
        {

        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            SQLiteConnectionStringBuilder connstr = new SQLiteConnectionStringBuilder(this.Database.Connection.ConnectionString);
            string path = AppDomain.CurrentDomain.BaseDirectory + connstr.DataSource;
            System.IO.FileInfo fi = new System.IO.FileInfo(path);
            if (!System.IO.Directory.Exists(fi.DirectoryName))
            {
                System.IO.Directory.CreateDirectory(fi.DirectoryName);
            }
            if (!System.IO.File.Exists(fi.FullName))
            {
                SQLiteConnection.CreateFile(fi.FullName);
            }

            connstr.DataSource = path;
            //connstr.Password = "admin";//设置密码，SQLite ADO.NET实现了数据库密码保护
            using (var conn = new SQLiteConnection(connstr.ConnectionString))
            {
                string cmdText = @"
                CREATE TABLE IF NOT EXISTS AppExceptions (
                    Id INTEGER  primary key autoincrement, 
                    ModuleName varchar(50),
                    Priority INTEGER ,
                    CreatedAt TimeStamp NOT NULL DEFAULT (datetime('now','localtime')) ,
                    Message varchar(50) ,
                    StackTrace varchar(500) ,
                    Status INTEGER ,
                    BatchNumber varchar(100),
                    Type varchar(50) 
                );

                CREATE TABLE IF NOT EXISTS CellCultivations (
                    Id INTEGER  primary key autoincrement, 
                    Description varchar(50),
                    Name varchar(50),
                    UserName varchar(50),
                    Cell varchar(50),
                    CreatedAt TimeStamp NOT NULL DEFAULT (datetime('now','localtime')) ,
                    BatchNumber varchar(100)
                );

                CREATE TABLE IF NOT EXISTS Pumps (
                    Id INTEGER  primary key autoincrement, 
                    CultivationId INTEGER ,
                    Volume double,
                    ProcessMode INTEGER,
                    Period INTEGER,
                    TimeType INTEGER,
                    FirstSpan INTEGER,
                    FlowRate double,
                    Name varchar(50),
                    DeviceId INTEGER,
                    IsEnabled INTEGER,
                    StartTime INTEGER,
                    EndTime INTEGER,
                    Direction INTEGER,
                    InitialVolume double,
                    InitialFlowRate double,
                    DeviceType INTEGER,
                    IsRunning INTEGER,
                    IsChecked INTEGER,
                    Icon varchar(100)
                );

                CREATE TABLE IF NOT EXISTS Rockers (
                    Id INTEGER  primary key autoincrement, 
                    CultivationId INTEGER ,
                    DeviceId INTEGER ,
                    Name varchar(50),
                    IsEnabled INTEGER,
                    Speed double,
                    Angle double,
                    IsRunning INTEGER,
                    IsChecked INTEGER,
                    Icon varchar(100)
                );

                CREATE TABLE IF NOT EXISTS Gases (
                    Id INTEGER  primary key autoincrement, 
                    CultivationId INTEGER ,
                    DeviceId INTEGER ,
                    Name varchar(50),
                    IsEnabled INTEGER,
                    Concentration double,
                    FlowRate double,
                    IsRunning INTEGER,
                    IsChecked INTEGER,
                    Icon varchar(100)
                );

                CREATE TABLE IF NOT EXISTS TemperatureGauges (
                    Id INTEGER  primary key autoincrement, 
                    CultivationId INTEGER ,
                    DeviceId INTEGER ,
                    Name varchar(50),
                    IsEnabled INTEGER,
                    Level INTEGER,
                    Temperature double,
                    EnvTemperature double,
                    HeaterTemperature double,
                    IsRunning INTEGER,
                    IsChecked INTEGER,
                    Icon varchar(100)
                );

                CREATE INDEX IF NOT EXISTS CellCultivations_CreatedAt on CellCultivations(CreatedAt);

                CREATE TABLE IF NOT EXISTS TemperatureRecords (
                    Id INTEGER  primary key autoincrement, 
                    DeviceId INTEGER,
                    CreatedAt TimeStamp NOT NULL DEFAULT (datetime('now','localtime')) ,
                    HeaterTemperature double,
                    EnvTemperature double,
                    Temperature double
                );

                CREATE TABLE IF NOT EXISTS GasRecords (
                    Id INTEGER  primary key autoincrement, 
                    DeviceId INTEGER,
                    CreatedAt TimeStamp NOT NULL DEFAULT (datetime('now','localtime')) ,
                    Concentration double,
                    FlowRate double
                );

                CREATE TABLE IF NOT EXISTS PumpRecords (
                    Id INTEGER  primary key autoincrement, 
                    DeviceId INTEGER,
                    IsManual INTEGER,
                    StartTime TimeStamp NOT NULL DEFAULT (datetime('now','localtime')) ,
                    EndTime TimeStamp NOT NULL DEFAULT (datetime('now','localtime')) ,
                    Volume double,
                    FlowRate double
                );
                ";
                conn.Open();
                using (var cmd = new SQLiteCommand(cmdText, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            Debug.WriteLine("db create success");
        }
    }

}
