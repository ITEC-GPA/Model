using System;
using System.Runtime.Serialization;
using GPC.Model.Elements;
using GPC.Model.PostProcessing;

namespace GPC.Model.ElementProperties
{
    /// <summary>Constant thicknesses in mm. Physical geometry and equivalent FEM stiffness thicknesses are independent.</summary>
    [Serializable]
    public sealed class ShellThickness : ISerializable
    {
        public double Physical { get; }
        public double Membrane { get; }
        public double Bending { get; }
        public ShellThickness(double physical, double membrane, double bending)
        {
            Positive(physical); Positive(membrane); Positive(bending);
            Physical = physical; Membrane = membrane; Bending = bending;
        }
        private static void Positive(double value)
        { NumericGuard.Finite(value, "thickness"); if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "PositiveThicknessRequired"); }
        public static ShellThickness From(AreaElement element)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            var physical=PlateSections.PhysicalThickness(element);
            if (!physical.HasValue) throw new ArgumentException("MissingPhysicalThickness");
            if (!(element.PlateProperty is IFemPlateProperty property)) throw new NotSupportedException("UnsupportedPlateThickness");
            return new ShellThickness(physical.Value, property.MembraneThickness, property.BendingThickness);
        }
        private ShellThickness(SerializationInfo info, StreamingContext context)
            : this(info.GetDouble("Physical"), info.GetDouble("Membrane"), info.GetDouble("Bending")) { }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        { info.AddValue("Physical", Physical); info.AddValue("Membrane", Membrane); info.AddValue("Bending", Bending); }
    }
}
