using System;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.Core.Coordinates;
using GPC.Model.LoadCases;
using GPC.Model.Structure.Assignments;

namespace GPC.Model.Structure.Assignments
{
    /// <summary>Straus7-style projection of a global face pressure: none, on the resultant direction, or per component.</summary>
    public enum SurfacePressureProjection { None, Resultant, Components }
}

namespace GPC.Model.Loads
{
    /// <summary>
    /// Uniform pressure on one outer face of the shell element that carries it in <c>Element.Loads</c>, N/mm². Exactly one of
    /// <see cref="NormalPressure"/> and <see cref="GlobalPressure"/> is set.
    /// <para>Normal pressure is a FOLLOWER load: it acts along the current face normal and is positive INWARDS on the loaded face,
    /// so on <see cref="ShellFace.PositiveNormal"/> it pushes along -V3 and on <see cref="ShellFace.NegativeNormal"/> along +V3 of the
    /// element. Global pressure keeps its global direction; <see cref="Projection"/> applies to it only. The face geometry includes
    /// thickness and offset.</para>
    /// <para>Unlike <see cref="NormalAreaLoad"/> (fixed direction, positive along +V3 of its shape) the load is face-specific and,
    /// when normal, deformation-dependent: a consumer that cannot represent it must reject it explicitly, never read it as a
    /// fixed pressure. The coordinate system of the load is the global one.</para>
    /// </summary>
    [Serializable]
    public sealed class ShellFacePressure : Load
    {
        public ShellFace Face { get; set; }
        /// <summary>Follower pressure [N/mm²], positive inwards on <see cref="Face"/>; null for a global pressure.</summary>
        public double? NormalPressure { get; set; }
        /// <summary>Global pressure vector [N/mm²]; null for a normal pressure.</summary>
        public Vector3d GlobalPressure { get; set; }
        public SurfacePressureProjection Projection { get; set; }
        /// <summary>True for the deformation-dependent normal pressure.</summary>
        public bool IsFollower => NormalPressure.HasValue;

        private ShellFacePressure(LoadCaseBase loadCase, ShellFace face, string name, int id)
            : base(loadCase, CoordinateSystem.Global, name, id) { Face = face; }

        /// <summary>Follower pressure along the current face normal, positive inwards on <paramref name="face"/>.</summary>
        public static ShellFacePressure Normal(LoadCaseBase loadCase, ShellFace face, double pressure, string name = "", int id = IDUNASSIGNED)
        { var load = new ShellFacePressure(loadCase, face, name, id) { NormalPressure = pressure }; load.Validate(); return load; }

        /// <summary>Global pressure on <paramref name="face"/>, with the solver projection rule.</summary>
        public static ShellFacePressure Global(LoadCaseBase loadCase, ShellFace face, Vector3d pressure,
            SurfacePressureProjection projection = SurfacePressureProjection.None, string name = "", int id = IDUNASSIGNED)
        {
            if (pressure == null) throw new ArgumentNullException(nameof(pressure));
            var load = new ShellFacePressure(loadCase, face, name, id) { GlobalPressure = new Vector3d(pressure.X, pressure.Y, pressure.Z), Projection = projection };
            load.Validate(); return load;
        }

        private ShellFacePressure(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            // Schema check only: values are validated by the edit session and the exporters (review B1).
            SerializationVersions.Read(info, "ShellFacePressureVersion", 1);
            Face = (ShellFace)info.GetInt32("Face");
            NormalPressure = SerializationFields.Read<double?>(info, "NormalPressure");
            GlobalPressure = SerializationFields.Read<Vector3d>(info, "GlobalPressure");
            Projection = (SurfacePressureProjection)info.GetInt32("Projection");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ShellFacePressureVersion", 1); info.AddValue("Face", (int)Face);
            if (NormalPressure.HasValue) info.AddValue("NormalPressure", NormalPressure);
            if (GlobalPressure != null) info.AddValue("GlobalPressure", GlobalPressure);
            info.AddValue("Projection", (int)Projection);
        }

        public void Validate()
        {
            if (LoadCase == null || string.IsNullOrWhiteSpace(LoadCase.Name) || !Enum.IsDefined(typeof(ShellFace), Face)
                || !Enum.IsDefined(typeof(SurfacePressureProjection), Projection) || (GlobalPressure != null) == NormalPressure.HasValue)
                throw new ArgumentException("InvalidShellSurfacePressure");
            if (GlobalPressure != null && !Axes.IsFinite(GlobalPressure)) throw new ArgumentException("NonFiniteShellSurfacePressure");
            if (NormalPressure.HasValue && (double.IsNaN(NormalPressure.Value) || double.IsInfinity(NormalPressure.Value) || Projection != SurfacePressureProjection.None))
                throw new ArgumentException("InvalidNormalSurfacePressure");
        }

        /// <summary>No geometry of its own: the load covers the whole face of the shell element that carries it.</summary>
        public override GeometryBase GetGeometryBase() => null;

        public override bool Equals(object obj) => ReferenceEquals(this, obj) || obj is ShellFacePressure other && base.Equals(other)
            && Face == other.Face && Nullable.Equals(NormalPressure, other.NormalPressure) && Projection == other.Projection
            && (GlobalPressure == null ? other.GlobalPressure == null : other.GlobalPressure != null && GlobalPressure.Equals(other.GlobalPressure));

        public override int GetHashCode() { unchecked { return base.GetHashCode() * -17 + (int)Face * 3 + (int)Projection; } }
    }
}
