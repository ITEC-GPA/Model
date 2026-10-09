using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.ElementProperties;
using GPC.Model.Results;
using GPC.Model.Results.Processing;

namespace GPC.Model.PostProcessing
{
    /// <summary>Canonical shell evidence: membrane/shear N/mm, moments Nmm/mm, coordinates mm.</summary>
    [Serializable]
    public sealed class ShellInputSnapshot : ISerializable
    {
        public string PhysicalSurfaceId { get; }
        public string SurfaceZoneId { get; }
        public string PropertyFingerprint { get; }
        public string SampleFingerprint { get; }
        public string ForcesFingerprint { get; }
        public ShellThickness Thickness { get; }
        private readonly double[] _values;
        private readonly CoordinateSystem _axes;
        public IReadOnlyList<double> Values => Array.AsReadOnly(_values);
        public CoordinateSystem Axes => ActionTransformations.AtPoint(_axes, _axes.Origin);
        public ShellInputSnapshot(string propertyFingerprint, string sampleFingerprint, string forcesFingerprint, ResultPlateForces forces, ShellThickness thickness)
            : this(propertyFingerprint,sampleFingerprint,forcesFingerprint,forces,thickness,null,null) { }
        public ShellInputSnapshot(string propertyFingerprint,string sampleFingerprint,string forcesFingerprint,ResultPlateForces forces,ShellThickness thickness,
            string physicalSurfaceId,string surfaceZoneId)
        {
            if(surfaceZoneId!=null && string.IsNullOrWhiteSpace(physicalSurfaceId) || physicalSurfaceId!=null && string.IsNullOrWhiteSpace(physicalSurfaceId)
                || surfaceZoneId!=null && string.IsNullOrWhiteSpace(surfaceZoneId)) throw new ArgumentException("InvalidSurfaceEvidence");
            PhysicalSurfaceId=physicalSurfaceId; SurfaceZoneId=surfaceZoneId;
            if (forces == null) throw new ArgumentNullException(nameof(forces));
            CheckValue.Text(propertyFingerprint, nameof(propertyFingerprint)); CheckValue.Text(sampleFingerprint, nameof(sampleFingerprint)); CheckValue.Text(forcesFingerprint, nameof(forcesFingerprint));
            PropertyFingerprint = propertyFingerprint; SampleFingerprint = sampleFingerprint; ForcesFingerprint = forcesFingerprint;
            Thickness = thickness ?? throw new ArgumentNullException(nameof(thickness));
            _values = new[] { forces.Fxx, forces.Fyy, forces.Fxy, forces.Fxz, forces.Fyz, forces.Mxx, forces.Myy, forces.Mxy };
            foreach (var value in _values) NumericGuard.Finite(value, "shell action");
            _axes = ActionTransformations.AtPoint(forces.CoordinateSystem, forces.CoordinateSystem.Origin);
        }
        private ShellInputSnapshot(SerializationInfo info, StreamingContext context)
        {
            PhysicalSurfaceId=SerializationFields.Read<string>(info,"PhysicalSurfaceId"); SurfaceZoneId=SerializationFields.Read<string>(info,"SurfaceZoneId");
            if(SurfaceZoneId!=null && string.IsNullOrWhiteSpace(PhysicalSurfaceId) || PhysicalSurfaceId!=null && string.IsNullOrWhiteSpace(PhysicalSurfaceId)
                || SurfaceZoneId!=null && string.IsNullOrWhiteSpace(SurfaceZoneId)) throw new SerializationException("InvalidSurfaceEvidence");
            PropertyFingerprint = info.GetString("Property"); SampleFingerprint = info.GetString("Sample"); ForcesFingerprint = info.GetString("Forces");
            CheckValue.Text(PropertyFingerprint, "Property"); CheckValue.Text(SampleFingerprint, "Sample"); CheckValue.Text(ForcesFingerprint, "Forces");
            Thickness = (ShellThickness)info.GetValue("Thickness", typeof(ShellThickness));
            if (Thickness == null) throw new SerializationException("MissingShellThickness");
            _values = (double[])info.GetValue("Values", typeof(double[])); _axes = (CoordinateSystem)info.GetValue("Axes", typeof(CoordinateSystem));
            if (_values == null || _values.Length != 8) throw new SerializationException("InvalidShellEvidence");
            foreach (var value in _values) NumericGuard.Finite(value, "shell action");
            PostProcessing.Axes.Validate(_axes);
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Property", PropertyFingerprint); info.AddValue("Sample", SampleFingerprint); info.AddValue("Forces", ForcesFingerprint);
            info.AddValue("Values", _values); info.AddValue("Axes", _axes);
            info.AddValue("Thickness", Thickness);
            if(PhysicalSurfaceId!=null) info.AddValue("PhysicalSurfaceId",PhysicalSurfaceId);
            if(SurfaceZoneId!=null) info.AddValue("SurfaceZoneId",SurfaceZoneId);
        }
    }
}
