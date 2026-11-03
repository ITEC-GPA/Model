using System;

namespace GPC.Converter.Straus7
{
    /// <summary>Dimensional beam geometry required by Straus7 taper attributes. N-mm units.</summary>
    public interface IStraus7SectionGeometryWriteApi
    {
        void SetBeamSectionGeometry(int property,int shape,double[] dimensions,double[] referenceProperties);
        void SetBeamIntegrationSlices(int property,int slices,double[] referenceProperties);
    }

    public sealed partial class Straus7NativeWriter : IStraus7SectionGeometryWriteApi
    {
        public void SetBeamIntegrationSlices(int property,int slices,double[] referenceProperties)
        {
            if(slices<2||slices>10||referenceProperties==null||referenceProperties.Length!=11)throw new ArgumentException("InvalidBeamIntegrationSlices");
            api.CheckWrite(api.WriteFunction<ModelSection>("St7SetBeamSectionPropertyData")(Id,property,new[]{slices},referenceProperties),"St7SetBeamSectionPropertyData");
            api.CheckWrite(api.WriteFunction<MovingThree>("St7UpdateElementPropertyData")(Id,1,property),"St7UpdateElementPropertyData");
        }
        public void SetBeamSectionGeometry(int property,int shape,double[] dimensions,double[] referenceProperties)
        {
            if(dimensions==null||dimensions.Length!=6||referenceProperties==null||referenceProperties.Length!=11)throw new ArgumentException("InvalidBeamSectionGeometryArrays");
            api.CheckWrite(api.WriteFunction<ThreeDoubles>("St7SetBeamSectionGeometry")(Id,property,shape,dimensions),"St7SetBeamSectionGeometry");
            // Geometry must not replace the explicitly supplied GPC rigidities or shear areas.
            api.CheckWrite(api.WriteFunction<ModelSection>("St7SetBeamSectionPropertyData")(Id,property,new[]{0},referenceProperties),"St7SetBeamSectionPropertyData");
            api.CheckWrite(api.WriteFunction<MovingThree>("St7UpdateElementPropertyData")(Id,1,property),"St7UpdateElementPropertyData");
        }
    }
}
