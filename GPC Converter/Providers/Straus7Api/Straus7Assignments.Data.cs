using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Core.Coordinates;

namespace GPC.Converter.Straus7
{
    /// <summary>Masses and temperatures (R3 attributes aoMTranslation, aoMRotation, aoNSMass, aoTemperature, aoBeamNSMass, aoBeamTempGradient,
    /// aoPlateNSMass, aoPlateTempGradient) as normalized data of <see cref="Straus7ModelData"/>. Node masses act in every case; the other
    /// attributes belong to the load case of their sequence entry. Nothing becomes a GPC load.</summary>
    internal sealed partial class Straus7Assignments
    {
        private IStraus7MassTemperatureReadApi Data => api as IStraus7MassTemperatureReadApi;
        public Straus7ModelData ModelData { get; } = new Straus7ModelData();
        private readonly Dictionary<int, double> referenceTemperatures = new Dictionary<int, double>();
        private readonly HashSet<int> thermalCases = new HashSet<int>();

        /// <summary>Absolute temperature in degrees Celsius (tuCELSIUS 0, tuFAHRENHEIT 1, tuKELVIN 2, tuRANKINE 3).</summary>
        private double Celsius(double value)
        {
            switch (temperatureUnit)
            {
                case 0: return value;
                case 1: return (value - 32) / 1.8;
                case 2: return value - 273.15;
                case 3: return (value - 491.67) / 1.8;
                default: throw new NotSupportedException("Unknown Straus7 temperature unit code.");
            }
        }
        /// <summary>Temperature difference per model length in °C/mm.</summary>
        private double GradientPerMillimetre(double value) => value / thermal / length;

        private static void Finite(double[] values, int count, string record) => Straus7ApiConverter.Finite(values, count, record);

        private void NodeData(int node, string record)
        {
            var data = Data; if (data == null) return;
            if (Attributes(Straus7Entity.Node, node, Straus7Attributes.NodeTranslationalMass).Length != 0)
            {
                var m = data.ReadNodeTranslationalMass(node); Finite(m, 3, record + "/translational mass");
                if (m.Any(v => v != 0)) ModelData.Masses.Add(new Straus7MassData { Kind = Straus7MassKind.NodeTranslational, Number = node,
                    Values = m.Select(v => v * mass).ToArray(), Record = record + "/translational mass" });
            }
            if (Attributes(Straus7Entity.Node, node, Straus7Attributes.NodeRotationalMass).Length != 0)
            {
                var m = data.ReadNodeRotationalMass(node, out int ucs); Finite(m, 3, record + "/rotational mass");
                var axes = ucs == 1 ? CoordinateSystem.Global : Ucs(ucs);
                if (axes == null) Warn("Straus7RotationalMassUcsPreserved", "Rotational mass UCS", "Rotational masses in non-Cartesian coordinate systems are not mapped.");
                else if (m.Any(v => v != 0)) ModelData.Masses.Add(new Straus7MassData { Kind = Straus7MassKind.NodeRotational, Number = node,
                    Values = m.Select(v => v * mass * length * length).ToArray(), Axes = AxesOf(axes), Record = record + "/rotational mass/UCS " + Id(ucs) });
            }
            foreach (var a in Attributes(Straus7Entity.Node, node, Straus7Attributes.NodeNonStructuralMass))
            {
                var r = record + "/non-structural mass/case " + Id(a.Case) + "/id " + Id(a.Id);
                var m = data.ReadNodeNonStructuralMass(node, a.Case, a.Id); Finite(m, 5, r);
                ModelData.Masses.Add(new Straus7MassData { Kind = Straus7MassKind.NodeNonStructural, Number = node, Case = Case(a.Case, r), Values = new[] { m[0] * mass },
                    DynamicFactor = m[1], Offset = new[] { m[2] * length, m[3] * length, m[4] * length }, Record = r });
            }
            foreach (var a in Attributes(Straus7Entity.Node, node, Straus7Attributes.NodeTemperature))
            {
                var r = record + "/temperature/case " + Id(a.Case);
                double t = data.ReadNodeTemperature(node, a.Case, out int type);
                if (!Straus7ApiConverter.Finite(t)) throw new InvalidDataException("Missing/nonfinite values at " + r);
                ModelData.Temperatures.Add(new Straus7TemperatureData { Kind = Straus7TemperatureKind.NodeTemperature, Number = node, Case = Case(a.Case, r),
                    Values = new[] { Celsius(t) }, Type = TemperatureType(type), Record = r });
                thermalCases.Add(a.Case);
            }
        }

        private static string TemperatureType(int type)
        {
            switch (type)
            {
                case 0: return "Reference";
                case 1: return "Fixed";
                case 2: return "Initial";
                case 3: return "Table";
                default: return "Type " + type.ToString(CultureInfo.InvariantCulture);
            }
        }

        private void BeamData(int number, string record)
        {
            var data = Data; if (data == null) return;
            foreach (var a in Attributes(Straus7Entity.Beam, number, Straus7Attributes.BeamNonStructuralMass))
            {
                var r = record + "/non-structural mass/case " + Id(a.Case) + "/id " + Id(a.Id);
                var m = data.ReadBeamNonStructuralMass(number, a.Case, a.Id, out int distribution); Finite(m, 10, r);
                var points = Distribution(new Straus7DistributedLoad { Type = distribution, Values = m.Take(6).ToArray() }, r);
                ModelData.Masses.Add(new Straus7MassData { Kind = Straus7MassKind.BeamNonStructural, Number = number, Case = Case(a.Case, r),
                    Stations = points.Select(p => p.Station).ToArray(), Values = points.Select(p => p.Value * mass / length).ToArray(),
                    DynamicFactor = m[6], Offset = new[] { m[7] * length, m[8] * length, m[9] * length }, Record = r });
            }
            foreach (var a in Attributes(Straus7Entity.Beam, number, Straus7Attributes.BeamTemperatureGradient))
            {
                var r = record + "/temperature gradient/case " + Id(a.Case);
                var g = data.ReadBeamTemperatureGradient(number, a.Case); Finite(g, 2, r);
                ModelData.Temperatures.Add(new Straus7TemperatureData { Kind = Straus7TemperatureKind.BeamGradient, Number = number, Case = Case(a.Case, r),
                    Values = g.Select(GradientPerMillimetre).ToArray(), Record = r });
            }
        }

        private void PlateData(int number, string record)
        {
            var data = Data; if (data == null) return;
            foreach (var a in Attributes(Straus7Entity.Plate, number, Straus7Attributes.PlateNonStructuralMass))
            {
                var r = record + "/non-structural mass/case " + Id(a.Case) + "/id " + Id(a.Id);
                var m = data.ReadPlateNonStructuralMass(number, a.Case, a.Id); Finite(m, 5, r);
                ModelData.Masses.Add(new Straus7MassData { Kind = Straus7MassKind.PlateNonStructural, Number = number, Case = Case(a.Case, r), Values = new[] { m[0] * mass / (length * length) },
                    DynamicFactor = m[1], Offset = new[] { m[2] * length, m[3] * length, m[4] * length }, Record = r });
            }
            foreach (var a in Attributes(Straus7Entity.Plate, number, Straus7Attributes.PlateTemperatureGradient))
            {
                var r = record + "/temperature gradient/case " + Id(a.Case);
                double g = data.ReadPlateTemperatureGradient(number, a.Case);
                if (!Straus7ApiConverter.Finite(g)) throw new InvalidDataException("Missing/nonfinite values at " + r);
                ModelData.Temperatures.Add(new Straus7TemperatureData { Kind = Straus7TemperatureKind.PlateGradient, Number = number, Case = Case(a.Case, r),
                    Values = new[] { GradientPerMillimetre(g) }, Record = r });
            }
        }

        /// <summary>The reference temperature and the mass options of a load case, read with its other data.</summary>
        private void CaseData(int number, Straus7LoadCaseData caseData, string record)
        {
            var data = Data; if (data == null || caseData == null) return;
            if (caseData.Defaults != null && caseData.Defaults.Length > 0 && Straus7ApiConverter.Finite(caseData.Defaults[0])) referenceTemperatures[number] = caseData.Defaults[0];
            if (caseData.Type != 1) return;
            var option = data.ReadLoadCaseMassOption(number);
            if (option == null || option.Length != 2) throw new InvalidDataException("Invalid mass options: " + record);
            ModelData.Inertia.Add(new Straus7LoadCaseInertia { Case = Case(number, record), StructuralMass = option[0], NonStructuralMass = option[1] });
        }

        /// <summary>Reference temperatures of the cases with node temperatures, after the attributes have been read.</summary>
        private void FinishData()
        {
            foreach (int c in thermalCases.OrderBy(c => c))
                if (referenceTemperatures.TryGetValue(c, out var reference))
                    ModelData.Temperatures.Insert(0, new Straus7TemperatureData { Kind = Straus7TemperatureKind.ReferenceTemperature, Case = Case(c, "LoadCase/" + Id(c)),
                        Values = new[] { Celsius(reference) }, Record = "LoadCase/" + Id(c) + "/reference temperature" });
            if (ModelData.Masses.Count + ModelData.Temperatures.Count != 0)
                Warn("Straus7MassesAndTemperaturesAsData", "Masses and temperatures",
                    "Node masses, non-structural masses, node temperatures and temperature gradients are kept as data (Straus7ModelData): no GPC load, mass matrix or thermal strain is derived from them.");
        }

        private static string Id(int value) => value.ToString(CultureInfo.InvariantCulture);
        private static double[] AxesOf(CoordinateSystem axes) => new[] { axes.V1.X, axes.V1.Y, axes.V1.Z, axes.V2.X, axes.V2.Y, axes.V2.Z, axes.V3.X, axes.V3.Y, axes.V3.Z };
    }
}
