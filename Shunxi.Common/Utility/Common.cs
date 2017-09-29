using System;
using System.Linq;
using System.Management;
using System.Net;

namespace Shunxi.Common.Utility
{
    public static class Common
    {
        public static string BytesToString(byte[] bytes)
        {
            return bytes.Aggregate("", (current, t) => current + (Convert.ToString(t, 16).PadLeft(2, '0') + ",")).TrimEnd(',');
        }

        public static string GetLocalIp()
        {
            System.Net.IPAddress[] addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            return addressList.Any() ? addressList[0].ToString() : "";
        }

        public static string GetLocalIpex()
        {
            string AddressIP = "11.11.11.11";
            foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    AddressIP = _IPAddress.ToString();
                }
            }

            return AddressIP;
        }

        public static string GetUniqueId()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }


        public static DateTime FromUnixTime(long unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddMilliseconds(unixTime);
        }

        public static int ToUnixTime(DateTime date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt32((date.ToUniversalTime() - epoch).TotalSeconds);
        }


        public static string GetMacAddress()
        {
            try
            {
                using (ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration"))
                {
                    using (ManagementObjectCollection moc = mc.GetInstances())
                    {
                        string macAddress = "";
                        foreach (var o in moc)
                        {
                            var mo = (ManagementObject) o;
                            if ((bool)mo["IPEnabled"] == true)
                            {
                                macAddress = mo["MacAddress"].ToString();
                                break;
                            }
                        }
                        return macAddress;
                    }
                }
            }
            catch
            {
                return "unknown";
            }
            finally
            {
            }
        }
    }
}
