using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace GPC.Model
{
    /// <summary>
    /// Serialization versions of the ISerializable domain types (architecture review R10). The archives are compatible backwards only: a
    /// deserialization constructor refuses a version newer than the one its class writes, instead of reading the older layout and leaving
    /// the newer data NaN or default (ANTHEA register S4-1: generic sections of version 5 read by a Model that knew version 4).
    /// </summary>
    internal static class SerializationVersions
    {
        /// <summary>The version of the entry, or <paramref name="missing"/> for data written before the entry existed.</summary>
        /// <param name="current">The version that the class writes.</param>
        /// <exception cref="SerializationException">The data were written by a newer GPCModel.</exception>
        internal static int Read(SerializationInfo info, string entry, int current, int missing)
        {
            int version = missing;
            foreach (SerializationEntry e in info)
                if (e.Name == entry) { version = Convert.ToInt32(e.Value, CultureInfo.InvariantCulture); break; }
            return Check(info, entry, version, current);
        }

        /// <summary>The version of an entry that the data always contain.</summary>
        internal static int Read(SerializationInfo info, string entry, int current)
            => Check(info, entry, info.GetInt32(entry), current);

        private static int Check(SerializationInfo info, string entry, int version, int current)
        {
            if (version > current)
                throw new SerializationException("NewerSerializationVersion: " + info.FullTypeName + " " + entry + " = " + version
                    + ", this GPCModel reads up to " + current + ". Update GPCModel to read these data.");
            return version;
        }
    }
}
