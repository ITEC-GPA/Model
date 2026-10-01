using System;
using System.Collections.Generic;
using GPC.Model.PostProcessing;

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
        public List<ModelDiagnostic> Diagnostics { get; } = new List<ModelDiagnostic>();
        // Native tables remain in native units/conventions until an explicit normalization profile maps them.
    }
}
