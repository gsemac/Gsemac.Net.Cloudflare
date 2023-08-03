using Gsemac.Core;
using Gsemac.IO;
using Gsemac.IO.Compression;
using Gsemac.IO.Logging;
using Gsemac.Net.Extensions;
using Gsemac.Net.GitHub;
using Gsemac.Net.GitHub.Extensions;
using Gsemac.Net.Http;
using Gsemac.Net.Http.Extensions;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Gsemac.Net.Cloudflare.FlareSolverr {

    public class FlareSolverrUpdater :
        IFlareSolverrUpdater {

        // Public members

        public event DownloadFileProgressChangedEventHandler DownloadFileProgressChanged;
        public event DownloadFileCompletedEventHandler DownloadFileCompleted;

        public FlareSolverrUpdater() :
           this(FlareSolverrOptions.Default) {
        }
        public FlareSolverrUpdater(IFlareSolverrOptions options) :
            this(new HttpWebRequestFactory(), options) {
        }
        public FlareSolverrUpdater(ILogger logger) :
            this(FlareSolverrOptions.Default, logger) {
        }
        public FlareSolverrUpdater(IFlareSolverrOptions options, ILogger logger) :
            this(HttpWebRequestFactory.Default, options, logger) {
        }
        public FlareSolverrUpdater(IHttpWebRequestFactory webRequestFactory, IFlareSolverrOptions options) :
            this(webRequestFactory, options, new NullLogger()) {
        }
        public FlareSolverrUpdater(IHttpWebRequestFactory webRequestFactory, IFlareSolverrOptions options, ILogger logger) {

            if (webRequestFactory is null)
                throw new ArgumentNullException(nameof(webRequestFactory));

            if (options is null)
                throw new ArgumentNullException(nameof(options));

            if (logger is null)
                throw new ArgumentNullException(nameof(logger));

            this.webRequestFactory = webRequestFactory;
            this.options = options;
            this.logger = new NamedLogger(logger, nameof(FlareSolverrUpdater));

        }

        public IFlareSolverrInfo UpgradeToLatestVersion(CancellationToken cancellationToken) {

            logger.Info("Checking for FlareSolverr updates");

            IFlareSolverrInfo flareSolverrInfo = GetFlareSolverrInfo();
            System.Version latestVersion = GetLatestFlareSolverrVersion();

            bool updateRequired = (!flareSolverrInfo.Version?.Equals(latestVersion) ?? true) ||
                !File.Exists(flareSolverrInfo.ExecutablePath);

            if (updateRequired) {

                logger.Info($"Updating FlareSolverr to version {latestVersion}");

                if (DownloadFlareSolverr(cancellationToken)) {

                    flareSolverrInfo = new FlareSolverrInfo() {
                        ExecutablePath = FlareSolverrUtilities.GetExecutablePath(options),
                        Version = latestVersion,
                    };

                    SaveFlareSolverrInfo(flareSolverrInfo);

                }

            }
            else
                logger.Info($"FlareSolverr is up to date ({flareSolverrInfo.Version})");

            return flareSolverrInfo;

        }

        // Protected members

        protected void OnDownloadFileProgressChanged(object sender, DownloadFileProgressChangedEventArgs e) {

            DownloadFileProgressChanged?.Invoke(this, e);

        }
        protected void OnDownloadFileCompleted(object sender, DownloadFileCompletedEventArgs e) {

            DownloadFileCompleted?.Invoke(this, e);

        }

        // Private members

        private readonly IFlareSolverrOptions options;
        private readonly IHttpWebRequestFactory webRequestFactory;
        private readonly ILogger logger;

        private string GetFlareSolverrInfoPath() {

            string currentDirectory = options.FlareSolverrDirectoryPath;

            if (string.IsNullOrWhiteSpace(currentDirectory))
                currentDirectory = Directory.GetCurrentDirectory();

            return Path.Combine(currentDirectory, "FlareSolverr.json");

        }
        private IFlareSolverrInfo GetFlareSolverrInfo() {

            string flareSolverrInfoPath = GetFlareSolverrInfoPath();

            if (File.Exists(flareSolverrInfoPath)) {

                string metadataJson = File.ReadAllText(flareSolverrInfoPath);

                return JsonConvert.DeserializeObject<FlareSolverrInfo>(metadataJson);

            }
            else {

                // If no metadata file exists, we can attempt to extract version information from the executable path.

                string flareSolverrExecutablePath = FlareSolverrUtilities.GetExecutablePath(options);

                if (!string.IsNullOrWhiteSpace(flareSolverrExecutablePath)) {

                    Match versionMatch = Regex.Match(flareSolverrExecutablePath, @"\bv(\d+\.\d+\.\d+)\b");

                    if (versionMatch.Success) {

                        return new FlareSolverrInfo() {
                            ExecutablePath = flareSolverrExecutablePath,
                            Version = new System.Version(versionMatch.Groups[1].Value),
                        };

                    }

                }

            }

            // We couldn't find any metadata.

            return new FlareSolverrInfo();

        }
        private void SaveFlareSolverrInfo(IFlareSolverrInfo flareSolverrInfo) {

            File.WriteAllText(GetFlareSolverrInfoPath(), JsonConvert.SerializeObject(flareSolverrInfo, Formatting.Indented));

        }
        private System.Version GetLatestFlareSolverrVersion() {

            logger.Info("Checking latest FlareSolverr version");

            System.Version version = new System.Version();

            IGitHubClient gitHubClient = new GitHubWebClient(webRequestFactory);
            IRelease latestRelease = gitHubClient.GetLatestRelease(Properties.Urls.FlareSolverrRepository);

            if (latestRelease is object) {

                if (Core.Version.TryParse(latestRelease.Tag, out IVersion parsedVersion))
                    version = parsedVersion.ToVersion();

            }

            logger.Info($"Latest FlareSolverr version is {version}");

            return version;

        }
        private bool DownloadFlareSolverr(CancellationToken cancellationToken) {

            logger.Info("Getting FlareSolverr download URL");

            IGitHubClient gitHubClient = new GitHubWebClient(webRequestFactory);
            IRelease latestRelease = gitHubClient.GetLatestRelease(Properties.Urls.FlareSolverrRepository);
            IReleaseAsset asset = latestRelease.Assets.Where(a => Regex.IsMatch(a.Name, GetPlatformRegexPattern(), RegexOptions.IgnoreCase)).FirstOrDefault();

            if (asset is null) {

                logger.Warning($"Could not find an appropriate release for this platform ({GetPlatformRegexPattern()}).");

            }
            else {

                logger.Info($"Downloading {asset.DownloadUrl}");

                string currentDirectory = options.FlareSolverrDirectoryPath;

                if (string.IsNullOrWhiteSpace(currentDirectory))
                    currentDirectory = Directory.GetCurrentDirectory();

                string downloadFilePath = Path.Combine(currentDirectory, asset.Name);

                using (IWebClient client = webRequestFactory.ToWebClientFactory().Create()) {

                    client.DownloadProgressChanged += (sender, e) => OnDownloadFileProgressChanged(this, new DownloadFileProgressChangedEventArgs(new Uri(asset.DownloadUrl), downloadFilePath, e));
                    client.DownloadFileCompleted += (sender, e) => OnDownloadFileCompleted(this, new DownloadFileCompletedEventArgs(new Uri(asset.DownloadUrl), downloadFilePath, e.Error is null));

                    try {

                        client.DownloadFileSync(new Uri(asset.DownloadUrl), downloadFilePath, cancellationToken);

                        logger.Info($"Extracting {PathUtilities.GetFileName(downloadFilePath)}");

                        ArchiveUtilities.Extract(downloadFilePath, extractToNewFolder: true);

                    }
                    catch (Exception ex) {

                        logger.Info(ex.ToString());

                        throw ex;

                    }
                    finally {

                        if (File.Exists(downloadFilePath))
                            File.Delete(downloadFilePath);

                    }

                }

            }

            // If we found and downloaded the appropriate release asset, the process was successful.

            return asset is object;

        }

        private string GetPlatformRegexPattern() {

            // Depending on the version, the platform strings will take one of two forms:
            // platform-architecture (e.g. "windows-x64")
            // platform_architecture (e.g. "windows_x64")

            string operatingSystemStr = EnvironmentUtilities.GetPlatformInfo().Id != PlatformId.Windows ? "linux" : "windows";
            string architectureStr = Environment.Is64BitOperatingSystem ? "x64" : "x86";

            return $"{operatingSystemStr}[-_]{architectureStr}";

        }

    }

}