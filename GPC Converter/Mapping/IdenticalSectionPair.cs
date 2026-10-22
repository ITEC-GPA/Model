using System;
using System.Linq;
using GPC.Model.Sections;

namespace GPC.Converter
{
    /// <summary>Direction in the source SECTION x/y plane, never the sorted principal axes.</summary>
    public enum SectionPairMirror { Left, Right, Bottom, Top }

    /// <summary>Exactly two identical components. Gap is the clear boundary-to-boundary
    /// separation in mm. CatalogDesignation names ONE C or L. This transient import
    /// contract cannot encode two different components or infer their connections.</summary>
    public sealed class IdenticalSectionPair
    {
        public string ComponentCatalogDesignation { get; set; }
        public SectionShapeKind ComponentShape { get; set; }
        public double[] ComponentDimensions { get; set; }
        public SectionPairMirror Mirror { get; set; }
        public double Gap { get; set; }

        internal SectionBuiltUp Create(Section component,string name)
        {
            if(component==null)throw new ArgumentNullException(nameof(component));
            if(component.GetType()!=typeof(SectionC)&&component.GetType()!=typeof(SectionL))
                throw new NotSupportedException("An identical pair requires one exact C or L component.");
            if(!(Gap>=0)||double.IsInfinity(Gap))throw new ArgumentOutOfRangeException(nameof(Gap));
            if(!Enum.IsDefined(typeof(SectionPairMirror),Mirror))throw new ArgumentOutOfRangeException(nameof(Mirror));
            var pts=component.GetSectionPoints();
            double xmin=pts.Min(p=>p.X),xmax=pts.Max(p=>p.X),ymin=pts.Min(p=>p.Y),ymax=pts.Max(p=>p.Y);
            bool x=Mirror==SectionPairMirror.Left||Mirror==SectionPairMirror.Right;
            double shift=Mirror==SectionPairMirror.Left?2*xmin-Gap:Mirror==SectionPairMirror.Right?2*xmax+Gap:
                Mirror==SectionPairMirror.Bottom?2*ymin-Gap:2*ymax+Gap;
            return new SectionBuiltUp(new[]{new SectionBuiltUp.Part(component,0,0),new SectionBuiltUp.Part(component,x?shift:0,x?0:shift,x,!x)},name);
        }
    }
}
