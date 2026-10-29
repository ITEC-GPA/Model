using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Xml;
using GPC.Model.ElementProperties;
using GPC.Model.Core;
using GPC.Model.Sections.Concrete;

namespace GPC.Model.Analysis
{
    internal static class AnalysisStorage
    {
        internal static byte[] Write<T>(T value)
        {
            using (var stream = new MemoryStream())
            {
                ModelValues.Serializer(typeof(T)).WriteObject(stream, value);
                return stream.ToArray();
            }
        }
        /// <summary>A value written by <see cref="Write{T}"/> (scenario values and samples, also read from archives): an XML
        /// document within the ceilings of one document and within its size (a byte is at most one character, see
        /// <see cref="ModelValues.DocumentSerializer"/>).</summary>
        internal static T Read<T>(byte[] bytes)
        {
            long characters = Math.Max(1, Math.Min(bytes.Length, DomainXmlSerializer.DocumentCharacters));
            using (var stream = new MemoryStream(bytes, false))
            using (var reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null, MaxCharactersInDocument = characters }))
                return (T)ModelValues.DocumentSerializer(typeof(T), characters).ReadObject(reader);
        }
        /// <summary>A model from snapshot bytes: a compact image, or the XML of the snapshots written before the images. The bytes
        /// match the digest declared next to them, which a forged archive declares as well: both are read within their size, the
        /// XML without the ceiling of one document (the graph of a large model exceeds it).</summary>
        internal static Models.Model ReadModel(byte[] bytes)
        {
            if (ModelImage.IsImage(bytes)) return ModelImage.FromBytes(bytes).Open();
            using (var stream = new MemoryStream(bytes, false))
                return (Models.Model)ModelImage.ReadGraph(stream, typeof(Models.Model), bytes.Length);
        }
        internal static string Digest(byte[] bytes)
        {
            using (var sha = SHA256.Create()) return Convert.ToBase64String(sha.ComputeHash(bytes));
        }
        internal static string Design(Models.Model model) => ModelValues.Fingerprint(model.BeamElements.Values
            .Select(b => (object)new object[] { b.Guid, b.BeamProperty, b.Assignments.Sections.ToArray() })
            .Concat(model.AreaElements.Values.Select(a => (object)new object[] { a.Guid, a.Assignments }))
            .Concat(model.AreaElements.Values.Where(a=>a.PlateProperty is ReinforcedConcretePlateSection).Select(a=>(object)a.PlateProperty))
            .Concat(model.PhysicalSurfaces.Count==0 ? Array.Empty<object>() : new object[] { model.PhysicalSurfaces }));
        internal static string Reinforcement(Models.Model model) => ModelValues.Fingerprint(ConcreteSections(model)
            .Select(s => (object)new object[] { s.Rebars.ToArray(), s.ShearData, s.TorsionData }).Concat(model.AreaElements.Values.Select(a => (object)new object[] {
                a.Guid, a.PlateProperty is ReinforcedConcretePlateSection ? PlateSections.SectionAxes(a) : a.Assignments.LayerAxes, PlateSections.Rebars(a).ToArray() })));
        internal static string Prestress(Models.Model model) => ModelValues.Fingerprint(ConcreteSections(model)
            .Select(s => (object)s.Rebars.Where(r => r.EpsilonP != 0).ToArray()));
        private static IEnumerable<ReinforcedConcreteSection> ConcreteSections(Models.Model model) => model.BeamElements.Values
            .SelectMany(b => new[] { b.BeamProperty }.Concat(b.Assignments.Sections.SelectMany(s =>
                new[] { s.Property, s.EndProperty }.Concat(s.Stations.Select(p => p.Property)))))
            .OfType<ReinforcedConcreteSection>();
        /// <summary>Digest of the results of all elements, from their compact form: results not read since the copy are not decoded,
        /// read ones are encoded again (an unchanged list gives the same segments). Runtime token, never persisted.</summary>
        internal static string Results(Models.Model model)
        {
            var elements = model.AllElements.ToArray();
            var results = ModelImage.CaptureResults(model, elements);
            using (var sha = SHA256.Create())
            {
                for (int i = 0; i < elements.Length; i++)
                {
                    var guid = elements[i].Guid.ToByteArray(); sha.TransformBlock(guid, 0, guid.Length, null, 0);
                    var digest = results[i].Digest; sha.TransformBlock(digest, 0, digest.Length, null, 0);
                }
                sha.TransformFinalBlock(new byte[0], 0, 0);
                return Convert.ToBase64String(sha.Hash);
            }
        }
    }
}
