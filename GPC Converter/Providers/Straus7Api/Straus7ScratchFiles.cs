using System;
using System.IO;
using System.Linq;

namespace GPC.Converter.Straus7
{
    /// <summary>Native files created by one export or scratch session. Only files that did not exist before the session
    /// (same stem, any extension) are removed, so an existing user file is never deleted.</summary>
    internal sealed class Straus7ScratchFiles
    {
        private readonly string directory, pattern;
        private readonly string[] before;
        internal string Path { get; }
        internal Straus7ScratchFiles(string path)
        {
            Path = System.IO.Path.GetFullPath(path); directory = System.IO.Path.GetDirectoryName(Path);
            pattern = System.IO.Path.GetFileNameWithoutExtension(Path) + ".*"; before = Current();
        }
        private string[] Current() => Directory.Exists(directory) ? Directory.GetFiles(directory, pattern) : new string[0];
        /// <summary>Deletes the files this session created; failures to delete are ignored so the original error is preserved.</summary>
        internal void DeleteCreated()
        {
            foreach (var file in Current().Except(before, StringComparer.OrdinalIgnoreCase))
                try { File.Delete(file); } catch (IOException) { } catch (UnauthorizedAccessException) { }
        }
        /// <summary>A new scratch model path in <paramref name="scratchDirectory"/>.</summary>
        internal static Straus7ScratchFiles Create(string scratchDirectory, string prefix)
        {
            var directory = System.IO.Path.GetFullPath(scratchDirectory); if (!Directory.Exists(directory)) throw new DirectoryNotFoundException(directory);
            return new Straus7ScratchFiles(System.IO.Path.Combine(directory, prefix + Guid.NewGuid().ToString("N") + ".st7"));
        }
    }
}
