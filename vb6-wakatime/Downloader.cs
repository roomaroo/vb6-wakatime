
namespace vb6_wakatime
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Net;
    using System.Threading.Tasks;

    class Downloader
    {
        public Downloader()
        {
            Directory.CreateDirectory(WakaTimeConstants.InstallFolder);
        }

        public async Task DownloadAndInstallWakaTimeAsync()
        {
            var destinationFolder = Path.Combine(WakaTimeConstants.InstallFolder, "WakaTime");
            var zipFile = Path.Combine(WakaTimeConstants.InstallFolder, "wakatime-cli.zip");

            await DownloadFileAsync(Properties.Settings.Default.CliUri, zipFile);
            await UnpackFileAsync(zipFile, destinationFolder);
            File.Delete(zipFile);
        }

        public async Task DownloadAndInstallPythonAsync()
        {
            var destinationFolder = Path.Combine(WakaTimeConstants.InstallFolder, "Python");
            var zipFile = Path.Combine(WakaTimeConstants.InstallFolder, "python.zip");
            
            var pythonUri = new Uri(PythonManager.PythonDownloadUrl);
            await DownloadFileAsync(pythonUri, zipFile);
            await UnpackFileAsync(zipFile, destinationFolder);
            File.Delete(zipFile);
        }

        private async Task DownloadFileAsync(Uri uri, string localPath)
        {
            var proxyUri = Properties.Settings.Default.Proxy;
            var proxy = string.IsNullOrEmpty(proxyUri) ? null : new WebProxy(proxyUri);

            var client = new WebClient { Proxy = proxy };
            await client.DownloadFileTaskAsync(uri, localPath);
        }

        private async Task UnpackFileAsync(string zipFile, string destinationFolder)
        {
            await Task.Run(() => ZipFile.ExtractToDirectory(zipFile, destinationFolder));
        }
    }
}
