using System;
using System.Runtime.InteropServices;
using GPC.Model.Materials;

namespace GPC.Converter.Straus7
{
    public interface IStraus7MaterialWriteApi
    {
        void SetMaterialLaw(int family,int property,int response,int hardening,double[] strainStressPairs);
        void SetCutoffLaw(int property,int failure,bool keepMass,double tensionLimit,double compressionLimit);
    }
    public sealed partial class Straus7ModelWritePlan
    {
        private sealed partial class Builder
        {
            internal bool RequiresMaterialApi;
            private void MaterialLaw(int id,int family,Material material)
            {
                var data=material?.AnalysisData;if(data?.StressStrainPairs==null)return;data.Validate();
                if(material.Orthotropy!=null)throw new NotSupportedException("UniaxialTableCannotDefineOrthotropicNonlinearity");
                var pairs=(double[])data.StressStrainPairs.Clone();int response=(int)data.NonlinearResponse,hardening=(int)data.Hardening;
                RequiresMaterialApi=true;
                Operations.Add(a=>((IStraus7MaterialWriteApi)a).SetMaterialLaw(family,id,response,hardening,(double[])pairs.Clone()));
            }
        }
    }
    public sealed partial class Straus7NativeWriter:IStraus7MaterialWriteApi
    {
        public void SetCutoffLaw(int property,int failure,bool keepMass,double tensionLimit,double compressionLimit) =>
            api.CheckWrite(api.WriteFunction<ModelSection>("St7SetCutoffBarData")(Id,property,new[]{failure,keepMass?1:0},new[]{tensionLimit,compressionLimit}),"St7SetCutoffBarData");
        public void SetMaterialLaw(int family,int property,int response,int hardening,double[] strainStressPairs)
        {
            if(family!=1&&family!=2)throw new ArgumentOutOfRangeException(nameof(family));
            int count=0,last=0;api.CheckWrite(api.WriteFunction<MaterialTables>("St7GetNumTables")(Id,4,ref count,ref last),"St7GetNumTables");int table=last+1;
            api.CheckWrite(api.WriteFunction<MaterialNewTable>("St7NewTableType")(Id,4,table,strainStressPairs.Length/2,"GPC material "+family+"/"+property,strainStressPairs),"St7NewTableType");
            api.CheckWrite(api.WriteFunction<Four>("St7SetPropertyTable")(Id,family==1?1011:2007,property,table),"St7SetPropertyTable");
            string op=family==1?"St7SetBeamNonlinearType":"St7SetPlateNonlinearType";
            api.CheckWrite(api.WriteFunction<Four>(op)(Id,property,response,family==1?2:1),op); // SDK uses different von Mises codes for beams and plates.
            api.CheckWrite(api.WriteFunction<Four>("St7SetHardeningType")(Id,family,property,hardening),"St7SetHardeningType");
        }
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int MaterialTables(int file,int type,ref int count,ref int last);
        [UnmanagedFunctionPointer(CallingConvention.Winapi,CharSet=CharSet.Ansi)] private delegate int MaterialNewTable(int file,int type,int table,int count,string name,double[] values);
    }
    public sealed partial class Straus7NativeApi
    {
        public double[] ReadCutoffLaw(int property,out int failure,out bool keepMass)
        {
            var flags=new int[2];var values=new double[2];Check(Call<CutoffRead>("St7GetCutoffBarData")(FileId,property,flags,values),"St7GetCutoffBarData");
            failure=flags[0];keepMass=flags[1]!=0;return values;
        }
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int CutoffRead(int file,int property,int[] flags,double[] values);
        public double[] ReadMaterialLaw(int family,int property,out int response,out int hardening)
        {
            if(family!=1&&family!=2)throw new ArgumentOutOfRangeException(nameof(family));int table=0;
            Check(Call<LibraryForceCount>("St7GetPropertyTable")(FileId,family==1?1011:2007,property,ref table),"St7GetPropertyTable");
            int rows=0;Check(Call<LibraryForceCount>("St7GetNumTableTypeRows")(FileId,4,table,ref rows),"St7GetNumTableTypeRows");
            var values=new double[checked(2*rows)];Check(Call<MaterialTableRead>("St7GetTableTypeData")(FileId,4,table,rows,ref rows,values),"St7GetTableTypeData");
            response=0;hardening=0;int criterion=0;string op=family==1?"St7GetBeamNonlinearType":"St7GetPlateNonlinearType";
            Check(Call<MaterialLawRead>(op)(FileId,property,ref response,ref criterion),op);
            Check(Call<LibraryForceCount>("St7GetHardeningType")(FileId,family,property,ref hardening),"St7GetHardeningType");return values;
        }
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int MaterialTableRead(int file,int type,int table,int max,ref int rows,double[] values);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int MaterialLawRead(int file,int property,ref int response,ref int criterion);
    }
}
