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

using GPC.Model.PostProcessing;

namespace GPC.Model.PostProcessing
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
        internal static T Read<T>(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes, false))
            using (var reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null, MaxCharactersInDocument = 256L * 1024 * 1024 }))
                return (T)ModelValues.Serializer(typeof(T)).ReadObject(reader);
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
        internal static string Results(Models.Model model) => ModelValues.Fingerprint(model.AllElements
            .Select(e => (object)new object[] { e.Guid, e.Results.ToArray() }));
    }
}
