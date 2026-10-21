using GPC.Model.Core;
using GPC.Model.Persistence;
using GPC.Model.Sections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest;

[TestClass]
public class CoupledSectionQualificationTest
{
    // Independent area integrals of disjoint rectangles (parallel-axis theorem),
    // elastic section moduli and integral |coordinate-PNA| dA. No component
    // property is read to construct an expected number.
    private record Rect(double X,double Y,double B,double H);
    public static IEnumerable<object[]> Cases =>
        from layout in new[] {"2L-v", "2L-h", "2C-back", "2C-toe"}
        from gap in new[] {0d, 10d, 100d, 400d}
        select new object[] {layout,gap};

    [DataTestMethod,DynamicData(nameof(Cases))]
    public void GeometryAgainstIndependentAreaIntegrals(string layout,double gap)
    {
        bool angle=layout.StartsWith("2L"), horizontal=layout=="2L-h", back=layout=="2C-back";
        var single=angle ? new[]{new Rect(0,0,100,10),new Rect(0,10,10,140)}
            : new[]{new Rect(0,0,80,12),new Rect(0,12,8,176),new Rect(0,188,80,12)};
        var r=new List<Rect>();
        foreach(var a in single)
        {
            if(horizontal){r.Add(a with {Y=a.Y+gap/2});r.Add(a with {Y=-a.Y-a.H-gap/2});}
            else if(angle||back){r.Add(a with {X=a.X+gap/2});r.Add(a with {X=-a.X-a.B-gap/2});}
            else {r.Add(a with {X=a.X-80-gap/2});r.Add(a with {X=-a.X-a.B+80+gap/2});}
        }
        SectionBuiltUp s=angle ? SectionBuiltUp.DoubleAngle(new SectionL(100,10,150,10,"L"),gap,
            horizontal?SectionBuiltUp.AnglePairOrientation.HorizontalLegsBackToBack:SectionBuiltUp.AnglePairOrientation.VerticalLegsBackToBack,"2L")
            :SectionBuiltUp.DoubleChannel(new SectionC(200,8,80,12,80,12,"C"),gap,back,"2C");
        double area=r.Sum(a=>a.B*a.H),cx=r.Sum(a=>a.B*a.H*(a.X+a.B/2))/area,cy=r.Sum(a=>a.B*a.H*(a.Y+a.H/2))/area;
        double ix=r.Sum(a=>a.B*a.H*(a.H*a.H/12+Math.Pow(a.Y+a.H/2-cy,2)));
        double iy=r.Sum(a=>a.B*a.H*(a.B*a.B/12+Math.Pow(a.X+a.B/2-cx,2)));
        double ixy=r.Sum(a=>a.B*a.H*(a.X+a.B/2-cx)*(a.Y+a.H/2-cy));
        Close(area,s.Area);Close(cx,s.Centroid.X);Close(cy,s.Centroid.Y);
        Close(ix,s.Jxx);Close(iy,s.Jyy);Close(ixy,s.Jxy);
        Close(Math.Max(ix,iy),s.J11);Close(Math.Min(ix,iy),s.J22);
        double top=r.Max(a=>a.Y+a.H)-cy,bottom=cy-r.Min(a=>a.Y);
        double right=r.Max(a=>a.X+a.B)-cx,left=cx-r.Min(a=>a.X);
        Close(ix/Math.Max(top,bottom),s.WelX);Close(iy/Math.Max(left,right),s.WelY);
        double wx=Plastic(r,false),wy=Plastic(r,true);
        Close(wx,s.WplX,1e-6);Close(wy,s.WplY,1e-6);
        Close(ix>=iy?wx:wy,s.Wpl1,1e-6);Close(ix>=iy?wy:wx,s.Wpl2,1e-6);
        Assert.IsTrue(double.IsNaN(s.Jw),"Geometry does not supply a connected-member warping constant.");
    }

    private static double Plastic(List<Rect> r,bool x)
    {
        double lo=r.Min(a=>x?a.X:a.Y),hi=r.Max(a=>x?a.X+a.B:a.Y+a.H),area=r.Sum(a=>a.B*a.H);
        for(int i=0;i<80;i++)
        {
            double p=(lo+hi)/2,below=r.Sum(a=>(x?a.H:a.B)*Math.Max(0,Math.Min(x?a.B:a.H,p-(x?a.X:a.Y))));
            if(below<area/2)lo=p;else hi=p;
        }
        double n=(lo+hi)/2;
        return r.Sum(a=>{double a0=(x?a.X:a.Y)-n,a1=a0+(x?a.B:a.H);return(x?a.H:a.B)*(a1*Math.Abs(a1)-a0*Math.Abs(a0))/2;});
    }

    [DataTestMethod]
    [DataRow(true,false)] [DataRow(false,true)] [DataRow(true,true)] [DataRow(false,false)]
    public void ProductOfInertiaTracksMirrorsAndTranslation(bool mx,bool my)
    {
        // L 100x150x10: two rectangles, A=2400, centroid=(23.75,48.75).
        double ix=5576250,iy=2026250,ixy=-1968750;
        var s=new SectionBuiltUp(new[]{new SectionBuiltUp.Part(new SectionL(100,10,150,10,""),300,-50,mx,my)},"placed");
        Close(2400,s.Area);Close(ix,s.Jxx);Close(iy,s.Jyy);Close(mx==my?ixy:-ixy,s.Jxy);
        Close(300+(mx?-23.75:23.75),s.Centroid.X);Close(-50+(my?-48.75:48.75),s.Centroid.Y);
    }

    [DataTestMethod]
    [DataRow(-1d)] [DataRow(double.NaN)] [DataRow(double.PositiveInfinity)]
    public void InvalidSpacingRejected(double gap)
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(()=>SectionBuiltUp.DoubleAngle(new SectionL(100,10,100,10,""),gap,""));
        Assert.ThrowsException<ArgumentOutOfRangeException>(()=>SectionBuiltUp.DoubleChannel(new SectionC(200,8,80,12,80,12,""),gap,true,""));
    }

    [TestMethod]
    public void PlacementValidationAndReadOnlyParts()
    {
        var l=new SectionL(100,10,150,10,"");
        Assert.ThrowsException<ArgumentOutOfRangeException>(()=>new SectionBuiltUp.Part(l,double.NaN,0));
        Assert.ThrowsException<ArgumentException>(()=>new SectionBuiltUp(new SectionBuiltUp.Part[]{null!},""));
        var s=SectionBuiltUp.DoubleAngle(l,10,"");
        Assert.ThrowsException<NotSupportedException>(()=>((IList<SectionBuiltUp.Part>)s.Parts)[0]=new SectionBuiltUp.Part(l,20,0));
    }

    [DataTestMethod]
    [DataRow(false)] [DataRow(true)]
    public void ArchiveAndFingerprintPreservePlacement(bool horizontal)
    {
        var l=new SectionL(100,10,150,10,"");
        var orientation=horizontal?SectionBuiltUp.AnglePairOrientation.HorizontalLegsBackToBack:SectionBuiltUp.AnglePairOrientation.VerticalLegsBackToBack;
        var a=SectionBuiltUp.DoubleAngle(l,10,orientation,"");
        var b=ModelArchive.CopyValue(a);
        Close(a.J11,b.J11);Close(a.WplX,b.WplX);
        string fingerprint=ModelValues.Fingerprint(new object[]{a});
        Assert.AreEqual(fingerprint,ModelValues.Fingerprint(new object[]{b}));
        Assert.AreNotEqual(fingerprint,ModelValues.Fingerprint(new object[]{SectionBuiltUp.DoubleAngle(l,11,orientation,"")}));
        Assert.AreNotEqual(fingerprint,ModelValues.Fingerprint(new object[]{SectionBuiltUp.DoubleAngle(l,10,horizontal?SectionBuiltUp.AnglePairOrientation.VerticalLegsBackToBack:SectionBuiltUp.AnglePairOrientation.HorizontalLegsBackToBack,"")}));
    }

    private static void Close(double expected,double actual,double relative=1e-10)
        =>Assert.AreEqual(expected,actual,Math.Max(1,Math.Abs(expected))*relative);
}
