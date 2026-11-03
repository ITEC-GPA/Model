using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Runtime.InteropServices;
using GPC.Geometry;
using GPC.Model.Sections;
using GPC.Model.ElementProperties;
using GPC.Model.Materials;

namespace GPC.Converter.Straus7
{
    /// <summary>Detached native geometry, in mm. Standard/BGL dimensions or embedded BXS bytes.
    /// Libraries are resolved once; subsequent exports require neither a library nor a source BXS path.
    /// The explicit binary codec retains IEEE double bits and rejects unknown schemas.</summary>
    public sealed class Straus7SectionDefinition
    {
        public int Shape { get; }
        public bool IsBgl { get; }
        public string Source { get; }
        private readonly double[] dimensions,reference;
        private readonly byte[] bxs;
        public double[] Dimensions=>(double[])dimensions.Clone();
        public double[] ReferenceProperties=>(double[])reference.Clone();
        public byte[] BxsData=>(byte[])bxs.Clone();
        public bool IsResolved=>reference.Length==11;
        internal Straus7SectionDefinition(int shape,bool bgl,double[] dimensions,byte[] bxs,double[] reference,string source)
        {
            Shape=shape;IsBgl=bgl;this.dimensions=(double[])(dimensions??Array.Empty<double>()).Clone();
            this.bxs=(byte[])(bxs??Array.Empty<byte>()).Clone();this.reference=(double[])(reference??Array.Empty<double>()).Clone();Source=source??"";
            if(Source.Length>65536||this.bxs.Length>64*1024*1024||this.reference.Length!=0&&this.reference.Length!=11
                ||this.dimensions.Concat(this.reference).Any(v=>double.IsNaN(v)||double.IsInfinity(v)))throw new ArgumentException("InvalidNativeSectionDefinition");
            if(this.reference.Length==11&&(this.reference[0]<=0||this.reference.Skip(1).Take(3).Any(v=>v<0)))throw new ArgumentException("NativeSectionHasNoPhysicalProperties");
            if(this.bxs.Length!=0){if(shape!=11||bgl||this.dimensions.Length!=0)throw new ArgumentException("InvalidBxsSectionDefinition");}
            else if(this.dimensions.Length!=(bgl?16:6)||shape<1||shape>(bgl?6:16)||!bgl&&shape==11)throw new ArgumentException("InvalidNativeSectionDimensions");
        }
        public static Straus7SectionDefinition Standard(int shape,double[] dimensions)=>new Straus7SectionDefinition(shape,false,dimensions,null,null,"standard");
        public static Straus7SectionDefinition Bgl(int shape,double[] dimensions)=>new Straus7SectionDefinition(shape,true,dimensions,null,null,"BGL geometry");
        public static Straus7SectionDefinition CaptureBxs(string path)=>new Straus7SectionDefinition(11,false,null,File.ReadAllBytes(path),null,"BXS/"+Path.GetFileName(path));
        public ElasticBeamProperty ToBeamProperty(string name,Material material)
        {
            if(!IsResolved)throw new InvalidOperationException("NativeSectionMustBeResolved");
            // Native section-property slots: A,I11,I22,J,SL1,SL2,SA1,SA2,Xbar,Ybar,principal angle [rad].
            double angle=reference[10],c=Math.Cos(angle),s=Math.Sin(angle),i1=reference[1],i2=reference[2];
            var section=new Section(reference[0],i1,i2,reference[3],0,
                new Point2d(reference[8],reference[9]),new Point3d(reference[8]+reference[4]*c-reference[5]*s,reference[9]+reference[4]*s+reference[5]*c,0),angle,name);
            return new ElasticBeamProperty(name,material,section,reference[6],reference[7]);
        }
        public byte[] Save()
        {
            using(var stream=new MemoryStream())using(var writer=new BinaryWriter(stream,Encoding.UTF8,true))
            {
                writer.Write(0x53435047);writer.Write(1);writer.Write(Shape);writer.Write(IsBgl);writer.Write(Source);
                writer.Write(dimensions.Length);foreach(double d in dimensions)writer.Write(d);
                writer.Write(reference.Length);foreach(double d in reference)writer.Write(d);
                writer.Write(bxs.Length);writer.Write(bxs);writer.Flush();return stream.ToArray();
            }
        }
        public static Straus7SectionDefinition Load(byte[] data)
        {
            if(data==null||data.Length>65*1024*1024)throw new ArgumentException("NativeSectionArchiveSize");
            using(var stream=new MemoryStream(data,false))using(var reader=new BinaryReader(stream,Encoding.UTF8,true))
            {
                if(reader.ReadInt32()!=0x53435047||reader.ReadInt32()!=1)throw new InvalidDataException("UnsupportedNativeSectionSchema");
                int shape=reader.ReadInt32();bool bgl=reader.ReadBoolean();string source=reader.ReadString();
                int n=reader.ReadInt32();if(n!=0&&n!=6&&n!=16)throw new InvalidDataException("InvalidNativeSectionDimensions");
                var dimensions=Enumerable.Range(0,n).Select(_=>reader.ReadDouble()).ToArray();
                n=reader.ReadInt32();if(n!=0&&n!=11)throw new InvalidDataException("InvalidNativeSectionProperties");
                var reference=Enumerable.Range(0,n).Select(_=>reader.ReadDouble()).ToArray();
                n=reader.ReadInt32();if(n<0||n>64*1024*1024||n!=stream.Length-stream.Position)throw new InvalidDataException("InvalidBxsPayloadSize");
                return new Straus7SectionDefinition(shape,bgl,dimensions,reader.ReadBytes(n),reference,source);
            }
        }
        public string Fingerprint(){using(var hash=SHA256.Create())return BitConverter.ToString(hash.ComputeHash(Save())).Replace("-","");}
    }

    public static class Straus7SectionLibrary
    {
        public static Straus7SectionDefinition Resolve(Straus7SectionDefinition definition,string scratchDirectory)
        {
            if(definition==null)throw new ArgumentNullException(nameof(definition));
            using(var writer=Scratch(scratchDirectory))
            {
                writer.CreateSectionProperty();
                if(definition.IsResolved)writer.SetNativeSection(1,definition,definition.ReferenceProperties);else writer.AssignNativeSection(1,definition);
                return writer.Reader.ReadExpandedSection(1,scratchDirectory,definition.Source);
            }
        }
        public static Straus7SectionDefinition ResolveLibrary(bool bgl,string library,string item,string scratchDirectory)
        {
            if(string.IsNullOrWhiteSpace(library)||string.IsNullOrWhiteSpace(item))throw new ArgumentException("MissingSectionLibraryReference");
            using(var writer=Scratch(scratchDirectory))
            {writer.CreateSectionProperty();writer.AssignSectionLibrary(bgl,library,item);return writer.Reader.ReadExpandedSection(1,scratchDirectory,(bgl?"BGL/":"BSL/")+library+"/"+item);}
        }
        private static Straus7NativeWriter Scratch(string directory)
        {
            directory=Path.GetFullPath(directory);if(!Directory.Exists(directory))throw new DirectoryNotFoundException(directory);
            return new Straus7NativeWriter(Path.Combine(directory,"section-resolve-"+Guid.NewGuid().ToString("N")+".st7"),directory);
        }
    }

    public interface IStraus7NativeSectionWriteApi
    {
        void SetNativeSection(int property,Straus7SectionDefinition definition,double[] referenceProperties);
    }
    public sealed partial class Straus7NativeWriter : IStraus7NativeSectionWriteApi
    {
        internal void CreateSectionProperty()=>api.CheckWrite(api.WriteFunction<ModelProperty>("St7NewBeamProperty")(Id,1,6,"resolved"),"St7NewBeamProperty");
        internal void AssignSectionLibrary(bool bgl,string libraryName,string itemName)
        {
            int library=0,item=0,type=bgl?6:1;
            api.CheckWrite(api.WriteFunction<LibraryId>("St7GetLibraryID")(type,libraryName,ref library),"St7GetLibraryID");
            api.CheckWrite(api.WriteFunction<LibraryItemId>("St7GetLibraryItemID")(type,library,itemName,ref item),"St7GetLibraryItemID");
            string op=bgl?"St7AssignLibraryBeamSectionBGL":"St7AssignLibraryBeamSection";
            api.CheckWrite(api.WriteFunction<SectionLibraryAssign>(op)(Id,1,library,item,new[]{0,1,0,0}),op);
        }
        internal void AssignNativeSection(int property,Straus7SectionDefinition definition)
        {
            byte[] bxs=definition.BxsData;
            if(bxs.Length!=0)
            {
                string path=Path.Combine(Path.GetDirectoryName(OutputPath),"section-input-"+Guid.NewGuid().ToString("N")+".bxs");
                try{using(var stream=new FileStream(path,FileMode.CreateNew,FileAccess.Write))stream.Write(bxs,0,bxs.Length);
                    api.CheckWrite(api.WriteFunction<SectionAssign>("St7AssignBXS")(Id,property,path),"St7AssignBXS");}
                finally{if(File.Exists(path))File.Delete(path);}
            }
            else
            {
                string op=definition.IsBgl?"St7SetBeamSectionGeometryBGL":"St7SetBeamSectionGeometry";
                api.CheckWrite(api.WriteFunction<ThreeDoubles>(op)(Id,property,definition.Shape,definition.Dimensions),op);
                if(!definition.IsBgl)api.CheckWrite(api.WriteFunction<SectionCalculate>("St7CalculateBeamSectionProperties")(Id,property,1),"St7CalculateBeamSectionProperties");
            }
        }
        public void SetNativeSection(int property,Straus7SectionDefinition definition,double[] referenceProperties)
        {
            if(definition==null||!definition.IsResolved||referenceProperties==null||referenceProperties.Length!=11)throw new ArgumentException("InvalidResolvedNativeSection");
            AssignNativeSection(property,definition);
            api.CheckWrite(api.WriteFunction<ModelSection>("St7SetBeamSectionPropertyData")(Id,property,new[]{0},referenceProperties),"St7SetBeamSectionPropertyData");
            api.CheckWrite(api.WriteFunction<MovingThree>("St7UpdateElementPropertyData")(Id,1,property),"St7UpdateElementPropertyData");
        }
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int SectionLibraryAssign(int a,int b,int c,int d,int[] flags);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int SectionCalculate(int a,int b,byte shear);
    }
    public sealed partial class Straus7NativeApi
    {
        /// <summary>Captures a section from the open N-mm model; a BXS export uses an owned temporary file in scratchDirectory.</summary>
        public Straus7SectionDefinition ReadSectionDefinition(int property,string scratchDirectory)
        {
            if(ReadUnits()[0]!=2)throw new NotSupportedException("NativeSectionSnapshotRequiresMillimetres");
            return ReadExpandedSection(property,scratchDirectory,"model property/"+property);
        }
        internal Straus7SectionDefinition ReadExpandedSection(int property,string scratch,string source)
        {
            int type=0;var dimensions=new double[6];Check(Call<SectionCall>("St7GetBeamSectionGeometry")(FileId,property,ref type,dimensions),"St7GetBeamSectionGeometry");
            if(type!=1&&type!=2)
            {
                int mirror=0,twist=0;var gaps=new double[2];Check(Call<MirrorCall>("St7GetBeamMirrorOption")(FileId,property,ref mirror,ref twist,gaps),"St7GetBeamMirrorOption");
                if(mirror!=0)throw new NotSupportedException("NativeSectionMirrorRequiresExplicitExpandedPhysicalGeometry");
            }
            var integers=new int[4];var values=new double[20];Check(Call<SectionData>("St7GetBeamSectionPropertyData")(FileId,property,integers,values),"St7GetBeamSectionPropertyData");
            bool bgl=type==17;byte[] payload=null;
            if(bgl){dimensions=new double[16];Check(Call<SectionCall>("St7GetBeamSectionGeometryBGL")(FileId,property,ref type,dimensions),"St7GetBeamSectionGeometryBGL");}
            else if(type==11)
            {
                string path=Path.Combine(Path.GetFullPath(scratch),"section-snapshot-"+Guid.NewGuid().ToString("N")+".bxs");
                try{Check(Call<SectionExport>("St7ExportBXS")(FileId,property,path),"St7ExportBXS");payload=File.ReadAllBytes(path);dimensions=null;}
                finally{if(File.Exists(path))File.Delete(path);}
            }
            return new Straus7SectionDefinition(type,bgl,dimensions,payload,values.Take(11).ToArray(),source+"; "+Version);
        }
        public System.Collections.Generic.IReadOnlyDictionary<string,string[]> SectionLibraryCatalog(bool bgl)
        {
            int type=bgl?6:1,count=0;Check(Call<LibraryCount>("St7GetNumLibraries")(type,ref count),"St7GetNumLibraries");
            var result=new System.Collections.Generic.Dictionary<string,string[]>(StringComparer.Ordinal);
            for(int library=1;library<=count;library++)
            {
                var name=new StringBuilder(1024);Check(Call<LibraryName>("St7GetLibraryName")(type,library,name,name.Capacity),"St7GetLibraryName");
                int items=0;Check(Call<LibraryItems>("St7GetNumLibraryItems")(type,library,ref items),"St7GetNumLibraryItems");var names=new string[items];
                for(int item=1;item<=items;item++){var text=new StringBuilder(1024);Check(Call<LibraryItemName>("St7GetLibraryItemName")(type,library,item,text,text.Capacity),"St7GetLibraryItemName");names[item-1]=text.ToString();}
                result.Add(name.ToString(),names);
            }
            return result;
        }
        [UnmanagedFunctionPointer(CallingConvention.Winapi,CharSet=CharSet.Ansi)] private delegate int SectionExport(int a,int b,string path);
    }
}
