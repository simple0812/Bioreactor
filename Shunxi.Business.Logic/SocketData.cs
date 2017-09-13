using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Shunxi.Business.Logic
{
    public enum SocketDataType
    {
        Text,
        Json
    }

    public class SocketData
    {
        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("type")]
        [JsonConverter(typeof (StringEnumConverter))]
        public SocketDataType Type { get; private set; }

        [JsonProperty("data")]
        public object Data { get; set; }

        public SocketData(object data, string action)
        {
            this.Data = data;
            this.Action = action;
            this.Type = this.Data is string ? SocketDataType.Text : SocketDataType.Json;
        }

        public string Pack()
        {
            var p = JsonConvert.SerializeObject(this);

            return $"|>{p.Length}<|{p}";
        }

        public string PackRaw()
        {
            var p = JsonConvert.SerializeObject(this);

            return p;
        }
    }
}
