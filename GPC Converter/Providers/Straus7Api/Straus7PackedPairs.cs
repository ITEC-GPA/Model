using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Sections;

namespace GPC.Converter.Straus7
{
    internal sealed partial class Straus7Assignments
    {
        private readonly Dictionary<int,double> pairedAngles=new Dictionary<int,double>();
        // Installed R31 API Manual pp1003,1005,1007-1009. A is the clear gap for
        // top/bottom/left/right mirroring. B belongs to diagonal/four-part layouts.
        private bool InterpretPair(Straus7Property p,SectionRecord section)
        {
            if(p.MirrorType<1 || p.MirrorType>4 || p.MirrorGaps==null || p.MirrorGaps.Length!=2
                || p.MirrorGaps.Any(v=>double.IsNaN(v)||double.IsInfinity(v)) || p.MirrorGaps[0]<0 || p.MirrorGaps[1]!=0)
                return false;
            var candidates=new List<(SectionShapeKind Kind,double[] Dimensions)>();
            var d=(p.Geometry??Array.Empty<double>()).Select(v=>v*length).ToArray();
            if(p.SectionType==9 && d.Length>=6)
            {
                // The area/inertia checks below also guard the T1/T2 placement.
                candidates.Add((SectionShapeKind.Angle,new[]{d[1],d[0],d[4],d[3]}));
            }
            // Standard lip channel with zero lips (bsLipChannel=5). R31 BGL
            // sections reject mirror operations; do not infer a mirrored BGL geometry.
            if(p.SectionType==5 && d.Length>=6 && d[2]==0 && d[5]==0)
                candidates.Add((SectionShapeKind.Channel,new[]{d[1],d[0],d[4],d[3],d[0],d[3]}));
            var data=p.SectionProperties;
            if(data==null || data.Length<11 || data.Any(v=>double.IsNaN(v)||double.IsInfinity(v)))return false;
            foreach(var candidate in candidates)
            {
                var pair=new IdenticalSectionPair {ComponentShape=candidate.Kind,ComponentDimensions=candidate.Dimensions,
                    Gap=p.MirrorGaps[0]*length,Mirror=p.MirrorType==1?SectionPairMirror.Top:p.MirrorType==2?SectionPairMirror.Bottom:
                        p.MirrorType==3?SectionPairMirror.Left:SectionPairMirror.Right};
                try
                {
                    var component=(Section)PropertyMapper.Parametric(new SectionRecord{Id=section.Id+"/chord",Shape=candidate.Kind,Dimensions=candidate.Dimensions},"chord");
                    component.SetEdgeTypeFromSteelType(Section.SectionTypes.Rolled);
                    var shape=pair.Create(component,section.Name??section.Id);
                    double co=Math.Cos(data[10]),si=Math.Sin(data[10]);
                    double i1=co*co*shape.Jxx+si*si*shape.Jyy-2*co*si*shape.Jxy;
                    double i2=si*si*shape.Jxx+co*co*shape.Jyy+2*co*si*shape.Jxy;
                    double product=co*si*(shape.Jxx-shape.Jyy)+(co*co-si*si)*shape.Jxy;
                    bool Close(double a,double b)=>Math.Abs(a-b)<=.005*Math.Max(Math.Abs(b),1e-9);
                    if(!Close(shape.Area,data[0]*length*length) || !Close(i1,data[1]*Math.Pow(length,4))
                        || !Close(i2,data[2]*Math.Pow(length,4)) || Math.Abs(product)>1e-8*Math.Max(i1,i2))continue;
                    section.IdenticalPair=pair;pairedAngles[p.Number]=data[10];
                    Warn("Straus7IdenticalPairGeometry",section.Record,
                        "Two identical C/L outlines and source principal axes verified against solver area/inertias. Packing and connection action are not imported. Native torsion data remain in Values; Model warping is unavailable.");
                    return true;
                }
                catch(ArgumentException) { }
            }
            return false;
        }

        private void PairAxes(int property,BeamRecord beam)
        {
            if(!pairedAngles.TryGetValue(property,out double angle))return;
            var a=beam.CoordinateSystem;double co=Math.Cos(angle),si=Math.Sin(angle);
            // e1 = cos(theta) ex + sin(theta) ey. Retain native/result frame on
            // the beam; only Assignments.SectionAxes describes the geometric outline.
            beam.SectionCoordinateSystem=new CoordinateSystem(a.Origin,co*a.V1-si*a.V2,si*a.V1+co*a.V2,a.V3);
        }
    }
}
