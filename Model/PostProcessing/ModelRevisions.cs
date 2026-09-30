using System.Collections.Generic;
using System.Linq;
using GPC.Model.Elements;
using GPC.Model.Persistence;

namespace GPC.Model.PostProcessing
{
    public static class ModelRevisions
    {
        /// <summary>Evaluated on demand. Observes legacy mutable points, properties, attributes, loads and collections.</summary>
        public static string AnalysisFingerprint(this Models.Model model)
        {
            return ModelArchive.Fingerprint(AnalysisInputs(model));
        }
        private static IEnumerable<object> AnalysisInputs(Models.Model model)
        {
            yield return model.Guid;
            foreach (var e in model.AllElements)
            {
                yield return e.Id; yield return e.Guid; yield return e.Source; yield return e.CoordinateSystem;
                // Organizing groups after importing results does not alter the physical FEM problem.
                yield return e.Loads; yield return e.Attributes;
                if (e is NodeElement n)
                {
                    yield return n.Position;
                    // RestrainAssignment points back to the node; avoid following its results in a revision fingerprint.
                    foreach (var restraint in n.Assignments.Restrains)
                    {
                        yield return restraint.Case; yield return restraint.Phase; yield return restraint.Restrain.CoordinateSystem;
                        yield return restraint.Restrain.Restrains;
                    }
                    yield return n.Assignments.GroundSprings.ToArray(); yield return n.Assignments.Links.ToArray();
                    yield return n.Assignments.Mass; yield return n.Assignments.Dofs; yield return n.Assignments.Active;
                }
                if (e is BeamElement b)
                {
                    yield return b.StartPoint; yield return b.EndPoint; yield return b.NodeI?.Guid; yield return b.NodeJ?.Guid;
                    if (b.BeamProperty is Sections.Concrete.ReinforcedConcreteSection rc) { yield return rc.SectionShape; yield return rc.ConcreteMaterial; }
                    else yield return b.BeamProperty;
                    yield return b.RotationAroundFirstAxis;
                    yield return b.Assignments.Formulation; yield return b.Assignments.SectionAxes;
                    yield return b.Assignments.SectionGeometryAxes;
                    yield return b.Assignments.OrientationNodeId; yield return b.Assignments.StationDomain;
                    yield return b.Assignments.ActionsAtSectionCentroidConfirmed;
                    yield return b.Assignments.OffsetI; yield return b.Assignments.OffsetJ; yield return b.Assignments.OffsetAxes;
                    yield return b.Assignments.RigidLengthI; yield return b.Assignments.RigidLengthJ;
                    yield return b.Assignments.SectionCentroidOffset; yield return b.Assignments.AnalysisProfile;
                    yield return b.Assignments.OtherAssignments.ToArray();
                    yield return b.Assignments.Loads.ToArray();
                    foreach (var assignment in b.Assignments.Sections)
                    {
                        yield return assignment.Start; yield return assignment.End; yield return assignment.Law;
                        yield return assignment.Section?.SectionShape; yield return assignment.Section?.ConcreteMaterial;
                        yield return assignment.EndSection?.SectionShape; yield return assignment.EndSection?.ConcreteMaterial;
                        foreach (var station in assignment.Stations)
                        {
                            yield return station.Station; yield return station.Side;
                            yield return station.Section?.SectionShape; yield return station.Section?.ConcreteMaterial;
                        }
                    }
                }
                if (e is AreaElement a)
                {
                    yield return a.Points; yield return a.PlateProperty; yield return a.Assignments.PhysicalThickness; yield return a.Assignments.Offset;
                    foreach (var n2 in a.Nodes) yield return n2.Guid;
                }
                if (e is VolumeElement v) { yield return v.Nodes; yield return v.PlateProperty; }
            }
            yield return model.LoadCases; yield return model.Combinations; yield return model.FreedomCases;
            yield return model.Stages; yield return model.StageCombinationsMap; yield return model.Costrains;
        }
        public static string VerificationFingerprint(this Models.Model model, string settings)
        {
            return ModelArchive.Fingerprint(new object[] { model.AnalysisFingerprint(), settings }
                .Concat(model.BeamElements.Values.Select(b => (object)b.Assignments.Sections.ToArray()))
                .Concat(model.AreaElements.Values.Select(a => (object)a.Assignments)));
        }
    }
}
