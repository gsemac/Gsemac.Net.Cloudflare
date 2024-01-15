using Gsemac.Core;
using Gsemac.Net.Cloudflare.FlareSolverr.Properties;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Gsemac.Net.Cloudflare.FlareSolverr {

    internal static class FlareSolverrUtilities {

        // Public members

        public const int DefaultPort = 8191;

        // Internal members

        internal static string GetExecutablePath(IFlareSolverrOptions options) {

            string flareSolverrDirectoryPath = string.IsNullOrWhiteSpace(options.FlareSolverrDirectoryPath) ?
                Directory.GetCurrentDirectory() :
                options.FlareSolverrDirectoryPath;

            string flareSolverrFileName = string.IsNullOrWhiteSpace(options.FlareSolverrFileName) ?
                GetExecutableFileName() :
                options.FlareSolverrFileName;

            if (!Directory.Exists(flareSolverrDirectoryPath))
                return string.Empty;

            // FlareSolverr may be located at any of the following paths:
            //
            // ./flaresolverr.exe
            // ./flaresolverr/flaresolverr.exe
            // ./flaresolverr-vx.x.x-windows-xxx/flaresolverr/flaresolverr.exe
            // ./flaresolverr_windows_x64/flaresolverr/flaresolverr.exe
            // ./flaresolverr_windows_x64/flaresolver/flaresolverr.exe
            //
            // The "flaresolver" subfolder is a typo that was present in the v3.2.2 release archive (later releases corrected this).

            // Find all FlareSolverr binaries, and order them by version.
            // For binaries where a version cannot be determined, sort by the last write time (the last created time is not always updated for binaries).

            DirectoryInfo flareSolverrDirectoryInfo = new DirectoryInfo(flareSolverrDirectoryPath);

            string flareSolverrExecutablePath = flareSolverrDirectoryInfo.EnumerateDirectories("flaresolverr*", SearchOption.TopDirectoryOnly)
                .Select(directoryPath => directoryPath.FullName)
                .Concat(new[] {
                    Path.GetFullPath(flareSolverrDirectoryPath),
                })
                .SelectMany(directoryPath => new[] {
                    Path.Combine(directoryPath, flareSolverrFileName),
                    Path.Combine(directoryPath, "flaresolverr", flareSolverrFileName),
                    Path.Combine(directoryPath, "flaresolver", flareSolverrFileName),
                })
                .Where(filePath => !string.IsNullOrWhiteSpace(filePath))
                .Where(filePath => File.Exists(filePath))
                .OrderByDescending(filePath => GetFlareSolverrVersion(filePath))
                .ThenByDescending(filePath => new FileInfo(filePath).LastWriteTime)
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(flareSolverrExecutablePath))
                flareSolverrExecutablePath = string.Empty;

            return flareSolverrExecutablePath;

        }
        internal static System.Version GetFlareSolverrVersion(string flareSolverrExecutablePath) {

            if (!File.Exists(flareSolverrExecutablePath))
                return new System.Version();

            // For FlareSolverr v3.0.0 and onward, we can find version information in the "package.json" file.

            string packageJsonFilePath = Path.Combine(Directory.GetParent(flareSolverrExecutablePath).FullName, "package.json");

            if (File.Exists(packageJsonFilePath)) {

                bool jsonParseError = false;

                IFlareSolverrPackageInfo packageInfo = JsonConvert.DeserializeObject<FlareSolverrPackageInfo>(File.ReadAllText(packageJsonFilePath),
                    new JsonSerializerSettings() {
                        Error = (sender, e) => {
                            jsonParseError = true;
                            e.ErrorContext.Handled = true;
                        },
                    });

                if (!jsonParseError && !string.IsNullOrWhiteSpace(packageInfo.Version) && System.Version.TryParse(packageInfo.Version, out System.Version packageVersion))
                    return packageVersion;

            }

            // For FlareSolverr v1.2.2 through v2.2.10, we can find the version information in the directory name.
            // e.g.: "./flaresolverr-vx.x.x-windows-xxx/flaresolverr/flaresolverr.exe"

            Match directoryNameVersionMatch = Regex.Match(flareSolverrExecutablePath, @"flaresolverr-v(?<version>\d+\.\d+\.\d+)-(?:linux|windows)-x64", RegexOptions.RightToLeft);

            if (directoryNameVersionMatch.Success && System.Version.TryParse(directoryNameVersionMatch.Groups["version"].Value, out System.Version directoryNameVersion))
                return directoryNameVersion;

            // Get the product version directly from the executable.
            // This field isn't set for FlareSolverr's official binary distributions (see caveat below), but may be set for custom builds.
            // Don't do this for versions prior to v3.0.0, because the executable version will be set to the Node.js version instead (e.g. "16.16.0.0").

            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(flareSolverrExecutablePath);
            string productName = fileVersionInfo.ProductName ?? string.Empty;

            if (!productName.Equals("Node.js")) {

                string versionStr = fileVersionInfo.ProductVersion ?? fileVersionInfo.FileVersion;

                // Both "ProductVersion" and "FileVersion" may return an empty string for some executables despite the fields being set.
                // https://stackoverflow.com/q/41690893/5383169
                // In this case, attempt to construct the version string manually.

                if (string.IsNullOrWhiteSpace(versionStr))
                    versionStr = $"{fileVersionInfo.ProductMajorPart}.{fileVersionInfo.ProductMinorPart}.{fileVersionInfo.ProductBuildPart}.{fileVersionInfo.ProductPrivatePart}".Trim('.');

                if (!string.IsNullOrWhiteSpace(versionStr) && System.Version.TryParse(versionStr, out System.Version productVersion))
                    return productVersion;

            }

            // We couldn't determine the version for this executable.

            return new System.Version();

        }

        // Private members

        private static string GetExecutableFileName() {

            switch (EnvironmentUtilities.GetPlatformInfo().Id) {

                case PlatformId.Linux:
                case PlatformId.MacOS:
                    return Paths.FlareSolverrFileNameLinux;

                default:
                    return Paths.FlareSolverrFileNameWindows;

            }

        }

    }

}