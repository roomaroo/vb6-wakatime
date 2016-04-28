namespace vb6_wakatime
{
    using System;
    using System.IO;

    internal static class WakaTimeConstants
    {
        internal const string CliUrl = "https://github.com/wakatime/wakatime/archive/master.zip";
        internal const string CliFolder = @"wakatime-master\wakatime\cli.py";

        internal static string UserConfigDir => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        internal static string CliPath => Path.Combine(UserConfigDir, CliFolder);
    }
}
