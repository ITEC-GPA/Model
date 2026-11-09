using GPC.Converter;
using GPC.Converter.Straus7;
using GPC.Model.Core;
using GPC.Model.ElementProperties;
using GPC.Model.Materials;
using GPC.Model.Models;
using GPC.Model.Loads;
using GPC.Model.Persistence;
using GPC.Model.Sections;
using GPC.Model.Structure.Assignments;
using System.Runtime.InteropServices;

namespace UnitTest;

[TestClass]
public class Straus7SectionDefinitionTest
{
    private static string Folder()
    {
        string root=Environment.GetEnvironmentVariable("GPC_STRAUS7_WRITE")!;
        if(string.IsNullOrWhiteSpace(root))Assert.Inconclusive("Set GPC_STRAUS7_WRITE to an artifact directory.");
        string folder=Path.Combine(root,"native-section-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(folder);return folder;
    }
    [TestMethod]
    public void SectionSnapshotCodecPreservesBitsAndRejectsInvalidPayloads()
    {
        var original=Straus7SectionDefinition.Standard(3,new[]{20d,10,BitConverter.Int64BitsToDouble(long.MinValue),0,0,0});
        var bytes=original.Save();var copy=Straus7SectionDefinition.Load(bytes);
        Assert.AreEqual(original.Fingerprint(),copy.Fingerprint());Assert.AreEqual(long.MinValue,BitConverter.DoubleToInt64Bits(copy.Dimensions[2]));
        var dimensions=copy.Dimensions;dimensions[0]=500;Assert.AreEqual(20,copy.Dimensions[0]);
        bytes[4]=2;Assert.ThrowsException<InvalidDataException>(()=>Straus7SectionDefinition.Load(bytes));
        Assert.ThrowsException<InvalidDataException>(()=>Straus7SectionDefinition.Load(original.Save().Concat(new byte[]{1}).ToArray()));
        Assert.ThrowsException<ArgumentException>(()=>Straus7SectionDefinition.Standard(3,new[]{double.NaN,1,0,0,0,0}));
    }
    /// <summary>A02b: schema 1 is frozen. The bytes are spelled out here, independently of the codec, with the SHA-256 they had before the mirror option.</summary>
    [TestMethod]
    public void ADefinitionWithoutAMirrorKeepsSchema1ItsBytesAndItsFingerprint()
    {
        var dimensions=new[]{150d,100,0,10,12,0};
        byte[] expected;
        using(var stream=new MemoryStream())using(var writer=new BinaryWriter(stream,System.Text.Encoding.UTF8,true))
        {
            writer.Write(0x53435047);writer.Write(1);writer.Write(9);writer.Write(false);writer.Write("standard");
            writer.Write(6);foreach(double d in dimensions)writer.Write(d);writer.Write(0);writer.Write(0);writer.Flush();expected=stream.ToArray();
        }
        const string frozen="A07015C17E2AE2D6015E6EDA5B691897501DE3710D96FE03ACC4911992E3FB22";
        var plain=Straus7SectionDefinition.Standard(9,dimensions);
        CollectionAssert.AreEqual(expected,plain.Save());Assert.AreEqual(frozen,plain.Fingerprint());
        // The mirror overload without a mirror is the same definition, whatever gaps and twist it is given.
        var none=Straus7SectionDefinition.Standard(9,dimensions,0,new[]{8d,3},true);
        CollectionAssert.AreEqual(expected,none.Save());Assert.AreEqual(frozen,none.Fingerprint());
        Assert.AreEqual(0,none.MirrorType);Assert.IsFalse(none.CompatibleTwist);CollectionAssert.AreEqual(new double[2],none.MirrorGaps);
        // Schema 1 still reads, without a mirror, and writes back the same bytes.
        var read=Straus7SectionDefinition.Load(expected);
        Assert.AreEqual(0,read.MirrorType);Assert.IsFalse(read.CompatibleTwist);CollectionAssert.AreEqual(new double[2],read.MirrorGaps);CollectionAssert.AreEqual(expected,read.Save());
    }
    /// <summary>A02b: schema 2 holds the mirror, keeps its bits and enters the fingerprint; invalid mirrors are refused by the factory and by the codec.</summary>
    [TestMethod]
    public void AMirroredDefinitionIsSchema2AndEntersTheFingerprint()
    {
        var dimensions=new[]{150d,100,0,10,12,0};double negativeZero=BitConverter.Int64BitsToDouble(long.MinValue);
        var mirrored=Straus7SectionDefinition.Standard(9,dimensions,3,new[]{8.5,negativeZero},true);
        var bytes=mirrored.Save();Assert.AreEqual(2,BitConverter.ToInt32(bytes,4));
        var copy=Straus7SectionDefinition.Load(bytes);
        Assert.AreEqual(3,copy.MirrorType);Assert.IsTrue(copy.CompatibleTwist);Assert.AreEqual(8.5,copy.MirrorGaps[0]);Assert.AreEqual(long.MinValue,BitConverter.DoubleToInt64Bits(copy.MirrorGaps[1]));
        CollectionAssert.AreEqual(bytes,copy.Save());Assert.AreEqual(mirrored.Fingerprint(),copy.Fingerprint());
        var gaps=copy.MirrorGaps;gaps[0]=99;Assert.AreEqual(8.5,copy.MirrorGaps[0]);
        // Each mirror datum moves the fingerprint, and a mirror differs from no mirror.
        var fingerprints=new[]
        {
            mirrored.Fingerprint(),Straus7SectionDefinition.Standard(9,dimensions).Fingerprint(),
            Straus7SectionDefinition.Standard(9,dimensions,4,new[]{8.5,negativeZero},true).Fingerprint(),
            Straus7SectionDefinition.Standard(9,dimensions,3,new[]{8.6,negativeZero},true).Fingerprint(),
            Straus7SectionDefinition.Standard(9,dimensions,3,new[]{8.5,1d},true).Fingerprint(),
            Straus7SectionDefinition.Standard(9,dimensions,3,new[]{8.5,negativeZero},false).Fingerprint(),
        };
        Assert.AreEqual(fingerprints.Length,fingerprints.Distinct().Count());
        // Invalid mirrors.
        foreach(int type in new[]{-1,5})Assert.ThrowsException<ArgumentException>(()=>Straus7SectionDefinition.Standard(9,dimensions,type,new[]{0d,0}));
        Assert.ThrowsException<ArgumentException>(()=>Straus7SectionDefinition.Standard(9,dimensions,3,new[]{1d}));
        Assert.ThrowsException<ArgumentException>(()=>Straus7SectionDefinition.Standard(9,dimensions,3,null));
        Assert.ThrowsException<ArgumentException>(()=>Straus7SectionDefinition.Standard(9,dimensions,3,new[]{double.NaN,0}));
        Assert.ThrowsException<ArgumentException>(()=>Straus7SectionDefinition.Standard(9,dimensions,3,new[]{0d,double.PositiveInfinity}));
        // The circular shapes have no native mirror option.
        foreach(int shape in new[]{1,2})Assert.ThrowsException<ArgumentException>(()=>Straus7SectionDefinition.Standard(shape,new[]{100d,0,0,5,0,0},3,new[]{0d,0}));
        // Schema 2 without a mirror, a mirror of an unknown type or an unknown schema are refused.
        foreach(int type in new[]{0,5})
        {
            var invalid=(byte[])bytes.Clone();BitConverter.GetBytes(type).CopyTo(invalid,invalid.Length-4-2*sizeof(double)-1-sizeof(int));
            Assert.ThrowsException<InvalidDataException>(()=>Straus7SectionDefinition.Load(invalid));
        }
        var unknown=(byte[])bytes.Clone();unknown[4]=3;Assert.ThrowsException<InvalidDataException>(()=>Straus7SectionDefinition.Load(unknown));
        Assert.ThrowsException<InvalidDataException>(()=>Straus7SectionDefinition.Load(bytes.Concat(new byte[]{1}).ToArray()));
    }
    [DllImport("kernel32",CharSet=CharSet.Unicode)] private static extern IntPtr GetModuleHandle(string name);
    [DllImport("kernel32",CharSet=CharSet.Ansi)] private static extern IntPtr GetProcAddress(IntPtr module,string name);
    [UnmanagedFunctionPointer(CallingConvention.Winapi,CharSet=CharSet.Ansi)] private delegate int NewFile(int uid,string path,string scratch);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int OneInt(int uid);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int Units(int uid,int[] units);
    [UnmanagedFunctionPointer(CallingConvention.Winapi,CharSet=CharSet.Ansi)] private delegate int NewProperty(int uid,int property,int type,string name);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int IntDoubles(int uid,int property,int value,double[] doubles);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int IntsDoubles(int uid,int property,int[] ints,double[] doubles);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int Doubles(int uid,int property,double[] doubles);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int Update(int uid,int type,int property);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int Calculate(int uid,int property,byte shear);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int SetMirror(int uid,int property,int type,int twist,double[] gaps);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int GetMirror(int uid,int property,ref int type,ref int twist,[Out] double[] gaps);
    [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int GetGeometry(int uid,int property,ref int type,[Out] double[] dimensions);
    private static T St7<T>(string name) where T:class=>Marshal.GetDelegateForFunctionPointer(GetProcAddress(GetModuleHandle("St7api.dll"),name),typeof(T)) as T;
    private static void Ok(int code,string what){if(code!=0)throw new InvalidOperationException(what+" returned "+code);}
    /// <summary>A02b: a double angle and a double channel written and read back through the Converter give the type, the dimensions, the mirror
    /// option and the section data of the call sequence of BridGHe 0.3.0 (StrausR3Helper at 95ab7c3: geometry, the section data of the single
    /// section, St7SetBeamMirrorOption with mtLeft, btFalse and the two gaps, material, update, St7CalculateBeamSectionProperties with the shear).</summary>
    [DataTestMethod,TestCategory("Straus7Native")]
    [DataRow(9,150d,100d,0d,10d,12d,0d,8d,0d)]
    [DataRow(9,150d,100d,0d,10d,12d,0d,8d,3d)]
    [DataRow(5,100d,300d,0d,10d,8d,8d,12d,0d)]
    [DataRow(5,100d,300d,0d,10d,8d,8d,0d,0d)]
    public void AMirroredStandardSectionIsTheOneOfTheFormerCallSequence(int shape,double d0,double d1,double d2,double d3,double d4,double d5,double gapA,double gapB)
    {
        const int mtLeft=3;string folder=Folder();var dimensions=new[]{d0,d1,d2,d3,d4,d5};var gaps=new[]{gapA,gapB};
        var resolved=Straus7SectionLibrary.Resolve(Straus7SectionDefinition.Standard(shape,dimensions,mtLeft,gaps),folder);
        Assert.AreEqual(shape,resolved.Shape);CollectionAssert.AreEqual(dimensions,resolved.Dimensions);
        Assert.AreEqual(mtLeft,resolved.MirrorType);Assert.IsFalse(resolved.CompatibleTwist);CollectionAssert.AreEqual(gaps,resolved.MirrorGaps);
        var restored=Straus7SectionDefinition.Load(resolved.Save());Assert.AreEqual(resolved.Fingerprint(),restored.Fingerprint());
        var single=Straus7SectionLibrary.Resolve(Straus7SectionDefinition.Standard(shape,dimensions),folder).ReferenceProperties;
        // The double section: twice the area of the single one.
        Assert.AreEqual(2*single[0],resolved.ReferenceProperties[0],1e-9*single[0]);
        var material=new Material("S",210000,.3,7.85e-9,1.2e-5);
        var model=ElasticExportContractsTest.Cantilever();var beam=model.BeamElements.Values.Single();beam.BeamProperty=restored.ToBeamProperty("double",material);
        var plan=Straus7ModelWritePlan.Prepare(model,null,null,new Dictionary<Guid,Straus7SectionDefinition>{{beam.Guid,restored}});
        string path=Path.Combine(folder,"double.st7");
        using(var writer=new Straus7NativeWriter(path,folder)){plan.Write(writer,model);writer.Save();}
        using(var reader=new Straus7NativeApi())
        {
            reader.OpenModelReadOnly(path,folder);
            var captured=reader.ReadSectionDefinition(1,folder);var property=reader.ReadProperty(Straus7Entity.Beam,1);
            Assert.AreEqual(shape,captured.Shape);CollectionAssert.AreEqual(dimensions,captured.Dimensions);
            Assert.AreEqual(mtLeft,captured.MirrorType);Assert.IsFalse(captured.CompatibleTwist);CollectionAssert.AreEqual(gaps,captured.MirrorGaps);
            Assert.AreEqual(mtLeft,property.MirrorType);CollectionAssert.AreEqual(gaps,property.MirrorGaps);Assert.AreEqual(false,property.CompatibleTwist);
            // 0.3.0 replayed on a scratch model: the section data written before the mirror are those of the single section.
            const int former=7;
            Ok(St7<NewFile>("St7NewFile")(former,Path.Combine(folder,"former.st7"),folder),"St7NewFile");
            try
            {
                Ok(St7<Units>("St7SetUnits")(former,new[]{2,0,2,1,0,0}),"St7SetUnits");
                Ok(St7<NewProperty>("St7NewBeamProperty")(former,1,6,"former"),"St7NewBeamProperty");
                Ok(St7<IntDoubles>("St7SetBeamSectionGeometry")(former,1,shape,dimensions),"St7SetBeamSectionGeometry");
                Ok(St7<IntsDoubles>("St7SetBeamSectionPropertyData")(former,1,new[]{0},single),"St7SetBeamSectionPropertyData");
                Ok(St7<SetMirror>("St7SetBeamMirrorOption")(former,1,mtLeft,0,gaps),"St7SetBeamMirrorOption");
                var m=new double[9];m[0]=210000;m[2]=.3;m[3]=7.85e-9;m[4]=1.2e-5;
                Ok(St7<Doubles>("St7SetBeamMaterialData")(former,1,m),"St7SetBeamMaterialData");
                Ok(St7<Update>("St7UpdateElementPropertyData")(former,1,1),"St7UpdateElementPropertyData");
                Ok(St7<Calculate>("St7CalculateBeamSectionProperties")(former,1,1),"St7CalculateBeamSectionProperties");
                int type=0,mirror=0,twist=0;var read=new double[6];var readGaps=new double[2];var expected=new double[20];
                Ok(St7<GetGeometry>("St7GetBeamSectionGeometry")(former,1,ref type,read),"St7GetBeamSectionGeometry");
                Ok(St7<GetMirror>("St7GetBeamMirrorOption")(former,1,ref mirror,ref twist,readGaps),"St7GetBeamMirrorOption");
                Ok(St7<IntsDoubles>("St7GetBeamSectionPropertyData")(former,1,new int[4],expected),"St7GetBeamSectionPropertyData");
                Assert.AreEqual(type,captured.Shape);CollectionAssert.AreEqual(read,captured.Dimensions);
                Assert.AreEqual(mirror,captured.MirrorType);Assert.AreEqual(twist,captured.CompatibleTwist?1:0);CollectionAssert.AreEqual(readGaps,captured.MirrorGaps);
                var data=captured.ReferenceProperties;
                double worst=Enumerable.Range(0,11).Max(j=>Math.Abs(data[j]-expected[j])/Math.Max(1,Math.Abs(expected[j])));
                Console.WriteLine("A02b shape "+shape+" gaps "+gapA+" "+gapB+": section data "+string.Join(" ",data.Select(v=>v.ToString("R",System.Globalization.CultureInfo.InvariantCulture)))
                    +"; former sequence "+string.Join(" ",expected.Take(11).Select(v=>v.ToString("R",System.Globalization.CultureInfo.InvariantCulture)))+"; worst relative difference "+worst.ToString("R",System.Globalization.CultureInfo.InvariantCulture));
                Assert.IsTrue(worst<=1e-12,"section data differ from the former sequence by "+worst);
            }
            finally{St7<OneInt>("St7CloseFile")(former);}
        }
    }
    /// <summary>A02b: Straus7 itself has no mirror for a BXS or BGL section: St7SetBeamMirrorOption refuses it (ERR7_SectionCannotBeMirrored, 67), so the refusal of
    /// ReadExpandedSection is only a guard, and such a section reads without a mirror.</summary>
    [DataTestMethod,DataRow(false),DataRow(true),TestCategory("Straus7Native")]
    public void Straus7HasNoMirrorForABxsOrBglSection(bool bgl)
    {
        const int sectionCannotBeMirrored=67;
        string folder=Folder(),path=Path.Combine(folder,"source.st7");
        var model=ElasticExportContractsTest.Cantilever();var beam=model.BeamElements.Values.Single();var material=new Material("S",210000,.3,0,0);
        Dictionary<Guid,Straus7SectionDefinition> native=null;
        if(bgl)
        {
            var resolved=Straus7SectionLibrary.Resolve(Straus7SectionCatalog.Get("BglRectangle").Create(new[]{100d,50,5,5,0,0}),folder);
            beam.BeamProperty=resolved.ToBeamProperty("bgl",material);native=new Dictionary<Guid,Straus7SectionDefinition>{{beam.Guid,resolved}};
        }
        else beam.BeamProperty=new ElasticBeamProperty("bxs",material,new SectionRectangular(20,10,.17));
        using(var writer=new Straus7NativeWriter(path,folder)){Straus7ModelWritePlan.Prepare(model,null,null,native).Write(writer,model);writer.Save();}
        using(var reader=new Straus7NativeApi())
        {
            reader.OpenModelReadOnly(path,folder);Assert.AreEqual(0,reader.ReadSectionDefinition(1,folder).MirrorType);reader.CloseModel();
            const int file=7;
            Ok(St7<NewFile>("St7OpenFile")(file,path,folder),"St7OpenFile");
            try{Assert.AreEqual(sectionCannotBeMirrored,St7<SetMirror>("St7SetBeamMirrorOption")(file,1,3,0,new[]{0d,0}));}
            finally{St7<OneInt>("St7CloseFile")(file);}
        }
    }
    [DataTestMethod,DataRow(false),DataRow(true),TestCategory("Straus7Native")]
    public void ResolvedNativeHollowSectionKeepsItsTaperRuleAndAnalyticalAxialResponse(bool bgl)
    {
        string folder=Folder();
        var input=bgl?Straus7SectionCatalog.Get("BglRectangle").Create(new[]{100d,50,5,5,0,0}):Straus7SectionCatalog.Get("RectangleHollow").Create(new[]{50d,100,5,5});
        var definition=Straus7SectionLibrary.Resolve(input,folder);
        Assert.AreEqual(bgl,definition.IsBgl);Assert.AreEqual(1400,definition.ReferenceProperties[0],1e-8);
        var model=ElasticExportContractsTest.Cantilever();var beam=model.BeamElements.Values.Single();
        beam.BeamProperty=definition.ToBeamProperty("native hollow",new Material("E1000",1000,.3,0,0));
        beam.Assignments.Physical.Taper=new BeamSectionTaper {ScaleJ1=2,ScaleJ2=2,Alignment1=TaperAlignment.Symmetric,Alignment2=TaperAlignment.Symmetric};
        var free=beam.NodeJ;free.Loads.Clear();free.Loads.Add(new PointLoad(1000,0,0,0,0,0,free.Position,model.LoadCases.Values.Single()));
        string fingerprint=model.AnalysisFingerprint();var restored=Straus7SectionDefinition.Load(definition.Save());
        var plan=Straus7ModelWritePlan.Prepare(model,null,new Dictionary<Guid,int>{{beam.Guid,10}},new Dictionary<Guid,Straus7SectionDefinition>{{beam.Guid,restored}});
        var run=plan.RunLinearStatic(model,Path.Combine(folder,"taper.st7"),folder);
        using(var reader=new Straus7NativeApi())
        {
            reader.OpenModelReadOnly(run.ModelPath,folder);var captured=reader.ReadSectionDefinition(1,folder);
            Assert.AreEqual(bgl,captured.IsBgl);CollectionAssert.AreEqual(definition.Dimensions,captured.Dimensions);
            reader.OpenResults(run.ResultPath);
            // Standard: walls stay 5 mm, A(t)=1400+1500*t. BGL scales all dimensions, A(t)=1400*(1+t)^2.
            double compliance=bgl?1d/2800:Math.Log(2900d/1400)/1500;
            double expected=1000*1000d/1000*compliance;
            Assert.AreEqual(expected,reader.ReadNodeResult(free.Id,1,false).Values[0],expected*5e-5);
        }
        Assert.AreEqual(ImportStatus.Completed,run.ImportResults(model,"native section").Status);
        Assert.AreEqual(plan.ConfigurationFingerprint,model.Datasets["native section"].SourceHashes["section-export-configuration"]);
        Assert.IsTrue(model.PreservedSourceData.Any(p=>p.SourceRecord.StartsWith("NativeSectionDefinitions/",StringComparison.Ordinal)));
        using(var stream=new MemoryStream()){ModelArchive.SaveDocument(model,stream);stream.Position=0;Assert.AreEqual(fingerprint,ModelArchive.Load(stream).AnalysisFingerprint());}
        Assert.AreEqual(fingerprint,model.AnalysisFingerprint());
        Assert.AreEqual(0,Directory.GetFiles(folder,"section-*.bxs").Length);
    }
    [DataTestMethod,DataRow(false),DataRow(true),TestCategory("Straus7Native")]
    public void InstalledSectionLibrariesAreExpandedAndRewrittenWithoutLibraryLookup(bool bgl)
    {
        string folder=Folder(),library,item;
        using(var api=new Straus7NativeApi())
        {
            var entry=api.SectionLibraryCatalog(bgl).First(p=>p.Value.Length!=0);library=entry.Key;item=entry.Value[0];
        }
        var section=Straus7SectionLibrary.ResolveLibrary(bgl,library,item,folder);
        var model=ElasticExportContractsTest.Cantilever();var beam=model.BeamElements.Values.Single();beam.BeamProperty=section.ToBeamProperty(item,new Material("E",210000,.3,0,0));
        string path=Path.Combine(folder,"library-expanded.st7");
        var plan=Straus7ModelWritePlan.Prepare(model,null,null,new Dictionary<Guid,Straus7SectionDefinition>{{beam.Guid,Straus7SectionDefinition.Load(section.Save())}});
        using(var writer=new Straus7NativeWriter(path,folder)){plan.Write(writer,model);writer.Save();}
        using(var reader=new Straus7NativeApi())
        {
            reader.OpenModelReadOnly(path,folder);var read=reader.ReadSectionDefinition(1,folder);
            CollectionAssert.AreEqual(section.Dimensions,read.Dimensions);
            for(int i=0;i<11;i++)Assert.AreEqual(section.ReferenceProperties[i],read.ReferenceProperties[i],1e-8*Math.Max(1,Math.Abs(section.ReferenceProperties[i])));
        }
    }
    [TestMethod,TestCategory("Straus7Native")]
    public void EmbeddedBxsSurvivesRemovalOfTheSourceModelAndRetainsSectionProperties()
    {
        string folder=Folder(),path=Path.Combine(folder,"source.st7");var model=ElasticExportContractsTest.Cantilever();var beam=model.BeamElements.Values.Single();
        beam.BeamProperty=new ElasticBeamProperty("rotated",new Material("E",210000,.3,0,0),new SectionRectangular(20,10,.17));
        using(var writer=new Straus7NativeWriter(path,folder)){Straus7ModelWritePlan.Prepare(model).Write(writer,model);writer.Save();}
        Straus7SectionDefinition definition;
        using(var reader=new Straus7NativeApi()){reader.OpenModelReadOnly(path,folder);definition=reader.ReadSectionDefinition(1,folder);}
        Assert.IsTrue(definition.BxsData.Length>0);File.Delete(path);
        var copy=Straus7SectionDefinition.Load(definition.Save());var expanded=Straus7SectionLibrary.Resolve(copy,folder);
        Assert.AreEqual(11,expanded.Shape);Assert.AreEqual(200,expanded.ReferenceProperties[0],1e-8);
        beam.BeamProperty=copy.ToBeamProperty("restored",new Material("E",210000,.3,0,0));
        var plan=Straus7ModelWritePlan.Prepare(model,null,null,new Dictionary<Guid,Straus7SectionDefinition>{{beam.Guid,copy}});
        var run=plan.RunLinearStatic(model,Path.Combine(folder,"restored.st7"),folder);
        Assert.AreEqual(ImportStatus.Completed,run.ImportResults(model,"BXS").Status);
        Assert.AreEqual(0,Directory.GetFiles(folder,"section-*.bxs").Length);
    }
    [TestMethod,TestCategory("Straus7Native")]
    public void SectionMeshIsADisplayTriangulationOfTheSectionAndLeavesNoFiles()
    {
        string folder=Folder();
        static double Area(Straus7SectionMesh mesh)
        {
            var v=mesh.Vertices;double area=0;
            foreach(var f in mesh.Faces)for(int k=1;k+1<f.Length;k++)
                area+=((v[f[k]][0]-v[f[0]][0])*(v[f[k+1]][1]-v[f[0]][1])-(v[f[k+1]][0]-v[f[0]][0])*(v[f[k]][1]-v[f[0]][1]))/2;
            return Math.Abs(area);
        }
        var rectangle=Straus7SectionLibrary.Mesh(Straus7SectionCatalog.Get("RectangleSolid").Create(new[]{20d,10}),folder);
        Assert.AreEqual(200,Area(rectangle),1e-9);Assert.IsTrue(rectangle.Vertices.All(p=>p.Length==3&&p[2]==0));
        Assert.AreEqual(200,rectangle.Section.ReferenceProperties[0],1e-9);
        var input=Straus7SectionCatalog.Get("CircularHollow").Create(new[]{100d,10});
        var resolved=Straus7SectionLibrary.Resolve(input,folder);
        var tube=Straus7SectionLibrary.Mesh(input,folder,6);
        // Properties read before the display discretisation: those of Resolve.
        CollectionAssert.AreEqual(resolved.ReferenceProperties,tube.Section.ReferenceProperties);
        Assert.IsTrue(Straus7SectionLibrary.Mesh(resolved,folder,6).Faces.Length==tube.Faces.Length);
        // A coarse display polygon: within 3 % of the exact area that stays the analysis value.
        Assert.AreEqual(resolved.ReferenceProperties[0],Area(tube),.03*resolved.ReferenceProperties[0]);
        string library,item;
        using(var api=new Straus7NativeApi()){var entry=api.SectionLibraryCatalog(false).First(p=>p.Value.Length!=0);library=entry.Key;item=entry.Value[0];}
        var fromLibrary=Straus7SectionLibrary.MeshLibrary(false,library,item,folder);
        CollectionAssert.AreEqual(Straus7SectionLibrary.ResolveLibrary(false,library,item,folder).ReferenceProperties,fromLibrary.Section.ReferenceProperties);
        Assert.AreEqual(fromLibrary.Section.ReferenceProperties[0],Area(fromLibrary),.03*fromLibrary.Section.ReferenceProperties[0]);
        Assert.AreEqual(0,Directory.GetFiles(folder).Length);
        Assert.ThrowsException<ArgumentOutOfRangeException>(()=>Straus7SectionLibrary.Mesh(resolved,folder,-1));
    }
}
