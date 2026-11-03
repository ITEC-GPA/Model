using System.Runtime.InteropServices;

namespace GPC.Converter.Straus7
{
    /// <summary>Readback of physical attributes, in native model units and SDK integer conventions.</summary>
    public interface IStraus7PhysicalReadApi
    {
        double ReadBeamInitialState(int beam,int loadCase,out int kind);
        double[] ReadBeamTaper(int beam,int axis,out int alignment);
        double[] ReadBeamSupport(int beam,int direction,int freedomCase,out bool compressionOnly);
        double[] ReadPlateInitialStress(int plate,int loadCase,out int kind);
        double[] ReadPlateSupport(int plate,int surface,int freedomCase,out int[] flags);
    }
    public sealed partial class Straus7NativeApi : IStraus7PhysicalReadApi
    {
        public int ReadBeamStringGroup(int beam)
        {int value=0;Check(Call<StringGroupRead>("St7GetBeamStringGroup1")(FileId,beam,ref value),"St7GetBeamStringGroup1");return value;}
        public double[] ReadLoadPatch(int plate,out int kind,out int edgeBits)
        {kind=0;edgeBits=0;var values=new double[4];Check(Call<LoadPatchRead>("St7GetPlateLoadPatch4")(FileId,plate,ref kind,ref edgeBits,values),"St7GetPlateLoadPatch4");return values;}
        public double ReadBeamInitialState(int beam,int loadCase,out int kind) => ReadPhysical("St7GetBeamPreLoad1",beam,loadCase,1,out kind)[0];
        public double[] ReadBeamTaper(int beam,int axis,out int alignment) => ReadPhysical("St7GetBeamTaper2",beam,axis,2,out alignment);
        public double[] ReadPlateInitialStress(int plate,int loadCase,out int kind) => ReadPhysical("St7GetPlatePreLoad3",plate,loadCase,3,out kind);
        private double[] ReadPhysical(string operation,int element,int scope,int size,out int kind)
        {
            var values=new double[size]; int type=0;
            Check(Call<PhysicalRead>(operation)(FileId,element,scope,ref type,values),operation); kind=type; return values;
        }
        public double[] ReadBeamSupport(int beam,int direction,int freedomCase,out bool compressionOnly)
        {
            var values=new double[2]; int flag=0;
            Check(Call<BeamSupportRead>("St7GetBeamSupport2")(FileId,beam,direction,freedomCase,ref flag,values),"St7GetBeamSupport2"); compressionOnly=flag!=0; return values;
        }
        public double[] ReadPlateSupport(int plate,int surface,int freedomCase,out int[] flags)
        {
            var values=new double[4]; flags=new int[2];
            Check(Call<PlateSupportRead>("St7GetPlateFaceSupport4")(FileId,plate,surface,freedomCase,flags,values),"St7GetPlateFaceSupport4"); return values;
        }
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int PhysicalRead(int a,int b,int c,ref int kind,[Out] double[] values);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int BeamSupportRead(int a,int b,int c,int d,ref int flag,[Out] double[] values);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int PlateSupportRead(int a,int b,int c,int d,[Out] int[] flags,[Out] double[] values);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int StringGroupRead(int a,int b,ref int group);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int LoadPatchRead(int a,int b,ref int kind,ref int edges,[Out] double[] values);
    }
}
