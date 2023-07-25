using Gsemac.Core;
using Gsemac.IO;
using Gsemac.IO.Logging;
using Gsemac.Net.Cloudflare.FlareSolverr.Properties;
using Gsemac.Net.Cloudflare.Properties;
using Gsemac.Net.Http;
using Gsemac.Net.Http.Extensions;
using Gsemac.Net.Sockets;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
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

                bool isFlareSolverrReady = StartFlareSolverrIfNotRunning();

                return isFlareSolverrReady;

            }

        }
        public override bool Stop() {

            bool flareSolverrIsRunning = processState == ProcessState.Started;

            updaterCancellationTokenSource.Cancel();

            lock (mutex) {

                if (flareSolverrIsRunning) {

                    logger.Info("Stopping FlareSolverr service");

                    StopFlareSolverr();

                    processState = ProcessState.Stopped;

                }

            }

            return flareSolverrIsRunning &&
                processState == ProcessState.Stopped;

        }

        public override IFlareSolverrResponse GetResponse(IFlareSolverrCommand command) {

            return GetResponse(command, ensureFlareSolverrIsRunning: true, ensureSessionIsCreated: true);

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

        private enum ProcessState {
            Stopped,
            Started,
        }

        private readonly IHttpWebRequestFactory webRequestFactory;
        private readonly IFlareSolverrOptions options;
        private readonly ILogger logger;
        private readonly object mutex = new object();
        private readonly Lazy<string> flareSolverrExecutablePath;
        private readonly CancellationTokenSource updaterCancellationTokenSource = new CancellationTokenSource();
        private ProcessState processState = ProcessState.Stopped;
        private string sessionId;
        private bool sessionsSupported = true;
        private Process flareSolverrProcess;

        private string GetFlareSolverrExecutablePath() {

            return FlareSolverrUtilities.GetExecutablePath(options);

        }
        private Process CreateFlareSolverrProcess(string fileName) {

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
                    processStartInfo.EnvironmentVariables[EnvironmentalVariables.LogLevel] = "info";
                    break;

                case LogLevel.Debug:
                    processStartInfo.EnvironmentVariables[EnvironmentalVariables.LogLevel] = "debug";
                    break;

            }

            processStartInfo.EnvironmentVariables[EnvironmentalVariables.LogHtml] = options.LogHtml ? "true" : "false";
            processStartInfo.EnvironmentVariables[EnvironmentalVariables.Headless] = options.Headless ? "true" : "false";

            if (options.BrowserTimeout > TimeSpan.Zero)
                processStartInfo.EnvironmentVariables[EnvironmentalVariables.BrowserTimeout] = options.BrowserTimeout.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);

            if (!string.IsNullOrWhiteSpace(options.TestUrl))
                processStartInfo.EnvironmentVariables[EnvironmentalVariables.TestUrl] = options.TestUrl;

            processStartInfo.EnvironmentVariables[EnvironmentalVariables.Port] = options.Port.ToString(CultureInfo.InvariantCulture);

            if (options.SkipPlatformCheck)
                processStartInfo.EnvironmentVariables[EnvironmentalVariables.SkipPlatformCheck] = "1";

            Process process = new Process {
                StartInfo = processStartInfo
            };

            process.OutputDataReceived += (sender, e) => OutputDataReceivedHandler(e, LogLevel.Info);
            process.ErrorDataReceived += (sender, e) => OutputDataReceivedHandler(e, LogLevel.Error);

            return process;

        }

        private Uri GetFlareSolverrProxyAddress() {

            return new Uri($"http://localhost:{options.Port}/v1");

        }

        private bool StartFlareSolverr() {

            if (!File.Exists(flareSolverrExecutablePath.Value)) {

                logger.Error($"Unable to locate FlareSolverr executable. Download FlareSolverr at {Urls.FlareSolverrLatestRelease}");

                throw new FileNotFoundException(string.IsNullOrWhiteSpace(flareSolverrExecutablePath.Value) ?
                    ExceptionMessages.FlareSolverrExecutableNotFound :
                    string.Format(ExceptionMessages.FileNotFoundWithFilePath, flareSolverrExecutablePath.Value));

            }

            logger.Info($"Starting FlareSolverr process");

            flareSolverrProcess = CreateFlareSolverrProcess(flareSolverrExecutablePath.Value);

            bool success = flareSolverrProcess.Start();

            if (success) {

                flareSolverrProcess.BeginOutputReadLine();
                flareSolverrProcess.BeginErrorReadLine();

                success = success && WaitForFlareSolverrToBeReady();

            }

            if (success)
                logger.Info($"FlareSolverr is now listening on port {options.Port}");

            return success;

        }
        private bool WaitForFlareSolverrToBeReady() {

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
        private bool StartFlareSolverrIfNotRunning() {

            lock (mutex) {

                bool success = false;

                if (processState != ProcessState.Started) {

                    if (!SocketUtilities.IsPortAvailable(options.Port)) {

                        // If FlareSolverr already appears to be running (port 8191 in use), don't attempt to start it.

                        logger.Warning($"Port {options.Port} is already in use. Assuming FlareSolverr is already running.");

                        success = true;

                    }
                    else {

                        if (options.DownloadUpdates)
                            UpdateFlareSolverr();

                        logger.Info("Starting FlareSolverr service");

                        if (StartFlareSolverr())
                            processState = ProcessState.Started;

                        success = processState == ProcessState.Started;

                        if (!success)
                            logger.Error("Failed to start FlareSolverr process");

                    }

                }

                return success;

            }

        }
        private void StopFlareSolverr() {

            lock (mutex) {

                updaterCancellationTokenSource.Cancel();

                if (!string.IsNullOrWhiteSpace(sessionId))
                    DestroySession();

                KillFlareSolverrProcesses();

            }

        }
        private void KillFlareSolverrProcesses() {

            if (processState == ProcessState.Started && flareSolverrProcess is object && !flareSolverrProcess.HasExited) {

                logger.Info($"Closing {flareSolverrProcess.ProcessName} process ({flareSolverrProcess.Id})");

                // Attempt to stop the process gracefully so that it can close running browser instances.

                if (!(flareSolverrProcess.CloseMainWindow() && flareSolverrProcess.WaitForExit(TimeSpan.FromSeconds(15).Milliseconds)))
                    flareSolverrProcess.Kill();

                flareSolverrProcess.Dispose();

                // FlareSolverr 3.0.0+ leaves an extra process behind when closed, so attempt to close any residual processes.

                // We may get a "ComponentModel.Win32Exception" when attempting to access the process(es).
                // I believe this occurs when FlareSolverr finishes exiting early, so it should be something we can safely ignore.

                try {

                    foreach (Process process in ProcessUtilities.GetProcessesByFilePath(flareSolverrExecutablePath.Value)
                        .Where(p => !p.HasExited)) {

                        logger.Info($"Killing {process.ProcessName} process ({process.Id})");

                        process.Kill();

                    }

                }
                catch (Exception e) {

                    logger.Error($"Error occurred while killing FlareSolverr process(es): {e}");

                }

                // There may still be leftover browser instances, so we should attempt to kill those too.
                // FlareSolverr should terminte these on its own, but sometimes it leaves them behind (https://github.com/FlareSolverr/FlareSolverr/issues/761).

                try {

                    string webBrowserExecutablePath = Path.Combine(PathUtilities.GetParentPath(flareSolverrExecutablePath.Value), "chrome/chrome.exe");

                    foreach (Process process in ProcessUtilities.GetProcessesByFilePath(webBrowserExecutablePath)
                        .Where(p => !p.HasExited)) {

                        logger.Info($"Killing {process.ProcessName} process ({process.Id})");

                        process.Kill();

                    }

                }
                catch (Exception e) {

                    logger.Error($"Error occurred while killing browser process(es): {e}");

                }

                flareSolverrProcess = null;

            }

        }
        private void UpdateFlareSolverr() {

            try {

                IFlareSolverrUpdater updater = new FlareSolverrUpdater(webRequestFactory, options, logger);

                updater.DownloadFileProgressChanged += OnDownloadFileProgressChanged;
                updater.DownloadFileCompleted += OnDownloadFileCompleted;

                updater.UpgradeToLatestVersion(updaterCancellationTokenSource.Token);

            }
            catch (Exception ex) {

                logger.Error(ex.ToString());

                if (!options.IgnoreUpdateErrors)
                    throw ex;

            }

        }

        private void OutputDataReceivedHandler(DataReceivedEventArgs e, LogLevel suggestedLogLevel) {

            if (string.IsNullOrWhiteSpace(e.Data))
                return;

            LogLevel logLevel = suggestedLogLevel;
            string logMessage = e.Data;

            // The log message format can vary depending on the version.

            // 2023-03-12T17:48:45-06:00 INFO FlareSolverr v2.2.10
            // 2023-03-12 17:49:50 INFO FlareSolverr 3.0.4

            Match m = Regex.Match(logMessage, @"^(?<timestamp>.+?)\s+(?<level>DEBUG|INFO|WARN(?:ING)?|ERROR)", RegexOptions.IgnoreCase);

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
                    case "warning":
                        logLevel = LogLevel.Warning;
                        break;

                    case "error":
                        logLevel = LogLevel.Error;
                        break;

                }

            }

            logger.Log(logLevel, logger.Name, logMessage);

        }

        private IWebClient CreateWebClientForCommand() {

            IWebClient webClient = webRequestFactory.ToWebClientFactory().Create();

            // WebClient will use Encoding.Default, which varies by .NET implementation.
            // For example, older implementations use ANSI, while newer implementations use UTF8. Using ANSI will give us garbled characters.
            // Furthermore, RFC4627 states that JSON text will always be encoded in UTF8.

            webClient.Encoding = Encoding.UTF8;

            webClient.Headers[HttpRequestHeader.ContentType] = "application/json";

            return webClient;

        }

        private IFlareSolverrResponse GetResponse(IFlareSolverrCommand command, bool ensureFlareSolverrIsRunning, bool ensureSessionIsCreated) {

            if (command is null)
                throw new ArgumentNullException(nameof(command));

            // Start FlareSolverr if it hasn't yet been started manually.

            bool isFlareSolverrRunning = !ensureFlareSolverrIsRunning || Start();

            // Create a new session if one hasn't already been created, and apply it to the command.

            lock (mutex) {

                if (isFlareSolverrRunning && ensureSessionIsCreated && options.UseSessions && string.IsNullOrWhiteSpace(sessionId) && sessionsSupported)
                    CreateSession();

                if (options.UseSessions && !string.IsNullOrWhiteSpace(sessionId))
                    command.Session = sessionId;

            }

            using (IWebClient webClient = CreateWebClientForCommand()) {

                // Send the command to FlareSolverr and get the response.

                Uri flareSolverrUri = GetFlareSolverrProxyAddress();

                IFlareSolverrResponse response;

                try {

                    response = DeserializeResponse(webClient.UploadString(flareSolverrUri, command.ToString()));

                }
                catch (WebException ex) {

                    // FlareSolverr will return a "500 Internal Server Error" for invalid requests, but some of these errors can be handled.

                    if (ex.Response is object) {

                        using (Stream responseSteam = ex.Response.GetResponseStream())
                            response = DeserializeResponse(responseSteam);

                        if (IsSessionDoesNotExistError(response)) {

                            // If the error was because we tried to use a session that doesn't exist, we will attempt to create a new session and issue the command again.

                            logger.Warning($"No session with ID {command.Session} exists. The session may have been destroyed.");

                            if (isFlareSolverrRunning && ensureSessionIsCreated && options.UseSessions && sessionsSupported) {

                                lock (mutex) {

                                    // Clear the existing session, and create a new one.

                                    sessionId = string.Empty;

                                    CreateSession();

                                }

                                // Attempt to issue the command again with the new session.

                                return GetResponse(command, ensureFlareSolverrIsRunning, ensureSessionIsCreated: false);

                            }

                        }
                        else {

                            // For all other error responses, just return the error response so it can be handled elsewhere.

                            return response;

                        }

                    }

                    throw;

                }

                return response;

            }

        }
        private IFlareSolverrResponse DeserializeResponse(string responseJson) {

            if (responseJson is null)
                throw new ArgumentNullException(nameof(responseJson));

            return JsonConvert.DeserializeObject<FlareSolverrResponse>(responseJson);

        }
        private IFlareSolverrResponse DeserializeResponse(Stream stream) {

            if (stream is null)
                throw new ArgumentNullException(nameof(stream));

            using (StreamReader sr = new StreamReader(stream, Encoding.UTF8))
                return DeserializeResponse(sr.ReadToEnd());

        }
        private bool IsSessionDoesNotExistError(IFlareSolverrResponse response) {

            if (response is null)
                throw new ArgumentNullException(nameof(response));

            return response.Status == FlareSolverrResponseStatus.Error &&
                response.Message.StartsWith("Error: This session does not exist.", StringComparison.OrdinalIgnoreCase);

        }
        private bool IsNotImplementedYetError(IFlareSolverrResponse response) {

            if (response is null)
                throw new ArgumentNullException(nameof(response));

            return response.Status == FlareSolverrResponseStatus.Error &&
                response.Message.StartsWith("Error: Not implemented yet.", StringComparison.OrdinalIgnoreCase);

        }

        private void CreateSession() {

            if (string.IsNullOrWhiteSpace(sessionId) && sessionsSupported) {

                logger.Info($"Creating a new session");

                IFlareSolverrResponse response = GetResponse(new FlareSolverrCommand(FlareSolverrCommand.CreateSession) {
                    UserAgent = options.UserAgent,
                }, ensureFlareSolverrIsRunning: false, ensureSessionIsCreated: false);

                if (response.Status == FlareSolverrResponseStatus.Ok) {

                    logger.Info($"Created session with ID {response.Session}");

                    sessionId = response.Session;

                }
                else if (IsNotImplementedYetError(response)) {

                    logger.Warning($"Sessions are not supported in FlareSolverr v{response.Version}. Proceeding without creating a session.");

                    sessionsSupported = false;

                }
                else {

                    // An unknown error occurred while creating the session.

                    throw new FlareSolverrException(string.Format(ExceptionMessages.FlareSolverrReturnedAFailureResponseWithStatus, response.Status));

                }

            }

        }
        private void DestroySession() {

            logger.Info($"Destroying session {sessionId}");

            GetResponse(new FlareSolverrCommand(FlareSolverrCommand.DestroySession) {
                Session = sessionId,
            }, ensureFlareSolverrIsRunning: false, ensureSessionIsCreated: false);

        }

    }

}