using Gsemac.Collections;
using System.IO;
using System.Linq;

namespace Gsemac.Net.Cloudflare.FlareSolverr {

    internal static class FlareSolverrUtilities {

        public const string FlareSolverrExecutablePath = "flaresolverr.exe";
        public const int DefaultPort = 8191;

        internal static string FindFlareSolverrExecutablePath(string inDirectoryPath) {

            if (string.IsNullOrWhiteSpace(inDirectoryPath))
                inDirectoryPath = Directory.GetCurrentDirectory();

            // ./flaresolverr.exe

            if (File.Exists(Path.Combine(inDirectoryPath, FlareSolverrExecutablePath)))
                return Path.Combine(inDirectoryPath, FlareSolverrExecutablePath);

            // ./flaresolverr/flaresolverr.exe
            // ./flaresolverr-vx.x.x-windows-xxx/flaresolverr/flaresolverr.exe

            // Use natural sorting so that newer versions are listed before older versions.

            foreach (string directoryPath in Directory.EnumerateDirectories(inDirectoryPath, "flaresolverr*", SearchOption.TopDirectoryOnly).OrderByDescending(path => path, new NaturalSortComparer())) {

                foreach (string candidateExecutablePath in new[] {
                    Path.Combine(directoryPath, FlareSolverrExecutablePath),
                    Path.Combine(directoryPath, "flaresolverr", FlareSolverrExecutablePath),
                }) {

                    if (File.Exists(candidateExecutablePath))
                        return candidateExecutablePath;

                }

            }

            // The FlareSolverr executable could not be found.

            return string.Empty;

        }

    }

}