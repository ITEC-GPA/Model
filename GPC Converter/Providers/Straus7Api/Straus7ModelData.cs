using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;

namespace GPC.Converter.Straus7
{
    public enum Straus7MassKind { NodeTranslational, NodeRotational, NodeNonStructural, BeamNonStructural, PlateNonStructural }

    /// <summary>A mass of the Straus7 model in GPC units: tonne (= N s²/mm, the mass consistent with N and mm), mm, global axes.
    /// <list type="bullet">
    /// <item>NodeTranslational: Values = mass along global X, Y, Z [t]; active in every case.</item>
    /// <item>NodeRotational: Values = rotational mass about Axes V1, V2, V3 [t mm²]; active in every case.</item>
    /// <item>NodeNonStructural: Values = [mass, t] of load case Case, with DynamicFactor and global Offset [mm].</item>
    /// <item>BeamNonStructural: Values [t/mm] at Stations (fractions of the node-to-node length, piecewise linear) of load case Case.</item>
    /// <item>PlateNonStructural: Values = [mass per area, t/mm²] of load case Case.</item>
    /// </list>
    /// Data only: GPC derives no load from them. Straus7 applies the acceleration of a gravity case to the structural mass and to the
    /// non-structural mass of that case according to <see cref="Straus7LoadCaseInertia"/>.</summary>
    public sealed class Straus7MassData
    {
        public Straus7MassKind Kind { get; set; }
        /// <summary>Straus7 number of the node, beam or plate.</summary>
        public int Number { get; set; }
        public string Case { get; set; }
        public double[] Values { get; set; }
        public double[] Stations { get; set; }
        /// <summary>NodeRotational: the unit vectors V1, V2, V3 of the axes (nine global components).</summary>
        public double[] Axes { get; set; }
        public double? DynamicFactor { get; set; }
        public double[] Offset { get; set; }
        public string Record { get; set; }
    }

    public enum Straus7TemperatureKind { ReferenceTemperature, NodeTemperature, BeamGradient, PlateGradient }

    /// <summary>A temperature of a load case in degrees Celsius and mm: the reference temperature of the case (Number 0), a node temperature
    /// (Type: Reference, Fixed, Initial or Table, as St7GetNodeTemperatureType1), the gradients of a beam along its principal axes 1 and 2 and the
    /// gradient of a plate along its local z axis [°C/mm]. Data only: GPC derives no load from them.</summary>
    public sealed class Straus7TemperatureData
    {
        public Straus7TemperatureKind Kind { get; set; }
        public int Number { get; set; }
        public string Case { get; set; }
        public double[] Values { get; set; }
        public string Type { get; set; }
        public string Record { get; set; }
    }

    /// <summary>Accelerations of a gravity case applied to the structural and to the non-structural mass (St7GetLoadCaseMassOption).</summary>
    public sealed class Straus7LoadCaseInertia
    {
        public string Case { get; set; }
        public bool StructuralMass { get; set; }
        public bool NonStructuralMass { get; set; }
    }

    public enum Straus7StageType { Unmorphed, Morphed, Reset }

    /// <summary>A stage of the model (Global &gt; Stages): the groups enabled in it and the beams and plates of those groups (Straus7 numbers).
    /// ModelName is the name of the GPC <see cref="GPC.Model.Stages.Stage"/>, whose definition holds the group changes from the previous stage.</summary>
    public sealed class Straus7StageData
    {
        public int Number { get; set; }
        public string Name { get; set; }
        public string ModelName { get; set; }
        public Straus7StageType Type { get; set; }
        public bool MoveFixedNodes { get; set; }
        public bool RotateClusters { get; set; }
        public bool StageFluidLevel { get; set; }
        public int[] EnabledGroupIds { get; set; }
        public string[] EnabledGroups { get; set; }
        public int[] ActiveBeams { get; set; }
        public int[] ActivePlates { get; set; }
        public string Record { get; set; }
    }

    /// <summary>Stages, masses and temperatures of a model imported by <see cref="Straus7ApiConverter"/>, normalized to GPC units. They are kept
    /// as one preserved record of the Model (<see cref="Kind"/>) and read back with <see cref="Read"/>.</summary>
    public sealed class Straus7ModelData
    {
        public const string Kind = "Straus7 normalized data", Record = "Straus7ModelData";
        public List<Straus7StageData> Stages { get; set; } = new List<Straus7StageData>();
        public List<Straus7MassData> Masses { get; set; } = new List<Straus7MassData>();
        public List<Straus7TemperatureData> Temperatures { get; set; } = new List<Straus7TemperatureData>();
        public List<Straus7LoadCaseInertia> Inertia { get; set; } = new List<Straus7LoadCaseInertia>();

        /// <summary>The data of a Model imported from Straus7; empty lists when the Model has none (or was imported by an older converter).</summary>
        public static Straus7ModelData Read(GPC.Model.Models.Model model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            var record = model.PreservedSourceData.LastOrDefault(p => p.Kind == Kind && p.SourceRecord == Record && p.RawData != null);
            if (record == null) return new Straus7ModelData();
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(record.RawData)))
            {
                var data = (Straus7ModelData)new DataContractJsonSerializer(typeof(Straus7ModelData)).ReadObject(stream);
                data.Stages = data.Stages ?? new List<Straus7StageData>(); data.Masses = data.Masses ?? new List<Straus7MassData>();
                data.Temperatures = data.Temperatures ?? new List<Straus7TemperatureData>(); data.Inertia = data.Inertia ?? new List<Straus7LoadCaseInertia>();
                return data;
            }
        }

        internal GPC.Model.Structure.Assignments.PreservedAssignment Preserve()
        {
            using (var stream = new MemoryStream())
            {
                new DataContractJsonSerializer(typeof(Straus7ModelData)).WriteObject(stream, this);
                return new GPC.Model.Structure.Assignments.PreservedAssignment { Kind = Kind, SourceRecord = Record, RawData = Encoding.UTF8.GetString(stream.ToArray()),
                    UnitsAndAxes = "GPC units: tonne (N s2/mm), mm, degrees Celsius; global axes unless stated (beam gradients: principal axes 1, 2; plate gradient: local z).",
                    UnsupportedReason = "Stages, masses and temperatures are data: the Model derives no load, mass matrix or activation from them." };
            }
        }
    }
}
