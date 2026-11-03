using System;
using System.IO;
using System.Runtime.InteropServices;
using GPC.Model.Structure.Assignments;

namespace GPC.Converter.Straus7
{
    public sealed partial class Straus7NativeApi
    {
        private bool writable;
        internal void CreateWritable(string output, string scratch, string source)
        {
            if(modelOpen) throw new InvalidOperationException("A model is already open.");
            if(!Path.IsPathRooted(output)||!Path.IsPathRooted(scratch)) throw new ArgumentException("Absolute output and scratch paths required.");
            if(File.Exists(output)) throw new IOException("Refusing to overwrite an existing Straus7 model.");
            if(source!=null) File.Copy(source,output,false);
            Check(Call<OpenModel>(source==null?"St7NewFile":"St7OpenFile")(FileId,output,scratch),source==null?"St7NewFile":"St7OpenFile");
            modelOpen=true; writable=true;
        }
        internal T WriteFunction<T>(string operation) where T:class
        { if(!modelOpen||!writable) throw new InvalidOperationException("Straus7ReadOnlySession"); return Call<T>(operation); }
        internal void CheckWrite(int code,string operation) => Check(code,operation);
    }

    /// <summary>Native attribute writer sharing one runtime owner with its readback API. The input model is copied to a new output;
    /// existing outputs are never overwritten. Dispose releases this session only. No writes are exposed on the public read API.</summary>
    public sealed partial class Straus7NativeWriter : IStraus7WriteApi, IDisposable
    {
        private readonly Straus7NativeApi api;
        private const int Id=1;
        public Straus7NativeApi Reader => api;
        public string OutputPath { get; }
        public Straus7NativeWriter(string outputPath,string scratchPath,string sourceModelPath=null,string libraryPath=null)
        {
            OutputPath=Path.GetFullPath(outputPath); api=new Straus7NativeApi(libraryPath);
            try {
                api.CreateWritable(OutputPath,Path.GetFullPath(scratchPath),sourceModelPath);
                if(sourceModelPath==null) api.CheckWrite(api.WriteFunction<Units>("St7SetUnits")(Id,new[]{2,0,2,1,0,0}),"St7SetUnits");
                var u=api.ReadUnits();
                if(u[0]!=2||u[1]!=0||u[2]!=2||u[3]!=1||u[4]!=0) throw new NotSupportedException("StrausPhysicalWriterRequiresNmmTonneCelsius");
            } catch { api.Dispose(); throw; }
        }
        public void Save() => api.CheckWrite(api.WriteFunction<One>("St7SaveFile")(Id),"St7SaveFile");
        public void SetNodeTemperature(int node,int loadCase,NodalTemperatureKind kind,double celsius)
        {
            if(!Enum.IsDefined(typeof(NodalTemperatureKind),kind)) throw new ArgumentOutOfRangeException(nameof(kind));
            Three("St7SetNodeTemperature1",node,loadCase,new[]{celsius});
            api.CheckWrite(api.WriteFunction<Four>("St7SetNodeTemperatureType1")(Id,node,loadCase,(int)kind),"St7SetNodeTemperatureType1");
        }
        public void SetBeamTemperatureGradient(int beam,int loadCase,double axis1,double axis2) => Three("St7SetBeamTempGradient2",beam,loadCase,new[]{axis1,axis2});
        public void SetBeamInitialState(int beam,int loadCase,BeamInitialStateKind kind,double value)
        {
            if(!Enum.IsDefined(typeof(BeamInitialStateKind),kind)) throw new ArgumentOutOfRangeException(nameof(kind));
            FourValues("St7SetBeamPreLoad1",beam,loadCase,(int)kind,new[]{value});
        }
        public void SetBeamSupport(int beam,int direction,int freedomCase,bool compressionOnly,double stiffness,double gap) =>
            api.CheckWrite(api.WriteFunction<FiveDoubles>("St7SetBeamSupport2")(Id,beam,direction,freedomCase,compressionOnly?1:0,new[]{stiffness,gap}),"St7SetBeamSupport2");
        public void SetBeamTaper(int beam,int axis,TaperAlignment alignment,double atI,double atJ)
        {
            int code=alignment==TaperAlignment.Symmetric?0:alignment==TaperAlignment.PositiveSide?1:alignment==TaperAlignment.NegativeSide?2:throw new ArgumentOutOfRangeException(nameof(alignment));
            FourValues("St7SetBeamTaper2",beam,axis,code,new[]{atI,atJ});
        }
        public void SetPlateInitialStress(int plate,int loadCase,double sigma11,double sigma22,double sigma33) => FourValues("St7SetPlatePreLoad3",plate,loadCase,0,new[]{sigma11,sigma22,sigma33});
        public void SetPlateNonStructuralMass(int plate,int loadCase,double massPerArea,double dynamicFactor,double offsetX,double offsetY,double offsetZ) =>
            // R31 manual lists [0..5] but defines mass, factor and three global offsets; keep the spare slot allocated.
            FourValues("St7SetPlateNSMass5ID",plate,loadCase,1,new[]{massPerArea,dynamicFactor,offsetX,offsetY,offsetZ,0});
        public void SetPlateSupport(int plate,ShellFace face,int freedomCase,bool compressionOnly,double stiffness,double gap)
        {
            if(!Enum.IsDefined(typeof(ShellFace),face)) throw new ArgumentOutOfRangeException(nameof(face));
            api.CheckWrite(api.WriteFunction<Support>("St7SetPlateFaceSupport4")(Id,plate,face==ShellFace.PositiveNormal?2:1,freedomCase,
                new[]{compressionOnly?1:0,0},new[]{stiffness,0,gap,0}),"St7SetPlateFaceSupport4");
        }
        private void Three(string operation,int element,int loadCase,double[] values) => api.CheckWrite(api.WriteFunction<ThreeDoubles>(operation)(Id,element,loadCase,values),operation);
        private void FourValues(string operation,int element,int caseOrAxis,int option,double[] values) => api.CheckWrite(api.WriteFunction<FourDoubles>(operation)(Id,element,caseOrAxis,option,values),operation);
        public void Dispose() => api.Dispose();
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int One(int a);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int Four(int a,int b,int c,int d);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int Units(int a,int[] values);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int ThreeDoubles(int a,int b,int c,double[] values);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int FourDoubles(int a,int b,int c,int d,double[] values);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int FiveDoubles(int a,int b,int c,int d,int e,double[] values);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int Support(int a,int b,int c,int d,int[] status,double[] values);
    }
}
