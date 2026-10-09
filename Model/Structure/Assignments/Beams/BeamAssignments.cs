using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.LoadCases;
using GPC.Model.Restrains;
using GPC.Model.Sections.Concrete;
using GPC.Model.Materials;

namespace GPC.Model.PostProcessing
{

    [Serializable]
    public sealed class BeamAssignments
    {
        public BeamFormulation Formulation { get; set; }
        public string LogicalMember { get; set; }
        public int? OrientationNodeId { get; set; }
        public CoordinateSystem SectionAxes { get; set; }
        /// <summary>Physical axes of the stored section geometry/rebars when different from the beam's oriented cut frame.
        /// Used after I/J reversal to retain arbitrary, asymmetric section shapes without mirroring their coordinates.</summary>
        [field: OptionalField] public CoordinateSystem SectionGeometryAxes { get; set; }
        public Vector3d OffsetI { get; set; }
        public Vector3d OffsetJ { get; set; }
        public CoordinateSystem OffsetAxes { get; set; }
        /// <summary>Rigid zones measured along the offset reference line, in mm.</summary>
        [field: OptionalField] public double RigidLengthI { get; set; }
        [field: OptionalField] public double RigidLengthJ { get; set; }
        /// <summary>Centroid relative to the offset reference line, in SectionAxes V1,V2 (mm). Null means unresolved.</summary>
        [field: OptionalField] public Vector2d SectionCentroidOffset { get; set; }
        public string StationDomain { get; set; }
        public bool ActionsAtSectionCentroidConfirmed { get; set; }
        public List<BeamLoadAssignment> Loads { get; private set; } = new List<BeamLoadAssignment>();
        public List<BeamSectionAssignment> Sections { get; private set; } = new List<BeamSectionAssignment>();
        [field: OptionalField] public BeamAnalysisProfile AnalysisProfile { get; set; }
        public List<PreservedAssignment> OtherAssignments { get; private set; } = new List<PreservedAssignment>();
        public ReinforcedConcreteSection SectionAt(double station, SectionSide side)
            => SectionLaws.RequireConcrete(PropertyAt(station, side));

        /// <summary>Resolves the exact assigned property at a station/side. Missing intervals and ambiguous cuts fail explicitly.</summary>
        public GPC.Model.ElementProperties.BeamProperty PropertyAt(double station, SectionSide side)
        {
            NumericGuard.Station(station);
            var matches = Sections.Where(a => station >= a.Start && station <= a.End
                && (station != a.Start || station == 0 || side != SectionSide.Left)
                && (station != a.End || station == 1 || side != SectionSide.Right)).ToArray();
            if (matches.Length != 1) throw new InvalidOperationException("MissingOrAmbiguousSectionAssignment");
            return SectionLaws.EvaluateProperty(matches[0], station, side);
        }
    }
}
