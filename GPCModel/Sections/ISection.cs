using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Model.Materials;
using static GPC.Model.Sections.Section;
using static GPC.Model.Sections.ThinWallSection;

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

        double Rxy { get; }

        Geometry.Point2d Centroid { get; }

        Geometry.Point2d ShearCenter { get; }

        /// <summary>
        /// The angle of rotation of the principal axis.
        /// </summary>
        double AngleX1 { get; }

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

        /// <summary>
        /// The elastic modulus calculated respect the 1-principal axes and the minimum (with sign) distance respect the centroid.
        /// </summary>
        double Wel1Min { get; }

        /// <summary>
        /// The elastic modulus calculated respect the 1-principal axes and the maximum (with sign) distance respect the centroid.
        /// </summary>
        double Wel1Max { get; }

        /// <summary>
        /// The elastic modulus calculated respect the 2-principal axes and the minimum (with sign) distance respect the centroid.
        /// </summary>
        double Wel2Min { get; }

        /// <summary>
        /// The elastic modulus calculated respect the 2-principal axes and the maximum (with sign) distance respect the centroid.
        /// </summary>
        double Wel2Max { get; }

        /// <summary>
        /// The elastic modulus calculated respect the X axes and the minimum (with sign) distance respect the centroid.
        /// </summary>
        double WelXMin { get; }

        /// <summary>
        /// The elastic modulus calculated respect the X axes and the maximum distance (with sign) respect the centroid.
        /// </summary>
        double WelXMax { get; }

        /// <summary>
        /// The elastic modulus calculated respect the Y axes and the minimum (with sign) distance respect the centroid.
        /// </summary>
        double WelYMin { get; }

        /// <summary>
        /// The elastic modulus calculated respect the Y axes and the maximum (with sign) distance respect the centroid.
        /// </summary>
        double WelYMax { get; }

        /// <summary>
        /// The minimum elastic modulus calculated respect the X-principal axes.
        /// </summary>
        double WelX { get; }

        /// <summary>
        /// The minimum elastic modulus calculated respect the X-principal axes.
        /// </summary>
        double WelY { get; }

        bool IsSymmetricAlongXLocalAxis { get; }

        bool IsSymmetricAlongYLocalAxis { get; }

        bool IsDoubleSymmetric { get; }

        Material Material { get; }

        ThinWall[] ThinWalls { get; }

        Point2d[] GetSectionPoints();

        void SetEdgeTypeFromSteelType(SectionTypes sectionType);

        void SetMechanicalProperties();

        Mesh GetMesh(double meshSize = 0, bool initialMeshOnly = false, bool recombine = true, bool refine = false);

        void SetMeshSize(double size);
    }
}
