using System;
using System.Threading.Tasks;
using Shunxi.Business.Enums;
using Shunxi.Business.Tables;
using Shunxi.Common.Log;

namespace Shunxi.Business.Models
{
    public sealed class CustomException : Exception
    {
        public string ModuleName { get; set; }
        public ExceptionPriority Priority { get; set; }
        public string Trace { get; set; }

        public string BaseException => GetBaseException().GetType().FullName;

        public CustomException(string msg, string moduleName = "", ExceptionPriority priority = ExceptionPriority.Normal) : base(msg)
        {
            this.ModuleName = moduleName;
            this.Priority = priority;
            this.Trace = StackTrace;

            LogFactory.Create().Info($"CustomException->{msg},{moduleName}");
        }
    }
}
