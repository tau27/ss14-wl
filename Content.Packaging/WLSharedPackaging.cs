using Robust.Packaging.AssetProcessing;
using System.Text.RegularExpressions;

namespace Content.Packaging
{
    /// <summary>
    /// COPIED FROM <see cref="Robust.Packaging.RobustSharedPackaging"/>.
    /// </summary>
    public sealed partial class WLSharedPackaging
    {
        [GeneratedRegex(@".*_SERVER.*", RegexOptions.Multiline)]
        private static partial Regex ServerIgnoreRegex();

        public static IReadOnlySet<Regex> ContentClientIgnoredResources { get; } = new HashSet<Regex>
        {
            ServerIgnoreRegex()
        };

        public static Task DoResourceCopy(
            string diskSource,
            AssetPass pass,
            IReadOnlySet<string> ignoreSet,
            IReadOnlySet<Regex> ignoreRegexes,
            string targetDir = "",
            CancellationToken cancel = default)
        {
            foreach (var path in Directory.EnumerateFileSystemEntries(diskSource))
            {
                cancel.ThrowIfCancellationRequested();

                var filename = Path.GetFileName(path);

                var blacklisted_f = ignoreSet.Contains(filename);

                var blacklisted_r = ignoreRegexes.Any(regex =>
                {
                    return regex.IsMatch(path);
                });

                if (blacklisted_r || blacklisted_f)
                    continue;

                var targetPath = Path.Combine(targetDir, filename);
                if (Directory.Exists(path))
                    CopyDirIntoZip(path, targetPath, pass);
                else
                    pass.InjectFileFromDisk(targetPath, path);
            }

            return Task.CompletedTask;
        }

        private static void CopyDirIntoZip(string directory, string basePath, AssetPass pass)
        {
            foreach (var file in Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories))
            {
                var relPath = Path.GetRelativePath(directory, file);
                var zipPath = $"{basePath}/{relPath}";

                if (Path.DirectorySeparatorChar != '/')
                    zipPath = zipPath.Replace(Path.DirectorySeparatorChar, '/');

                // Console.WriteLine($"{directory}/{zipPath} -> /{zipPath}");
                pass.InjectFileFromDisk(zipPath, file);
            }
        }
    }
}
