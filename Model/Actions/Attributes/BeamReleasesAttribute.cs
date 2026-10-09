namespace GPC.Model.Attributes
{
    using System;
    using System.Runtime.Serialization;
    using GPC.Geometry;

    public enum BeamConnectionKind { Continuous, Released, AbsoluteStiffness, RelativeFixity }

    [Serializable]
    public sealed class BeamDofConnection
    {
        public BeamConnectionKind Kind { get; private set; }
        public double Value { get; private set; }
        public BeamDofConnection(BeamConnectionKind kind, double value = 0)
        {
            Validate(kind, value);
            Kind = kind; Value = value;
        }
        [OnDeserialized] private void OnDeserialized(StreamingContext context) => Validate(Kind, Value);
        private static void Validate(BeamConnectionKind kind, double value)
        {
            if (!Enum.IsDefined(typeof(BeamConnectionKind), kind)) throw new ArgumentOutOfRangeException(nameof(kind));
            NumericGuard.Finite(value, nameof(value));
            if (value < 0 || (kind == BeamConnectionKind.RelativeFixity && value > 1)) throw new ArgumentOutOfRangeException(nameof(value));
            if ((kind == BeamConnectionKind.Continuous || kind == BeamConnectionKind.Released) && value != 0) throw new ArgumentException("No coefficient applies to this connection kind.");
        }
    }

    /// <summary>Per beam, in DX,DY,DZ,RX,RY,RZ order. Relative fixity is dimensionless.</summary>
    [Serializable]
    public class BeamReleasesAttribute : Attribute
    {
        public BeamDofConnection[] I { get; private set; }
        public BeamDofConnection[] J { get; private set; }
        public CoordinateSystem CoordinateSystem { get; set; }
        public string Phase { get; set; }
        public string SourceRecord { get; set; }
        /// <summary>
        /// Creates the attribute, without name and id
        /// </summary>
        public BeamReleasesAttribute()
            : base()
        {
            I = new BeamDofConnection[6]; J = new BeamDofConnection[6];
            for (int k = 0; k < 6; k++) { I[k] = new BeamDofConnection(BeamConnectionKind.Continuous); J[k] = new BeamDofConnection(BeamConnectionKind.Continuous); }
        }
        protected BeamReleasesAttribute(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            I = (BeamDofConnection[])info.GetValue("I", typeof(BeamDofConnection[]));
            J = (BeamDofConnection[])info.GetValue("J", typeof(BeamDofConnection[]));
            CoordinateSystem = (CoordinateSystem)info.GetValue("ReleaseAxes", typeof(CoordinateSystem));
            Phase = info.GetString("Phase"); SourceRecord = info.GetString("SourceRecord");
            if (I.Length != 6 || J.Length != 6) throw new SerializationException("Six connections at each end required.");
        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("I", I); info.AddValue("J", J); info.AddValue("ReleaseAxes", CoordinateSystem);
            info.AddValue("Phase", Phase); info.AddValue("SourceRecord", SourceRecord);
        }
    }
}
