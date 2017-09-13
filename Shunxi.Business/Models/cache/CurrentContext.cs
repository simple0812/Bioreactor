using System.Collections.Generic;
using Shunxi.Business.Enums;

namespace Shunxi.Business.Models.cache
{
    public static class CurrentContext
    {
        public static SysStatusEnum Status { get; set; }
        public static SystemCache SysCache { get; set; }
        public static readonly List<double> TemperaturesCache = new List<double>();
        public static readonly List<double> HeaterTemperaturesCache = new List<double>();
        public static readonly List<double> EnvTemperaturesCache = new List<double>();

        //每个培养周期的批号
        public static string BatchNumber { get; set; }
//        public static string BatchNumber
//        {
//            get => ApplicationData.Current.LocalSettings.Values["BatchNumber"]?.ToString();
//            set => ApplicationData.Current.LocalSettings.Values["BatchNumber"] = value;
//        }
    }
}
