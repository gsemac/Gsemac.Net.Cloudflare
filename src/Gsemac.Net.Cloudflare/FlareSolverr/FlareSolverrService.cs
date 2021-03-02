using Gsemac.IO.Logging;
using Gsemac.IO.Logging.Extensions;
using System;
using System.Diagnostics;
using System.IO;

namespace Gsemac.Net.Cloudflare.FlareSolverr {

    public class FlareSolverrService :
        FlareSolverrServiceBase {

        // Public members

        public FlareSolverrService(IFlareSolverrOptions options, ILogger logger = null) {

            this.options = options;

            flareSolverrExecutablePath = new Lazy<string>(GetFlareSolverrExecutablePath);

            if (logger is object)
                Log += logger.CreateLogEventHandler();

        }

        public override bool Start() {

            lock (mutex) {

                if (!processStarted) {

                    if (!SocketUtilities.IsPortAvailable(FlareSolverrUtilities.DefaultPort)) {

                        // If FlareSolverr already appears to be running (port 8191 in use), don't attempt to start it.

                        OnLog.Warning($"Port {FlareSolverrUtilities.DefaultPort} is already in use; assuming FlareSolverr is already running");

                        return true;

                    }
                    else {

                        OnLog.Info("Starting FlareSolverr service");

                        processStarted = StartFlareSolverr();

                        if (!processStarted)
                            OnLog.Error("Failed to start FlareSolverr process");

                    }

                }

            }

            return processStarted;

        }
        public override bool Stop() {

            lock (mutex) {

                if (processStarted) {

                    OnLog.Info("Stopping FlareSolverr service");

                    StopFlareSolverr();

                    processStarted = false;

                }

            }

            return processStarted;

        }

        // Protected members

        protected override void Dispose(bool disposing) {

            if (disposing) {

                lock (mutex)
                    StopFlareSolverr();

            }

        }

        // Private members

        private readonly IFlareSolverrOptions options;
        private readonly object mutex = new object();
        private readonly Lazy<string> flareSolverrExecutablePath;
        private bool processStarted = false;
        private Process flareSolverrProcess;

        private bool StartFlareSolverr() {

            if (!File.Exists(flareSolverrExecutablePath.Value)) {

                OnLog.Error($"FlareSolverr was not found at '{flareSolverrExecutablePath.Value}'");

                throw new FileNotFoundException(flareSolverrExecutablePath.Value);

            }

            OnLog.Info($"Starting FlareSolverr process");

            flareSolverrProcess = CreateProcess(flareSolverrExecutablePath.Value);

            bool success = flareSolverrProcess.Start();

            flareSolverrProcess.BeginOutputReadLine();
            flareSolverrProcess.BeginErrorReadLine();

            // Give the process some time to fail so we can detect if FlareSolverr failed to start.

            if (success) {

                if (flareSolverrProcess.WaitForExit((int)TimeSpan.FromSeconds(1).TotalMilliseconds))
                    success = success && flareSolverrProcess.ExitCode == 0;

            }

            if (success)
                OnLog.Info($"FlareSolverr is now listening on port {FlareSolverrUtilities.DefaultPort}");

            return success;

        }
        private void StopFlareSolverr() {

            if (processStarted && flareSolverrProcess != null && !flareSolverrProcess.HasExited) {

                OnLog.Info("Stopping FlareSolverr process");

                flareSolverrProcess.Kill();
                flareSolverrProcess.Dispose();

                flareSolverrProcess = null;

            }

        }

        private string GetFlareSolverrExecutablePath() {

            string baseDirectoryPath = options.FlareSolverrDirectoryPath;

            if (string.IsNullOrWhiteSpace(baseDirectoryPath))
                baseDirectoryPath = Directory.GetCurrentDirectory();

            // ./flaresolverr.exe

            if (File.Exists(Path.Combine(baseDirectoryPath, FlareSolverrUtilities.FlareSolverrExecutablePath)))
                return Path.Combine(baseDirectoryPath, FlareSolverrUtilities.FlareSolverrExecutablePath);

            // ./flaresolverr/flaresolverr.exe
            // ./flaresolverr-vx.x.x-windows-xxx/flaresolverr/flaresolverr.exe

            foreach (string directoryPath in Directory.EnumerateDirectories(baseDirectoryPath, "flaresolverr*", SearchOption.TopDirectoryOnly)) {

                foreach (string candidateExecutablePath in new[] {
                    Path.Combine(directoryPath, FlareSolverrUtilities.FlareSolverrExecutablePath),
                    Path.Combine(directoryPath, "flaresolverr", FlareSolverrUtilities.FlareSolverrExecutablePath),
                }) {

                    if (File.Exists(candidateExecutablePath))
                        return candidateExecutablePath;

                }

            }

            // The FlareSolverr executable could not be found.

            return string.Empty;

        }

        private Process CreateProcess(string fileName) {

            ProcessStartInfo processStartInfo = new ProcessStartInfo() {
                FileName = fileName,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            Process process = new Process {
                StartInfo = processStartInfo
            };

            process.OutputDataReceived += (sender, e) => OnLog.Info(e.Data);
            process.ErrorDataReceived += (sender, e) => OnLog.Error(e.Data);

            return process;

        }

    }

}