using System;
using System.Net.Http;
using System.Threading.Tasks;
using Shunxi.Business.Models;
using Shunxi.Business.Protocols.Helper;
using Shunxi.Business.Protocols.SimDirectives;
using Shunxi.Common.Log;
using Shunxi.Infrastructure.Common.Configuration;

namespace Shunxi.Business.Logic
{
    public static class HttpHelper
    {
        private static async Task SendByHttp(string url,CnetScan cnetScans, Action<string> cb )
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var uri = new Uri($"http://{Config.SERVER_ADDR}:{Config.SERVER_PORT}/api/sim/location?mcc={cnetScans.MCC}&mnc={cnetScans.MNC}&lac={cnetScans.Lac}&ci={cnetScans.Cellid}&deviceid={Common.Utility.Common.GetUniqueId()}");
                    HttpResponseMessage response = await client.GetAsync(uri);
                    if (response.EnsureSuccessStatusCode().StatusCode.ToString().ToLower() == "ok")
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        cb?.Invoke(responseBody);
                    }
                }
                catch (HttpRequestException ex)
                {
                    LogFactory.Create().Info(ex.Message);
                }
            }
        }

        private static void SendBySim(string url, Action<string> cb)
        {
            SimWorker.Instance.Enqueue(new HttpCompositeDirective(url, x =>
            {
                cb(x.Code.ToString());
            }));
        }
    }
}
