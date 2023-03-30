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

            // ./flaresolverr.exe

            if (File.Exists(Path.Combine(inDirectoryPath, flareSolverrFileName)))
                return Path.Combine(inDirectoryPath, flareSolverrFileName);

            // ./flaresolverr/flaresolverr.exe
            // ./flaresolverr-vx.x.x-windows-xxx/flaresolverr/flaresolverr.exe
            // ./flaresolverr_windows_x64/flaresolverr/flaresolverr.exe

            // Use natural sorting so that newer versions are listed before older versions.

            foreach (string directoryPath in Directory.EnumerateDirectories(inDirectoryPath, "flaresolverr*", SearchOption.TopDirectoryOnly).OrderByDescending(path => path, new NaturalSortComparer())) {

                foreach (string candidateExecutablePath in new[] {
                    Path.Combine(directoryPath, flareSolverrFileName),
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