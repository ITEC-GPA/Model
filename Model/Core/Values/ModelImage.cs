using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Xml;
using GPC.Model.Elements;
using GPC.Model.Results.Storage;

namespace GPC.Model.Core
{
    /// <summary>
    /// Compact copy of a model for copies, preparations, snapshots and archives: the graph without the element results (the same
    /// data contracts as the XML documents, deflated, without a ceiling of objects or characters) and the compact results of each
    /// element (<see cref="ResultStorage"/>), immutable and shared with the source: no sample is copied.
    /// </summary>
    internal sealed class ModelImage
    {
        private const int Magic = 0x49435047; // "GPCI"
        private const byte Version = 1;
        private readonly byte[] _graph;
        private readonly byte[] _graphDigest;
        private readonly Guid[] _elements;
        private readonly ResultStorage[] _results;

        private ModelImage(byte[] graph, byte[] graphDigest, Guid[] elements, ResultStorage[] results)
        { _graph = graph; _graphDigest = graphDigest; _elements = elements; _results = results; }

        internal static ModelImage Capture(Models.Model model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            var elements = model.AllElements.ToArray();
            return Capture(model, elements, CaptureResults(model, elements));
        }

        /// <summary>Image with results already captured (<see cref="CaptureResults"/>) for the elements of the model, in order.</summary>
        internal static ModelImage Capture(Models.Model model, Element[] elements, ResultStorage[] results)
        {
            using (var packed = new MemoryStream())
            {
                byte[] digest;
                using (var deflate = new DeflateStream(packed, CompressionLevel.Fastest, true))
                using (var hashed = new HashingStream(deflate))
                {
                    WriteGraph(hashed, model, typeof(Models.Model), elements);
                    digest = hashed.Finish();
                }
                return new ModelImage(packed.ToArray(), digest, elements.Select(e => e.Guid).ToArray(), results);
            }
        }

        /// <summary>The compact results of the elements, in order (see <see cref="Element.CaptureResults"/>).</summary>
        internal static ResultStorage[] CaptureResults(Models.Model model, Element[] elements)
        {
            var cases = new ResultCaseTable(model);
            return elements.Select(e => e.CaptureResults(cases)).ToArray();
        }

        /// <summary>A new, independent graph. The compact results are attached, not decoded.</summary>
        internal Models.Model Open()
        {
            Models.Model model;
            using (var inflate = new DeflateStream(new MemoryStream(_graph, false), CompressionMode.Decompress))
                model = (Models.Model)ReadGraph(inflate, typeof(Models.Model));
            AttachResults(model, _elements, _results);
            return model;
        }

        /// <summary>Exact token of the content: graph and results.</summary>
        internal byte[] Digest()
        {
            using (var sha = SHA256.Create())
            {
                sha.TransformBlock(_graphDigest, 0, _graphDigest.Length, null, 0);
                for (int i = 0; i < _elements.Length; i++)
                {
                    var guid = _elements[i].ToByteArray(); sha.TransformBlock(guid, 0, guid.Length, null, 0);
                    var digest = _results[i].Digest; sha.TransformBlock(digest, 0, digest.Length, null, 0);
                }
                sha.TransformFinalBlock(new byte[0], 0, 0);
                return sha.Hash;
            }
        }

        internal int ElementCount => _elements.Length;
        internal Guid ElementGuid(int index) => _elements[index];
        internal ResultStorage Results(int index) => _results[index];

        #region Graph

        /// <summary>Writes <paramref name="root"/> with the data contracts of the model; the <paramref name="externalized"/> elements
        /// leave their results out.</summary>
        internal static void WriteGraph(Stream destination, object root, Type rootType, IEnumerable<Element> externalized)
        {
            using (ResultScope.Externalize(externalized))
                ModelValues.ImageSerializer(rootType).WriteObject(destination, root);
        }

        internal static void WriteGraph(XmlWriter destination, object root, Type rootType, IEnumerable<Element> externalized)
        {
            using (ResultScope.Externalize(externalized))
                ModelValues.ImageSerializer(rootType).WriteObject(destination, root);
        }

        internal static object ReadGraph(Stream source, Type rootType) => ModelValues.ImageSerializer(rootType).ReadObject(source, 0);

        internal static object ReadGraph(XmlReader source, Type rootType) => ModelValues.ImageSerializer(rootType).ReadObject(source);

        /// <summary>Binds the compact results to the elements of <paramref name="model"/>, in the order of the image.</summary>
        internal static void AttachResults(Models.Model model, Guid[] elements, ResultStorage[] results)
        {
            if (elements.Length == 0) return;
            var targets = model.AllElements.ToArray();
            if (targets.Length != elements.Length) throw new SerializationException("ResultElementCountMismatch");
            for (int i = 0; i < targets.Length; i++)
                if (targets[i].Guid != elements[i]) throw new SerializationException("ResultElementMismatch");
            for (int i = 0; i < targets.Length; i++) targets[i].AttachResults(results[i], model);
        }

        #endregion

        #region Bytes

        internal static bool IsImage(byte[] bytes) => bytes != null && bytes.Length >= 5 && BitConverter.ToInt32(bytes, 0) == Magic;

        /// <summary>Self-contained bytes (analysis snapshots). Elements without results are not listed.</summary>
        internal byte[] ToBytes()
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true))
                {
                    writer.Write(Magic); writer.Write(Version);
                    writer.Write(_graph.Length); writer.Write(_graph); writer.Write(_graphDigest);
                    bool any = _results.Any(r => r.Count != 0);
                    WriteResults(writer, any ? _elements : new Guid[0], any ? _results : new ResultStorage[0]);
                }
                return stream.ToArray();
            }
        }

        internal static ModelImage FromBytes(byte[] bytes)
        {
            using (var reader = new BinaryReader(new MemoryStream(bytes, false)))
            {
                try
                {
                    if (reader.ReadInt32() != Magic || reader.ReadByte() != Version) throw new SerializationException("UnsupportedModelImage");
                    int length = reader.ReadInt32(); if (length < 0 || length > bytes.Length) throw new SerializationException("CorruptModelImage");
                    var graph = reader.ReadBytes(length); var digest = reader.ReadBytes(32);
                    if (graph.Length != length || digest.Length != 32) throw new SerializationException("CorruptModelImage");
                    ReadResults(reader, out var elements, out var results);
                    if (reader.BaseStream.Position != bytes.Length) throw new SerializationException("CorruptModelImage");
                    return new ModelImage(graph, digest, elements, results);
                }
                catch (EndOfStreamException ex) { throw new SerializationException("CorruptModelImage", ex); }
            }
        }

        /// <summary>Results section: element GUIDs and segments, in element order.</summary>
        internal static void WriteResults(BinaryWriter writer, Guid[] elements, ResultStorage[] results)
        {
            writer.Write(elements.Length);
            for (int i = 0; i < elements.Length; i++)
            {
                writer.Write(elements[i].ToByteArray());
                var segments = results[i].Segments; writer.Write(segments.Length);
                foreach (var s in segments)
                {
                    writer.Write(s.Count); writer.Write(s.Length); writer.Write(s.Digest);
                    writer.Write(s.Datasets.Length); foreach (var d in s.Datasets) writer.Write(d);
                    writer.Write(s.Payload.Length); writer.Write(s.Payload);
                }
            }
        }

        internal static void ReadResults(BinaryReader reader, out Guid[] elements, out ResultStorage[] results)
        {
            long remaining = reader.BaseStream.CanSeek ? reader.BaseStream.Length - reader.BaseStream.Position : long.MaxValue;
            int count = reader.ReadInt32(); if (count < 0 || count > remaining / 20) throw new SerializationException("CorruptModelImage");
            elements = new Guid[count]; results = new ResultStorage[count];
            for (int i = 0; i < count; i++)
            {
                elements[i] = new Guid(Exactly(reader, 16));
                int n = reader.ReadInt32(); if (n < 0 || n > remaining / 44) throw new SerializationException("CorruptModelImage");
                var segments = new ResultSegment[n];
                for (int k = 0; k < n; k++)
                {
                    int items = reader.ReadInt32(), length = reader.ReadInt32(); var digest = Exactly(reader, 32);
                    int datasets = reader.ReadInt32(); if (items < 0 || length < 0 || datasets < 0 || datasets > remaining) throw new SerializationException("CorruptModelImage");
                    var names = new string[datasets]; for (int d = 0; d < datasets; d++) names[d] = reader.ReadString();
                    int size = reader.ReadInt32(); if (size < 0 || size > remaining) throw new SerializationException("CorruptModelImage");
                    segments[k] = new ResultSegment(Exactly(reader, size), length, items, digest, names);
                }
                results[i] = n == 0 ? ResultStorage.Empty : new ResultStorage(segments);
            }
        }

        private static byte[] Exactly(BinaryReader reader, int count)
        {
            var bytes = reader.ReadBytes(count);
            if (bytes.Length != count) throw new SerializationException("CorruptModelImage");
            return bytes;
        }

        #endregion

        /// <summary>Write-only pass-through computing the SHA-256 of the bytes written.</summary>
        internal sealed class HashingStream : Stream
        {
            private readonly Stream _inner;
            private readonly SHA256 _sha = SHA256.Create();
            internal HashingStream(Stream inner) { _inner = inner; }
            internal byte[] Finish() { _inner.Flush(); _sha.TransformFinalBlock(new byte[0], 0, 0); return _sha.Hash; }
            public override void Write(byte[] buffer, int offset, int count) { _sha.TransformBlock(buffer, offset, count, null, 0); _inner.Write(buffer, offset, count); }
            public override void Flush() => _inner.Flush();
            public override bool CanRead => false;
            public override bool CanSeek => false;
            public override bool CanWrite => true;
            public override long Length => throw new NotSupportedException();
            public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
            public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
            protected override void Dispose(bool disposing) { if (disposing) _sha.Dispose(); base.Dispose(disposing); }
        }
    }
}
