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

            string inDirectoryPath = string.IsNullOrWhiteSpace(options.FlareSolverrDirectoryPath) ?
                Directory.GetCurrentDirectory() :
                options.FlareSolverrDirectoryPath;

            string flareSolverrFileName = string.IsNullOrWhiteSpace(options.FlareSolverrFileName) ?
                GetExecutableFilename() :
                options.FlareSolverrFileName;

            // FlareSolverr may be located at any of the following paths:

            // ./flaresolverr.exe
            // ./flaresolverr/flaresolverr.exe
            // ./flaresolverr-vx.x.x-windows-xxx/flaresolverr/flaresolverr.exe
            // ./flaresolverr_windows_x64/flaresolverr/flaresolverr.exe
            // ./flaresolverr_windows_x64/flaresolver/flaresolverr.exe

            // The "flaresolver" subfolder is a typo that was present in the v3.2.2 release archive.

            if (File.Exists(Path.Combine(inDirectoryPath, flareSolverrFileName)))
                return Path.Combine(inDirectoryPath, flareSolverrFileName);

            // Use natural sorting so that newer versions are listed before older versions.
            // Newer binary releases of Cloudflare (v3.1.0+) do not contain version information, so we still might end up locating an older version.

            foreach (string directoryPath in Directory.EnumerateDirectories(inDirectoryPath, "flaresolverr*", SearchOption.TopDirectoryOnly).OrderByDescending(path => path, new NaturalSortComparer())) {

                foreach (string candidateExecutablePath in new[] {
                    Path.Combine(directoryPath, flareSolverrFileName),
                    Path.Combine(directoryPath, "flaresolver", flareSolverrFileName),
                    Path.Combine(directoryPath, "flaresolverr", flareSolverrFileName),
                }) {

                    if (File.Exists(candidateExecutablePath))
                        return candidateExecutablePath;

                }

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