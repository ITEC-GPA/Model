using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Core.Coordinates;
using GPC.Model.Core.Identity;
using GPC.Model.Models;
using GPC.Model.Structure.Assignments;

namespace GPC.Converter.Straus7
{
    /// <summary>Verifiable R3 physical-attribute boundary. IDs are positive native IDs; inputs are N, mm, tonne, Celsius.
    /// The caller owns the model, runtime and transaction. Implementations throw on native errors.</summary>
    public interface IStraus7WriteApi
    {
        void SetNodeTemperature(int node, int loadCase, NodalTemperatureKind kind, double celsius);
        void SetBeamTemperatureGradient(int beam, int loadCase, double axis1, double axis2);
        void SetBeamInitialState(int beam, int loadCase, BeamInitialStateKind kind, double value);
        void SetBeamSupport(int beam, int direction, int freedomCase, bool compressionOnly, double stiffness, double gap);
        void SetBeamTaper(int beam, int axis, TaperAlignment alignment, double atI, double atJ);
        void SetPlateInitialStress(int plate, int loadCase, double sigma11, double sigma22, double sigma33);
        void SetPlateNonStructuralMass(int plate, int loadCase, double massPerArea, double dynamicFactor, double offsetX, double offsetY, double offsetZ);
        void SetPlateSupport(int plate, ShellFace face, int freedomCase, bool compressionOnly, double stiffness, double gap);
    }

    /// <summary>Validated, detached physical-attribute operations for an already created N-mm Straus model.
    /// This is one part of an export, not a topology/property/load exporter. Preparation performs no native writes.
    /// Unsupported constitutive/material inputs are diagnosed, never silently converted to isotropic data.</summary>
    public sealed class Straus7PhysicalWritePlan
    {
        private readonly Action<IStraus7WriteApi>[] operations;
        private readonly KeyValuePair<int,int>[] stations;
        public string AnalysisFingerprint { get; }
        public int OperationCount => operations.Length;
        /// <summary>Native beam ID -> minimum result stations. Applied by the results reader, not a model-edit API.</summary>
        public IReadOnlyDictionary<int,int> MinimumOutputStations => stations.ToDictionary(v=>v.Key,v=>v.Value);
        private Straus7PhysicalWritePlan(string fingerprint, IEnumerable<Action<IStraus7WriteApi>> operations, IEnumerable<KeyValuePair<int,int>> stations)
        { AnalysisFingerprint=fingerprint; this.operations=operations.ToArray(); this.stations=stations.ToArray(); }

        public static Straus7PhysicalWritePlan Prepare(global::GPC.Model.Models.Model model, SolverIdentityMap identity,
            IReadOnlyDictionary<string,int> loadCases, IReadOnlyDictionary<string,int> freedomCases, int defaultFreedomCase=1)
        {
            if(model==null || identity==null || loadCases==null || freedomCases==null) throw new ArgumentNullException();
            if(identity.ModelGuid!=model.Guid || identity.AnalysisFingerprint!=model.AnalysisFingerprint()) throw new ArgumentException("StaleStrausExportIdentity");
            if(defaultFreedomCase<=0) throw new ArgumentOutOfRangeException(nameof(defaultFreedomCase));
            ValidateIds(loadCases); ValidateIds(freedomCases);
            var ids=identity.Entries.ToDictionary(e=>e.EntityGuid,e=>e);
            int Id(Guid guid, EntityFamily family) {
                if(!ids.TryGetValue(guid,out var value) || value.Family!=family || value.SolverId<=0) throw new ArgumentException("MissingStrausExportIdentity");
                return value.SolverId;
            }
            int Load(global::GPC.Model.LoadCases.LoadCaseBase value) {
                if(value==null || !model.LoadCases.TryGetValue(value.Name,out var registered) || registered.Guid!=value.Guid || !loadCases.TryGetValue(value.Name,out var id))
                    throw new ArgumentException("MissingStrausLoadCaseIdentity"); return id;
            }
            int Freedom(global::GPC.Model.FreedomCases.FreedomCase value) {
                if(value==null) return defaultFreedomCase;
                if(!model.FreedomCases.TryGetValue(value.Name,out var registered) || registered.Guid!=value.Guid || !freedomCases.TryGetValue(value.Name,out var id))
                    throw new ArgumentException("MissingStrausFreedomCaseIdentity"); return id;
            }
            var ops=new List<Action<IStraus7WriteApi>>(); var samples=new List<KeyValuePair<int,int>>();
            foreach(var e in model.AllElements) PhysicalAssignments.Validate(e);
            foreach(var node in model.NodesElements.Values)
            {
                var p=node.Assignments.Physical; if(p==null) continue; int n=Id(node.Guid,EntityFamily.Node);
                foreach(var t in p.Temperatures) { int c=Load(t.Case); var kind=t.Kind; double value=t.Celsius; ops.Add(a=>a.SetNodeTemperature(n,c,kind,value)); }
            }
            foreach(var beam in model.BeamElements.Values)
            {
                var p=beam.Assignments.Physical; if(p==null) continue; int n=Id(beam.Guid,EntityFamily.Beam);
                foreach(var t in p.TemperatureGradients) { int c=Load(t.Case); double x=t.Axis1,y=t.Axis2; ops.Add(a=>a.SetBeamTemperatureGradient(n,c,x,y)); }
                foreach(var t in p.InitialStates) { int c=Load(t.Case); var kind=t.Kind; double value=t.Value; ops.Add(a=>a.SetBeamInitialState(n,c,kind,value)); }
                var used=new HashSet<string>(StringComparer.Ordinal);
                foreach(var s in p.Supports)
                {
                    int c=Freedom(s.FreedomCase), direction=SupportDirection(s,beam.Assignments.SectionAxes);
                    bool compression=s.Behavior!=DistributedSpringBehavior.Bilateral;
                    if(s.Behavior==DistributedSpringBehavior.TensionOnly) direction^=1;
                    if(!used.Add(c+":"+direction)) throw new ArgumentException("ConflictingStrausBeamSupportDirection");
                    double k=s.Stiffness,gap=s.Gap; ops.Add(a=>a.SetBeamSupport(n,direction,c,compression,k,gap));
                }
                if(p.Taper!=null)
                {
                    var t=p.Taper; double i1=t.ScaleI1,j1=t.ScaleJ1,i2=t.ScaleI2,j2=t.ScaleJ2; var a1=t.Alignment1; var a2=t.Alignment2;
                    ops.Add(a=>a.SetBeamTaper(n,1,a1,i1,j1)); ops.Add(a=>a.SetBeamTaper(n,2,a2,i2,j2));
                }
                if(p.MinimumOutputStations.HasValue) samples.Add(new KeyValuePair<int,int>(n,p.MinimumOutputStations.Value));
            }
            foreach(var shell in model.AreaElements.Values)
            {
                var p=shell.Assignments.Physical; if(p==null) continue; int n=Id(shell.Guid,EntityFamily.Shell);
                if(p.Membrane!=null) throw new NotSupportedException("StrausMembraneRequiresExplicitConstitutivePropertyWriter");
                foreach(var t in p.InitialStresses)
                {
                    // GPC AreaElement here denotes a shell; z preload is supported by R3 only for plane strain/axisymmetric plates.
                    if(t.Components.Z!=0) throw new NotSupportedException("StrausShellCannotApplyThroughThicknessInitialStress");
                    int c=Load(t.Case); double x=t.Components.X,y=t.Components.Y,z=t.Components.Z; ops.Add(a=>a.SetPlateInitialStress(n,c,x,y,z));
                }
                foreach(var m in p.NonStructuralMasses)
                { int c=Load(m.Case); double mass=m.MassPerArea,f=m.DynamicFactor,x=m.GlobalOffset.X,y=m.GlobalOffset.Y,z=m.GlobalOffset.Z; ops.Add(a=>a.SetPlateNonStructuralMass(n,c,mass,f,x,y,z)); }
                var used=new HashSet<string>(StringComparer.Ordinal);
                foreach(var s in p.Supports)
                {
                    if(s.Behavior==DistributedSpringBehavior.TensionOnly) throw new NotSupportedException("StrausShellTensionOnlySupportRequiresExplicitFaceConvention");
                    int c=Freedom(s.FreedomCase); var face=s.Face; bool compression=s.Behavior==DistributedSpringBehavior.CompressionOnly;
                    if(!used.Add(c+":"+face)) throw new ArgumentException("ConflictingStrausShellSupportFace");
                    double k=s.Stiffness,gap=s.Gap; ops.Add(a=>a.SetPlateSupport(n,face,c,compression,k,gap));
                }
            }
            return new Straus7PhysicalWritePlan(model.AnalysisFingerprint(),ops,samples);
        }

        public void Write(IStraus7WriteApi api, global::GPC.Model.Models.Model currentInput)
        {
            if(api==null || currentInput==null) throw new ArgumentNullException();
            if(currentInput.AnalysisFingerprint()!=AnalysisFingerprint) throw new InvalidOperationException("StaleStrausPhysicalWritePlan");
            foreach(var op in operations) op(api);
        }
        private static void ValidateIds(IReadOnlyDictionary<string,int> values)
        { if(values.Any(p=>string.IsNullOrWhiteSpace(p.Key)||p.Value<=0)||values.Values.Distinct().Count()!=values.Count) throw new ArgumentException("InvalidStrausCaseMap"); }
        private static int SupportDirection(BeamDistributedSupport support, CoordinateSystem beamAxes)
        {
            if(beamAxes==null) throw new NotSupportedException("StrausBeamSupportRequiresExplicitSectionAxes"); Axes.Validate(beamAxes);
            var v=support.Direction; var a=support.Axes;
            var global=a==null ? v : new Vector3d(v.X*a.V1.X+v.Y*a.V2.X+v.Z*a.V3.X,v.X*a.V1.Y+v.Y*a.V2.Y+v.Z*a.V3.Y,v.X*a.V1.Z+v.Y*a.V2.Z+v.Z*a.V3.Z);
            double x=Axes.Dot(global,beamAxes.V1),y=Axes.Dot(global,beamAxes.V2),z=Axes.Dot(global,beamAxes.V3);
            if(Math.Abs(z)>1e-8) throw new NotSupportedException("StrausBeamSupportCannotActAlongAxis3");
            if(Math.Abs(Math.Abs(x)-1)<1e-8 && Math.Abs(y)<1e-8) return x<0?0:1;
            if(Math.Abs(Math.Abs(y)-1)<1e-8 && Math.Abs(x)<1e-8) return y<0?2:3;
            throw new NotSupportedException("StrausBeamSupportRequiresPrincipalDirection");
        }
    }
}
