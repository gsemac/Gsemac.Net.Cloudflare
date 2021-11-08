using Gsemac.IO.Logging;
using Gsemac.IO.Logging.Extensions;
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
        public FlareSolverrService(ILogger logger) :
            this(FlareSolverrOptions.Default, logger) {
        }
        public FlareSolverrService(IFlareSolverrOptions options, ILogger logger) :
           this(HttpWebRequestFactory.Default, options, logger) {
        }
        public FlareSolverrService(IHttpWebRequestFactory webRequestFactory, IFlareSolverrOptions options) :
            this(webRequestFactory, options, new NullLogger()) {
        }
        public FlareSolverrService(IHttpWebRequestFactory webRequestFactory, IFlareSolverrOptions options, ILogger logger) {

            if (webRequestFactory is null)
                throw new ArgumentNullException(nameof(webRequestFactory));

            if (options is null)
                throw new ArgumentNullException(nameof(options));

            if (logger is null)
                throw new ArgumentNullException(nameof(logger));

            this.webRequestFactory = webRequestFactory;
            this.options = options;
            this.logger = new NamedLogger(logger, nameof(FlareSolverrService));

            flareSolverrExecutablePath = new Lazy<string>(GetFlareSolverrExecutablePath);

        }

        public override bool Start() {

            lock (mutex) {

                bool isFlareSolverrReady = DetectOrStartFlareSolverr();

                if (isFlareSolverrReady && options.UseSession)
                    CreateSession();

                return isFlareSolverrReady;

            }

        }
        public override bool Stop() {

            updaterCancellationTokenSource.Cancel();

            lock (mutex) {

                if (processStarted) {

                    logger.Info("Stopping FlareSolverr service");

                    StopFlareSolverr();

                    processStarted = false;

                }

            }

            return processStarted;

        }

        public override IFlareSolverrResponse ExecuteCommand(IFlareSolverrCommand command) {

            return ExecuteCommandInternal(command, initFlareSolverr: true);

        }

        // Protected members

        protected override void Dispose(bool disposing) {

            if (disposing) {

                updaterCancellationTokenSource.Cancel();

                lock (mutex)
                    StopFlareSolverr();

                updaterCancellationTokenSource.Dispose();

            }

        }

        // Private members

        private readonly IHttpWebRequestFactory webRequestFactory;
        private readonly IFlareSolverrOptions options;
        private readonly ILogger logger;
        private readonly object mutex = new object();
        private readonly Lazy<string> flareSolverrExecutablePath;
        private readonly CancellationTokenSource updaterCancellationTokenSource = new CancellationTokenSource();
        private bool processStarted = false;
        private string sessionId;
        private Process flareSolverrProcess;

        private bool StartFlareSolverr() {

            if (!File.Exists(flareSolverrExecutablePath.Value)) {

                logger.Error($"FlareSolverr was not found at '{flareSolverrExecutablePath.Value}'");

                throw new FileNotFoundException(string.IsNullOrWhiteSpace(flareSolverrExecutablePath.Value) ?
                    Properties.ExceptionMessages.FlareSolverrExecutableNotFound :
                    string.Format(Properties.ExceptionMessages.FileNotFoundWithFilePath, flareSolverrExecutablePath.Value));

            }

            logger.Info($"Starting FlareSolverr process");

            flareSolverrProcess = CreateProcess(flareSolverrExecutablePath.Value);

            bool success = flareSolverrProcess.Start();

            if (success) {

                flareSolverrProcess.BeginOutputReadLine();
                flareSolverrProcess.BeginErrorReadLine();

                success = success && WaitForFlareSolverr();

            }

            if (success)
                logger.Info($"FlareSolverr is now listening on port {FlareSolverrUtilities.DefaultPort}");

            return success;

        }
        private bool DetectOrStartFlareSolverr() {

            lock (mutex) {

                bool success = false;

                if (!processStarted) {

                    if (!SocketUtilities.IsPortAvailable(FlareSolverrUtilities.DefaultPort)) {

                        // If FlareSolverr already appears to be running (port 8191 in use), don't attempt to start it.

                        logger.Warning($"Port {FlareSolverrUtilities.DefaultPort} is already in use; assuming FlareSolverr is already running");

                        success = true;

                    }
                    else {

                        if (options.AutoUpdateEnabled)
                            UpdateFlareSolverr();

                        logger.Info("Starting FlareSolverr service");

                        processStarted = StartFlareSolverr();

                        if (!processStarted)
                            logger.Error("Failed to start FlareSolverr process");

                        success = processStarted;

                    }

                    if (success && options.UseSession)
                        CreateSession();

                }

                return success;

            }

        }
        private void StopFlareSolverr() {

            updaterCancellationTokenSource.Cancel();

            if (!string.IsNullOrWhiteSpace(sessionId))
                DestroySession();

            if (processStarted && flareSolverrProcess is object && !flareSolverrProcess.HasExited) {

                logger.Info("Stopping FlareSolverr process");

                flareSolverrProcess.Kill();
                flareSolverrProcess.Dispose();

                flareSolverrProcess = null;

            }

        }
        private void UpdateFlareSolverr() {

            try {

                IFlareSolverrUpdater updater = new FlareSolverrUpdater(webRequestFactory, new FlareSolverrUpdaterOptions() {
                    FlareSolverrDirectoryPath = options.FlareSolverrDirectoryPath,
                }, logger);

                updater.DownloadFileProgressChanged += OnDownloadFileProgressChanged;
                updater.DownloadFileCompleted += OnDownloadFileCompleted;

                updater.Update(updaterCancellationTokenSource.Token);

            }
            catch (Exception ex) {

                logger.Error(ex.ToString());

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

        private IFlareSolverrResponse ExecuteCommandInternal(IFlareSolverrCommand command, bool initFlareSolverr) {

            if (command is null)
                throw new ArgumentNullException(nameof(command));

            // Start FlareSolverr if it hasn't yet been started manually.

            if (initFlareSolverr)
                Start();

            if (string.IsNullOrEmpty(command.Session) && !string.IsNullOrWhiteSpace(sessionId) && command is FlareSolverrCommand mutableCommand)
                mutableCommand.Session = sessionId;

            using (IWebClient webClient = CreateWebClient()) {

                Uri flareSolverrUri = GetFlareSolverrUri();

                string responseJson = webClient.UploadString(flareSolverrUri, command.ToString());

                return JsonConvert.DeserializeObject<FlareSolverrResponse>(responseJson);

            }

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

            if (string.IsNullOrWhiteSpace(sessionId)) {

                logger.Info($"Starting new session");

                IFlareSolverrResponse response = ExecuteCommandInternal(new FlareSolverrCommand("sessions.create") {
                    UserAgent = options.UserAgent,
                }, initFlareSolverr: false);

                logger.Info($"Started session with ID {response.Session}");

                sessionId = response.Session;

            }

        }
        private void DestroySession() {

            logger.Info($"Destroying session {sessionId}");

            ExecuteCommandInternal(new FlareSolverrCommand("sessions.destroy") {
                Session = sessionId,
            }, initFlareSolverr: false);

        }

        private string GetFlareSolverrExecutablePath() {

            return FlareSolverrUtilities.FindFlareSolverrExecutablePath(options.FlareSolverrDirectoryPath);

        }
        private Uri GetFlareSolverrUri() {

            return new Uri($"http://localhost:{FlareSolverrUtilities.DefaultPort}/v1");

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

            process.OutputDataReceived += OutputDataReceivedHandler;
            process.ErrorDataReceived += ErrorDataReceivedHandler;

            return process;

        }
        private void OutputDataReceivedHandler(object sender, DataReceivedEventArgs e) {

            if (!string.IsNullOrWhiteSpace(e.Data))
                logger.Debug(e.Data);

        }
        private void ErrorDataReceivedHandler(object sender, DataReceivedEventArgs e) {

            if (!string.IsNullOrWhiteSpace(e.Data))
                logger.Error(e.Data);

        }

    }

}