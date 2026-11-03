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

        public static ArchiveBlock Create(byte kind, byte[] bytes, bool compress = true)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            if (kind > 127 || bytes.Length > MaximumBytes) throw new ArgumentOutOfRangeException();
            byte[] stored;
            if (compress)
            {
                using (var memory = new MemoryStream())
                {
                    using (var deflate = new DeflateStream(memory, CompressionLevel.Fastest, true)) deflate.Write(bytes, 0, bytes.Length);
                    stored = memory.ToArray();
                }
            }
            else stored = (byte[])bytes.Clone();
            if (stored.Length > MaximumBytes) throw new InvalidDataException("Blocco codificato troppo grande.");
            return new ArchiveBlock(Digest(bytes), compress ? kind : (byte)(kind | 128), bytes.LongLength, stored);
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
        public byte[] Decode()
        {
            byte[] bytes;
            if ((Kind & 128) != 0)
            {
                if (Length != Payload.LongLength) throw new InvalidDataException("Lunghezza del blocco non valida.");
                bytes = (byte[])Payload.Clone();
            }
            else bytes = Inflate(Payload, Length);
            if (Digest(bytes) != Address) throw new InvalidDataException("blocco danneggiato (impronta diversa)");
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
        { using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", ""); }
        internal static void ValidateAddress(string address)
        {
            if (address == null || address.Length != 64) throw new InvalidDataException("Indirizzo del blocco non valido.");
            foreach (char c in address) if (!(c >= '0' && c <= '9' || c >= 'A' && c <= 'F')) throw new InvalidDataException("Indirizzo del blocco non valido.");
        }
        internal static void ReadExactly(Stream stream, byte[] bytes)
        {
            int at = 0;
            while (at < bytes.Length) { int n = stream.Read(bytes, at, bytes.Length - at); if (n == 0) throw new EndOfStreamException(); at += n; }
        }
    }
}
