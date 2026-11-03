using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;

namespace GPC.Persistence
{
    /// <summary>Immutable prepared block. The high kind bit denotes stored bytes (already encoded by a domain codec).
    /// Kinds 0..127 use deflate, preserving ANTHEA format 1 byte layout. Addresses are SHA-256 of the decoded block bytes.</summary>
    public sealed class ArchiveBlock
    {
        public const int MaximumBytes = 256 * 1024 * 1024;
        public byte Kind { get; }
        public long Length { get; }
        public string Address { get; }
        internal byte[] Payload { get; }
        private ArchiveBlock(string address, byte kind, long length, byte[] payload)
        { Address = address; Kind = kind; Length = length; Payload = payload; }

        /// <summary>Hashes <paramref name="bytes"/> once and deflates them (or copies them, detached from the caller, when not compressed).</summary>
        public static ArchiveBlock Create(byte kind, byte[] bytes, bool compress = true)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            return compress ? Deflated(kind, bytes) : Stored(kind, (byte[])bytes.Clone());
        }

        /// <summary>As <see cref="Create"/> without compression, taking ownership of <paramref name="bytes"/> instead of copying them:
        /// the caller must not change the array afterwards (for example the buffer of a codec that is discarded).</summary>
        public static ArchiveBlock TakeStored(byte kind, byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            return Stored(kind, bytes);
        }

        private static ArchiveBlock Deflated(byte kind, byte[] bytes)
        {
            if (kind > 127 || bytes.Length > MaximumBytes) throw new ArgumentOutOfRangeException();
            byte[] stored;
            using (var memory = new MemoryStream())
            {
                using (var deflate = new DeflateStream(memory, CompressionLevel.Fastest, true)) deflate.Write(bytes, 0, bytes.Length);
                stored = memory.ToArray();
            }
            if (stored.Length > MaximumBytes) throw new InvalidDataException("Blocco codificato troppo grande.");
            return new ArchiveBlock(Digest(bytes), kind, bytes.LongLength, stored);
        }

        private static ArchiveBlock Stored(byte kind, byte[] bytes)
        {
            if (kind > 127 || bytes.Length > MaximumBytes) throw new ArgumentOutOfRangeException();
            return new ArchiveBlock(Digest(bytes), (byte)(kind | 128), bytes.LongLength, bytes);
        }

        /// <summary>Already prepared deflate from an application codec; verifies identity once on import.</summary>
        public static ArchiveBlock Import(string address, byte kind, long length, byte[] payload)
        {
            ValidateAddress(address);
            if (payload == null || payload.Length > MaximumBytes) throw new ArgumentOutOfRangeException(nameof(payload));
            var block = new ArchiveBlock(address, kind, length, (byte[])payload.Clone());
            block.Decode();
            return block;
        }

        internal static ArchiveBlock FromFile(string address, byte kind, long length, byte[] payload) => new ArchiveBlock(address, kind, length, payload);
        public byte[] EncodedBytes() => (byte[])Payload.Clone();

        /// <summary>The decoded bytes, checked against the address.</summary>
        public byte[] Decode() => Decode(true);

        /// <summary>The decoded bytes, a new array. <paramref name="verify"/> false skips the SHA-256 check, for the caller that created
        /// the block from those bytes and already hashed them once (no second hash of the same data).</summary>
        public byte[] Decode(bool verify)
        {
            byte[] bytes;
            if ((Kind & 128) != 0)
            {
                if (Length != Payload.LongLength) throw new InvalidDataException("Lunghezza del blocco non valida.");
                bytes = (byte[])Payload.Clone();
            }
            else bytes = Inflate(Payload, Length);
            if (verify && Digest(bytes) != Address) throw new InvalidDataException("blocco danneggiato (impronta diversa)");
            return bytes;
        }

        /// <summary>Decodes a payload read from a file into a buffer of its own: stored bytes are returned as they are, without a copy.</summary>
        internal static byte[] DecodeOwned(string address, byte kind, long length, byte[] payload)
        {
            byte[] bytes;
            if ((kind & 128) != 0)
            {
                if (length != payload.LongLength) throw new InvalidDataException("Lunghezza del blocco non valida.");
                bytes = payload;
            }
            else bytes = Inflate(payload, length);
            if (Digest(bytes) != address) throw new InvalidDataException("blocco danneggiato (impronta diversa)");
            return bytes;
        }

        public static byte[] Inflate(byte[] payload, long length)
        {
            if (length < 0 || length > MaximumBytes || length > (long)payload.Length * 1032) throw new InvalidDataException("Lunghezza del blocco non valida.");
            var bytes = new byte[(int)length];
            using (var source = new MemoryStream(payload, false))
            using (var deflate = new DeflateStream(source, CompressionMode.Decompress))
            {
                ReadExactly(deflate, bytes);
                if (deflate.ReadByte() != -1) throw new InvalidDataException("Blocco più lungo del previsto.");
            }
            return bytes;
        }

        public static string Digest(byte[] bytes)
        { using (var sha = SHA256.Create()) return Hex(sha.ComputeHash(bytes)); }

        /// <summary>Upper-case hexadecimal, the spelling of the addresses.</summary>
        internal static string Hex(byte[] hash)
        {
            var text = new char[hash.Length * 2];
            for (int i = 0; i < hash.Length; i++)
            { int high = hash[i] >> 4, low = hash[i] & 15; text[2 * i] = (char)(high < 10 ? '0' + high : 'A' + high - 10); text[2 * i + 1] = (char)(low < 10 ? '0' + low : 'A' + low - 10); }
            return new string(text);
        }

        /// <summary>True for a block address: 64 upper-case hexadecimal digits.</summary>
        public static bool IsAddress(string? address)
        {
            if (address == null || address.Length != 64) return false;
            foreach (char c in address) if (!(c >= '0' && c <= '9' || c >= 'A' && c <= 'F')) return false;
            return true;
        }

        internal static void ValidateAddress(string address)
        {
            if (!IsAddress(address)) throw new InvalidDataException("Indirizzo del blocco non valido.");
        }

        internal static void ReadExactly(Stream stream, byte[] bytes)
        {
            int at = 0;
            while (at < bytes.Length) { int n = stream.Read(bytes, at, bytes.Length - at); if (n == 0) throw new EndOfStreamException(); at += n; }
        }
    }
}
