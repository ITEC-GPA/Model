using System;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.Core.Coordinates;

namespace GPC.Model.Structure.Members
{
    public enum BridgeMemberFamily { Girder, SolidDiaphragm, TopChord, BottomChord, Post, Diagonal, LongitudinalBracing, BracingPost, DeckTransverse }

    /// <summary>Physical bridge labels attached to an explicitly ordered member, independent of
    /// organizational groups and FEM material states. Stations are coordinates along the alignment
    /// in mm (they may decrease); SectionUp is a global geometric direction, not a force convention.</summary>
    [Serializable, DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class BridgeMemberMetadata
    {
        [DataMember(Order = 0)] public string PhysicalId { get; private set; }
        [DataMember(Order = 1)] public BridgeMemberFamily Family { get; private set; }
        [DataMember(Order = 2)] public int Group { get; private set; }
        [DataMember(Order = 3)] public int Segment { get; private set; }
        [DataMember(Order = 4)] public int Span { get; private set; }
        [DataMember(Order = 5)] public double StationFromMm { get; private set; }
        [DataMember(Order = 6)] public double StationToMm { get; private set; }
        [DataMember(Order = 7)] private Vector3d sectionUp;
        [DataMember(Order = 8)] public string GeometryId { get; private set; }
        public Vector3d SectionUp => new Vector3d(sectionUp.X, sectionUp.Y, sectionUp.Z);

        public BridgeMemberMetadata(string physicalId, BridgeMemberFamily family, int group, int segment, int span,
            double stationFromMm, double stationToMm, Vector3d sectionUp, string geometryId = "")
        {
            PhysicalId = physicalId; Family = family; Group = group; Segment = segment; Span = span;
            StationFromMm = stationFromMm; StationToMm = stationToMm; GeometryId = geometryId;
            this.sectionUp = sectionUp == null ? null : new Vector3d(sectionUp.X, sectionUp.Y, sectionUp.Z); Validate();
        }
        [OnDeserialized] private void OnRead(StreamingContext context) => Validate();
        private void Validate()
        {
            if (string.IsNullOrWhiteSpace(PhysicalId) || !Enum.IsDefined(typeof(BridgeMemberFamily), Family) || sectionUp == null || GeometryId == null)
                throw new ArgumentException("InvalidBridgeMemberMetadata");
            NumericGuard.Finite(StationFromMm, nameof(StationFromMm)); NumericGuard.Finite(StationToMm, nameof(StationToMm));
            if (NumericGuard.Finite(Axes.Length(sectionUp), nameof(SectionUp)) <= 0) throw new ArgumentException("ZeroBridgeSectionUp");
        }
    }
}
