using Gsemac.IO.Logging;
using Gsemac.IO.Logging.Extensions;
using Gsemac.Net.Extensions;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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

            bool flareSolverrIsRunning = processState == FlareSolverrProcessState.Started;

            updaterCancellationTokenSource.Cancel();

            lock (mutex) {

                if (flareSolverrIsRunning) {

                    logger.Info("Stopping FlareSolverr service");

                    StopFlareSolverr();

                    processState = FlareSolverrProcessState.Stopped;

                }

            }

            return flareSolverrIsRunning &&
                processState == FlareSolverrProcessState.Stopped;

        }

        public override IFlareSolverrResponse SendCommand(IFlareSolverrCommand command) {

            return SendCommandInternal(command, initFlareSolverr: true);

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

        private enum FlareSolverrProcessState {
            Stopped,
            Started,
        }

        private readonly IHttpWebRequestFactory webRequestFactory;
        private readonly IFlareSolverrOptions options;
        private readonly ILogger logger;
        private readonly object mutex = new object();
        private readonly Lazy<string> flareSolverrExecutablePath;
        private readonly CancellationTokenSource updaterCancellationTokenSource = new CancellationTokenSource();
        private FlareSolverrProcessState processState = FlareSolverrProcessState.Stopped;
        private string sessionId;
        private Process flareSolverrProcess;

        private string GetFlareSolverrExecutablePath() {

            return FlareSolverrUtilities.GetExecutablePath(options);

        }
        private Uri GetFlareSolverrUri() {

            return new Uri($"http://localhost:{options.Port}/v1");

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
        private Process CreateProcess(string fileName) {

            ProcessStartInfo processStartInfo = new ProcessStartInfo() {
                FileName = fileName,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            switch (options.LogLevel) {

                case LogLevel.Info:
                    processStartInfo.EnvironmentVariables["LOG_LEVEL"] = "info";
                    break;

                case LogLevel.Debug:
                    processStartInfo.EnvironmentVariables["LOG_LEVEL"] = "debug";
                    break;

            }

            processStartInfo.EnvironmentVariables["LOG_HTML"] = options.LogHtml ? "true" : "false";
            processStartInfo.EnvironmentVariables["HEADLESS"] = options.Headless ? "true" : "false";

            if (options.BrowserTimeout > TimeSpan.Zero)
                processStartInfo.EnvironmentVariables["BROWSER_TIMEOUT"] = options.BrowserTimeout.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);

            if (!string.IsNullOrWhiteSpace(options.TestUrl))
                processStartInfo.EnvironmentVariables["TEST_URL"] = options.TestUrl;

            processStartInfo.EnvironmentVariables["PORT"] = options.Port.ToString(CultureInfo.InvariantCulture);

            Process process = new Process {
                StartInfo = processStartInfo
            };

            process.OutputDataReceived += (sender, e) => OutputDataReceivedHandler(e, LogLevel.Info);
            process.ErrorDataReceived += (sender, e) => OutputDataReceivedHandler(e, LogLevel.Error);

            return process;

        }

        private void UpdateFlareSolverr() {

            try {

                IFlareSolverrUpdater updater = new FlareSolverrUpdater(webRequestFactory, options, logger);

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
        private bool DetectOrStartFlareSolverr() {

            lock (mutex) {

                bool success = false;

                if (processState != FlareSolverrProcessState.Started) {

                    if (!SocketUtilities.IsPortAvailable(options.Port)) {

                        // If FlareSolverr already appears to be running (port 8191 in use), don't attempt to start it.

                        logger.Warning($"Port {options.Port} is already in use; assuming FlareSolverr is already running");

                        success = true;

                    }
                    else {

                        if (options.AutoUpdateEnabled)
                            UpdateFlareSolverr();

                        logger.Info("Starting FlareSolverr service");

                        if (StartFlareSolverr())
                            processState = FlareSolverrProcessState.Started;

                        success = processState == FlareSolverrProcessState.Started;

                        if (!success)
                            logger.Error("Failed to start FlareSolverr process");

                    }

                    if (success && options.UseSession)
                        CreateSession();

                }

                return success;

            }

        }
        private bool StartFlareSolverr() {

            if (!File.Exists(flareSolverrExecutablePath.Value)) {

                logger.Error($"Unable to locate FlareSolverr executable. Download FlareSolverr at {Properties.Urls.FlareSolverrLatestRelease}");

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

                success = success && WaitForFlareSolverrToStart();

            }

            if (success)
                logger.Info($"FlareSolverr is now listening on port {options.Port}");

            return success;

        }
        private bool WaitForFlareSolverrToStart() {

            // Wait for FlareSolverr to start listening on its designated port.

            DateTimeOffset startTime = DateTimeOffset.Now;
            TimeSpan timeout = TimeSpan.FromSeconds(60);

            do {

                // While we wait, we'll see if the FlareSolverr process exits, indicating that it wasn't able to start up properly.
                // FlareSolverr v2.0.0+ will make a test request as part of the initialization process, which can sometimes take a while before failing (e.g. due to firewall rules).

                if (flareSolverrProcess.WaitForExit((int)TimeSpan.FromSeconds(1).TotalMilliseconds) && flareSolverrProcess.ExitCode != 0)
                    break;

            }
            while (SocketUtilities.IsPortAvailable(options.Port) && (DateTimeOffset.Now - startTime) < timeout);

            return !SocketUtilities.IsPortAvailable(options.Port);

        }
        private void StopFlareSolverr() {

            updaterCancellationTokenSource.Cancel();

            if (!string.IsNullOrWhiteSpace(sessionId))
                DestroySession();

            if (processState == FlareSolverrProcessState.Started && flareSolverrProcess is object && !flareSolverrProcess.HasExited) {

                logger.Info("Stopping FlareSolverr process");

                // Attempt to stop the process gracefully so that it can close running browser instances.

                if (!flareSolverrProcess.CloseMainWindow())
                    flareSolverrProcess.Kill();

                flareSolverrProcess.Dispose();

                flareSolverrProcess = null;

            }

        }

        private void OutputDataReceivedHandler(DataReceivedEventArgs e, LogLevel suggestedLogLevel) {

            if (string.IsNullOrWhiteSpace(e.Data))
                return;

            LogLevel logLevel = suggestedLogLevel;
            string logMessage = e.Data;

            Match m = Regex.Match(logMessage, @"^(?<timestamp>\d+-\d+-\d+T\d+:\d+:\d+-\d+:\d+)\s(?<level>DEBUG|INFO|WARN|ERROR)", RegexOptions.IgnoreCase);

            if (m.Success) {

                logMessage = logMessage.Substring(m.Length).TrimStart();

                switch (m.Groups["level"].Value.ToLowerInvariant()) {

                    case "debug":
                        logLevel = LogLevel.Debug;
                        break;

                    case "info":
                        logLevel = LogLevel.Info;
                        break;

                    case "warn":
                        logLevel = LogLevel.Warning;
                        break;

                    case "error":
                        logLevel = LogLevel.Error;
                        break;

                }

            }

            logger.Log(logLevel, logger.Name, logMessage);

        }

        private IFlareSolverrResponse SendCommandInternal(IFlareSolverrCommand command, bool initFlareSolverr) {

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

        private void CreateSession() {

            if (string.IsNullOrWhiteSpace(sessionId)) {

                logger.Info($"Starting new session");

                IFlareSolverrResponse response = SendCommandInternal(new FlareSolverrCommand("sessions.create") {
                    UserAgent = options.UserAgent,
                }, initFlareSolverr: false);

                logger.Info($"Started session with ID {response.Session}");

                sessionId = response.Session;

            }

        }
        private void DestroySession() {

            logger.Info($"Destroying session {sessionId}");

            SendCommandInternal(new FlareSolverrCommand("sessions.destroy") {
                Session = sessionId,
            }, initFlareSolverr: false);

        }

    }

}