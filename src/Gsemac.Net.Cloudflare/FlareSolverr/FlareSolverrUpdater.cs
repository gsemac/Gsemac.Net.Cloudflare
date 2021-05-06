using Gsemac.Core;
using Gsemac.Core.Extensions;
using Gsemac.IO;
using Gsemac.IO.Compression;
using Gsemac.IO.Logging;
using Gsemac.Net.Extensions;
using Gsemac.Net.GitHub;
using Gsemac.Net.GitHub.Extensions;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace Gsemac.Net.Cloudflare.FlareSolverr {

    public class FlareSolverrUpdater :
        IFlareSolverrUpdater {

        // Public members

        public event DownloadFileProgressChangedEventHandler DownloadFileProgressChanged;
        public event DownloadFileCompletedEventHandler DownloadFileCompleted;
        public event LogEventHandler Log;

        public FlareSolverrUpdater() :
           this(FlareSolverrUpdaterOptions.Default) {
        }
        public FlareSolverrUpdater(IFlareSolverrUpdaterOptions options) :
            this(new HttpWebRequestFactory(), options) {
        }
        public FlareSolverrUpdater(IHttpWebRequestFactory webRequestFactory, IFlareSolverrUpdaterOptions options) {

            if (webRequestFactory is null)
                throw new ArgumentNullException(nameof(webRequestFactory));

            if (options is null)
                throw new ArgumentNullException(nameof(options));

            this.webRequestFactory = webRequestFactory;
            this.options = options;

        }

        public IFlareSolverrInfo Update() {

            OnLog.Info("Checking for FlareSolverr updates");

            IFlareSolverrInfo flareSolverrInfo = GetFlareSolverrInfo();
            System.Version latestVersion = GetLatestFlareSolverrVersion();

            bool updateRequired = !flareSolverrInfo.Version?.Equals(latestVersion) ?? true;

            if (updateRequired) {

                OnLog.Info($"Updating FlareSolverr to version {latestVersion}");

                DownloadFlareSolverr();

                flareSolverrInfo = new FlareSolverrInfo() {
                    ExecutablePath = FlareSolverrUtilities.FindFlareSolverrExecutablePath(options.FlareSolverrDirectoryPath),
                    Version = latestVersion,
                };

                SaveFlareSolverrInfo(flareSolverrInfo);

            }
            else
                OnLog.Info($"FlareSolverr is up to date ({flareSolverrInfo.Version})");

            return flareSolverrInfo;

        }

        // Protected members

        protected LogEventHelper OnLog => new LogEventHelper("FlareSolverr Updater", Log);

        protected void OnDownloadFileProgressChanged(object sender, DownloadFileProgressChangedEventArgs e) {

            DownloadFileProgressChanged?.Invoke(this, e);

        }
        protected void OnDownloadFileCompleted(object sender, DownloadFileCompletedEventArgs e) {

            DownloadFileCompleted?.Invoke(this, e);

        }

        // Private members

        private readonly IFlareSolverrUpdaterOptions options;
        private readonly IHttpWebRequestFactory webRequestFactory;

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

                string flareSolverrExecutablePath = FlareSolverrUtilities.FindFlareSolverrExecutablePath(options.FlareSolverrDirectoryPath);

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

            OnLog.Info("Checking latest FlareSolverr version");

            System.Version version = new System.Version();

            IGitHubClient gitHubClient = new GitHubWebClient(webRequestFactory);
            IRelease latestRelease = gitHubClient.GetLatestRelease("https://github.com/FlareSolverr/FlareSolverr");

            if (latestRelease is object) {

                if (Core.Version.TryParse(latestRelease.Tag, out IVersion parsedVersion))
                    version = parsedVersion.ToVersion();

            }

            OnLog.Info($"Latest FlareSolverr version is {version}");

            return version;

        }
        private void DownloadFlareSolverr() {

            OnLog.Info("Getting FlareSolverr download url");

            IGitHubClient gitHubClient = new GitHubWebClient(webRequestFactory);
            IRelease latestRelease = gitHubClient.GetLatestRelease("https://github.com/FlareSolverr/FlareSolverr");
            IReleaseAsset asset = latestRelease.Assets.Where(a => a.Name.Contains(GetPlatformOS())).FirstOrDefault();

            OnLog.Info($"Downloading {asset.DownloadUrl}");

            using (WebClient client = webRequestFactory.ToWebClientFactory().Create()) {

                string currentDirectory = options.FlareSolverrDirectoryPath;

                if (string.IsNullOrWhiteSpace(currentDirectory))
                    currentDirectory = Directory.GetCurrentDirectory();

                string downloadFilePath = Path.Combine(currentDirectory, asset.Name);

                client.DownloadProgressChanged += (sender, e) => OnDownloadFileProgressChanged(this, new DownloadFileProgressChangedEventArgs(new Uri(asset.DownloadUrl), downloadFilePath, e));
                client.DownloadFileCompleted += (sender, e) => OnDownloadFileCompleted(this, new DownloadFileCompletedEventArgs(new Uri(asset.DownloadUrl), downloadFilePath, e.Error is null));

                client.DownloadFileSync(new Uri(asset.DownloadUrl), downloadFilePath);

                OnLog.Info($"Extracting {PathUtilities.GetFilename(downloadFilePath)}");

                Archive.Extract(downloadFilePath, extractToNewFolder: true);

            }

        }

        private string GetPlatformOS() {

            return "windows-x64";

        }

    }

}