using System;
using System.Net;
using System.Threading.Tasks;
using log4net;

namespace vb6_wakatime
{
    internal class Wakatime
    {
        ILog log = LogManager.GetLogger(nameof(Wakatime));
        private readonly WakaTimeCli wakatimeCli;
        private readonly PythonManager pythonManager;
        
        internal Wakatime(PythonManager pythonManager, WakaTimeCli wakatimeCli)
        {
            this.pythonManager = pythonManager;
            this.wakatimeCli = wakatimeCli;
        }

        public async Task DownloadDependenciesAsync()
        {
            try
            {
                var downloader = new Downloader();

                // Make sure python is installed
                if (await this.pythonManager.IsPythonInstalledAsync() == false)
                {
                    await downloader.DownloadAndInstallPythonAsync();
                }

                if (!this.wakatimeCli.DoesCliExist() || ! await this.wakatimeCli.IsCliLatestVersion())
                {
                    await downloader.DownloadAndInstallWakaTimeAsync();
                }
            }
            catch (WebException ex)
            {
                log.Error("Are you behind a proxy? Try setting a proxy in WakaTime Settings with format https://user:pass@host:port. Exception Traceback:", ex);
                throw;
            }
            catch (Exception ex)
            {
                log.Error("Error detecting dependencies. Exception Traceback:", ex);
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
