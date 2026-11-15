using System.Reflection;
using System.Text.Json;
using GPC.Converter.Straus7;
using GPC.Geometry;
using GPC.Model.ElementProperties;
using GPC.Model.Sections;

namespace UnitTest;

/// <summary>190: parametric export, authoritative GPC values and independent rectangle integrals. No native API.</summary>
[TestClass]
public class UniqueOpenSections190Test
{
    private static void Near(double expected,double actual,string label,double relative=1e-10) =>
        Assert.AreEqual(expected,actual,relative*Math.Max(1,Math.Abs(expected)),label);

    private static Straus7ExporterCorrectionsTest.Recorder Record(Section section)
    {
        var model=Straus7ModelWriteTest.Cantilever();var beam=model.BeamElements.Values.Single();
        var old=(ElasticBeamProperty)beam.BeamProperty;
        // Deliberately different from geometric shear areas: the exporter must retain caller-supplied GPC rigidities.
        beam.BeamProperty=new ElasticBeamProperty("190",old.Material,section,123.25,456.75);
        return Straus7ExporterCorrectionsTest.Record(model);
    }

    private static void Authoritative(Section section,double[] p,double originX=0,double originY=0)
    {
        Near(section.Area,p[0],"A");Near(section.J11,p[1],"I11");Near(section.J22,p[2],"I22");Near(section.Jt,p[3],"J");
        double c=Math.Cos(section.AngleX1),s=Math.Sin(section.AngleX1);
        double dx=section.ShearCenter.X-section.Centroid.X,dy=section.ShearCenter.Y-section.Centroid.Y;
        Near(c*dx+s*dy,p[4],"SL1");Near(-s*dx+c*dy,p[5],"SL2");
        Near(123.25,p[6],"SA1");Near(456.75,p[7],"SA2");
        Near(section.Centroid.X-originX,p[8],"Xbar");Near(section.Centroid.Y-originY,p[9],"Ybar");Near(section.AngleX1,p[10],"principal angle");
    }

    [DataTestMethod]
    [DataRow("T")] [DataRow("C")] [DataRow("L")]
    public void SingleSectionsUseNativeGeometryAndKeepAllElevenGpcProperties(string family)
    {
        Section section=family switch { "T"=>new SectionT(300,140,8,12,"T"),"C"=>new SectionC(300,8,140,12,140,12,"C"),_=>new SectionL(140,12,300,8,"L") };
        var log=Record(section);var geometry=log.Named("SetBeamSectionGeometry");
        Assert.AreEqual(1,geometry.Length,"One dimensional native geometry, no BXS");
        Assert.AreEqual(family=="T"?8:family=="C"?5:9,geometry[0].Args[1]);
        CollectionAssert.AreEqual(new[]{140d,300,0,12,8,0},(double[])geometry[0].Args[2]!);
        Authoritative(section,(double[])geometry[0].Args[3]!);
        Assert.AreEqual(0,log.Named("SetBeamSectionMesh").Length);Assert.AreEqual(0,log.Named("SetNativeSection").Length);
        var rectangles=family=="T"?new[]{(70d-4,0d,8d,288d),(0d,288d,140d,12d)}:
            family=="C"?new[]{(0d,0d,140d,12d),(0d,12d,8d,276d),(0d,288d,140d,12d)}:
            new[]{(0d,0d,140d,12d),(0d,12d,8d,288d)};
        Integrals(section,rectangles);
        var axes=(double[])log.Named("SetBeamAxes").Single().Args[1]!;
        // Cantilever section frame is y/z; principal axes rotate about x by the actual GPC principal angle.
        Near(Math.Cos(section.AngleX1),axes[1],"principal V1.y");Near(Math.Sin(section.AngleX1),axes[2],"principal V1.z");
    }

    private static void Integrals(Section section,IEnumerable<(double X,double Y,double B,double H)> rectangles)
    {
        var r=rectangles.ToArray();double a=r.Sum(v=>v.B*v.H),x=r.Sum(v=>v.B*v.H*(v.X+v.B/2))/a,y=r.Sum(v=>v.B*v.H*(v.Y+v.H/2))/a;
        Near(a,section.Area,"analytical A");Near(x,section.Centroid.X,"analytical X");Near(y,section.Centroid.Y,"analytical Y");
        Near(r.Sum(v=>v.B*v.H*(v.H*v.H/12+Math.Pow(v.Y+v.H/2-y,2))),section.Jxx,"analytical Ixx");
        Near(r.Sum(v=>v.B*v.H*(v.B*v.B/12+Math.Pow(v.X+v.B/2-x,2))),section.Jyy,"analytical Iyy");
        Assert.AreEqual(r.Sum(v=>v.B*v.H*(v.X+v.B/2-x)*(v.Y+v.H/2-y)),section.Jxy,1e-10*Math.Max(section.Jxx,section.Jyy),"analytical Ixy (inertia scale)");
    }

    [DataTestMethod]
    [DataRow(false,1)] [DataRow(false,2)] [DataRow(false,3)] [DataRow(false,4)]
    [DataRow(true,1)] [DataRow(true,2)] [DataRow(true,3)] [DataRow(true,4)]
    public void MirroredSectionsMatchFrozenStrausAreaInertiasCentroidAndNativeMirror(bool channel,int mirror)
    {
        using var json=JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory,"Fixtures","straus7-identical-pairs.json")));
        var row=json.RootElement.EnumerateArray().Single(r=>r.GetProperty("kind").GetString()==(channel?"C-standard":"L")&&r.GetProperty("mirror").GetInt32()==mirror);
        var dims=row.GetProperty("geometry").EnumerateArray().Select(v=>v.GetDouble()).ToArray();
        var old=row.GetProperty("properties").EnumerateArray().Select(v=>v.GetDouble()).ToArray();double gap=row.GetProperty("gap").GetDouble();
        Section single=channel?new SectionC(dims[1],dims[4],dims[0],dims[3],dims[0],dims[3],"C"):new SectionL(dims[0],dims[3],dims[1],dims[4],"L");
        var second=mirror switch {1=>new SectionBuiltUp.Part(single,0,2*single.Height+gap,mirrorY:true),2=>new SectionBuiltUp.Part(single,0,-gap,mirrorY:true),3=>new SectionBuiltUp.Part(single,-gap,0,mirrorX:true),_=>new SectionBuiltUp.Part(single,2*single.Width+gap,0,mirrorX:true)};
        var pair=new SectionBuiltUp(new[]{new SectionBuiltUp.Part(single,0,0),second},"pair");
        // Independent geometry and frozen R31 values, no GPC number used as an analytical oracle.
        var expected=IdenticalPairReference.Integrate(channel,dims[1],dims[0],dims[4],dims[3],gap,mirror);
        Near(expected.Area,pair.Area,"A");Near(expected.Ixx,pair.Jxx,"Ixx");Near(expected.Iyy,pair.Jyy,"Iyy");Assert.AreEqual(expected.Ixy,pair.Jxy,1e-10*Math.Max(expected.Ixx,expected.Iyy),"Ixy (inertia scale)");
        double originX=mirror==3?-single.Width-gap:0,originY=mirror==2?-single.Height-gap:0;
        Near(old[0],pair.Area,"Straus A",1e-9);Near(old[8],pair.Centroid.X-originX,"Straus Xbar",1e-9);Near(old[9],pair.Centroid.Y-originY,"Straus Ybar",1e-9);
        double c=Math.Cos(old[10]),s=Math.Sin(old[10]);
        Near(old[1],c*c*pair.Jxx+s*s*pair.Jyy-2*c*s*pair.Jxy,"Straus I11 in captured axes",1e-9);
        Near(old[2],s*s*pair.Jxx+c*c*pair.Jyy+2*c*s*pair.Jxy,"Straus I22 in captured axes",1e-9);
        Console.WriteLine($"190 comparison {(channel?"C":"L")} mirror {mirror}: J GPC={pair.Jt:R}, Straus={old[3]:R}; native shear areas {old[6]:R},{old[7]:R} (capture did not request shear)");
        var log=Record(pair);var call=log.Named("SetNativeSection").SingleOrDefault();Assert.IsNotNull(call.Args,"Native mirrored geometry, not BXS");
        var definition=(Straus7SectionDefinition)call.Args[1]!;
        Assert.AreEqual(channel?5:9,definition.Shape);CollectionAssert.AreEqual(dims,definition.Dimensions);
        Assert.AreEqual(mirror,definition.MirrorType);Assert.IsFalse(definition.CompatibleTwist);CollectionAssert.AreEqual(new[]{gap,0},definition.MirrorGaps);
        Authoritative(pair,(double[])call.Args[2]!,originX,originY);CollectionAssert.AreEqual((double[])call.Args[2]!,definition.ReferenceProperties);
        Assert.AreEqual(0,log.Named("SetBeamSectionMesh").Length);
    }

    [DataTestMethod]
    [DataRow(false,0d)] [DataRow(false,10d)] [DataRow(true,0d)] [DataRow(true,10d)]
    public void TranslatedReorderedPairPreservesGapAndNativeOrigin(bool channel,double gap)
    {
        Section single=channel?new SectionC(200,10,75,12,75,12,"C"):new SectionL(100,12,150,10,"L");
        var pair=new SectionBuiltUp(new[]{new SectionBuiltUp.Part(single,17-gap,23,mirrorX:true),new SectionBuiltUp.Part(single,17,23)},"translated");
        var log=Record(pair);var call=log.Named("SetNativeSection").SingleOrDefault();Assert.IsNotNull(call.Args,"Native pair");
        var definition=(Straus7SectionDefinition)call.Args[1]!;
        Assert.AreEqual(3,definition.MirrorType);CollectionAssert.AreEqual(new[]{gap,0},definition.MirrorGaps);
        Authoritative(pair,(double[])call.Args[2]!,17-gap-single.Width,23);
        Assert.AreEqual(0,log.Named("SetBeamSectionMesh").Length);
    }

    [TestMethod]
    public void RoundedUnequalAndNonIdenticalDomainsRetainTheirOutlines()
    {
        var c=new SectionC(300,8,140,12,100,10,"unequal");
        var t=new SectionT(300,140,8,12,"radius",5);
        var l=new SectionL(140,12,300,8,"radius",5);
        foreach(var section in new Section[]{c,t,l,new SectionBuiltUp(new[]{new SectionBuiltUp.Part(c,0,0),new SectionBuiltUp.Part(new SectionL(140,12,300,8,"L"),-10,0,true)},"mixed")})
            Assert.AreEqual(1,Record(section).Named("SetBeamSectionMesh").Length,"An unqualified domain keeps the BXS geometry");
    }
}
