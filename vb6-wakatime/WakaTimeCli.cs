namespace vb6_wakatime
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using log4net;

    class WakaTimeCli
    {
        private readonly PythonManager pythonManager;
        private ILog log = LogManager.GetLogger(nameof(WakaTimeCli));

        internal WakaTimeCli(PythonManager pythonManager)
        {
            this.pythonManager = pythonManager;
        }

        internal bool DoesCliExist()
        {
            return File.Exists(WakaTimeConstants.CliPath);
        }

        internal async Task<bool> IsCliLatestVersion()
        {
            var results = await ProcessRunner.RunProcessAsync(await this.pythonManager.GetPythonAsync(), WakaTimeConstants.CliPath, "--version");

            if (results.Success)
            {
                var currentVersion = results.Errors.Trim();
                log.Info($"Current wakatime-cli version is {currentVersion}");

                log.Info("Checking for updates to wakatime-cli...");
                var latestVersion = await this.LatestWakaTimeCliVersionAsync();

                if (currentVersion.Equals(latestVersion))
                {
                    log.Info("wakatime-cli is up to date.");
                    return true;
                }

                log.Info(string.Format("Found an updated wakatime-cli v{0}", latestVersion));
            }

            return false;
        }

        internal async Task<string> LatestWakaTimeCliVersionAsync()
        {
            var regex = new Regex(@"(__version_info__ = )(\(( ?\'[0-9]+\'\,?){3}\))");

            var proxy = string.IsNullOrEmpty(Properties.Settings.Default.Proxy) ?
                                null :
                                new WebProxy(Properties.Settings.Default.Proxy);

            var client = new WebClient { Proxy = proxy };

            try
            {
                var about = await client.DownloadStringTaskAsync(Properties.Settings.Default.WakaTimeUri);
                var match = regex.Match(about);

                if (match.Success)
                {
                    var grp1 = match.Groups[2];
                    var regexVersion = new Regex("([0-9]+)");
                    var match2 = regexVersion.Matches(grp1.Value);
                    return string.Format("{0}.{1}.{2}", match2[0].Value, match2[1].Value, match2[2].Value);
                }
                else
                {
                    log.Warn("Couldn't auto resolve wakatime cli version");
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception when checking current wakatime cli version: ", ex);
            }

            return string.Empty;
        }
    }
}
