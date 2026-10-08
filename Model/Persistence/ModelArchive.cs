using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Xml;
using GPC.Geometry;
using GPC.Model.Elements;
using GPC.Model.Collections;
using GPC.Model.Attributes;
using GPC.Model.ElementProperties;
using GPC.Model.LoadCases;
using GPC.Model.Loads;
using GPC.Model.Results.ElementResults;
using GPC.Model.Results.ResultLocations;
using GPC.Model.Sections.Concrete;

namespace GPC.Model.Persistence
{
    /// <summary>Versioned XML with a closed set of locally compiled data contracts. No CLR type names from files are resolved.</summary>
    public static class ModelArchive
    {
        private static readonly Type[] KnownTypes = DataTypes();
        private static Type[] DataTypes()
        {
            // The assemblies and namespaces are fixed here, never selected by an input file.
            var domain = ArchiveContractRegistry.Domain;
            var geometry = new[] { typeof(Point3d), typeof(Point2d), typeof(Vector3d), typeof(Vector2d), typeof(CoordinateSystem),
                typeof(Shape), typeof(Shape2d), typeof(Polygon3d), typeof(Polygon2d), typeof(Line3d), typeof(Line2d) };
            var collections = new[] { typeof(SortedCollection<NodeElement>), typeof(SortedCollection<BeamElement>), typeof(SortedCollection<AreaElement>), typeof(SortedCollection<VolumeElement>),
                typeof(UniqueNameCollection<Group>), typeof(UniqueIdCollection<Attributes.Attribute>), typeof(UniqueIdCollection<Load>),
                typeof(UniqueNameCollection<BeamProperty>), typeof(UniqueNameCollection<PlateProperty>), typeof(UniqueNameCollection<BrickProperty>),
                typeof(UniqueNameCollection<LoadCaseBase>), typeof(UniqueNameCollection<FreedomCases.FreedomCase>), typeof(UniqueNameCollection<Combinations.Combination>),
                typeof(UniqueIdCollection<Stages.Stage>), typeof(UniqueIdCollection<Costrains.Costrain>), typeof(UniqueIdCollection<ReinforcedConcreteRebar>),
                typeof(Dictionary<int, HashSet<string>>), typeof(List<ElementResult>), typeof(List<ResultLocation>), typeof(List<Restrains.DofRestrain>),
                typeof(object[]), typeof(double[]), typeof(int[]), typeof(string[]), typeof(NodeElement[]), typeof(KeyValuePair<int,NodeElement>[]),
                typeof(KeyValuePair<int,BeamElement>[]), typeof(KeyValuePair<int,AreaElement>[]), typeof(KeyValuePair<int,VolumeElement>[]) };
            return domain.Concat(domain.Where(t => t != typeof(Group) && t != typeof(Restrains.DofRestrain) && t != typeof(Combinations.Combination.LoadCaseCoefficient)).Select(t => t.MakeArrayType()))
                .Concat(geometry).Concat(new[] { typeof(Polygon3d[]), typeof(Shape[]), typeof(Shape2d[]), typeof(Polygon2d[]) }).Concat(collections).Concat(new[] {
                typeof(MathNet.Numerics.LinearAlgebra.Double.DenseMatrix),
                typeof(MathNet.Numerics.LinearAlgebra.Storage.DenseColumnMajorMatrixStorage<double>),
                typeof(BeamDofConnection[]), typeof(List<Group>), typeof(Dictionary<string,PostProcessing.AnalysisDataset>),
                typeof(PostProcessing.SpringMatrix[]), typeof(PostProcessing.NodalLink[]),
                typeof(PostProcessing.ComponentAvailability[]), typeof(PostProcessing.BeamSectionAssignment[]),
                typeof(List<Combinations.Combination.LoadCaseCoefficient>),
                typeof(PostProcessing.PreservedAssignment[]), typeof(Point3d[])
            }).Distinct().ToArray();
        }
        internal static DataContractSerializer Serializer(Type type) => new DataContractSerializer(type, new DataContractSerializerSettings
        { KnownTypes = KnownTypes, PreserveObjectReferences = true, MaxItemsInObjectGraph = 2000000 });

        /// <summary>Explicit document contract for new applications, including models without an analysis snapshot.</summary>
        public static void SaveDocument(Models.Model model, Stream destination)
        {
            if (model == null || destination == null) throw new ArgumentNullException();
            CheckReportArchive.Validate(model.CheckReports.ToArray());
            using (var memory = new MemoryStream())
            {
                using (var writer = XmlWriter.Create(memory, new XmlWriterSettings { Indent = true, CloseOutput = false }))
                {
                    writer.WriteStartElement("GpcModelArchive"); writer.WriteAttributeString("version", "4");
                    Serializer(typeof(ModelDocument)).WriteObject(writer, ModelDocument.Capture(model)); writer.WriteEndElement();
                }
                memory.Position = 0; memory.CopyTo(destination);
            }
        }

        public static void Save(Models.Model model, Stream destination)
        {
            if (model?.Analysis?.UsesCanonicalFingerprint == true) { SaveDocument(model, destination); return; }
            CheckReportArchive.Validate(model.CheckReports.ToArray());
            // Build the archive before touching the caller's destination on serialization errors.
            using (var memory = new MemoryStream())
            {
                using (var writer = XmlWriter.Create(memory, new XmlWriterSettings { Indent = true, CloseOutput = false }))
                {
                    writer.WriteStartElement("GpcModelArchive"); writer.WriteAttributeString("version", model.Analysis != null || model.VerificationContext != null || model.VerificationScenarios.Count != 0 || CheckReportArchive.RequiresVersion3(model.CheckReports)
                        ? "3" : model.PhysicalMembers.Count != 0 || CheckReportArchive.RequiresVersion2(model.CheckReports) ? "2" : "1");
                    Serializer(typeof(Models.Model)).WriteObject(writer, model); writer.WriteEndElement();
                }
                memory.Position = 0; memory.CopyTo(destination);
            }
        }
        public static Models.Model Load(Stream source)
        {
            using (var reader = XmlReader.Create(source, new XmlReaderSettings
            { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = 256L * 1024 * 1024, CloseInput = false }))
            {
                reader.MoveToContent();
                string version = reader.GetAttribute("version");
                if (reader.LocalName != "GpcModelArchive" || (version != "1" && version != "2" && version != "3" && version != "4")) throw new SerializationException("Unsupported archive version.");
                reader.ReadStartElement();
                var model = version == "4" ? ((ModelDocument)Serializer(typeof(ModelDocument)).ReadObject(reader)).Restore()
                    : (Models.Model)Serializer(typeof(Models.Model)).ReadObject(reader);
                reader.ReadEndElement();
                CheckReportArchive.Validate(model.CheckReports.ToArray());
                if (version != "3" && version != "4" && (model.Analysis != null || model.VerificationContext != null || model.VerificationScenarios.Count != 0 || CheckReportArchive.RequiresVersion3(model.CheckReports)))
                    throw new SerializationException("Analysis/scenario provenance requires archive version 3.");
                model.Analysis?.OpenModel();
                if (version == "1" && (model.PhysicalMembers.Count != 0 || CheckReportArchive.RequiresVersion2(model.CheckReports))) throw new SerializationException("Physical member data require archive version 2.");
                return model;
            }
        }

        /// <summary>Deterministic value digest for Model data and geometry; independent of XML reference numbering.</summary>
        public static string Fingerprint(IEnumerable<object> input)
        {
            return InputFingerprint.Compute(input);
        }
    }
}
