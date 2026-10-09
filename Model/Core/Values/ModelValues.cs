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

namespace GPC.Model.Core
{
    /// <summary>Domain value copies and revision fingerprints. No file format, archive envelope or I/O project is required.</summary>
    public static class ModelValues
    {
        private static readonly Type[] KnownTypes = DataTypes();
        /// <summary>Closed leaf vocabulary shared by document/configuration codecs; caller files cannot add CLR types.</summary>
        public static IReadOnlyCollection<Type> DataContracts => Array.AsReadOnly(KnownTypes);
        private static Type[] DataTypes()
        {
            // The assemblies and namespaces are fixed here, never selected by an input file.
            var domain = ModelDataContracts.Domain;
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

        /// <summary>Detached copy of one registered domain value; aliases inside the value are preserved, references to the source are not.</summary>
        public static T CopyValue<T>(T value) where T : class
        {
            if (value == null) return null;
            if (!KnownTypes.Contains(value.GetType())) throw new SerializationException("UnregisteredDomainValue: " + value.GetType().FullName);
            if (value is Sections.ISectionShape section) { var shape = section.Shape; }
            var serializer = Serializer(value.GetType());
            using (var stream = new MemoryStream())
            { serializer.WriteObject(stream, value); stream.Position = 0; return (T)serializer.ReadObject(stream); }
        }
        /// <summary>Copies the complete graph, preserving IDs, aliases and group event wiring.</summary>
        public static Models.Model Copy(Models.Model model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            Checking.ReportSchema.Validate(model.CheckReports.ToArray());
            var copy = PostProcessing.AnalysisStorage.Read<Models.Model>(CaptureModel(model));
            Checking.ReportSchema.Validate(copy.CheckReports.ToArray());
            copy.Analysis?.OpenModel();
            return copy;
        }
        // Internal snapshot bytes are not a public file format. Kept stable for existing analysis/scenario provenance.
        internal static byte[] CaptureModel(Models.Model model) => PostProcessing.AnalysisStorage.Write(model);
        public static string Fingerprint(IEnumerable<object> input) => InputFingerprint.Compute(input);
    }
}
