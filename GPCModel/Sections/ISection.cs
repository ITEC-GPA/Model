using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Model.Materials;

namespace GPC.Model.Sections
{
    /// <summary>
    /// Only cross-section shape.
    /// </summary>
    public interface ISection
    {
        string Name { get; }

        double Height { get; }

        Shape2d Shape { get; }

        double Area { get; }

        double R11 { get; }

        double R22 { get; }

        Geometry.Point2d Centroid { get; }

        Geometry.Point2d ShearCenter { get; }

        double J11 { get; }

        double J22 { get; }

        double Jxx { get; }

        double Jyy { get; }

        double Jxy { get; }

        double Jp { get; }

        double Jt { get; }

        double Jw { get; }

        double Wpl1 { get; }

        double Wpl2 { get; }

        double Wel1 { get; }

        double Wel2 { get; }

        bool IsSymmetricAlongXLocalAxis { get; }

        bool IsSymmetricAlongYLocalAxis { get; }

        bool IsDoubleSymmetric { get; }

        Material Material { get; }
    }
}
