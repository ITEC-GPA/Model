using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GPC.Geometry;
using GPC.Model.Attributes;
using GPC.Model.Constraints;
using GPC.Model.Elements;
using GPC.Model.Sections;

namespace GPC.Model.Presentation
{
    /// <summary>Families of the entities of a <see cref="ModelView"/>: ids are unique within a family, as in the model.</summary>
    public enum ViewFamily { Node, Beam, Plate, Solid }

    /// <summary>A vector or a point copied from the model: the view never shares mutable model objects.</summary>
    public readonly struct ViewVector : IEquatable<ViewVector>
    {
        public ViewVector(double x, double y, double z) { X = x; Y = y; Z = z; }
        public double X { get; }
        public double Y { get; }
        public double Z { get; }
        internal static ViewVector Of(Vector3d v) => new ViewVector(v.X, v.Y, v.Z);
        internal static ViewVector Of(Point3d p) => new ViewVector(p.X, p.Y, p.Z);
        public bool Equals(ViewVector other) => X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
        public override bool Equals(object obj) => obj is ViewVector other && Equals(other);
        public override int GetHashCode() { unchecked { return (X.GetHashCode() * 397 ^ Y.GetHashCode()) * 397 ^ Z.GetHashCode(); } }
        public override string ToString() => "(" + X.ToString("R", System.Globalization.CultureInfo.InvariantCulture) + ", "
            + Y.ToString("R", System.Globalization.CultureInfo.InvariantCulture) + ", " + Z.ToString("R", System.Globalization.CultureInfo.InvariantCulture) + ")";
    }

    /// <summary>A point of a section outline, in the coordinates of the section definition.</summary>
    public readonly struct ViewPoint2 : IEquatable<ViewPoint2>
    {
        public ViewPoint2(double x, double y) { X = x; Y = y; }
        public double X { get; }
        public double Y { get; }
        public bool Equals(ViewPoint2 other) => X.Equals(other.X) && Y.Equals(other.Y);
        public override bool Equals(object obj) => obj is ViewPoint2 other && Equals(other);
        public override int GetHashCode() { unchecked { return X.GetHashCode() * 397 ^ Y.GetHashCode(); } }
    }

    /// <summary>Right-handed axes of an entity, as unit vectors in global coordinates.</summary>
    public sealed class ViewAxes
    {
        private ViewAxes(CoordinateSystem axes) { V1 = ViewVector.Of(axes.V1); V2 = ViewVector.Of(axes.V2); V3 = ViewVector.Of(axes.V3); }
        public ViewVector V1 { get; }
        public ViewVector V2 { get; }
        public ViewVector V3 { get; }
        internal static ViewAxes Of(CoordinateSystem axes) => axes == null ? null : new ViewAxes(axes);
    }

    /// <summary>The family and id of an entity.</summary>
    public readonly struct ViewKey : IEquatable<ViewKey>
    {
        public ViewKey(ViewFamily family, int id) { Family = family; Id = id; }
        public ViewFamily Family { get; }
        public int Id { get; }
        public bool Equals(ViewKey other) => Family == other.Family && Id == other.Id;
        public override bool Equals(object obj) => obj is ViewKey other && Equals(other);
        public override int GetHashCode() { unchecked { return (int)Family * 397 ^ Id; } }
        public override string ToString() => Family + " " + Id;
    }

    /// <summary>A node: position and axes of the model (mm), identities of the model and of the source program.</summary>
    public sealed class ViewNode
    {
        internal ViewNode(NodeElement node)
        {
            Id = node.Id; Guid = node.Guid; SourceId = node.Source?.OriginalId;
            Position = ViewVector.Of(node.Position); Axes = ViewAxes.Of(node.CoordinateSystem);
        }
        public int Id { get; }
        public Guid Guid { get; }
        /// <summary>The id in the source program (for example the MIDAS node number); null for a node created in GPC.</summary>
        public string SourceId { get; }
        public ViewVector Position { get; }
        /// <summary>The nodal axes of the model (global unless the source program assigns others).</summary>
        public ViewAxes Axes { get; }
    }

    /// <summary>
    /// A beam, plate or solid element. Beam axes are the section axes of GPC: V3 from I to J, the section outline drawn with its x along V1 and
    /// its y along V2 around the centroid. Plate axes are the element axes of the results: V3 normal to the plate.
    /// </summary>
    public sealed class ViewElement
    {
        internal ViewElement(ViewFamily family, Element element, int[] nodes, string property, ViewAxes axes, string[] groups)
        {
            Family = family; Id = element.Id; Guid = element.Guid; SourceId = element.Source?.OriginalId;
            Nodes = Array.AsReadOnly(nodes); Property = property; Axes = axes; Groups = Array.AsReadOnly(groups);
        }
        public ViewFamily Family { get; }
        public int Id { get; }
        public Guid Guid { get; }
        /// <summary>The id in the source program; null for an element created in GPC.</summary>
        public string SourceId { get; }
        /// <summary>The ids of the nodes, in the order of the element.</summary>
        public IReadOnlyList<int> Nodes { get; }
        /// <summary>The name of the beam, plate or solid property; null when the element has none.</summary>
        public string Property { get; }
        public ViewAxes Axes { get; }
        /// <summary>The names of the groups of the element.</summary>
        public IReadOnlyList<string> Groups { get; }
        /// <summary>Beams: the formulation (BeamFormulation of the model).</summary>
        public string Formulation { get; internal set; }
        /// <summary>Beams: the end offsets in global coordinates (mm); null when absent.</summary>
        public ViewVector? OffsetI { get; internal set; }
        public ViewVector? OffsetJ { get; internal set; }
        /// <summary>Beams: the centroid relative to the reference line, along V1 and V2 (mm); null when unresolved.</summary>
        public ViewPoint2? CentroidOffset { get; internal set; }
        /// <summary>Plates: the physical thickness (mm), from the element or its property; null when unknown.</summary>
        public double? Thickness { get; internal set; }
        /// <summary>Plates: the offset of the middle surface along V3 (mm); null when absent.</summary>
        public double? Offset { get; internal set; }
    }

    /// <summary>
    /// A beam, plate or solid property. Beam sections give their outline (the filled polygon, then the holes) in the coordinates of the section
    /// definition, with the centroid: the beam axis passes through the centroid.
    /// </summary>
    public sealed class ViewProperty
    {
        internal ViewProperty(ViewFamily family, string name, string kind) { Family = family; Name = name; Kind = kind; Outline = Array.Empty<IReadOnlyList<ViewPoint2>>(); }
        public ViewFamily Family { get; }
        public string Name { get; }
        /// <summary>The class of the property in the model (for example ReinforcedConcreteSection, SteelSection, ConcretePlateProperty).</summary>
        public string Kind { get; }
        /// <summary>Plates: the physical thickness of the property (mm).</summary>
        public double? Thickness { get; internal set; }
        /// <summary>Beam sections: the name of the shape (for example SectionRectangular, SectionH) and its height and width (mm).</summary>
        public string Shape { get; internal set; }
        public double? Height { get; internal set; }
        public double? Width { get; internal set; }
        public ViewPoint2? Centroid { get; internal set; }
        public IReadOnlyList<IReadOnlyList<ViewPoint2>> Outline { get; internal set; }
    }

    /// <summary>A group and its members.</summary>
    public sealed class ViewGroup
    {
        internal ViewGroup(string name, ViewKey[] members) { Name = name; Members = Array.AsReadOnly(members); }
        public string Name { get; }
        public IReadOnlyList<ViewKey> Members { get; }
    }

    /// <summary>
    /// A restraint, release, spring, link or constraint, as assigned in the model: Kind is Restraint (node), GroundSpring (node), Release
    /// (beam end), RigidLink or ElasticLink (two nodes) or Constraint (a constraint of the model). Dofs names the restrained, released or
    /// linked degrees of freedom.
    /// </summary>
    public sealed class ViewBoundary
    {
        internal ViewBoundary(string kind, int[] nodes, ViewKey? element, string dofs, string sourceRecord)
        { Kind = kind; Nodes = Array.AsReadOnly(nodes); Element = element; Dofs = dofs; SourceRecord = sourceRecord; }
        public string Kind { get; }
        public IReadOnlyList<int> Nodes { get; }
        public ViewKey? Element { get; }
        public string Dofs { get; }
        public string SourceRecord { get; }
    }

    /// <summary>A load case or a combination of the model.</summary>
    public sealed class ViewCase
    {
        internal ViewCase(string name, bool isCombination, string kind) { Name = name; IsCombination = isCombination; Kind = kind; }
        public string Name { get; }
        public bool IsCombination { get; }
        /// <summary>The class of the case in the model (for example LoadCaseBase, LoadCaseEn1990, Combination).</summary>
        public string Kind { get; }
    }

    /// <summary>An analysis dataset of the model: the provenance of its results.</summary>
    public sealed class ViewDataset
    {
        internal ViewDataset(Analysis.AnalysisDataset d)
        {
            Id = d.Id; Program = d.Program; SolverVersion = d.SolverVersion; AnalysisId = d.AnalysisId; AnalysisSnapshotId = d.AnalysisSnapshotId;
            ModelRevision = d.ModelRevision; InputFingerprint = d.InputFingerprint; Semantics = d.Semantics.ToString(); Units = d.NormalizedUnits; IsSynthetic = d.IsSynthetic;
        }
        public string Id { get; }
        public string Program { get; }
        public string SolverVersion { get; }
        public string AnalysisId { get; }
        public string AnalysisSnapshotId { get; }
        /// <summary>The revision of the model in the source program when the results were read.</summary>
        public string ModelRevision { get; }
        /// <summary>The fingerprint of the GPC model inputs the results belong to.</summary>
        public string InputFingerprint { get; }
        public string Semantics { get; }
        public string Units { get; }
        public bool IsSynthetic { get; }
    }

    /// <summary>
    /// Read-only view of a GPC Model for the model viewers (architecture review R6, decision of the user of 10/10/2026: the viewer is a client of
    /// GPC Model, not of a projection prepared offline). Nodes, elements, properties, groups, boundaries, cases and datasets are copied when the
    /// view is opened, so later edits of the model do not change it (<see cref="IsCurrent"/> tells them). The results are read on demand, one
    /// dataset at a time, from the compact results of the model (<see cref="Results"/>), in the GPC conventions that the Converter applies once
    /// for every source program. Units of the model: mm, N, rad. Identities: the ids and GUIDs of the model, the same used by ModelChecker
    /// and the check reports, and the ids of the source program.
    /// </summary>
    public sealed class ModelView
    {
        /// <summary>The schema of the view: a change of meaning of any member changes it.</summary>
        public const int Schema = 1;
        private readonly Models.Model _model;
        private readonly Dictionary<string, Lazy<ViewResults>> _results = new Dictionary<string, Lazy<ViewResults>>(StringComparer.Ordinal);
        private readonly object _gate = new object();

        private ModelView(Models.Model model, string revision)
        {
            _model = model; Revision = revision; ModelGuid = model.Guid; Name = model.Name;
        }

        /// <summary>Opens a view of the model (copies its geometry; the results stay in the model until read).</summary>
        public static ModelView Open(Models.Model model, CancellationToken cancellationToken = default)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            var view = new ModelView(model, Models.ModelRevisions.AnalysisFingerprint(model));
            view.Read(cancellationToken);
            return view;
        }

        public Guid ModelGuid { get; }
        public string Name { get; }
        /// <summary>The fingerprint of the model inputs when the view was opened (the analysis fingerprint used by the reports).</summary>
        public string Revision { get; }
        /// <summary>The units of the model.</summary>
        public string Units => "N,mm,rad";
        public IReadOnlyList<ViewNode> Nodes { get; private set; }
        public IReadOnlyList<ViewElement> Elements { get; private set; }
        public IReadOnlyList<ViewProperty> Properties { get; private set; }
        public IReadOnlyList<ViewGroup> Groups { get; private set; }
        public IReadOnlyList<ViewBoundary> Boundaries { get; private set; }
        public IReadOnlyList<ViewCase> Cases { get; private set; }
        public IReadOnlyList<ViewDataset> Datasets { get; private set; }

        /// <summary>True if the model inputs are still those of the view (it recomputes the fingerprint of the model).</summary>
        public bool IsCurrent => Models.ModelRevisions.AnalysisFingerprint(_model) == Revision;

        /// <summary>
        /// The results of a dataset, decoded once for all its cases at the first call and then kept by the view (about 100 bytes for a plate
        /// value with its 8 components) until <see cref="Release"/>. The model keeps its results compact. The elements are decoded in
        /// parallel: do not edit the model while a dataset is being read. Real abutment of the review (3,616 elements, 13 datasets of about
        /// 137,000 samples): 1.3 s and 17 MB per dataset.
        /// </summary>
        public ViewResults Results(string dataset, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dataset)) throw new ArgumentException("A dataset id is required.", nameof(dataset));
            if (!_model.Datasets.ContainsKey(dataset)) throw new KeyNotFoundException("UnknownDataset: " + dataset);
            Lazy<ViewResults> results;
            lock (_gate)
            {
                if (!_results.TryGetValue(dataset, out results))
                    _results.Add(dataset, results = new Lazy<ViewResults>(() => ViewResults.Read(_model, dataset, cancellationToken), LazyThreadSafetyMode.ExecutionAndPublication));
            }
            try { return results.Value; }
            catch (OperationCanceledException) { lock (_gate) _results.Remove(dataset); throw; }
        }

        /// <summary>Releases the decoded results of a dataset.</summary>
        public void Release(string dataset) { lock (_gate) _results.Remove(dataset); }

        private void Read(CancellationToken token)
        {
            var groups = new Dictionary<string, List<ViewKey>>(StringComparer.Ordinal);
            string[] GroupsOf(Element e, ViewKey key)
            {
                var names = e.Groups?.Keys.OrderBy(n => n, StringComparer.Ordinal).ToArray() ?? Array.Empty<string>();
                foreach (var name in names)
                {
                    if (!groups.TryGetValue(name, out var members)) groups.Add(name, members = new List<ViewKey>());
                    members.Add(key);
                }
                return names;
            }

            var nodes = new List<ViewNode>(); var elements = new List<ViewElement>(); var boundaries = new List<ViewBoundary>();
            foreach (var node in _model.NodesElements.Values.OrderBy(n => n.Id))
            {
                token.ThrowIfCancellationRequested();
                nodes.Add(new ViewNode(node));
                GroupsOf(node, new ViewKey(ViewFamily.Node, node.Id));
                foreach (var r in node.Assignments.Restrains)
                    boundaries.Add(new ViewBoundary("Restraint", new[] { node.Id }, null, Dofs(r.Restrain), r.SourceRecord));
                foreach (var restraint in node.Attributes.Values.OfType<Restraints.NodeRestrain>())
                    boundaries.Add(new ViewBoundary("Restraint", new[] { node.Id }, null, Dofs(restraint), null));
                foreach (var link in node.Assignments.Links)
                    boundaries.Add(new ViewBoundary(link.Spring != null ? "ElasticLink" : "RigidLink", new[] { node.Id, link.OtherNodeId }, null,
                        link.Spring != null ? "Matrice elastica" : LinkedDofs(link), link.SourceRecord));
                foreach (var spring in node.Assignments.GroundSprings)
                    boundaries.Add(new ViewBoundary("GroundSpring", new[] { node.Id }, null, "Matrice elastica", null));
            }
            foreach (var beam in _model.BeamElements.Values.OrderBy(b => b.Id))
            {
                token.ThrowIfCancellationRequested();
                var key = new ViewKey(ViewFamily.Beam, beam.Id); var a = beam.Assignments;
                var element = new ViewElement(ViewFamily.Beam, beam, new[] { beam.NodeI?.Id ?? 0, beam.NodeJ?.Id ?? 0 }, beam.BeamProperty?.Name,
                    ViewAxes.Of(a.SectionGeometryAxes ?? a.SectionAxes ?? beam.CoordinateSystem), GroupsOf(beam, key))
                {
                    Formulation = a.Formulation.ToString(),
                    OffsetI = Global(a.OffsetI, a.OffsetAxes), OffsetJ = Global(a.OffsetJ, a.OffsetAxes),
                    CentroidOffset = a.SectionCentroidOffset == null ? (ViewPoint2?)null : new ViewPoint2(a.SectionCentroidOffset.X, a.SectionCentroidOffset.Y)
                };
                elements.Add(element);
                foreach (var release in beam.Attributes.Values.OfType<BeamReleasesAttribute>())
                    foreach (var end in new[] { Tuple.Create(beam.NodeI, release.I, "I"), Tuple.Create(beam.NodeJ, release.J, "J") })
                        if (end.Item2 != null && end.Item2.Any(c => c != null && c.Kind != BeamConnectionKind.Continuous))
                            boundaries.Add(new ViewBoundary("Release", new[] { end.Item1?.Id ?? 0 }, key,
                                string.Join(", ", end.Item2.Select((c, i) => c == null || c.Kind == BeamConnectionKind.Continuous ? null : DofNames[i] + ": " + c.Kind).Where(s => s != null)),
                                release.SourceRecord == null ? null : release.SourceRecord + "/" + end.Item3));
            }
            foreach (var plate in _model.AreaElements.Values.OrderBy(p => p.Id))
            {
                token.ThrowIfCancellationRequested();
                var key = new ViewKey(ViewFamily.Plate, plate.Id);
                elements.Add(new ViewElement(ViewFamily.Plate, plate, plate.Nodes.Select(n => n.Id).ToArray(), plate.PlateProperty?.Name,
                    ViewAxes.Of(plate.CoordinateSystem), GroupsOf(plate, key))
                {
                    Thickness = plate.Assignments.PhysicalThickness ?? plate.PlateProperty?.PhysicalThickness,
                    Offset = plate.Assignments.Offset
                });
            }
            foreach (var solid in _model.VolumeElements.Values.OrderBy(s => s.Id))
            {
                token.ThrowIfCancellationRequested();
                var key = new ViewKey(ViewFamily.Solid, solid.Id);
                elements.Add(new ViewElement(ViewFamily.Solid, solid, (solid.Nodes ?? Array.Empty<NodeElement>()).Select(n => n.Id).ToArray(), solid.PlateProperty?.Name,
                    ViewAxes.Of(solid.CoordinateSystem), GroupsOf(solid, key)));
            }
            foreach (var constraint in _model.Costrains.Values.OrderBy(c => c.Id))
            {
                var linked = new[] { constraint.StartNode }.Concat(constraint.EndNodes ?? Array.Empty<NodeElement>()).Where(n => n != null).Select(n => n.Id).ToArray();
                boundaries.Add(new ViewBoundary("Constraint", linked, null, constraint.GetType().Name, constraint.Name));
            }

            Nodes = nodes.AsReadOnly(); Elements = elements.AsReadOnly(); Boundaries = boundaries.AsReadOnly();
            Groups = groups.OrderBy(g => g.Key, StringComparer.Ordinal).Select(g => new ViewGroup(g.Key, g.Value.ToArray())).ToList().AsReadOnly();
            Properties = _model.BeamProperties.Values.Select(Beam).Concat(_model.PlateProperties.Values.Select(p => new ViewProperty(ViewFamily.Plate, p.Name, p.GetType().Name) { Thickness = p.PhysicalThickness }))
                .Concat(_model.BrickProperties.Values.Select(p => new ViewProperty(ViewFamily.Solid, p.Name, p.GetType().Name))).ToList().AsReadOnly();
            Cases = _model.LoadCases.Values.Select(c => new ViewCase(c.Name, false, c.GetType().Name))
                .Concat(_model.Combinations.Values.Select(c => new ViewCase(c.Name, true, c.GetType().Name))).ToList().AsReadOnly();
            Datasets = _model.Datasets.Values.OrderBy(d => d.Id, StringComparer.Ordinal).Select(d => new ViewDataset(d)).ToList().AsReadOnly();
        }

        internal static readonly string[] DofNames = { "DX", "DY", "DZ", "RX", "RY", "RZ" };

        private static string Dofs(Restraints.GeometryRestrain restraint)
        {
            if (restraint?.Restrains == null) return "";
            return string.Join(", ", restraint.Restrains.Where(r => r != null).OrderBy(r => (int)r.Dof).Select(r => DofNames[(int)r.Dof]
                + (r.HasStiffness ? " (molla)" : r.HasImposedDisplacement ? " (spostamento imposto)" : "")));
        }

        private static string LinkedDofs(Structure.Assignments.NodalLink link)
        {
            // One equation of 12 coefficients per rigid degree of freedom: the slave dof is the first nonzero coefficient of the first node.
            var c = link.KinematicCoefficients;
            if (c == null || c.Length % 12 != 0) return "";
            var dofs = new SortedSet<int>();
            for (int row = 0; row < c.Length / 12; row++)
                for (int dof = 0; dof < 6; dof++)
                    if (c[row * 12 + dof] != 0) { dofs.Add(dof); break; }
            return string.Join(", ", dofs.Select(d => DofNames[d]));
        }

        private static ViewVector? Global(Vector3d offset, CoordinateSystem axes)
        {
            if (offset == null || offset.X == 0 && offset.Y == 0 && offset.Z == 0) return null;
            if (axes == null) return ViewVector.Of(offset);
            return new ViewVector(offset.X * axes.V1.X + offset.Y * axes.V2.X + offset.Z * axes.V3.X,
                offset.X * axes.V1.Y + offset.Y * axes.V2.Y + offset.Z * axes.V3.Y, offset.X * axes.V1.Z + offset.Y * axes.V2.Z + offset.Z * axes.V3.Z);
        }

        private static ViewProperty Beam(ElementProperties.BeamProperty property)
        {
            var view = new ViewProperty(ViewFamily.Beam, property.Name, property.GetType().Name);
            ISectionShape shape = (property as Sections.Concrete.ReinforcedConcreteSection)?.SectionShape ?? (property as Sections.Steel.SteelSection)?.SectionShape;
            if (shape == null) return view;
            view.Shape = shape.GetType().Name; view.Height = shape.Height; view.Width = shape.Width;
            view.Centroid = new ViewPoint2(shape.Centroid.X, shape.Centroid.Y);
            var outline = shape.Shape;
            if (outline != null)
            {
                var polygons = new List<IReadOnlyList<ViewPoint2>> { Points(outline.Fill2d) };
                if (outline.Holes2d != null) polygons.AddRange(outline.Holes2d.Select(Points));
                view.Outline = polygons.AsReadOnly();
            }
            return view;
        }

        private static IReadOnlyList<ViewPoint2> Points(Polygon2d polygon) =>
            Array.AsReadOnly((polygon?.ToArray() ?? Array.Empty<Point2d>()).Select(p => new ViewPoint2(p.X, p.Y)).ToArray());
    }
}
