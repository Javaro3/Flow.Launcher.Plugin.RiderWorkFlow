using System.Text.RegularExpressions;

namespace RiderWorkFlow
{
    public partial class LocalRiderInstallations
    {
        public const string OptionsFilePath = @"/options/recentSolutions.xml";

        public static string GetLatestRiderVersion()
        {
            string[] roots = new string[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
            };

            string? latestVersion = null;
            var latest = new Version(0, 0, 0);

            foreach (var root in roots)
            {
                var jetbrainsDir = Path.Combine(root, "JetBrains");
                if (!Directory.Exists(jetbrainsDir))
                    continue;

                foreach (var dir in Directory.GetDirectories(jetbrainsDir, "JetBrains Rider*"))
                {
                    var folderName = Path.GetFileName(dir);
                    // Match "2025", "2025.1", or "2025.1.5"
                    var match = RiderVersionRegex().Match(folderName);
                    if (!match.Success)
                        continue;

                    int year = int.Parse(match.Groups[1].Value);
                    int minor = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 0;
                    int patch = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : 0;

                    var version = new Version(year, minor, patch);

                    if (version <= latest) continue;

                    latest = version;
                    latestVersion = version.ToString();
                }
            }

            if (latestVersion == null)
                throw new InvalidOperationException("No JetBrains Rider installation found.");

            return latestVersion;
        }

        public static string GetRiderExecutablePath(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(version));

            string[] roots = new string[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
            };

            foreach (var root in roots)
            {
                var jetbrainsDir = Path.Combine(root, "JetBrains");
                if (!Directory.Exists(jetbrainsDir))
                    continue;

                foreach (var dir in Directory.GetDirectories(jetbrainsDir, $"JetBrains Rider*{version}*"))
                {
                    var exePath = Path.Combine(dir, "bin", "rider64.exe");
                    if (File.Exists(exePath))
                        return exePath;
                }
            }

            throw new InvalidOperationException($"Rider executable for version {version} not found.");
        }

        public static string GetRiderRecentSolutionsFilePath()
        {
            var appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JetBrains");
            if (!Directory.Exists(appDataFolder))
                throw new InvalidOperationException("JetBrains AppData folder not found.");

            var latest = new Version(0, 0, 0);
            string? folderWithLatestVersion = null;
            foreach (var dir in Directory.GetDirectories(appDataFolder, "Rider*"))
            {
                var folderName = Path.GetFileName(dir);
                // Match "2025", "2025.1", or "2025.1.5"
                var match = RiderVersionRegex().Match(folderName);
                if (!match.Success)
                    continue;

                int year = int.Parse(match.Groups[1].Value);
                int minor = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 0;
                int patch = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : 0;

                var version = new Version(year, minor, patch);

                if (version <= latest) continue;

                latest = version;
                folderWithLatestVersion = folderName;
            }


            if (folderWithLatestVersion == null)
                throw new InvalidOperationException("No Rider AppData folder found matching version.");

            var optionsFile = Path.Combine(appDataFolder, folderWithLatestVersion, "options", "recentSolutions.xml");

            return File.Exists(optionsFile)
                ? optionsFile
                : throw new FileNotFoundException("Recent solutions file not found.", optionsFile);
        }

        [GeneratedRegex(@"(\d{4})(?:\.(\d+))?(?:\.(\d+))?")]
        private static partial Regex RiderVersionRegex();
    }
}
