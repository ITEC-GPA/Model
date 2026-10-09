using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Attributes;
using GPC.Model.Data.Concrete;
using GPC.Model.Data.Steel;
using GPC.Model.Elements;
using GPC.Model.LoadCases;
using GPC.Model.Loads;
using GPC.Model.Models;
using GPC.Model.Results;
using GPC.Model.Results.Storage;
using GPC.Model.Results.Locations;
using GPC.Model.Sections;
using GPC.Model.Sections.Concrete;
using GPC.Model.Sections.Rebar;
using GPC.Model.Restraints;
using GPC.Model.Analysis;
using GPC.Model.Core.Coordinates;
using GPC.Model.Core.Identity;
using GPC.Model.Results.State;
using GPC.Model.Structure.Assignments;

namespace GPC.Examples
{
    /// <summary>Synthetic post-processing fixture. Forces below are assigned analytical data, not FEM analysis output.</summary>
    public static class MixedModelFactory
    {
        public static GPC.Model.Models.Model Create(Action<GPC.Model.Models.Model>? configureBeforeResults = null,
            ReinforcementAnalysisRole reinforcementRole = ReinforcementAnalysisRole.ExcludedFromAnalysis)
        {
            var model = new GPC.Model.Models.Model("SYNTHETIC beam and shell fixture");
            int[] ids = { 10, 40, 90, 130 };
            var points = new[] { new Point3d(0, 0, 0), new Point3d(0, 0, 2000), new Point3d(1000, 0, 2000), new Point3d(1000, 0, 0) };
            for (int i = 0; i < 4; i++) model.NodesElements.Add(new NodeElement(points[i], id: ids[i]) { Source = new SourceIdentity("Synthetic", "mixed-v1", EntityFamily.Node, ids[i].ToString()) });
            var section = new ReinforcedConcreteSection(new SectionRectangular(500, 300, "RC 300x500"), ConcreteMaterialEN1992Data.C25_30);
            var bar = new RebarSectionCircular(16, SteelMaterialEN1992Data.B450C);
            section.AddRebars(new[]{new ReinforcedConcreteRebar(bar,new Point2d(50,50)),new ReinforcedConcreteRebar(bar,new Point2d(250,50)),
                new ReinforcedConcreteRebar(bar,new Point2d(50,450)),new ReinforcedConcreteRebar(bar,new Point2d(250,450))});
            var beam = new BeamElement(null, null, section, id: 250) { Source = new SourceIdentity("Synthetic", "mixed-v1", EntityFamily.Beam, "B1") };
            model.BeamElements.Add(beam); model.AddProperty(section); model.ConnectBeam(250, 10, 40);
            beam.Assignments.Formulation = BeamFormulation.StraightTwoNode;
            beam.Assignments.SectionAxes = Axes.Beam(points[0], points[1], new Vector3d(1, 0, 0));
            beam.Assignments.ActionsAtSectionCentroidConfirmed = true; beam.Assignments.StationDomain = "NodeToNode";
            beam.Assignments.Sections.Add(new BeamSectionAssignment { Start = 0, End = 1, Section = section });
            var release = new BeamReleasesAttribute { CoordinateSystem = beam.Assignments.SectionAxes }; release.J[5] = new BeamDofConnection(BeamConnectionKind.Released); beam.Attributes.Add(release);
            var property = new ConcretePlateProperty(ConcreteMaterialEN1992Data.C25_30, 300, 300, "Shell 300"); model.AddProperty(property);
            var group = model.AddGroup("Wall");
            foreach (var connectivity in new[] { new[] { 10, 40, 90 }, new[] { 10, 90, 130 } })
            {
                var area = new AreaElement(null, property, id: connectivity[2] + 1000); model.AreaElements.Add(area); model.ConnectShell(area.Id, connectivity);
                area.AddGroup(group); area.Assignments.PhysicalThickness = 300; area.Assignments.ReinforcementZone = "Wall";
                area.CoordinateSystem = new CoordinateSystem(points[0], new Vector3d(1, 0, 0), new Vector3d(0, 0, 1));
                area.Assignments.LayerAxes = area.CoordinateSystem;
                area.Assignments.Layers.Add(new ShellRebarLayer { PhysicalFace = "ground", Steel = SteelMaterialEN1992Data.B450C, Diameter = 16, Pitch = 150, AxisPositionThroughThickness = 110, DirectionRadians = 0, Order = 1 });
                area.Assignments.Layers.Add(new ShellRebarLayer { PhysicalFace = "visible", Steel = SteelMaterialEN1992Data.B450C, Diameter = 12, Pitch = 150, AxisPositionThroughThickness = -110, DirectionRadians = 0, Order = 1 });
            }
            model.NodesElements[10].Assignments.Restrains.Add(new RestrainAssignment { Restrain = NodeRestrain.GetAllFixed(model.NodesElements[10], CoordinateSystem.Global) });
            var stiffness = new double[36]; stiffness[0] = 100; stiffness[7] = 200; stiffness[1] = stiffness[6] = 20;
            model.NodesElements[130].Assignments.GroundSprings.Add(new SpringMatrix(stiffness, CoordinateSystem.Global));
            foreach (var name in new[] { "P+", "P-" }) model.LoadCases.Add(new LoadCaseBase(name));
            model.NodesElements[40].Loads.Add(new PointLoad(0, 1000, 0, 0, 0, 0, points[1], model.LoadCases["P+"]));
            model.NodesElements[40].Loads.Add(new PointLoad(0, -1000, 0, 0, 0, 0, points[1], model.LoadCases["P-"]));
            configureBeforeResults?.Invoke(model);
            model.CaptureAnalysis(reinforcementRole, reinforcementRole == ReinforcementAnalysisRole.Unknown ? null : "Declared reinforcement role for the analytical fixture.");
            string fingerprint = model.AnalysisFingerprint();
            model.Datasets.Add("synthetic-static", new AnalysisDataset { Id = "synthetic-static", Program = "Synthetic", ModelRevision = "mixed-v1", AnalysisId = "static", InputFingerprint = fingerprint, NormalizedUnits = "N,mm,rad", IsSynthetic = true, Semantics = AnalysisSemantics.LinearStatic });
            foreach (var lc in model.LoadCases.Values)
            {
                double sign = lc.Name == "P+" ? 1 : -1;
                var samples = new List<IBeamResultLocation>();
                foreach (var xi in new[] { 0.0, .25, .5, .75, 1.0 })
                {
                    // Cantilever with transverse force +Y at J. Positive Z cut face: Vy=+P, Mx=-P*(L-s).
                    var axes = new CoordinateSystem(new Point3d(0, 0, xi * 2000), new Vector3d(1, 0, 0), new Vector3d(0, 1, 0));
                    samples.Add(new StationResultBeamForces(lc, new ResultBeamForces(0, 0, sign * 1000, 0, -sign * 1000 * 2000 * (1 - xi), 0, axes), xi)
                    { PhysicalDistance = xi * 2000, StationDomain = "NodeToNode", Body = ActionBody.PositiveSectionFace, State = State(lc.Name, fingerprint, 6) });
                }
                beam.AddResult(new BeamResult(samples));
                model.NodesElements[10].AddResult(new NodeResult(new List<INodeResultLocation> {
                    new NodeResultForces(lc,new ResultBeamForces(0,0,-sign*1000,0,sign*2000000,0,CoordinateSystem.Global))
                    {Kind=NodalForceKind.SupportReaction,Body=ActionBody.OnNode,State=State(lc.Name,fingerprint,6)},
                    new NodeResultDisplacement(lc,new ResultDisplacement(0,0,0,0,0,0)) {State=State(lc.Name,fingerprint,6)} }));
                foreach (var area in model.AreaElements.Values) area.AddResult(new PlateElementResult(new List<IPlateResultLocation> {
                    new PointResultPlateForces(lc,new ResultPlateForces(area.CoordinateSystem,150,50,10,4,6,sign*80000,sign*20000,sign*5000),new Point2d(1.0/3,1.0/3),"Natural centroid")
                    {State=State(lc.Name,fingerprint,8),PointKind=ShellResultPointKind.Centroid,CoordinateKind=ResultCoordinateKind.Natural} }));
            }
            return model;
        }
        private static ResultState State(string caseName, string fingerprint, int count) => new ResultState
        {
            DatasetId = "synthetic-static",
            ModelRevision = "mixed-v1",
            InputFingerprint = fingerprint,
            Semantics = AnalysisSemantics.LinearStatic,
            ConcomitantStateId = caseName,
            Components = Enumerable.Repeat(ComponentAvailability.Available, count).ToArray(),
            IsSynthetic = true,
            IsCombined = false,
            SourceRecord = "Analytical assigned fixture",
            Coverage = "Exported samples only; no claim of continuous maximum."
        };
    }
}
