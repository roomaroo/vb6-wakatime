namespace vb6_wakatime
{
    using System;
    using System.IO;

    internal static class WakaTimeConstants
    {
        internal const string PluginName = "vb6-wakatime";
        internal const string CliUrl = "https://github.com/wakatime/wakatime/archive/master.zip";
        internal const string CliFolder = @"WakaTime\wakatime-master\wakatime\cli.py";

        internal static string InstallFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "vb6-wakatime");

        internal static string CliPath => Path.Combine(InstallFolder, CliFolder);
    }
}
