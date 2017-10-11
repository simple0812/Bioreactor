using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shunxi.DataAccess
{
    public class ContextHelper
    {
        public ContextHelper()
        {
            Migrations = new Dictionary<int, IList>();
            MigrationVersion1();
        }

        public Dictionary<int, IList> Migrations { get; set; }

        private void MigrationVersion1()
        {
            var steps = new List<string>();

            string sqlSchemaInfo = @" CREATE TABLE IF NOT EXISTS  SchemaInfoes (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Version INTEGER);";

            string cmdText = @"
                        CREATE TABLE IF NOT EXISTS AppExceptions (
                            Id INTEGER  primary key autoincrement, 
                            CellCultivationId INTEGER,
                            ModuleName varchar(50),
                            Priority INTEGER ,
                            CreatedAt TimeStamp NOT NULL DEFAULT (datetime('now','localtime')) ,
                            Message varchar(50) ,
                            StackTrace varchar(500) ,
                            Status INTEGER ,
                            BatchNumber varchar(100),
                            Type varchar(50) 
                        );
                        CREATE INDEX IF NOT EXISTS AppExceptions_CellCultivationId on AppExceptions(CellCultivationId);
        
                        CREATE TABLE IF NOT EXISTS CellCultivations (
                            Id INTEGER  primary key autoincrement, 
                            Description varchar(50),
                            Name varchar(50),
                            UserName varchar(50),
                            Cell varchar(50),
                            CreatedAt TimeStamp NOT NULL DEFAULT (datetime('now','localtime')) ,
                            BatchNumber varchar(100)
                        );
                        CREATE INDEX IF NOT EXISTS CellCultivations_CreatedAt on CellCultivations(CreatedAt);
        
                        CREATE TABLE IF NOT EXISTS Pumps (
                            Id INTEGER  primary key autoincrement, 
                            CultivationId INTEGER ,
                            Volume double,
                            ProcessMode INTEGER,
                            Period INTEGER,
                            TimeType INTEGER,
                            InOrOut INTEGER,
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
                        CREATE INDEX IF NOT EXISTS Pumps_CultivationId on Pumps(CultivationId);
        
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
                        CREATE INDEX IF NOT EXISTS Rockers_CultivationId on Rockers(CultivationId);
        
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
                        CREATE INDEX IF NOT EXISTS Gases_CultivationId on Gases(CultivationId);
        
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
                        CREATE INDEX IF NOT EXISTS TemperatureGauges_CultivationId on TemperatureGauges(CultivationId);
        
        
                        CREATE TABLE IF NOT EXISTS TemperatureRecords (
                            Id INTEGER  primary key autoincrement, 
                            DeviceId INTEGER,
                            CellCultivationId INTEGER,
                            CreatedAt TimeStamp NOT NULL DEFAULT (datetime('now','localtime')) ,
                            HeaterTemperature double,
                            EnvTemperature double,
                            Temperature double
                        );
                        CREATE INDEX IF NOT EXISTS TemperatureRecords_CellCultivationId on TemperatureRecords(CellCultivationId);
        
                        CREATE TABLE IF NOT EXISTS GasRecords (
                            Id INTEGER  primary key autoincrement, 
                            DeviceId INTEGER,
                            CellCultivationId INTEGER,
                            CreatedAt TimeStamp NOT NULL DEFAULT (datetime('now','localtime')) ,
                            Concentration double,
                            FlowRate double
                        );
                        CREATE INDEX IF NOT EXISTS GasRecords_CellCultivationId on GasRecords(CellCultivationId);
        
                        CREATE TABLE IF NOT EXISTS PumpRecords (
                            Id INTEGER  primary key autoincrement, 
                            DeviceId INTEGER,
                            IsManual INTEGER,
                            CellCultivationId INTEGER,
                            StartTime TimeStamp NOT NULL DEFAULT (datetime('now','localtime')) ,
                            EndTime TimeStamp NOT NULL DEFAULT (datetime('now','localtime')) ,
                            Volume double,
                            FlowRate double
                        );
                        CREATE INDEX IF NOT EXISTS PumpRecords_CellCultivationId on PumpRecords(CellCultivationId);
                        ";

            steps.Add(sqlSchemaInfo);
            steps.Add(cmdText);

            Migrations.Add(1, steps);
        }
    }
}
