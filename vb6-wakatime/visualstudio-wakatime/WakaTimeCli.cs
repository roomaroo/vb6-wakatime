using System.IO;

namespace WakaTime
{
    internal static class WakaTimeCli
    {
        private static string _pythonCliPath;

        static WakaTimeCli()
        {
            _pythonCliPath = new PythonCliParameters().Cli;
        }

        internal static bool DoesCliExist()
        {
            return File.Exists(_pythonCliPath);
        }

        internal static bool IsCliLatestVersion()
        {
            var process = new RunProcess(PythonManager.GetPython(), _pythonCliPath, "--version");
            process.Run();

            if (process.Success)
            {
                var currentVersion = process.Error.Trim();
                Logger.Info(string.Format("Current wakatime-cli version is {0}", currentVersion));

                Logger.Info("Checking for updates to wakatime-cli...");
                var latestVersion = WakaTimeConstants.LatestWakaTimeCliVersion();

                if (currentVersion.Equals(latestVersion))
                {
                    Logger.Info("wakatime-cli is up to date.");
                    return true;
                }

                Logger.Info(string.Format("Found an updated wakatime-cli v{0}", latestVersion));
            }
            return false;
        }
    }
}
