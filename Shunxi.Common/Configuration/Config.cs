namespace Shunxi.Infrastructure.Common.Configuration
{
    public static class Config
    {
        //For file storage
        public static readonly int RockerSpeed = 0;
        public static byte DetectorId = 0x80;

        //Define all Pump names and Ids
        public static readonly string Pump1Name = "Pump A";
        public static readonly int Pump1Id = 1;
        public static readonly string Pump2Name = "Pump 2";
        public static readonly int Pump2Id = 2;
        public static readonly string Pump3Name = "Pump B";
        public static readonly int Pump3Id = 3;
        public static readonly string Pump4Name = "Pump 4";
        public static readonly int Pump4Id = 4;
        public static readonly int RockerId = 0x80;
        public static readonly int TemperatureId = 0x90;
        public static readonly int GasId = 0x91;
        public static readonly int PhId = 0xb0;
        public static readonly int DoId = 0xc0;

        //Default Settings
        public static readonly double DefaultFlowRate = 5D;
        public static readonly double DefaultInitialFlowRate = 5D;
        public static readonly double DefaultSpeed = 5D;
        public static readonly double DefaultAngle = 10D;

        //是否为调试模式
        public static bool IsDebug = true;
        public static bool IsLocal = true;
        //重发时间间隔 单位：ms
        public static readonly int RETRY_INTERVAL = 500;
        //重发超时时间 单位：ms
        public static readonly int RETRY_TIMEOUT = 1500;
        //尝试重发接次数
        public static readonly int RETRY_TIMES = 10;

        //泵最大流速 单位：min    
        public static readonly int MAX_VARIANT_SINGLE_RUNTIME = 30;
        //泵单次最大输入量 单位：ml/min    
        public static readonly int MAX_VARIANT_VOLUME = 600;
        //变量模式最大翻倍次数    
        public static readonly int MAX_VARIANT_TIMES = 5;


#if DEBUG
        public const string SERVER_ADDR = "10.0.0.15";
        public const string REMOTE_CONTROL_SERVER_PORT = "6003";
        public const string LOG_PORT = "6004";
        public const string SERVER_PORT = "6007";
        //        public const string SERVER_ADDR = "211.152.35.57";
        //        public const string REMOTE_CONTROL_SERVER_PORT = "8103";
        //        public const string LOG_PORT = "8104";
        //        public const string SERVER_PORT = "8101";
#endif

#if !DEBUG
        public const string SERVER_ADDR = "211.152.35.57";
        public const string REMOTE_CONTROL_SERVER_PORT = "6003";
        public const string LOG_PORT = "6004";
        public const string SERVER_PORT = "6007";
#endif
        public const string UPLOAD_ADDRESS = "http://localhost:3004/file/upload";
     
    }
}
