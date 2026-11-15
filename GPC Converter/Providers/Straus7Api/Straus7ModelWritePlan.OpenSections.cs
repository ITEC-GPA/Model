using System;
using System.Linq;
using GPC.Model.Sections;

namespace GPC.Converter.Straus7
{
    public sealed partial class Straus7ModelWritePlan
    {
        private sealed partial class Builder
        {
            // 190: exact standard domains only. Unequal C flanges, worked corners and derived forms retain their mesh.
            private static bool OpenSectionGeometry(ISectionShape section,out int shape,out double[] dimensions)
            {
                shape=0;dimensions=null;
                if(section?.GetType()==typeof(SectionT) && section is SectionT t && t.R==0)
                {shape=8;dimensions=new[]{t.LenghtFlange,t.Height,0,t.ThicknessFlange,t.ThicknessWeb,0};}
                else if(section?.GetType()==typeof(SectionC) && section is SectionC c && c.R1==0 && c.R2==0
                    && c.LengthTop==c.LengthBottom && c.ThicknessTop==c.ThicknessBottom)
                {shape=5;dimensions=new[]{c.LengthTop,c.Height,0,c.ThicknessTop,c.ThicknessWeb,0};}
                else if(section?.GetType()==typeof(SectionL) && section is SectionL l && l.R==0 && l.R2==0)
                {shape=9;dimensions=new[]{l.HorizontalLegLength,l.VerticalLegLength,0,l.HorizontalLegThickness,l.VerticalLegThickness,0};}
                return shape!=0;
            }

            private static Straus7SectionDefinition OpenSectionPair(ISectionShape section,double[] properties)
            {
                // Identical pairs in a common translated frame, with one reflected component and a clear gap.
                // Different components, rotations or four-part/diagonal layouts retain the general mesh.
                if(section?.GetType()!=typeof(SectionBuiltUp) || !(section is SectionBuiltUp pair) || pair.Parts.Count!=2)return null;
                var first=pair.Parts.FirstOrDefault(p=>!p.MirrorX&&!p.MirrorY);
                if(first==null || !OpenSectionGeometry(first.Section,out int shape,out var dimensions) || shape==8)return null;
                var second=pair.Parts.Single(p=>!ReferenceEquals(p,first));
                if(!OpenSectionGeometry(second.Section,out int otherShape,out var otherDimensions)
                    ||shape!=otherShape||!dimensions.SequenceEqual(otherDimensions))return null;
                int mirror;double gap;
                double dx=second.X-first.X,dy=second.Y-first.Y;
                if(second.MirrorX&&!second.MirrorY&&dy==0)
                {
                    if(dx<=0){mirror=3;gap=-dx;}
                    else if(dx>=2*first.Section.Width){mirror=4;gap=dx-2*first.Section.Width;}
                    else return null;
                }
                else if(second.MirrorY&&!second.MirrorX&&dx==0)
                {
                    if(dy<=0){mirror=2;gap=-dy;}
                    else if(dy>=2*first.Section.Height){mirror=1;gap=dy-2*first.Section.Height;}
                    else return null;
                }
                else return null;
                // Straus reports centroid coordinates from the native bounding-box minimum. GPC keeps
                // the parts' placement origin. Only the absolute centroid coordinates need this translation;
                // centroidal inertias, shear-centre offsets and principal directions are unchanged.
                properties=(double[])properties.Clone();
                properties[8]-=first.X+(mirror==3?-first.Section.Width-gap:0);
                properties[9]-=first.Y+(mirror==2?-first.Section.Height-gap:0);
                // Resolved here means supplied by GPC, not read from Straus7. The writer assigns the native
                // geometry/mirror then reapplies these eleven authoritative GPC values, including shear areas.
                return new Straus7SectionDefinition(shape,false,dimensions,null,properties,"GPC identical pair",mirror,new[]{gap,0},false);
            }
        }
    }
}
