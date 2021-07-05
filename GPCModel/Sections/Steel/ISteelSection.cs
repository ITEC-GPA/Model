using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Sections.Steel
{
    public interface ISteelSection
    {
        string Name { get; }
        Materials.Material Material { get; }
        double Height { get; }
        double Area { get; }
        double InertiaRadiusY { get; }
        double InertiaRadiusX { get; }
        GPC.Geometry.Point2d Centroid { get; }
        GPC.Geometry.Point2d ShearCenter { get; }
        double J11 { get; }
        double J22 { get; }
        double Jxx { get; }
        double Jyy { get; }
        double Jt { get; }
        double Jw { get; }
        double Sx { get; }
        double Wpl1 { get; }
        double Wpl2 { get; }
        double Wel1 { get; }
        double Wel2 { get; }
    }
}
