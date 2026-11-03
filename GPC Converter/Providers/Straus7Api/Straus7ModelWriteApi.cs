using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace GPC.Converter.Straus7
{
    /// <summary>Typed boundary for topology, properties and mechanical actions. Native arrays follow the R31 SDK;
    /// all values are already normalized to N, mm, t, rad, s and Celsius by the detached model plan.</summary>
    public interface IStraus7ModelWriteApi
    {
        void RequireEmptyTopology();
        void CreateLoadCase(int id, string name);
        void CreateFreedomCase(int id, string name);
        void CreateGroup(int id, int parent, string name);
        void CreateUcs(int id, double[] points);
        void SetNode(int id, double[] coordinates);
        void SetEntityGroup(int entity, int id, int group);
        void CreateBeamProperty(int id, string name, int formulation, double[] section, double[] material);
        void CreatePlateProperty(int id, string name, int formulation, double bending, double membrane, double[] material, bool orthotropic);
        void SetElement(int entity, int id, int property, int[] nodes);
        void SetBeamAxes(int id, double[] axes);
        void SetBeamOffset(int id, double[] offset);
        void SetBeamRelease(int id, int end, bool rotation, int[] status, double[] stiffness);
        void SetNodeRestraint(int node, int freedomCase, int ucs, int[] status, double[] prescribed);
        void SetNodeSpring(int node, int freedomCase, int ucs, bool rotation, double[] stiffness);
        void SetNodeLoad(int node, int loadCase, bool moment, double[] values);
        void SetBeamPointLoad(int beam, int loadCase, int id, bool moment, double[] globalValuesAndPosition);
        void SetBeamDistributedLoad(int beam, int loadCase, int id, int axis, bool moment, double[] values);
        void SetPlatePressure(int plate, int loadCase, int face, double[] globalPressure);
        void SetPlateOffset(int plate, double offset);
        void SetPlateAxes(int plate, double[] axes);
        void SetGravity(int loadCase, double[] acceleration);
        void AddCombination(int id, string name, int[] cases, double[] factors, int freedomCase);
    }

    public sealed partial class Straus7NativeWriter : IStraus7ModelWriteApi, IStraus7DistributionWriteApi
    {
        public void RequireEmptyTopology()
        {
            if(!freshFile)throw new InvalidOperationException("StrausModelWriterRequiresNewFile");
            if (Enumerable.Range(0,5).Any(t => api.Count((Straus7Entity)t) != 0) || api.StageCount != 0 || api.MovingTemplateCount != 0)
                throw new InvalidOperationException("StrausModelWriterRequiresEmptyTopology");
            if (api.LoadCaseCount > 1 || api.FreedomCaseCount > 1 || api.GroupCount > 1)
                throw new InvalidOperationException("StrausModelWriterRequiresFreshCasesAndGroups");
        }
        public void CreateLoadCase(int id, string name) => CreateCase(id,name,false);
        public void CreateFreedomCase(int id, string name) => CreateCase(id,name,true);
        private void CreateCase(int id,string name,bool freedom)
        {
            int count=freedom?api.FreedomCaseCount:api.LoadCaseCount;
            if(id==count+1)
            { string op=freedom?"St7NewFreedomCase":"St7NewLoadCase";api.CheckWrite(api.WriteFunction<ModelName>(op)(Id,name),op); }
            else if(id!=1 || count!=1) throw new InvalidOperationException("NonSequentialStrausCase");
            string set=freedom?"St7SetFreedomCaseName":"St7SetLoadCaseName";
            api.CheckWrite(api.WriteFunction<MovingName>(set)(Id,id,name),set);
        }
        public void CreateGroup(int id,int parent,string name)
        {
            if(id==1)
            { api.CheckWrite(api.WriteFunction<MovingName>("St7SetGroupIDName")(Id,1,name),"St7SetGroupIDName");return; }
            int created=0;api.CheckWrite(api.WriteFunction<ModelGroup>("St7NewChildGroup")(Id,parent,name,ref created),"St7NewChildGroup");
            if(created!=id)throw new InvalidOperationException("UnexpectedStrausGroupIdentity");
        }
        public void CreateUcs(int id,double[] points) => api.CheckWrite(api.WriteFunction<ThreeDoubles>("St7SetUCS")(Id,id,0,points),"St7SetUCS");
        public void SetNode(int id,double[] coordinates) => api.CheckWrite(api.WriteFunction<ModelValues>("St7SetNodeXYZ")(Id,id,coordinates),"St7SetNodeXYZ");
        public void SetEntityGroup(int entity,int id,int group) => api.CheckWrite(api.WriteFunction<Four>("St7SetEntityGroup")(Id,entity,id,group),"St7SetEntityGroup");
        public void CreateBeamProperty(int id,string name,int formulation,double[] section,double[] material)
        {
            api.CheckWrite(api.WriteFunction<ModelProperty>("St7NewBeamProperty")(Id,id,formulation,name),"St7NewBeamProperty");
            api.CheckWrite(api.WriteFunction<ModelSection>("St7SetBeamSectionPropertyData")(Id,id,new[]{0},section),"St7SetBeamSectionPropertyData");
            api.CheckWrite(api.WriteFunction<ModelValues>("St7SetBeamMaterialData")(Id,id,material),"St7SetBeamMaterialData");
            if(formulation!=4)api.CheckWrite(api.WriteFunction<MovingThree>("St7SetBeamShearModulusMode")(Id,id,1),"St7SetBeamShearModulusMode");
            api.CheckWrite(api.WriteFunction<MovingThree>("St7UpdateElementPropertyData")(Id,1,id),"St7UpdateElementPropertyData");
        }
        public void CreatePlateProperty(int id,string name,int formulation,double bending,double membrane,double[] material,bool orthotropic)
        {
            bool shearPanel=formulation==5;
            api.CheckWrite(api.WriteFunction<ModelPlateProperty>("St7NewPlateProperty")(Id,id,shearPanel?4:formulation,shearPanel||orthotropic?2:1,name),"St7NewPlateProperty");
            if(formulation==7)return; // Load patches have no physical material or stiffness thickness.
            api.CheckWrite(api.WriteFunction<ModelValues>("St7SetPlateThickness")(Id,id,new[]{membrane,bending}),"St7SetPlateThickness");
            if(shearPanel)
            {
                if(orthotropic)throw new ArgumentException("ShearPanelRequiresIsotropicMaterial");
                // R31 does not populate shear-panel G through SetPlateIsotropicMaterial. Seed G12 through
                // an orthotropic shell, then switch formulation. Confirmed by the uniform-shear solver test.
                double g=material[0],nu=material[1],e=2*g*(1+nu);
                var seed=new[]{e,e,e,g,g,g,nu,nu,nu,material[2],material[3],material[3],material[3],material[4],material[5],material[6],material[6],material[7]};
                api.CheckWrite(api.WriteFunction<ModelValues>("St7SetPlateOrthotropicMaterial")(Id,id,seed),"St7SetPlateOrthotropicMaterial");
                api.CheckWrite(api.WriteFunction<Four>("St7SetPlatePropertyType")(Id,id,5,1),"St7SetPlatePropertyType");
            }
            string op=orthotropic?"St7SetPlateOrthotropicMaterial":"St7SetPlateIsotropicMaterial";
            api.CheckWrite(api.WriteFunction<ModelValues>(op)(Id,id,material),op);
        }
        public void SetElement(int entity,int id,int property,int[] nodes)
        {
            var connection=new int[21];connection[0]=nodes.Length;Array.Copy(nodes,0,connection,1,nodes.Length);
            api.CheckWrite(api.WriteFunction<ModelElement>("St7SetElementConnection")(Id,entity,id,property,connection),"St7SetElementConnection");
        }
        public void SetBeamAxes(int id,double[] axes)
        {
            var current=new double[9];api.CheckWrite(api.WriteFunction<ModelValues>("St7GetBeamAxisSystemInitial")(Id,id,current),"St7GetBeamAxisSystemInitial");
            double x=Enumerable.Range(0,3).Sum(i=>axes[i]*current[i]);double y=Enumerable.Range(0,3).Sum(i=>axes[i]*current[i+3]);
            double angle=Math.Atan2(y,x)*180/Math.PI;
            api.CheckWrite(api.WriteFunction<ModelValues>("St7SetBeamReferenceAngle1")(Id,id,new[]{angle}),"St7SetBeamReferenceAngle1");
            api.CheckWrite(api.WriteFunction<ModelValues>("St7GetBeamAxisSystemInitial")(Id,id,current),"St7GetBeamAxisSystemInitial");
            if(current.Zip(axes,(a,b)=>Math.Abs(a-b)).Max()>1e-8)throw new InvalidOperationException("StrausBeamOrientationMismatch");
        }
        public void SetBeamOffset(int id,double[] offset) => api.CheckWrite(api.WriteFunction<ModelValues>("St7SetBeamOffset2")(Id,id,offset),"St7SetBeamOffset2");
        public void SetBeamRelease(int id,int end,bool rotation,int[] status,double[] stiffness)
        {
            string op=rotation?"St7SetBeamRRelease3":"St7SetBeamTRelease3";
            api.CheckWrite(api.WriteFunction<ModelRelease>(op)(Id,id,end,status,stiffness),op);
        }
        public void SetNodeRestraint(int node,int freedomCase,int ucs,int[] status,double[] prescribed) =>
            api.CheckWrite(api.WriteFunction<ModelRestraint>("St7SetNodeRestraint6")(Id,node,freedomCase,ucs,status,prescribed),"St7SetNodeRestraint6");
        public void SetNodeSpring(int node,int freedomCase,int ucs,bool rotation,double[] stiffness) =>
            FourValues(rotation?"St7SetNodeKRotation3F":"St7SetNodeKTranslation3F",node,freedomCase,ucs,stiffness);
        public void SetNodeLoad(int node,int loadCase,bool moment,double[] values) => Three(moment?"St7SetNodeMoment3":"St7SetNodeForce3",node,loadCase,values);
        public void SetBeamPointLoad(int beam,int loadCase,int id,bool moment,double[] values) => FourValues(moment?"St7SetBeamPointMomentGlobal4ID":"St7SetBeamPointForceGlobal4ID",beam,loadCase,id,values);
        public void SetBeamDistributedLoad(int beam,int loadCase,int id,int axis,bool moment,double[] values)
        {
            // Forces are global; distributed moments are expressed in principal beam axes by the plan.
            string op=moment?"St7SetBeamDistributedMomentPrincipal6ID":"St7SetBeamDistributedForceGlobal6ID";
            if(moment)api.CheckWrite(api.WriteFunction<ModelDistributedMoment>(op)(Id,beam,axis,loadCase,1,id,values),op);
            else api.CheckWrite(api.WriteFunction<ModelDistributedForce>(op)(Id,beam,axis,0,loadCase,1,id,values),op);
        }
        public void SetPlatePressure(int plate,int loadCase,int face,double[] globalPressure) =>
            api.CheckWrite(api.WriteFunction<FiveDoubles>("St7SetPlateGlobalPressure3S")(Id,plate,face,0,loadCase,globalPressure),"St7SetPlateGlobalPressure3S");
        public void SetPlateOffset(int plate,double offset) => api.CheckWrite(api.WriteFunction<ModelValues>("St7SetPlateOffset1")(Id,plate,new[]{offset}),"St7SetPlateOffset1");
        public void SetBeamStringGroup(int beam,int group) => api.CheckWrite(api.WriteFunction<MovingThree>("St7SetBeamStringGroup1")(Id,beam,group),"St7SetBeamStringGroup1");
        public void SetLoadPatch(int plate,int kind,int edges,double[] weights) => FourValues("St7SetPlateLoadPatch4",plate,kind,edges,weights);
        public void SetPlateAxes(int plate,double[] axes)
        {
            var current=new double[9];api.CheckWrite(api.WriteFunction<ModelValues>("St7GetPlateAxisSystemInitial")(Id,plate,current),"St7GetPlateAxisSystemInitial");
            if(Enumerable.Range(6,3).Any(i=>Math.Abs(axes[i]-current[i])>1e-8))throw new ArgumentException("PlateMaterialAxesNormalMismatch");
            double x=Enumerable.Range(0,3).Sum(i=>axes[i]*current[i]),y=Enumerable.Range(0,3).Sum(i=>axes[i]*current[i+3]);
            api.CheckWrite(api.WriteFunction<ModelValues>("St7SetPlateXAngle1")(Id,plate,new[]{Math.Atan2(y,x)*180/Math.PI}),"St7SetPlateXAngle1");
        }
        public void SetGravity(int loadCase,double[] acceleration)
        {
            int axis=Array.FindIndex(acceleration,v=>v!=0);
            if(axis<0 || acceleration.Count(v=>v!=0)!=1)throw new ArgumentException("StrausGravityRequiresOneGlobalAxis");
            api.CheckWrite(api.WriteFunction<MovingThree>("St7SetLoadCaseType")(Id,loadCase,1),"St7SetLoadCaseType");
            api.CheckWrite(api.WriteFunction<MovingThree>("St7SetLoadCaseGravityDir")(Id,loadCase,axis+1),"St7SetLoadCaseGravityDir");
            api.CheckWrite(api.WriteFunction<StageLevel>("St7SetLoadCaseGravity")(Id,loadCase,acceleration[axis]),"St7SetLoadCaseGravity");
        }
        public void AddCombination(int id,string name,int[] cases,double[] factors,int freedomCase)
        {
            api.CheckWrite(api.WriteFunction<ModelName>("St7AddLSACombination")(Id,name),"St7AddLSACombination");
            for(int i=0;i<cases.Length;i++) api.CheckWrite(api.WriteFunction<ModelCombination>("St7SetLSACombinationFactor")(Id,0,id,cases[i],freedomCase,factors[i]),"St7SetLSACombinationFactor");
        }
        /// <summary>Dimensionless displacement and residual convergence tolerances for nonlinear iterations.</summary>
        public void SetNonlinearConvergence(double displacementTolerance,double residualTolerance,int maximumIterations)
        {
            if(!(displacementTolerance>0&&displacementTolerance<1)||!(residualTolerance>0&&residualTolerance<1)||maximumIterations<1)throw new ArgumentException("InvalidNonlinearConvergenceControls");
            api.CheckWrite(api.WriteFunction<ModelScalar>("St7SetSolverDefaultsDouble")(Id,4,displacementTolerance),"St7SetSolverDefaultsDouble/displacement");
            api.CheckWrite(api.WriteFunction<ModelScalar>("St7SetSolverDefaultsDouble")(Id,5,residualTolerance),"St7SetSolverDefaultsDouble/residual");
            api.CheckWrite(api.WriteFunction<MovingThree>("St7SetSolverDefaultsInteger")(Id,5,maximumIterations),"St7SetSolverDefaultsInteger/iterations");
        }
        /// <summary>Runs only this newly created output. Existing result files are never overwritten.</summary>
        public string Solve(bool stagedNonlinear=false,bool nonlinearGeometry=false,bool nonlinearMaterial=false)
        {
            string result=System.IO.Path.ChangeExtension(OutputPath,stagedNonlinear?"nla":"lsa");
            if(System.IO.File.Exists(result))throw new System.IO.IOException("RefusingToOverwriteStrausResults");
            api.CheckWrite(api.WriteFunction<StageBool>("St7SetSolverNonlinearGeometry")(Id,(byte)(nonlinearGeometry?1:0)),"St7SetSolverNonlinearGeometry");
            api.CheckWrite(api.WriteFunction<StageBool>("St7SetSolverNonlinearMaterial")(Id,(byte)(nonlinearMaterial?1:0)),"St7SetSolverNonlinearMaterial");
            if(!stagedNonlinear)
            {
                if(api.StageCount!=0)throw new InvalidOperationException("StagedModelRequiresExplicitStagedSolve");
                if(api.FreedomCaseCount!=1)throw new NotSupportedException("LinearSolveRequiresExplicitFreedomCasePairing");
                for(int i=1;i<=api.LoadCaseCount;i++) api.CheckWrite(api.WriteFunction<MovingThree>("St7EnableLSALoadCase")(Id,i,1),"St7EnableLSALoadCase");
            }
            else if(!api.ReadStagedAnalysis())throw new InvalidOperationException("StagedAnalysisDefinitionRequired");
            Save();api.CheckWrite(api.WriteFunction<Four>("St7RunSolver")(Id,stagedNonlinear?3:1,3,1),"St7RunSolver");
            if(!System.IO.File.Exists(result))throw new InvalidOperationException("StrausSolverProducedNoResults");
            if(api.ValidateResults(result).ValidationFlags!=0)throw new InvalidOperationException("StrausSolverProducedIncompatibleResults");
            var opened=api.OpenResults(result);
            try
            {
                int expected=stagedNonlinear?Enumerable.Range(1,api.StageCount).Where(api.ReadStageEnabled).Sum(api.ReadStageIncrementCount):api.LoadCaseCount;
                if(opened.PrimaryCases<expected)throw new InvalidOperationException("StrausSolverProducedIncompleteResults");
                if(stagedNonlinear&&Enumerable.Range(1,opened.PrimaryCases).Any(c=>!api.ReadResultConverged(c)))throw new InvalidOperationException("StrausNonconvergedResult");
            }
            finally {api.CloseResults();}
            return result;
        }
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int ModelScalar(int file,int parameter,double value);
        [UnmanagedFunctionPointer(CallingConvention.Winapi,CharSet=CharSet.Ansi)] private delegate int ModelName(int a,string name);
        [UnmanagedFunctionPointer(CallingConvention.Winapi,CharSet=CharSet.Ansi)] private delegate int ModelGroup(int a,int parent,string name,ref int id);
        [UnmanagedFunctionPointer(CallingConvention.Winapi,CharSet=CharSet.Ansi)] private delegate int ModelProperty(int a,int id,int kind,string name);
        [UnmanagedFunctionPointer(CallingConvention.Winapi,CharSet=CharSet.Ansi)] private delegate int ModelPlateProperty(int a,int id,int kind,int material,string name);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int ModelValues(int a,int b,[In,Out] double[] values);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int ModelSection(int a,int b,int[] flags,double[] values);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int ModelElement(int a,int b,int c,int d,int[] nodes);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int ModelRelease(int a,int b,int c,int[] flags,double[] values);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int ModelRestraint(int a,int b,int c,int d,int[] flags,double[] values);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int ModelDistributedMoment(int a,int b,int c,int d,int e,int f,double[] values);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int ModelDistributedForce(int a,int b,int c,int d,int e,int f,int g,double[] values);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int ModelCombination(int a,int b,int c,int d,int e,double value);
    }
}
