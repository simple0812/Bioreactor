using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Shunxi.Infrastructure.Common.Extension;

namespace Shunxi.Common.Log
{
    public class LocalLog : ILog
    {
        private readonly ConcurrentQueue<string> _waitForSendMsg = new ConcurrentQueue<string>();


        public LocalLog()
        {
            Task.Run(() =>
            {
                WriteHandle().IgnorCompletion();
            });
        }

        private async Task WriteHandle()
        {
            while (true)
            {
                var data = ClearAndDequeue();
                if (!string.IsNullOrWhiteSpace(data))
                    await WriteToFile(data);

                await Task.Delay(1000);
            }
        }

        private string ClearAndDequeue()
        {
            var str = "";
            var ret = new StringBuilder();
            while (_waitForSendMsg.TryDequeue(out str))
            {
                if(!string.IsNullOrWhiteSpace(str))
                    ret.AppendLine(str) ;
            }

            return ret.ToString();
        }

        private async Task WriteToFile(string msg)
        {
            var fi = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "/cellmachine.log");
            using (var sw = fi.AppendText())
            {
                await sw.WriteLineAsync(msg);
            }
        }

        public void Info(string msg)
        {
            System.Diagnostics.Debug.WriteLine(msg);
            Enqueue(msg, LogLevel.Info);
        }

        public void Warnning(string msg)
        {
            System.Diagnostics.Debug.WriteLine(msg);
            Enqueue(msg, LogLevel.Warning);
        }

        public void Error(string msg)
        {
            System.Diagnostics.Debug.WriteLine(msg);
            Enqueue(msg, LogLevel.Error);
        }

        private void Enqueue(string msg, LogLevel level)
        {
            if(_waitForSendMsg.Count <=1000)
                _waitForSendMsg.Enqueue($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} {level}] {msg}");
        }
    }
}
