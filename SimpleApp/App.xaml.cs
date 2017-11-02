using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Shunxi.Business.Logic;
using Shunxi.DataAccess;
using Shunxi.Infrastructure.Common.Configuration;

namespace SimpleApp
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var processName = Assembly.GetExecutingAssembly().GetName().Name;
            int processCount = Process.GetProcessesByName(processName).Length;
            if (processCount > 1)
            {
                MessageBox.Show("程序运行中，请关闭后重试");
                Environment.Exit(-2);
            }

            base.OnStartup(e);

            Task.Run(() =>
            {
                using (var ctx = new IotContext())
                {
                    ctx.Initialize();
                }

                CultivationService.cleanRecord();
            });
        }
    }
}
