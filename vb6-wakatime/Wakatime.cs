using System;
using System.Net;
using System.Threading.Tasks;
using WakaTime;

namespace vb6_wakatime
{
    public class Wakatime
    {
        public Task DownloadDependenciesAsync()
        {
            return Task.Run(() => DownloadDependencies());
        }

        public void DownloadDependencies()
        {
            try
            {
                // Make sure python is installed
                if (!PythonManager.IsPythonInstalled())
                {
                    Downloader.DownloadAndInstallPython();
                }

                if (!WakaTimeCli.DoesCliExist() || !WakaTimeCli.IsCliLatestVersion())
                {
                    Downloader.DownloadAndInstallCli();
                }
            }
            catch (WebException ex)
            {
                Logger.Error("Are you behind a proxy? Try setting a proxy in WakaTime Settings with format https://user:pass@host:port. Exception Traceback:", ex);
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error("Error detecting dependencies. Exception Traceback:", ex);
                throw;
            }
        }

        internal void CheckSettings()
        {
            throw new NotImplementedException();
        }

        internal void PromptForMissingSettings()
        {
            throw new NotImplementedException();
        }
    }
}
