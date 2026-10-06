using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace GPC.Converter.Straus7
{
    /// <summary>Windows x64 / Straus7 R3 ABI. Owns the native runtime; use a dedicated process when another plug-in uses St7API.
    /// Only read calls and transient result-query settings are exposed. No save, editing, solver or combination generation.</summary>
    public sealed class Straus7NativeApi : IStraus7ReadApi, IStraus7ElementNodeReadApi, IStraus7AssignmentReadApi
    {
        private IntPtr library;
        private bool initialized, modelOpen, resultsOpen;
        private readonly Dictionary<string, Delegate> functions = new Dictionary<string, Delegate>();
        private readonly Dictionary<string, Straus7Property> properties = new Dictionary<string, Straus7Property>();
        private const int FileId = 1;
        public string Version { get; private set; }
        public string LibraryPath { get; }

        public Straus7NativeApi(string libraryPath = null)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || !Environment.Is64BitProcess)
                throw new PlatformNotSupportedException("Straus7 R3 API requires a Windows x64 process.");
            LibraryPath = ResolvePath(libraryPath);
            if (GetModuleHandle("St7api.dll") != IntPtr.Zero)
                throw new InvalidOperationException("St7API is already loaded by another session. Use an isolated process to avoid releasing its licence or model handles.");
            library = LoadLibraryEx(LibraryPath, IntPtr.Zero, 0x00000100 | 0x00001000); // DLL directory + safe default search directories.
            if (library == IntPtr.Zero) throw new DllNotFoundException("Cannot load " + LibraryPath + "; Windows error " + Marshal.GetLastWin32Error());
            try
            {
                int major = 0, minor = 0, point = 0;
                Check(Call<VersionCall>("St7Version")(ref major, ref minor, ref point), "St7Version");
                if (major != 3) throw new NotSupportedException("This native binding supports the documented R3 ABI; another API major version requires a separate binding.");
                Version = major + "." + minor + "." + point;
                Check(Call<ThreeInt>("St7SetLicenceOptions")(2, 0, 0), "St7SetLicenceOptions(lmAbort)");
                Check(Call<NoArgs>("St7Init")(), "St7Init"); initialized = true;
            }
            catch { Dispose(); throw; }
        }
        private static string ResolvePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) path = Environment.GetEnvironmentVariable("STRAUS7_API_PATH");
            if (!string.IsNullOrWhiteSpace(path))
            {
                if (Directory.Exists(path)) path = Path.Combine(path, "St7api.dll");
                if (!Path.IsPathRooted(path)) throw new ArgumentException("Supply an absolute Straus7 API path.");
                if (!File.Exists(path)) throw new FileNotFoundException("Straus7 API DLL not found.", path);
                return Path.GetFullPath(path);
            }
            foreach (var folder in new[] { "Straus7 R31", "Strand7 R31", "Straus7 R3", "Strand7 R3" })
            {
                var candidate = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), folder, "Bin64", "St7api.dll");
                if (File.Exists(candidate)) return candidate;
            }
            throw new DllNotFoundException("Set STRAUS7_API_PATH or supply the full path to the installed R3 St7api.dll.");
        }
        private T Call<T>(string name) where T : class
        {
            if (library == IntPtr.Zero) throw new ObjectDisposedException(nameof(Straus7NativeApi));
            if (!functions.TryGetValue(name, out var function))
            {
                var address = GetProcAddress(library, name);
                if (address == IntPtr.Zero) throw new EntryPointNotFoundException("Required R3 API function unavailable: " + name);
                function = Marshal.GetDelegateForFunctionPointer(address, typeof(T)); functions.Add(name, function);
            }
            return (T)(object)function;
        }
        private void Check(int code, string operation)
        {
            if (code == 0) return;
            var text = new StringBuilder(1024);
            var address = GetProcAddress(library, "St7GetAPIErrorString");
            if (address != IntPtr.Zero) ((ErrorString)Marshal.GetDelegateForFunctionPointer(address, typeof(ErrorString)))(code, text, text.Capacity);
            throw new Straus7ApiException(operation, code, text.ToString());
        }
        public void OpenModelReadOnly(string path, string scratchPath)
        {
            if (modelOpen) throw new InvalidOperationException("A model is already open.");
            Check(Call<OpenModel>("St7OpenFileReadOnly")(FileId, path, scratchPath), "St7OpenFileReadOnly"); modelOpen = true;
        }
        public void CloseModel()
        {
            if (!modelOpen) return;
            CloseResults(); Check(Call<OneInt>("St7CloseFile")(FileId), "St7CloseFile"); modelOpen = false; properties.Clear();
        }
        public int[] ReadUnits()
        {
            var units = new int[6]; Check(Call<IntArray>("St7GetUnits")(FileId, units), "St7GetUnits"); return units;
        }
        private int Count(string call)
        { int value = 0; Check(Call<IntRef>(call)(FileId, ref value), call); return value; }
        public int Count(Straus7Entity entity)
        { int value = 0; Check(Call<TwoIntRef>("St7GetTotal")(FileId, (int)entity, ref value), "St7GetTotal " + entity); return value; }
        public int GroupCount => Count("St7GetNumGroups");
        public int LoadCaseCount => Count("St7GetNumLoadCase");
        public int StageCount => Count("St7GetNumStages");
        private int Attribute(string call, int number)
        { int value = 0; Check(Call<TwoIntRef>(call)(FileId, number, ref value), call + " " + number); return value; }
        private double[] Values(string call, int number, int size)
        { var values = new double[size]; Check(Call<TwoIntDoubles>(call)(FileId, number, values), call + " " + number); return values; }
        private string Name(string call, int number)
        { var name = new StringBuilder(1024); Check(Call<NameCall>(call)(FileId, number, name, name.Capacity), call + " " + number); return name.ToString(); }
        public Straus7Node ReadNode(int number) => new Straus7Node { Number = number, UserId = Attribute("St7GetNodeID", number), Coordinates = Values("St7GetNodeXYZ", number, 3) };
        public string ReadLoadCase(int number) => Name("St7GetLoadCaseName", number);
        public Straus7Group ReadGroup(int index)
        {
            var name = new StringBuilder(1024); int group = 0;
            Check(Call<GroupCall>("St7GetGroupByIndex")(FileId, index, name, name.Capacity, ref group), "St7GetGroupByIndex " + index);
            return new Straus7Group { Id = group, Name = name.ToString(), ParentId = Attribute("St7GetGroupParent", group) };
        }
        public Straus7Element ReadElement(Straus7Entity entity, int number)
        {
            if (entity != Straus7Entity.Beam && entity != Straus7Entity.Plate) throw new NotSupportedException("Only beam and plate elements are mapped.");
            var connection = new int[21]; int property = 0, group = 0;
            Check(Call<ConnectionCall>("St7GetElementConnection")(FileId, (int)entity, number, connection), "St7GetElementConnection " + entity + "/" + number);
            if (connection[0] < 1 || connection[0] > 20) throw new InvalidDataException("Invalid native connectivity size.");
            Check(Call<ThreeIntRef>("St7GetElementProperty")(FileId, (int)entity, number, ref property), "St7GetElementProperty " + number);
            Check(Call<ThreeIntRef>("St7GetEntityGroup")(FileId, (int)entity, number, ref group), "St7GetEntityGroup " + number);
            var p = ReadProperty(entity, property); bool beam = entity == Straus7Entity.Beam;
            var centroid = new double[3];
            Check(Call<CentroidCall>("St7GetElementCentroid")(FileId, (int)entity, number, 0, centroid), "St7GetElementCentroid " + number);
            return new Straus7Element { Number = number, UserId = Attribute(beam ? "St7GetBeamID" : "St7GetPlateID", number),
                Nodes = connection.Skip(1).Take(connection[0]).ToArray(), Property = property, GroupId = group, Formulation = p.Formulation, Centroid = centroid,
                InitialAxes = (beam && p.Formulation == 6) || (!beam && p.Formulation == 4)
                    ? Values(beam ? "St7GetBeamAxisSystemInitial" : "St7GetPlateAxisSystemInitial", number, 9) : null };
        }
        public Straus7Property ReadProperty(Straus7Entity entity, int number)
        {
            var key = entity + "/" + number; if (properties.TryGetValue(key, out var existing)) return existing;
            var p = new Straus7Property { Number = number }; var name = new StringBuilder(1024);
            Check(Call<PropertyName>("St7GetPropertyName")(FileId, (int)entity, number, name, name.Capacity), "St7GetPropertyName " + key); p.Name = name.ToString();
            if (entity == Straus7Entity.Beam)
            {
                p.Formulation = Attribute("St7GetBeamPropertyType", number);
                if (p.Formulation == 6)
                {
                    int type = 0; var geometry = new double[6];
                    Check(Call<SectionCall>("St7GetBeamSectionGeometry")(FileId, number, ref type, geometry), "St7GetBeamSectionGeometry " + number);
                    p.SectionType = type; p.Geometry = geometry; p.Material = Values("St7GetBeamMaterialData", number, 9);
                    var integers = new int[4]; var section = new double[20];
                    Check(Call<SectionData>("St7GetBeamSectionPropertyData")(FileId, number, integers, section), "St7GetBeamSectionPropertyData " + number);
                    p.SectionProperties = section.Take(11).ToArray();
                    p.SectionName = Name("St7GetBeamSectionName", number);
                    if (type == 17)
                    {
                        int shape = 0; var dimensions = new double[16];
                        Check(Call<SectionCall>("St7GetBeamSectionGeometryBGL")(FileId, number, ref shape, dimensions), "St7GetBeamSectionGeometryBGL " + number);
                        p.BglShape = shape; p.BglDimensions = dimensions;
                    }
                    if (type != 1 && type != 2)
                    {
                        int mirror = 0, twist = 0; var gap = new double[2];
                        Check(Call<MirrorCall>("St7GetBeamMirrorOption")(FileId, number, ref mirror, ref twist, gap), "St7GetBeamMirrorOption " + number);
                        p.MirrorType = mirror;
                    }
                }
            }
            else if (entity == Straus7Entity.Plate)
            {
                int type = 0, material = 0;
                Check(Call<PlateType>("St7GetPlatePropertyType")(FileId, number, ref type, ref material), "St7GetPlatePropertyType " + number);
                p.Formulation = type; p.MaterialType = material;
                if (type == 4)
                {
                    p.Geometry = Values("St7GetPlateThickness", number, 2);
                    if (material == 1) p.Material = Values("St7GetPlateIsotropicMaterial", number, 8);
                }
            }
            if (p.Formulation == (entity == Straus7Entity.Beam ? 6 : 4))
            {
                var material = new StringBuilder(1024);
                Check(Call<PropertyName>("St7GetMaterialName")(FileId, (int)entity, number, material, material.Capacity), "St7GetMaterialName " + key);
                p.MaterialName = material.ToString();
            }
            else throw new NotSupportedException("Unsupported property family.");
            properties.Add(key, p); return p;
        }
        public Straus7ResultFile ValidateResults(string path)
        {
            int flags = 0, solver = 0;
            Check(Call<ValidateCall>("St7ValidateResultFile")(FileId, path, ref flags, ref solver), "St7ValidateResultFile");
            return new Straus7ResultFile { ValidationFlags = flags, Solver = solver };
        }
        public Straus7ResultFile OpenResults(string path)
        {
            if (resultsOpen) throw new InvalidOperationException("Results already open.");
            int primary = 0, secondary = 0;
            Check(Call<OpenResult>("St7OpenResultFile")(FileId, path, "", 0, ref primary, ref secondary), "St7OpenResultFile(kNoCombinations)"); resultsOpen = true;
            Check(Call<OneInt>("St7DisableModelRotationUnit")(FileId), "St7DisableModelRotationUnit(radians)");
            Check(Call<ReferenceDisplacement>("St7SetReferenceDisplacement")(FileId, 0, 0), "St7SetReferenceDisplacement(absolute)");
            return new Straus7ResultFile { PrimaryCases = primary, SecondaryCases = secondary };
        }
        public void CloseResults()
        { if (resultsOpen) { Check(Call<OneInt>("St7CloseResultFile")(FileId), "St7CloseResultFile"); resultsOpen = false; } }
        public Straus7ResultCase ReadResultCase(int number) => new Straus7ResultCase { Number = number, Name = Name("St7GetResultCaseName", number), Stage = Attribute("St7GetResultCaseStage", number) };
        private int[] State(Straus7Entity entity, int number, int resultCase)
        {
            var state = new int[3]; Check(Call<StateCall>("St7GetElementResultState")(FileId, (int)entity, number, resultCase, state), "St7GetElementResultState " + entity + "/" + number); return state;
        }
        public Straus7ResultTable ReadBeamForces(int number, int resultCase, int minimumStations)
        {
            var state = State(Straus7Entity.Beam, number, resultCase);
            if (state[0] != 1 || state[1] != 1) throw new InvalidDataException("Beam inactive or results unavailable: " + number + "/" + resultCase);
            Check(Call<TwoInt>("St7SetBeamResultPosMode")(FileId, 1), "St7SetBeamResultPosMode(bpParam)");
            int rows = 0, columns = 0; var positions = new double[4096]; var values = new double[4096];
            // Global vectors avoid confusing Straus7's moment-in-plane labels with moments about axes.
            Check(Call<BeamResults>("St7GetBeamResultArray")(FileId, 1, -2, number, minimumStations, resultCase, ref rows, ref columns, positions, values), "St7GetBeamResultArray " + number + "/" + resultCase);
            return Table(Straus7Entity.Beam, number, resultCase, "BeamForceGlobal", rows, columns, positions, values, state);
        }
        public Straus7ResultTable ReadPlateResult(int number, int resultCase, bool moments)
        {
            var state = State(Straus7Entity.Plate, number, resultCase);
            if (state[0] != 1 || state[1] != 1) throw new InvalidDataException("Plate inactive or results unavailable: " + number + "/" + resultCase);
            int rows = 0, columns = 0; var values = new double[1024];
            Check(Call<PlateResults>("St7GetPlateResultArray")(FileId, moments ? 5 : 4, 0, number, resultCase, 0, 0, 0, ref rows, ref columns, values), "St7GetPlateResultArray " + number + "/" + resultCase);
            return Table(Straus7Entity.Plate, number, resultCase, moments ? "PlateMomentLocalCentroid" : "PlateForceLocalCentroid", rows, columns, null, values, state);
        }
        public Straus7ResultTable ReadNodeResult(int number, int resultCase, bool reactions)
        {
            var values = new double[6];
            Check(Call<NodeResults>("St7GetNodeResult")(FileId, reactions ? 5 : 1, number, resultCase, values), "St7GetNodeResult " + number + "/" + resultCase);
            return Table(Straus7Entity.Node, number, resultCase, reactions ? "NodeReactionGlobal" : "NodeDisplacementGlobal", 1, 6, null, values, null);
        }
        public Straus7ResultTable ReadElementNodeForces(Straus7Entity entity, int number, int resultCase)
        {
            if (entity != Straus7Entity.Beam && entity != Straus7Entity.Plate) throw new NotSupportedException("Element node forces require beam or plate.");
            var state = State(entity, number, resultCase);
            if (state[0] != 1 || state[1] != 1) throw new InvalidDataException("Element inactive or results unavailable.");
            var connection = new int[21];
            Check(Call<ConnectionCall>("St7GetElementConnection")(FileId, (int)entity, number, connection), "St7GetElementConnection " + number);
            if (entity == Straus7Entity.Beam ? connection[0] != 2 : connection[0] != 3 && connection[0] != 4)
                throw new NotSupportedException("Unsupported element-node force connectivity.");
            int rows = 2, columns = 0; var values = new double[entity == Straus7Entity.Beam ? 4096 : 1024];
            if (entity == Straus7Entity.Beam)
                Check(Call<BeamEndResults>("St7GetBeamResultEndPos")(FileId, 13, -2, number, resultCase, ref columns, values), "St7GetBeamResultEndPos(rtBeamNodeReact) " + number);
            else
                Check(Call<PlateResults>("St7GetPlateResultArray")(FileId, 16, -1, number, resultCase, 2, 0, 0, ref rows, ref columns, values), "St7GetPlateResultArray(rtPlateNodeReact, no averaging) " + number);
            if (rows != connection[0] || columns != 6) throw new InvalidDataException("Unexpected element-node result layout.");
            var result = Table(entity, number, resultCase, "ElementNodeForceGlobal", rows, columns,
                entity == Straus7Entity.Beam ? new[] { 0d, 1d } : null, values, state);
            result.NodeNumbers = connection.Skip(1).Take(rows).ToArray(); return result;
        }
        public Straus7Attribute[] ReadAttributes(Straus7Entity entity, int number, int attribute)
        {
            int count = 0;
            Check(Call<AttributeCount>("St7GetEntityAttributeSequenceCount")(FileId, (int)entity, number, attribute, ref count), "St7GetEntityAttributeSequenceCount " + entity + "/" + number + "/" + attribute);
            if (count < 0 || count > 100000) throw new InvalidDataException("Invalid attribute count.");
            if (count == 0) return new Straus7Attribute[0];
            var values = new int[4 * count];
            Check(Call<AttributeSequence>("St7GetEntityAttributeSequence")(FileId, (int)entity, number, attribute, count, values), "St7GetEntityAttributeSequence " + entity + "/" + number + "/" + attribute);
            return Enumerable.Range(0, count).Select(i => new Straus7Attribute { Local = values[4 * i], Axis = values[4 * i + 1], Case = values[4 * i + 2], Id = values[4 * i + 3] }).ToArray();
        }
        public int FreedomCaseCount => Count("St7GetNumFreedomCase");
        public Straus7Restraint ReadRestraint(int node, int freedomCase)
        {
            int ucs = 0; var status = new int[6]; var values = new double[6];
            Check(Call<RestraintCall>("St7GetNodeRestraint6")(FileId, node, freedomCase, ref ucs, status, values), "St7GetNodeRestraint6 " + node + "/" + freedomCase);
            return new Straus7Restraint { Ucs = ucs, Status = status, Values = values };
        }
        public Straus7Ucs ReadUcs(int id)
        {
            int type = 0; var data = new double[10];
            Check(Call<UcsCall>("St7GetUCS")(FileId, id, ref type, data), "St7GetUCS " + id);
            return new Straus7Ucs { Type = type, Data = data };
        }
        public double[] ReadNodeLoad(int node, int loadCase, bool moment) => CaseValues(moment ? "St7GetNodeMoment3" : "St7GetNodeForce3", node, loadCase, 3);
        public double[] ReadBeamOffset(int beam) => Values("St7GetBeamOffset2", beam, 2);
        public double[] ReadPlateOffset(int plate) => Values("St7GetPlateOffset1", plate, 1);
        public double[] ReadPlateThickness(int plate) => Values("St7GetPlateThickness2", plate, 2);
        public Straus7DistributedLoad ReadBeamDistributedLoad(int beam, Straus7LoadFrame frame, int direction, int loadCase, int id)
        {
            int type = 0, project = 0; var values = new double[6];
            if (frame == Straus7LoadFrame.Global)
                Check(Call<DistributedGlobal>("St7GetBeamDistributedForceGlobal6ID")(FileId, beam, direction, loadCase, id, ref project, ref type, values), "St7GetBeamDistributedForceGlobal6ID " + beam);
            else
                Check(Call<DistributedPrincipal>(frame == Straus7LoadFrame.Principal ? "St7GetBeamDistributedForcePrincipal6ID" : "St7GetBeamDistributedMomentPrincipal6ID")(FileId, beam, direction, loadCase, id, ref type, values),
                    "St7GetBeamDistributed" + frame + " " + beam);
            return new Straus7DistributedLoad { Type = type, Project = project, Values = values };
        }
        public double[] ReadBeamPointLoad(int beam, int loadCase, int id, bool moment, bool global)
        {
            var name = "St7GetBeamPoint" + (moment ? "Moment" : "Force") + (global ? "Global" : "Principal") + "4ID"; var values = new double[4];
            Check(Call<FourIntDoubles>(name)(FileId, beam, loadCase, id, values), name + " " + beam); return values;
        }
        public double[] ReadPlateNormalPressure(int plate, int loadCase) => CaseValues("St7GetPlateNormalPressure2", plate, loadCase, 2);
        public double[] ReadPlateGlobalPressure(int plate, int surface, int loadCase, out int project)
        {
            int flag = 0; var values = new double[3];
            Check(Call<GlobalPressure>("St7GetPlateGlobalPressure3S")(FileId, plate, surface, loadCase, ref flag, values), "St7GetPlateGlobalPressure3S " + plate);
            project = flag; return values;
        }
        public Straus7LoadCaseData ReadLoadCaseData(int number)
        {
            var data = new Straus7LoadCaseData { Number = number, Type = Attribute("St7GetLoadCaseType", number) };
            if (data.Type == 1)
            {
                data.GravityDirection = Attribute("St7GetLoadCaseGravityDir", number); double gravity = 0;
                Check(Call<GravityCall>("St7GetLoadCaseGravity")(FileId, number, ref gravity), "St7GetLoadCaseGravity " + number); data.Gravity = gravity;
            }
            data.Defaults = Values("St7GetLoadCaseDefaults", number, 13);
            return data;
        }
        private double[] CaseValues(string call, int number, int loadCase, int size)
        { var values = new double[size]; Check(Call<ThreeIntDoubles>(call)(FileId, number, loadCase, values), call + " " + number + "/" + loadCase); return values; }

        private static Straus7ResultTable Table(Straus7Entity entity, int number, int resultCase, string quantity, int rows, int columns, double[] positions, double[] values, int[] state)
        {
            if (rows <= 0 || columns <= 0 || (long)rows * columns > values.Length || (positions != null && rows > positions.Length))
                throw new InvalidDataException("Invalid native result array dimensions.");
            return new Straus7ResultTable { Entity = entity, Number = number, CaseNumber = resultCase, Quantity = quantity, Rows = rows, Columns = columns,
                Positions = positions?.Take(rows).ToArray(), Values = values.Take(rows * columns).ToArray(), ElementState = state };
        }
        public void Dispose()
        {
            if (library == IntPtr.Zero) return;
            Exception failure = null;
            try { CloseResults(); } catch (Exception ex) { failure = ex; }
            try { CloseModel(); } catch (Exception ex) { failure = failure ?? ex; }
            try { if (initialized) { Check(Call<NoArgs>("St7Release")(), "St7Release"); initialized = false; } }
            catch (Exception ex) { failure = failure ?? ex; }
            finally { FreeLibrary(library); library = IntPtr.Zero; functions.Clear(); }
            if (failure != null) throw new InvalidOperationException("Straus7 API cleanup failed.", failure);
        }

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)] private static extern IntPtr LoadLibraryEx(string file, IntPtr reserved, uint flags);
        [DllImport("kernel32", CharSet = CharSet.Unicode)] private static extern IntPtr GetModuleHandle(string name);
        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true)] private static extern IntPtr GetProcAddress(IntPtr module, string name);
        [DllImport("kernel32")] private static extern bool FreeLibrary(IntPtr module);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int NoArgs();
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int OneInt(int a);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int TwoInt(int a, int b);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int ThreeInt(int a, int b, int c);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int IntRef(int a, ref int b);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int TwoIntRef(int a, int b, ref int c);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int ThreeIntRef(int a, int b, int c, ref int d);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int VersionCall(ref int a, ref int b, ref int c);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int IntArray(int a, [Out] int[] b);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int TwoIntDoubles(int a, int b, [Out] double[] c);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int ConnectionCall(int a, int b, int c, [Out] int[] d);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int SectionCall(int a, int b, ref int c, [Out] double[] d);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int PlateType(int a, int b, ref int c, ref int d);
        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)] private delegate int OpenModel(int a, string b, string c);
        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)] private delegate int NameCall(int a, int b, StringBuilder c, int d);
        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)] private delegate int ErrorString(int a, StringBuilder b, int c);
        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)] private delegate int GroupCall(int a, int b, StringBuilder c, int d, ref int e);
        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)] private delegate int PropertyName(int a, int b, int c, StringBuilder d, int e);
        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)] private delegate int ValidateCall(int a, string b, ref int c, ref int d);
        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)] private delegate int OpenResult(int a, string b, string c, int d, ref int e, ref int f);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int StateCall(int a, int b, int c, int d, [Out] int[] e);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int CentroidCall(int a, int b, int c, int d, [Out] double[] e);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int BeamResults(int a, int b, int c, int d, int e, int f, ref int g, ref int h, [Out] double[] i, [Out] double[] j);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int BeamEndResults(int a, int b, int c, int d, int e, ref int f, [Out] double[] g);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int PlateResults(int a, int b, int c, int d, int e, int f, int g, int h, ref int i, ref int j, [Out] double[] k);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int NodeResults(int a, int b, int c, int d, [Out] double[] e);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int ReferenceDisplacement(int a, int b, byte c);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int SectionData(int a, int b, [Out] int[] c, [Out] double[] d);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int MirrorCall(int a, int b, ref int c, ref int d, [Out] double[] e);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int AttributeCount(int a, int b, int c, int d, ref int e);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int AttributeSequence(int a, int b, int c, int d, int e, [Out] int[] f);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int RestraintCall(int a, int b, int c, ref int d, [Out] int[] e, [Out] double[] f);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int UcsCall(int a, int b, ref int c, [Out] double[] d);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int ThreeIntDoubles(int a, int b, int c, [Out] double[] d);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int FourIntDoubles(int a, int b, int c, int d, [Out] double[] e);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int DistributedPrincipal(int a, int b, int c, int d, int e, ref int f, [Out] double[] g);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int DistributedGlobal(int a, int b, int c, int d, int e, ref int f, ref int g, [Out] double[] h);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int GlobalPressure(int a, int b, int c, int d, ref int e, [Out] double[] f);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int GravityCall(int a, int b, ref double c);
    }
}
