using System;
using System.Collections.Generic;
using GPC.Model.Core.Diagnostics;

namespace GPC.Converter.Straus7
{
    // Values and native buffer dimensions are from the installed Straus7 R31 SDK.
    public enum Straus7Entity { Node = 0, Beam = 1, Plate = 2, Brick = 3, Link = 4 }
    public sealed class Straus7Node
    {
        public int Number { get; set; }
        public int UserId { get; set; }
        public double[] Coordinates { get; set; }
    }
    public sealed class Straus7Element
    {
        public int Number { get; set; }
        public int UserId { get; set; }
        public int[] Nodes { get; set; }
        public int Property { get; set; }
        public int GroupId { get; set; }
        public int Formulation { get; set; }
        public double[] InitialAxes { get; set; } // Three consecutive global unit vectors, 1/2/3 or x/y/z.
        public double[] Centroid { get; set; } // Native geometric centroid in model length units.
    }
    public sealed class Straus7Property
    {
        public int Number { get; set; }
        public string Name { get; set; }
        public int Formulation { get; set; }
        public int MaterialType { get; set; }
        public int SectionType { get; set; }
        public double[] Geometry { get; set; }
        public double[] Material { get; set; }
        // Optional (null when the binding does not read them): names, computed section data and BGL geometry.
        public string MaterialName { get; set; }
        public string SectionName { get; set; }
        /// <summary>St7GetBeamSectionPropertyData: A, I11, I22, J, SL1, SL2, SA1, SA2, XBAR, YBAR, ANGLE (model units, radians).</summary>
        public double[] SectionProperties { get; set; }
        public int BglShape { get; set; }
        public double[] BglDimensions { get; set; }
        public int MirrorType { get; set; }
        /// <summary>Native A/B mirror gaps, model length units. Null means not read.</summary>
        public double[] MirrorGaps { get; set; }
        public bool? CompatibleTwist { get; set; }
        /// <summary>St7GetSpringDamperData of a spring-damper property (btSpring): axial, lateral, torsional stiffness, dampings, mass.</summary>
        public double[] SpringData { get; set; }
    }

    /// <summary>One attribute instance of St7GetEntityAttributeSequence: local number, axis, load/freedom case, ID.</summary>
    public sealed class Straus7Attribute
    {
        public int Local { get; set; }
        public int Axis { get; set; }
        public int Case { get; set; }
        public int Id { get; set; }
    }
    public sealed class Straus7Restraint
    {
        public int Ucs { get; set; }
        public int[] Status { get; set; }
        public double[] Values { get; set; }
    }
    public sealed class Straus7Ucs
    {
        public int Type { get; set; }
        public double[] Data { get; set; }
    }
    public enum Straus7LoadFrame { Principal, Global, PrincipalMoment }
    /// <summary>Doubles PA, PB, P1, P2, a, b with the dl* type of the distributed load.</summary>
    public sealed class Straus7DistributedLoad
    {
        public int Type { get; set; }
        public int Project { get; set; }
        public double[] Values { get; set; }
    }
    public sealed class Straus7LoadCaseData
    {
        public int Number { get; set; }
        public int Type { get; set; }
        public int GravityDirection { get; set; }
        public double Gravity { get; set; }
        public double[] Defaults { get; set; }
    }
    /// <summary>Attribute ordinals of the R3 API (St7API.cs).</summary>
    public static class Straus7Attributes
    {
        public const int KTranslation = 7, KRotation = 8, BeamTEndRelease = 23, BeamREndRelease = 24;
        public const int Restraint = 1, Force = 2, Moment = 3, BeamOffset = 22, BeamDLL = 28, BeamDLG = 29, BeamCFL = 30, BeamCFG = 31, BeamCML = 32, BeamCMG = 33,
            BeamDML = 44, BeamTaper = 92, BeamSectionFactor = 94, PlateOffset = 52, PlateFacePressure = 54, PlateFaceShear = 55, PlateEdgeNormalPressure = 56,
            PlateGlobalPressure = 66, PlateThickness = 69, PlateEdgeGlobalPressure = 72, PlatePointForce = 99, PlatePointMoment = 100, PlateSectionFactor = 121;
        /// <summary>Masses and temperatures (aoTemperature, aoMTranslation, aoMRotation, aoNSMass, aoBeamTempGradient, aoBeamNSMass, aoPlateTempGradient,
        /// aoPlateNSMass).</summary>
        public const int NodeTemperature = 4, NodeTranslationalMass = 5, NodeRotationalMass = 6, NodeNonStructuralMass = 10, BeamTemperatureGradient = 34,
            BeamNonStructuralMass = 41, PlateTemperatureGradient = 59, PlateNonStructuralMass = 70;
    }
    /// <summary>A stage of Global &gt; Stages: name, the five flags of St7GetStageData (morph, move fixed nodes, rotate clusters, stage fluid level,
    /// reset; btTrue = 1) and the IDs of the groups enabled in it (St7GetStageGroupState). Stages are numbered from 1.</summary>
    public sealed class Straus7Stage
    {
        public int Number { get; set; }
        public string Name { get; set; }
        public int[] Data { get; set; }
        public int[] EnabledGroups { get; set; }
    }
    /// <summary>Optional read-only access to the stages of the model and to the result state of an element in a result case.</summary>
    public interface IStraus7StageReadApi
    {
        /// <summary>Stage <paramref name="number"/> (1-based) with the enabled state of each of <paramref name="groups"/>.</summary>
        Straus7Stage ReadStage(int number, IReadOnlyList<int> groups);
        /// <summary>St7GetElementResultState: active (1), results available (1), birth stage.</summary>
        int[] ReadElementResultState(Straus7Entity entity, int number, int resultCase);
    }
    /// <summary>Optional read-only access to masses and temperatures, in model units. Node masses act in every case; non-structural masses,
    /// temperatures and gradients belong to a load case and carry the attribute ID of their sequence.</summary>
    public interface IStraus7MassTemperatureReadApi
    {
        /// <summary>St7GetNodeTMass3: translational mass along global X, Y, Z.</summary>
        double[] ReadNodeTranslationalMass(int node);
        /// <summary>St7GetNodeRMass3: rotational mass about the axes of UCS <paramref name="ucs"/> (1 = global).</summary>
        double[] ReadNodeRotationalMass(int node, out int ucs);
        /// <summary>St7GetNodeNSMass5ID: mass, dynamic factor, global offset X, Y, Z.</summary>
        double[] ReadNodeNonStructuralMass(int node, int loadCase, int id);
        /// <summary>St7GetBeamNSMass10ID: PA, PB, P1, P2, a, b of the dl* law <paramref name="distribution"/>, dynamic factor, global offset X, Y, Z.</summary>
        double[] ReadBeamNonStructuralMass(int beam, int loadCase, int id, out int distribution);
        /// <summary>St7GetPlateNSMass5ID: mass per area, dynamic factor, global offset X, Y, Z.</summary>
        double[] ReadPlateNonStructuralMass(int plate, int loadCase, int id);
        /// <summary>St7GetNodeTemperature1 and St7GetNodeTemperatureType1 (nt* type).</summary>
        double ReadNodeTemperature(int node, int loadCase, out int type);
        /// <summary>St7GetBeamTempGradient2: gradients along the principal axes 1 and 2.</summary>
        double[] ReadBeamTemperatureGradient(int beam, int loadCase);
        /// <summary>St7GetPlateTempGradient1: gradient along the local z axis.</summary>
        double ReadPlateTemperatureGradient(int plate, int loadCase);
        /// <summary>St7GetLoadCaseMassOption: accelerations applied to the structural and to the non-structural mass.</summary>
        bool[] ReadLoadCaseMassOption(int loadCase);
    }
    /// <summary>Optional read-only access to element attributes, restraints and loads. A binding without it imports properties and geometry only.</summary>
    public interface IStraus7AssignmentReadApi
    {
        Straus7Attribute[] ReadAttributes(Straus7Entity entity, int number, int attribute);
        int FreedomCaseCount { get; }
        Straus7Restraint ReadRestraint(int node, int freedomCase);
        Straus7Ucs ReadUcs(int id);
        double[] ReadNodeLoad(int node, int loadCase, bool moment);
        double[] ReadBeamOffset(int beam);
        double[] ReadPlateOffset(int plate);
        double[] ReadPlateThickness(int plate);
        Straus7DistributedLoad ReadBeamDistributedLoad(int beam, Straus7LoadFrame frame, int direction, int loadCase, int id);
        double[] ReadBeamPointLoad(int beam, int loadCase, int id, bool moment, bool global);
        double[] ReadPlateNormalPressure(int plate, int loadCase);
        double[] ReadPlateGlobalPressure(int plate, int surface, int loadCase, out int project);
        Straus7LoadCaseData ReadLoadCaseData(int number);
    }
    /// <summary>St7GetBeamTRelease3/St7GetBeamRRelease3 of one end: status brReleased 0, brFixed 1, brPartial 2 for principal axes 1, 2, 3
    /// (axial for translations, torsion for rotations); partial values are stiffnesses in force/length or moment/radian.</summary>
    public sealed class Straus7BeamRelease
    {
        public int[] Status { get; set; }
        public double[] Values { get; set; }
    }
    /// <summary>St7GetNodeKTranslation3F/St7GetNodeKRotation3F: stiffnesses along/about the axes of a UCS (1 = global).</summary>
    public sealed class Straus7NodeStiffness
    {
        public int Ucs { get; set; }
        public double[] Values { get; set; }
    }
    /// <summary>A link element: ltMasterSlaveLink 1, ltPinnedLink 4, ltRigidLink 5 and the other ltXxx types. Nodes of two-node links;
    /// Ucs and Plane (rlPlaneXYZ 0, XY 1, YZ 2, ZX 3) of rigid links; Dofs (six flags) of master-slave links.</summary>
    public sealed class Straus7Link
    {
        public int Number { get; set; }
        public int Type { get; set; }
        public int[] Nodes { get; set; }
        public int Ucs { get; set; }
        public int Plane { get; set; }
        public int[] Dofs { get; set; }
    }
    /// <summary>An LSA combination: name, state and factor of each (load case, freedom case) pair, row-major [load case - 1, freedom case - 1].</summary>
    public sealed class Straus7Combination
    {
        public int Number { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public double[] Factors { get; set; }
    }
    /// <summary>Optional read-only access to releases, node springs, links and LSA combinations (R3 API).</summary>
    public interface IStraus7ExtendedReadApi
    {
        Straus7BeamRelease ReadBeamRelease(int beam, int end, bool rotational);
        Straus7NodeStiffness ReadNodeStiffness(int node, int freedomCase, bool rotational);
        Straus7Link ReadLink(int number);
        int CombinationCount { get; }
        Straus7Combination ReadCombination(int number, int loadCases, int freedomCases);
        int CombinedCombinationCount { get; }
    }
    /// <summary>Optional plate results at the element nodes, without averaging between elements (spNodesAverageNever).</summary>
    public interface IStraus7PlateNodeResultReadApi
    {
        /// <summary>Local force (false) or moment (true) tensor, one row per node in connectivity order (<see cref="Straus7ResultTable.NodeNumbers"/>).</summary>
        Straus7ResultTable ReadPlateNodeResult(int number, int resultCase, bool moments);
    }
    public sealed class Straus7Group
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string Name { get; set; }
    }
    public sealed class Straus7ResultFile
    {
        public int ValidationFlags { get; set; }
        public int Solver { get; set; }
        public int PrimaryCases { get; set; }
        public int SecondaryCases { get; set; }
    }
    public sealed class Straus7ResultCase
    {
        public int Number { get; set; }
        public string Name { get; set; }
        public int Stage { get; set; }
    }
    public sealed class Straus7ResultTable
    {
        public Straus7Entity Entity { get; set; }
        public int Number { get; set; }
        public int CaseNumber { get; set; }
        public string Quantity { get; set; }
        public int Rows { get; set; }
        public int Columns { get; set; }
        public double[] Positions { get; set; }
        public double[] Values { get; set; }
        public int[] ElementState { get; set; } // active, results available, birth stage.
        public int[] NodeNumbers { get; set; } // Element node-force rows, in native connectivity order.
        /// <summary>Native centroid-to-shear-centre vector in global axes and source length units.
        /// BeamForceGlobal retains solver torque about the shear centre; bending is about centroidal axes.
        /// Null means this metadata was not supplied, not that the two centres coincide.</summary>
        public double[] BeamShearCentreOffsetGlobal { get; set; }
    }
    /// <summary>Read-only native boundary. Implementations throw Straus7ApiException on nonzero API return codes.
    /// The caller owns one complete, exclusive API session and disposes it after closing results and model.</summary>
    public interface IStraus7ReadApi : IDisposable
    {
        string Version { get; }
        void OpenModelReadOnly(string path, string scratchPath);
        void CloseModel();
        int[] ReadUnits();
        int Count(Straus7Entity entity);
        Straus7Node ReadNode(int number);
        Straus7Element ReadElement(Straus7Entity entity, int number);
        Straus7Property ReadProperty(Straus7Entity entity, int number);
        int GroupCount { get; }
        Straus7Group ReadGroup(int index);
        int LoadCaseCount { get; }
        string ReadLoadCase(int number);
        int StageCount { get; }
        Straus7ResultFile ValidateResults(string path);
        Straus7ResultFile OpenResults(string path);
        void CloseResults();
        Straus7ResultCase ReadResultCase(int number);
        Straus7ResultTable ReadBeamForces(int number, int resultCase, int minimumStations);
        Straus7ResultTable ReadPlateResult(int number, int resultCase, bool moments);
        Straus7ResultTable ReadNodeResult(int number, int resultCase, bool reactions);
    }
    public sealed class Straus7ApiException : Exception
    {
        public int NativeCode { get; }
        public string Operation { get; }
        public Straus7ApiException(string operation, int code, string message)
            : base(operation + ": " + message + " (Straus7 " + code + ")") { NativeCode = code; Operation = operation; }
    }
    public interface IStraus7ElementNodeReadApi
    {
        Straus7ResultTable ReadElementNodeForces(Straus7Entity entity, int number, int resultCase);
    }
    public sealed class Straus7ImportRequest
    {
        public string ModelPath { get; set; }
        public string ModelRevision { get; set; }
        public string AnalysisId { get; set; }
    }
    public sealed class Straus7ResultsRequest
    {
        public string ModelPath { get; set; }
        public string ResultPath { get; set; }
        public int[] CaseNumbers { get; set; }
        public int[] BeamNumbers { get; set; } = new int[0];
        public int[] PlateNumbers { get; set; } = new int[0];
        public int[] NodeNumbers { get; set; } = new int[0];
        public int MinimumBeamStations { get; set; } = 11;
        // Explicit opt-in: missing native element-node force output rejects the acquisition, never fabricates it.
        public bool IncludeElementNodeForces { get; set; }
        /// <summary>Also reads the plate force/moment tensors at the element nodes, without averaging (corner values of each plate).</summary>
        public bool IncludePlateNodeResults { get; set; }
    }
    public sealed class Straus7ResultsReport
    {
        public ImportStatus Status { get; internal set; }
        public string ModelHash { get; internal set; }
        public string ResultHash { get; internal set; }
        public string ApiVersion { get; internal set; }
        public int[] Units { get; internal set; }
        public int ModelStageCount { get; internal set; }
        public Straus7ResultFile File { get; internal set; }
        public List<Straus7ResultCase> Cases { get; } = new List<Straus7ResultCase>();
        public List<Straus7ResultTable> Tables { get; } = new List<Straus7ResultTable>();
        /// <summary>Selected beams and plates skipped because inactive in the stage of the result case (case number, count).</summary>
        public Dictionary<int, int> InactiveElements { get; } = new Dictionary<int, int>();
        public List<ModelDiagnostic> Diagnostics { get; } = new List<ModelDiagnostic>();
        // Native tables remain in native units/conventions until an explicit normalization profile maps them.
    }
}
