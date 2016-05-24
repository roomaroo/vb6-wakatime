namespace vb6_wakatime
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using log4net;
    class Reporter
    {
        private DateTime lastSentTime;
        private string lastFile = null;
        private readonly PythonManager pythonManager;

        private ILog log = LogManager.GetLogger(nameof(Reporter));

        public Reporter(PythonManager pythonManager)
        {
            this.pythonManager = pythonManager;
        }

        public async Task SendHeartbeat(string project, string fileName, bool isSave)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            if (!isSave && this.lastFile != null && !this.EnoughTimePassed() && fileName.Equals(lastFile))
            {
                // Nothing to do
                return;
            }

            await this.SendHeartbeatAsync(project, fileName, isSave);
            this.lastFile = fileName;
            this.lastSentTime = DateTime.Now;
        }

        private async Task SendHeartbeatAsync(string project, string fileName, bool isSave)
        {
            var parameters = new PythonCliParameters
            {
                Key = Guid.Parse(Properties.Settings.Default.ApiKey),
                File = fileName,
                Plugin = WakaTimeConstants.PluginName,
                IsSave = isSave,
                Project = project
            };

            var pythonBinary = await this.pythonManager.GetPythonAsync();
            if (pythonBinary != null)
            {
                log.Debug($"Sending heartbeat for project {project}, file {fileName}");
                var results = await ProcessRunner.RunProcessAsync(pythonBinary, parameters.ToArray());

                if (!results.Success)
                {
                    log.Error($"Could not send heartbeat: {results.Errors}");
                }
            }
            else
            {
                log.Error("Could not send heartbeat because python is not installed");
            }
        }

        private bool EnoughTimePassed()
        {
            return (DateTime.Now - this.lastSentTime) > TimeSpan.FromMinutes(1);
        }

        class PythonCliParameters
        {
            public Guid Key { get; set; }
            public string File { get; set; }
            public string Plugin { get; set; }
            public bool IsSave { get; set; }
            public string Project { get; set; }

            public string[] ToArray(bool obfuscate = false)
            {
                var parameters = new Collection<string>
                {
                    WakaTimeConstants.CliPath,
                    "--key",
                    obfuscate ? string.Format("XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXX{0}", Key.ToString("D").Length - 4) : Key.ToString("D"),
                    "--file",
                    File,
                    "--plugin",
                    Plugin
                };

                if (IsSave)
                {
                    parameters.Add("--write");
                }
                
                if (!string.IsNullOrEmpty(Project))
                {
                    parameters.Add("--project");
                    parameters.Add(Project);
                }

                return parameters.ToArray();
            }
        }
    }
}
