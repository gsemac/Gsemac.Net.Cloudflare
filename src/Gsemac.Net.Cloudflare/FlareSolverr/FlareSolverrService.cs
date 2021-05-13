using Gsemac.Net.Extensions;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace Gsemac.Net.Cloudflare.FlareSolverr {

    public class FlareSolverrService :
        FlareSolverrServiceBase {

        // Public members

        public FlareSolverrService() :
            this(FlareSolverrOptions.Default) {
        }
        public FlareSolverrService(IFlareSolverrOptions options) :
            this(HttpWebRequestFactory.Default, options) {
        }
        public FlareSolverrService(IHttpWebRequestFactory webRequestFactory, IFlareSolverrOptions options) {

            this.webRequestFactory = webRequestFactory;
            this.options = options;

            flareSolverrExecutablePath = new Lazy<string>(GetFlareSolverrExecutablePath);

        }

        public override bool Start() {

            lock (mutex) {

                bool success = false;

                if (!processStarted) {

                    if (!SocketUtilities.IsPortAvailable(FlareSolverrUtilities.DefaultPort)) {

                        // If FlareSolverr already appears to be running (port 8191 in use), don't attempt to start it.

                        OnLog.Warning($"Port {FlareSolverrUtilities.DefaultPort} is already in use; assuming FlareSolverr is already running");

                        success = true;

                    }
                    else {

                        if (options.AutoUpdateEnabled)
                            UpdateFlareSolverr();

                        OnLog.Info("Starting FlareSolverr service");

                        processStarted = StartFlareSolverr();

                        if (!processStarted)
                            OnLog.Error("Failed to start FlareSolverr process");

                        success = processStarted;

                    }

                    if (options.UseSession)
                        CreateSession();

                }

                return success;

            }

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

        public override IFlareSolverrResponse ExecuteCommand(IFlareSolverrCommand command) {

            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (string.IsNullOrEmpty(command.Session) && !string.IsNullOrWhiteSpace(sessionId) && command is FlareSolverrCommand mutableCommand)
                mutableCommand.Session = sessionId;

            using (IWebClient webClient = CreateWebClient()) {

                Uri flareSolverrUri = new Uri($"http://localhost:{FlareSolverrUtilities.DefaultPort}/v1");

                OnLog.Info($"Waiting for response from {flareSolverrUri.AbsoluteUri}");

                string responseJson = webClient.UploadString(flareSolverrUri, command.ToString());

                return JsonConvert.DeserializeObject<FlareSolverrResponse>(responseJson);

            }

        }

        // Protected members

        protected override void Dispose(bool disposing) {

            if (disposing) {

                lock (mutex)
                    StopFlareSolverr();

            }

        }

        // Private members

        private readonly IHttpWebRequestFactory webRequestFactory;
        private readonly IFlareSolverrOptions options;
        private readonly object mutex = new object();
        private readonly Lazy<string> flareSolverrExecutablePath;
        private bool processStarted = false;
        private string sessionId;
        private Process flareSolverrProcess;

        private bool StartFlareSolverr() {

            if (!File.Exists(flareSolverrExecutablePath.Value)) {

                OnLog.Error($"FlareSolverr was not found at '{flareSolverrExecutablePath.Value}'");

                throw new FileNotFoundException(string.Format(Properties.ExceptionMessages.FileNotFound, flareSolverrExecutablePath.Value));

            }

            OnLog.Info($"Starting FlareSolverr process");

            flareSolverrProcess = CreateProcess(flareSolverrExecutablePath.Value);

            bool success = flareSolverrProcess.Start();

            if (success) {

                flareSolverrProcess.BeginOutputReadLine();
                flareSolverrProcess.BeginErrorReadLine();

                success = success && WaitForFlareSolverr();

            }

            if (success)
                OnLog.Info($"FlareSolverr is now listening on port {FlareSolverrUtilities.DefaultPort}");

            return success;

        }
        private void StopFlareSolverr() {

            if (!string.IsNullOrWhiteSpace(sessionId))
                DestroySession();

            if (processStarted && flareSolverrProcess is object && !flareSolverrProcess.HasExited) {

                OnLog.Info("Stopping FlareSolverr process");

                flareSolverrProcess.Kill();
                flareSolverrProcess.Dispose();

                flareSolverrProcess = null;

            }

        }
        private void UpdateFlareSolverr() {

            try {

                IFlareSolverrUpdater updater = new FlareSolverrUpdater(webRequestFactory, new FlareSolverrUpdaterOptions() {
                    FlareSolverrDirectoryPath = options.FlareSolverrDirectoryPath,
                });

                updater.DownloadFileProgressChanged += OnDownloadFileProgressChanged;
                updater.DownloadFileCompleted += OnDownloadFileCompleted;
                updater.Log += OnLog.Log;

                updater.Update();

            }
            catch (Exception ex) {

                OnLog.Error(ex.ToString());

                if (!options.IgnoreUpdateErrors)
                    throw ex;

            }

        }
        private bool WaitForFlareSolverr() {

            // Give the process some time to fail so we can detect if FlareSolverr failed to start.

            if (flareSolverrProcess.WaitForExit((int)TimeSpan.FromSeconds(1).TotalMilliseconds))
                if (flareSolverrProcess.ExitCode != 0)
                    return false;

            // Wait for FlareSolverr to start listening on its designated port.

            DateTimeOffset startTime = DateTimeOffset.Now;
            TimeSpan timeout = TimeSpan.FromSeconds(60);

            do {

                Thread.Sleep(TimeSpan.FromSeconds(1));

            }
            while (SocketUtilities.IsPortAvailable(FlareSolverrUtilities.DefaultPort) && (DateTimeOffset.Now - startTime) < timeout);

            return !SocketUtilities.IsPortAvailable(FlareSolverrUtilities.DefaultPort);

        }

        private IWebClient CreateWebClient() {

            IWebClient webClient = webRequestFactory.ToWebClientFactory().Create();

            // WebClient will use Encoding.Default, which varies by .NET implementation.
            // For example, older implementations use ANSI, while newer implementations use UTF8. Using ANSI will give us garbled characters.
            // Furthermore, RFC4627 states that JSON text will always be encoded in UTF8.

            webClient.Encoding = Encoding.UTF8;

            webClient.Headers[HttpRequestHeader.ContentType] = "application/json";

            return webClient;

        }
        private void CreateSession() {

            OnLog.Info($"Starting new session");

            IFlareSolverrResponse response = ExecuteCommand(new FlareSolverrCommand("sessions.create") {
                UserAgent = options.UserAgent,
            });

            OnLog.Info($"Started session with ID {response.Session}");

            sessionId = response.Session;

        }
        private void DestroySession() {

            OnLog.Info($"Destroying session {sessionId}");

            ExecuteCommand(new FlareSolverrCommand("sessions.destroy") {
                Session = sessionId,
            });

        }

        private string GetFlareSolverrExecutablePath() {

            return FlareSolverrUtilities.FindFlareSolverrExecutablePath(options.FlareSolverrDirectoryPath);

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

            process.OutputDataReceived += (sender, e) => OnFlareSolverrLog.Info(e.Data);
            process.ErrorDataReceived += (sender, e) => OnFlareSolverrLog.Error(e.Data);

            return process;

        }

    }

}