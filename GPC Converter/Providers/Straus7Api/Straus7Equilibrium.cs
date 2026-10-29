using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Core.Coordinates;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Core.Identity;
using GPC.Model.Elements;
using GPC.Model.Loads;
using GPC.Model.Results.Locations;
using GPC.Model.Results.Processing;
using GPC.Model.Results.Queries;
using GPC.Model.Results.State;
using GPC.Model.Structure.Topology;

namespace GPC.Converter.Straus7
{
    /// <summary>Global equilibrium of one linear static case of a Straus7 model: the loads of the imported Model against the support reactions of the
    /// solver, in global N and N mm about the global origin (<see cref="Equilibrium.Check"/>). Applied: node forces and moments, beam loads, uniform
    /// plate pressures and tractions, non-uniform plate pressures and, in a gravity case, the weight of beams and plates (<see cref="Equilibrium.Gravity"/>),
    /// of the node translational masses and of the non-structural masses of the case, as the load case mass options of Straus7 apply them.
    /// Reactions: the reported nodal reactions (force on the node), springs to ground included.</summary>
    public sealed class Straus7CaseBalance
    {
        public string Case { get; internal set; }
        /// <summary>Resultant of the applied actions: force (N) and moment about the global origin (N mm), global X, Y, Z.</summary>
        public double[] AppliedForce { get; internal set; }
        public double[] AppliedMoment { get; internal set; }
        /// <summary>Resultant of the reactions on the structure.</summary>
        public double[] ReactionForce { get; internal set; }
        public double[] ReactionMoment { get; internal set; }
        public int Reactions { get; internal set; }
        /// <summary>True when applied and reactions balance within the tolerances of <see cref="Straus7Equilibrium"/>; null when not evaluated.</summary>
        public bool? Balanced { get; internal set; }
        /// <summary>Actions the Model does not represent completely (for example elements without a mass property under gravity).</summary>
        public IReadOnlyList<string> NotRepresented { get; internal set; } = new string[0];
        public IReadOnlyList<ModelDiagnostic> Diagnostics { get; internal set; } = new ModelDiagnostic[0];
    }

    public static class Straus7Equilibrium
    {
        /// <summary>Tolerances of the check: 1e-6 N and 1e-3 N mm plus 1e-6 times the sum of the magnitudes of the terms (solver round-off).</summary>
        public const double RelativeTolerance = 1e-6;

        /// <summary>Balance of <paramref name="caseName"/> (a static load case of the Model) with the reactions of <paramref name="datasetId"/>. Staged
        /// cases and other states are not evaluated (Balanced = null).</summary>
        public static Straus7CaseBalance Check(GPC.Model.Models.Model model, string caseName, string datasetId)
        {
            if (model == null || string.IsNullOrWhiteSpace(caseName) || string.IsNullOrWhiteSpace(datasetId)) throw new ArgumentNullException();
            var balance = new Straus7CaseBalance { Case = caseName };
            var contributions = new List<EquilibriumContribution>(); var missing = new List<string>(); var diagnostics = new List<ModelDiagnostic>();
            int part = 0; string Id(string kind) => kind + " " + (++part).ToString(CultureInfo.InvariantCulture);
            var reactions = new List<(NodeElement Node, NodeResultForces Sample)>();
            foreach (var node in model.NodesElements.Values)
                foreach (var sample in node.Results.SelectMany(r => r.Results).OfType<NodeResultForces>())
                    if (sample.Kind == NodalForceKind.SupportReaction && sample.Case?.Name == caseName && sample.State?.DatasetId == datasetId) reactions.Add((node, sample));
            balance.Reactions = reactions.Count;
            var states = reactions.Select(r => r.Sample.State).ToArray();
            if (reactions.Count == 0 || states.Any(s => s.Phase != null || s.Step != null || s.Mode.HasValue || s.MovingLoadPosition != null || s.IsCombined != false)
                || states.Select(s => s.ConcomitantStateId).Distinct().Count() != 1)
            {
                diagnostics.Add(new ModelDiagnostic { Code = "Straus7EquilibriumNotEvaluated", Severity = DiagnosticSeverity.Information, Record = caseName,
                    Message = reactions.Count == 0 ? "No support reaction of the case in the dataset." : "Reactions of a staged, combined or ambiguous state are not checked." });
                balance.Diagnostics = diagnostics; return balance;
            }
            var data = Straus7ModelData.Read(model);
            var inertia = data.Inertia.FirstOrDefault(i => i.Case == caseName);
            bool structural = inertia?.StructuralMass ?? true, nonStructural = inertia?.NonStructuralMass ?? true;
            foreach (var node in model.NodesElements.Values)
                foreach (var load in node.Loads.Values.OfType<PointLoad>().Where(l => l.LoadCase?.Name == caseName))
                    contributions.Add(Equilibrium.PointLoad(load, Id("node load")));
            foreach (var beam in model.BeamElements.Values)
                foreach (var load in beam.Assignments.Loads.Where(l => (l.Concentrated?.LoadCase ?? l.StartIntensity?.LoadCase)?.Name == caseName))
                    contributions.Add(Equilibrium.BeamLoad(beam, load));
            foreach (var shell in model.AreaElements.Values)
                foreach (var load in shell.Loads.Values.Where(l => l.LoadCase?.Name == caseName))
                {
                    if (load is NormalAreaLoad normal) contributions.Add(Equilibrium.ShellPressure(shell, Global(normal.CoordinateSystem, 0, 0, normal.Pressure), caseName, Id("plate pressure"), "Plate/" + shell.Source?.OriginalId + "/" + load.GetType().Name));
                    else if (load is AreaLoad area) contributions.Add(Equilibrium.ShellPressure(shell, Global(area.CoordinateSystem, area.P1, area.P2, area.P3), caseName, Id("plate traction"), "Plate/" + shell.Source?.OriginalId + "/" + load.GetType().Name));
                    else if (load is NonUniformPlatePressure pressure) contributions.Add(Equilibrium.PlatePressure(pressure, Id("plate pressure")));
                    else missing.Add("plate load " + load.GetType().Name + " on plate " + shell.Source?.OriginalId);
                }
            foreach (var gravity in model.ModelLoads.Values.OfType<ModelGravityLoad>().Where(g => g.LoadCase?.Name == caseName))
            {
                var g = gravity.GravityVector;
                if (structural)
                {
                    var withoutMass = new List<Element>();
                    contributions.Add(Equilibrium.Gravity(model, gravity, Id("gravity"), withoutMass));
                    if (withoutMass.Count != 0) missing.Add(withoutMass.Count.ToString(CultureInfo.InvariantCulture) + " elements without a mass property under gravity");
                }
                foreach (var m in data.Masses)
                {
                    bool node = m.Kind == Straus7MassKind.NodeTranslational || m.Kind == Straus7MassKind.NodeNonStructural;
                    if (m.Kind == Straus7MassKind.NodeRotational || (m.Kind == Straus7MassKind.NodeTranslational ? !structural : !nonStructural || m.Case != caseName)) continue;
                    var (mass, point) = Weight(model, m, data);
                    if (mass == null) { missing.Add("mass " + m.Record); continue; }
                    contributions.Add(Equilibrium.Explicit(Id(node ? "node mass" : "element mass"), "gravity", caseName, point,
                        new Vector3d(mass.X * g.X, mass.Y * g.Y, mass.Z * g.Z), new Vector3d(0, 0, 0), m.Record));
                }
            }
            foreach (var (node, sample) in reactions) contributions.Add(Equilibrium.NodalAction(model, node.Id, sample, ActionBody.OnNode, Id("reaction"), "reaction"));
            var scope = new EquilibriumScope { Selection = new ResultSelection { Dataset = datasetId, Case = caseName, ConcomitantState = states[0].ConcomitantStateId },
                CoverageEvidence = "Loads of the imported Straus7 model and the reported support reactions of the case.", CompleteCoverageConfirmed = true,
                RelativeTolerance = RelativeTolerance };
            scope.ExpectedPhysicalActions.AddRange(contributions.Select(c => c.PhysicalActionId));
            var report = Equilibrium.Check(scope, contributions);
            Vector3d Sum(IEnumerable<EquilibriumContribution> terms, bool moment)
            {
                var total = new Vector3d(0, 0, 0);
                foreach (var c in terms) total += moment ? c.Moment + ((Vector3d)(c.Point - Point3d.Origin)).CrossProduct(c.Force) : c.Force;
                return total;
            }
            var applied = contributions.Where(c => c.Representation != "reaction").ToArray(); var supports = contributions.Where(c => c.Representation == "reaction").ToArray();
            balance.AppliedForce = Components(Sum(applied, false)); balance.AppliedMoment = Components(Sum(applied, true));
            balance.ReactionForce = Components(Sum(supports, false)); balance.ReactionMoment = Components(Sum(supports, true));
            balance.Balanced = report.IsBalanced; balance.NotRepresented = missing.ToArray();
            diagnostics.AddRange(report.Diagnostics);
            balance.Diagnostics = diagnostics;
            return balance;
        }

        private static double[] Components(Vector3d v) => new[] { v.X, v.Y, v.Z };
        private static Vector3d Global(CoordinateSystem axes, double x, double y, double z) => axes.V1 * x + axes.V2 * y + axes.V3 * z;

        /// <summary>Mass vector (per global direction, t) and centre of a node or element mass of <see cref="Straus7ModelData"/>; null when the
        /// element is not in the Model.</summary>
        private static (Vector3d Mass, Point3d Point) Weight(GPC.Model.Models.Model model, Straus7MassData m, Straus7ModelData data)
        {
            var offset = m.Offset == null ? new Vector3d(0, 0, 0) : new Vector3d(m.Offset[0], m.Offset[1], m.Offset[2]);
            var revision = model.AnalysisSource?.ModelRevision; string id = m.Number.ToString(CultureInfo.InvariantCulture);
            switch (m.Kind)
            {
                case Straus7MassKind.NodeTranslational:
                case Straus7MassKind.NodeNonStructural:
                {
                    var node = model.FindBySource(new GPC.Model.Core.Identity.SourceIdentity("Straus7", revision, EntityFamily.Node, id)) as NodeElement;
                    if (node == null) return (null, null);
                    var v = m.Kind == Straus7MassKind.NodeTranslational ? new Vector3d(m.Values[0], m.Values[1], m.Values[2]) : new Vector3d(m.Values[0], m.Values[0], m.Values[0]);
                    return (v, node.Position + offset);
                }
                case Straus7MassKind.BeamNonStructural:
                {
                    var beam = model.FindBySource(new GPC.Model.Core.Identity.SourceIdentity("Straus7", revision, EntityFamily.Beam, id)) as BeamElement;
                    if (beam == null || m.Stations == null || m.Stations.Length != m.Values.Length) return (null, null);
                    double total = 0, moment = 0, length = beam.Length;
                    for (int k = 1; k < m.Stations.Length; k++)
                    {
                        double a = m.Stations[k - 1], b = m.Stations[k], p = m.Values[k - 1], q = m.Values[k], w = (b - a) * length;
                        total += (p + q) / 2 * w; moment += w * (p * (2 * a + b) + q * (a + 2 * b)) / 6;
                    }
                    double centre = total == 0 ? 0.5 : moment / total;
                    return (new Vector3d(total, total, total), beam.StartPoint + (beam.EndPoint - beam.StartPoint) * centre + offset);
                }
                case Straus7MassKind.PlateNonStructural:
                {
                    var shell = model.FindBySource(new GPC.Model.Core.Identity.SourceIdentity("Straus7", revision, EntityFamily.Shell, id)) as AreaElement;
                    var points = shell?.Points;
                    if (points == null || points.Length < 3) return (null, null);
                    double area = 0; var first = new Vector3d(0, 0, 0);
                    for (int i = 1; i < points.Length - 1; i++)
                    {
                        Vector3d v = points[i] - points[0], w = points[i + 1] - points[0]; double a = Axes.Length(v.CrossProduct(w)) / 2;
                        area += a; first += ((Vector3d)(points[0] - Point3d.Origin) + (v + w) / 3) * a;
                    }
                    if (!(area > 0)) return (null, null);
                    var c = first / area; double total = m.Values[0] * area;
                    return (new Vector3d(total, total, total), new Point3d(c.X, c.Y, c.Z) + offset);
                }
                default: return (null, null);
            }
        }
    }
}
