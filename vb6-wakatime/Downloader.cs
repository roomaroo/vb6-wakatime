
namespace vb6_wakatime
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Net;
    using System.Threading.Tasks;

    class Downloader
    {
        public async Task DownloadAndInstallWakaTimeAsync()
        {
            var userConfigDir = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
            var zipFile = new FileInfo(Path.Combine(userConfigDir.FullName, "wakatime-cli.zip"));

            await DownloadFileAsync(Properties.Settings.Default.CliUri, zipFile);
            await UnpackFileAsync(zipFile, userConfigDir);
            zipFile.Delete();
        }


        public async Task DownloadAndInstallPythonAsync()
        {
            var userConfigDir = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
            var zipFile = new FileInfo(Path.Combine(userConfigDir.FullName, "python.zip"));

            var pythonUri = new Uri(PythonManager.PythonDownloadUrl);
            await DownloadFileAsync(pythonUri, zipFile);
            await UnpackFileAsync(zipFile, userConfigDir);
            zipFile.Delete();
        }

        private async Task DownloadFileAsync(Uri uri, FileInfo localPath)
        {
            var client = new WebClient { Proxy = new WebProxy(Properties.Settings.Default.Proxy) };
            await client.DownloadFileTaskAsync(uri, localPath.FullName);
        }

        private async Task UnpackFileAsync(FileInfo zipFile, DirectoryInfo destination)
        {
            await Task.Run(() => ZipFile.ExtractToDirectory(zipFile.FullName, destination.FullName));
        }
    }
}
