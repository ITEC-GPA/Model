using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace GPC.Converter.Straus7
{
    public sealed partial class Straus7NativeApi
    {
        internal void CreateSectionModel(string path,string scratch)
            =>Check(WriteFunction<OpenModel>("St7NewFile")(2,path,scratch),"St7NewFile BXS");
        internal void CloseSectionModel()=>Check(WriteFunction<OneInt>("St7CloseFile")(2),"St7CloseFile BXS");
    }
    /// <summary>Explicit section regions triangulated in XY, in mm. Faces use zero-based vertex indices.
    /// Holes are represented by absent faces; disconnected regions are allowed. GPC section properties remain authoritative.</summary>
    public interface IStraus7SectionMeshWriteApi
    {
        void SetBeamSectionMesh(int property,double[][] vertices,int[][] faces,double[] referenceProperties);
    }

    public sealed partial class Straus7NativeWriter : IStraus7SectionMeshWriteApi
    {
        public void SetBeamSectionMesh(int property,double[][] vertices,int[][] faces,double[] referenceProperties)
        {
            if(property<1||vertices==null||vertices.Length<3||faces==null||faces.Length==0||referenceProperties==null||referenceProperties.Length!=11
                ||vertices.Any(v=>v==null||v.Length!=3||v.Any(x=>double.IsNaN(x)||double.IsInfinity(x))||v[2]!=0)
                ||faces.Any(f=>f==null||(f.Length!=3&&f.Length!=4)||f.Distinct().Count()!=f.Length||f.Any(i=>i<0||i>=vertices.Length)))
                throw new ArgumentException("InvalidStrausSectionMesh");
            // A second file ID shares this writer's sole runtime owner; no second St7Init/St7Release pair.
            const int sectionFile=2;
            string directory=Path.GetDirectoryName(OutputPath),stem=Path.Combine(directory,"gpc-section-"+Guid.NewGuid().ToString("N"));
            string modelPath=stem+".st7",bxsPath=stem+".bxs";bool opened=false;
            try
            {
                api.CreateSectionModel(modelPath,directory);opened=true;
                api.CheckWrite(api.WriteFunction<Units>("St7SetUnits")(sectionFile,new[]{2,0,2,1,0,0}),"St7SetUnits BXS");
                api.CheckWrite(api.WriteFunction<ModelPlateProperty>("St7NewPlateProperty")(sectionFile,1,4,1,"Section regions"),"St7NewPlateProperty BXS");
                for(int i=0;i<vertices.Length;i++)api.CheckWrite(api.WriteFunction<ModelValues>("St7SetNodeXYZ")(sectionFile,i+1,vertices[i]),"St7SetNodeXYZ BXS");
                for(int i=0;i<faces.Length;i++)
                {
                    var connection=new[]{faces[i].Length}.Concat(faces[i].Select(n=>n+1)).ToArray();
                    api.CheckWrite(api.WriteFunction<ModelElement>("St7SetElementConnection")(sectionFile,2,i+1,1,connection),"St7SetElementConnection BXS");
                }
                api.CheckWrite(api.WriteFunction<SectionGenerate>("St7GenerateBXS")(sectionFile,bxsPath,new double[39]),"St7GenerateBXS");
                api.CheckWrite(api.WriteFunction<SectionAssign>("St7AssignBXS")(Id,property,bxsPath),"St7AssignBXS");
                api.CheckWrite(api.WriteFunction<ModelSection>("St7SetBeamSectionPropertyData")(Id,property,new[]{0},referenceProperties),"St7SetBeamSectionPropertyData");
                api.CheckWrite(api.WriteFunction<MovingThree>("St7UpdateElementPropertyData")(Id,1,property),"St7UpdateElementPropertyData");
            }
            finally
            {
                try {if(opened)api.CloseSectionModel();}
                finally {if(File.Exists(bxsPath))File.Delete(bxsPath);if(File.Exists(modelPath))File.Delete(modelPath);}
            }
        }
        [UnmanagedFunctionPointer(CallingConvention.Winapi,CharSet=CharSet.Ansi)] private delegate int SectionGenerate(int file,string path,double[] data);
        [UnmanagedFunctionPointer(CallingConvention.Winapi,CharSet=CharSet.Ansi)] private delegate int SectionAssign(int file,int property,string path);
    }
}
