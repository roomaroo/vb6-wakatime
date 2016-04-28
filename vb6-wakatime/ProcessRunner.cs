using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vb6_wakatime
{
    class ProcessResults
    {
        public string Output { get; }
        public string Errors { get; }
        public int ExitCode { get; }
        public bool Success => this.ExitCode == 0;

        Exception Exception { get; }

        public ProcessResults(int exitCode, string output = null, string errors = null, Exception exception = null)
        {
            this.ExitCode = exitCode;
            this.Output = output;
            this.Errors = errors;
            this.Exception = exception;
        }
    }

    static class ProcessRunner
    {
        public static async Task<ProcessResults> RunProcessAsync(string path, params string[] arguments)
        {
            var psi = new ProcessStartInfo
            {
                Arguments = string.Join(" ", arguments),
                CreateNoWindow = true,
                FileName = path,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            var process = new Process
            {
                StartInfo = psi,
                EnableRaisingEvents = true
            };

            var stderr = new StringBuilder();
            var stdout = new StringBuilder();
            Exception exception = null;

            process.ErrorDataReceived += (s, e) => stderr.AppendLine(e.Data);
            process.OutputDataReceived += (s, e) => stdout.AppendLine(e.Data);

            try
            {
                await Task.Run(() => {
                    process.Start();
                    process.WaitForExit();
                });
            }
            catch (Exception e)
            {
                exception = e;
            }

            return new ProcessResults(process.ExitCode, stdout.ToString(), stderr.ToString(), exception);
        }
    }
}
