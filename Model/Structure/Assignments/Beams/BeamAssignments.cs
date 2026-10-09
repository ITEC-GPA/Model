using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.LoadCases;
using GPC.Model.Restraints;
using GPC.Model.Sections.Concrete;
using GPC.Model.Materials;
using GPC.Model.Analysis;
using GPC.Model.Loads.Assignments;
using GPC.Model.Results.Locations;

namespace GPC.Model.Structure.Assignments
{
    [Serializable]
    [System.Runtime.Serialization.DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class BeamAssignments
    {
        [field: System.Runtime.Serialization.DataMember(Name = "<Formulation>k__BackingField", IsRequired = true)]
        public BeamFormulation Formulation { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<LogicalMember>k__BackingField", IsRequired = true)]
        public string LogicalMember { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<OrientationNodeId>k__BackingField", IsRequired = true)]
        public int? OrientationNodeId { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<SectionAxes>k__BackingField", IsRequired = true)]
        public CoordinateSystem SectionAxes { get; set; }

        /// <summary>Physical axes of the stored section geometry/rebars when different from the beam's oriented cut frame.
        /// Used after I/J reversal to retain arbitrary, asymmetric section shapes without mirroring their coordinates.</summary>
        [field: OptionalField]
        [field: System.Runtime.Serialization.DataMember(Name = "<SectionGeometryAxes>k__BackingField", IsRequired = false)]
        public CoordinateSystem SectionGeometryAxes { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<OffsetI>k__BackingField", IsRequired = true)]
        public Vector3d OffsetI { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<OffsetJ>k__BackingField", IsRequired = true)]
        public Vector3d OffsetJ { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<OffsetAxes>k__BackingField", IsRequired = true)]
        public CoordinateSystem OffsetAxes { get; set; }

        /// <summary>Rigid zones measured along the offset reference line, in mm.</summary>
        [field: OptionalField]
        [field: System.Runtime.Serialization.DataMember(Name = "<RigidLengthI>k__BackingField", IsRequired = false)]
        public double RigidLengthI { get; set; }

        [field: OptionalField]
        [field: System.Runtime.Serialization.DataMember(Name = "<RigidLengthJ>k__BackingField", IsRequired = false)]
        public double RigidLengthJ { get; set; }

        /// <summary>Centroid relative to the offset reference line, in SectionAxes V1,V2 (mm). Null means unresolved.</summary>
        [field: OptionalField]
        [field: System.Runtime.Serialization.DataMember(Name = "<SectionCentroidOffset>k__BackingField", IsRequired = false)]
        public Vector2d SectionCentroidOffset { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<StationDomain>k__BackingField", IsRequired = true)]
        public string StationDomain { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<ActionsAtSectionCentroidConfirmed>k__BackingField", IsRequired = true)]
        public bool ActionsAtSectionCentroidConfirmed { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Loads>k__BackingField", IsRequired = true)]
        public List<BeamLoadAssignment> Loads { get; private set; } = new List<BeamLoadAssignment>();

        [field: System.Runtime.Serialization.DataMember(Name = "<Sections>k__BackingField", IsRequired = true)]
        public List<BeamSectionAssignment> Sections { get; private set; } = new List<BeamSectionAssignment>();

        [field: OptionalField]
        [field: System.Runtime.Serialization.DataMember(Name = "<AnalysisProfile>k__BackingField", IsRequired = false)]
        public BeamAnalysisProfile AnalysisProfile { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<OtherAssignments>k__BackingField", IsRequired = true)]
        public List<PreservedAssignment> OtherAssignments { get; private set; } = new List<PreservedAssignment>();

        public ReinforcedConcreteSection SectionAt(double station, SectionSide side) => SectionLaws.RequireConcrete(PropertyAt(station, side));
        /// <summary>Resolves the exact assigned property at a station/side. Missing intervals and ambiguous cuts fail explicitly.</summary>
        public GPC.Model.ElementProperties.BeamProperty PropertyAt(double station, SectionSide side)
        {
            NumericGuard.Station(station);
            var matches = Sections.Where(a => station >= a.Start && station <= a.End && (station != a.Start || station == 0 || side != SectionSide.Left) && (station != a.End || station == 1 || side != SectionSide.Right)).ToArray();
            if (matches.Length != 1)
                throw new InvalidOperationException("MissingOrAmbiguousSectionAssignment");
            return SectionLaws.EvaluateProperty(matches[0], station, side);
        }
    }
}
