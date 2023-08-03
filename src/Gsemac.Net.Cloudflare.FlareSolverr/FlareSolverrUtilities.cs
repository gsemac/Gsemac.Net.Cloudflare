using Gsemac.Collections;
using Gsemac.Core;
using Gsemac.Net.Cloudflare.FlareSolverr.Properties;
using System.IO;
using System.Linq;

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
                GetExecutableFilename() :
                options.FlareSolverrFileName;

            // FlareSolverr may be located at any of the following paths:
            //
            // ./flaresolverr.exe
            // ./flaresolverr/flaresolverr.exe
            // ./flaresolverr-vx.x.x-windows-xxx/flaresolverr/flaresolverr.exe
            // ./flaresolverr_windows_x64/flaresolverr/flaresolverr.exe
            // ./flaresolverr_windows_x64/flaresolver/flaresolverr.exe
            //
            // The "flaresolver" subfolder is a typo that was present in the v3.2.2 release archive.
            // Later releases corrected this typo, so we should prefer the version that is correctly spelled.

            // If there is a FlareSolverr executable directly in the current directory, return it immediately.

            if (File.Exists(Path.Combine(flareSolverrDirectoryPath, flareSolverrFileName)))
                return Path.Combine(flareSolverrDirectoryPath, flareSolverrFileName);

            // Use natural sorting so that newer versions are listed before older versions.
            // Note that newer binary releases of Cloudflare (v3.1.0+) do not contain version information in the directory name.

            DirectoryInfo flareSolverrDirectoryInfo = new DirectoryInfo(flareSolverrDirectoryPath);

            foreach (string flareSolverrSubDirectoryPath in flareSolverrDirectoryInfo.EnumerateDirectories("flaresolverr*", SearchOption.TopDirectoryOnly)
                .OrderByDescending(dir => dir.Name, new NaturalSortComparer()) // Sort directories by version number
                .Select(dir => dir.FullName)) {

                // Prefer executables with a later last write time.
                // We can't use the creation time, because for some releases this will be earlier than the last write time.
                // Another option is to check the "package.json" file for the version number, but this wasn't available for versions prior to v.3.0.0. 

                string candidateExecutablePath = new[] {
                    Path.Combine(flareSolverrSubDirectoryPath, flareSolverrFileName),
                    Path.Combine(flareSolverrSubDirectoryPath, "flaresolverr", flareSolverrFileName),
                    Path.Combine(flareSolverrSubDirectoryPath, "flaresolver", flareSolverrFileName),
                }
                .Where(path => File.Exists(path))
                .OrderByDescending(path => new FileInfo(path).LastWriteTime)
                .FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(candidateExecutablePath))
                    return candidateExecutablePath;

            }

            // The FlareSolverr executable could not be found.

            return string.Empty;

        }

        // Private members

        private static string GetExecutableFilename() {

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