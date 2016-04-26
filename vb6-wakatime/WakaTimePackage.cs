using System.Net;

namespace WakaTime
{
    internal static class WakaTimePackage
    {
        public static WebProxy GetProxy()
        {
            var configFile = new WakaTimeConfigFile();
            configFile.Read();
            return new WebProxy(configFile.Proxy);
        }
    }
}
