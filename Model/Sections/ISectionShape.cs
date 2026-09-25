using GPC.Geometry;
using GPC.Geometry.Meshes;

namespace GPC.Model.Sections
{
    /// <summary>
    /// Only cross-section shape, without material.
    /// </summary>
    public interface ISectionShape
    {
        /// <summary>
        /// The moment of inertia about the principal axis 1 (the maximum one)
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The moment of inertia about the X axis through the centroid
        /// </summary>
        double Height { get; }

        /// <summary>
        /// The maximum overall width
        /// </summary>
        double Width { get; }

        /// <summary>
        /// The plastic modulus respect to the axis 2
        /// </summary>
        Shape2d Shape { get; }

        /// <summary>
        /// The minimum elastic modulus respect to the axis 2
        /// </summary>
        double Area { get; }

        /// <summary>
        /// True if the section is symmetric with respect to the Y axis
        /// </summary>
        double R11 { get; }

        /// <summary>
        /// The thin walls of the section
        /// </summary>
        double R22 { get; }

        /// <summary>
        ///
        /// </summary>
        double Rxx { get; }

        /// <summary>
        ///
        /// </summary>
        double Ryy { get; }

        /// <summary>
        ///
        /// </summary>
        double Rxy { get; }

        /// <summary>
        ///
        /// </summary>
        Point2d Centroid { get; }

        /// <summary>
        ///
        /// </summary>
        Point2d ShearCenter { get; }

        /// <summary>
        /// The angle of rotation of the principal axis 1 from X (radians)
        /// </summary>
        double AngleX1 { get; }

        /// <summary>
        ///
        /// </summary>
        double J11 { get; }

        /// <summary>
        ///
        /// </summary>
        double J22 { get; }

        /// <summary>
        ///
        /// </summary>
        double Jxx { get; }

        /// <summary>
        ///
        /// </summary>
        double Jyy { get; }

        /// <summary>
        ///
        /// </summary>
        double Jxy { get; }

        /// <summary>
        ///
        /// </summary>
        double Jp { get; }

        /// <summary>
        ///
        /// </summary>
        double Jt { get; }

        /// <summary>
        ///
        /// </summary>
        double Jw { get; }

        /// <summary>
        ///
        /// </summary>
        double Wpl1 { get; }

        /// <summary>
        ///
        /// </summary>
        double Wpl2 { get; }

        /// <summary>
        ///
        /// </summary>
        double Wel1 { get; }

        /// <summary>
        ///
        /// </summary>
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
        /// The minimum elastic modulus respect to the X axis
        /// </summary>
        double WelX { get; }

        /// <summary>
        /// The minimum elastic modulus respect to the Y axis
        /// </summary>
        double WelY { get; }

        /// <summary>
        ///
        /// </summary>
        bool IsSymmetricAlongXLocalAxis { get; }

        /// <summary>
        ///
        /// </summary>
        bool IsSymmetricAlongYLocalAxis { get; }

        /// <summary>
        ///
        /// </summary>
        bool IsDoubleSymmetric { get; }

        /// <summary>
        ///
        /// </summary>
        ThinWallSection.ThinWall[] ThinWalls { get; }

        /// <summary>
        ///
        /// </summary>
        Mesh Mesh { get; }

        /// <summary>
        /// The points that define the section
        /// </summary>
        /// <returns>The points</returns>
        Point2d[] GetSectionPoints();

        /// <summary>
        /// Sets the working of the corners from the type of the section (fillet for rolled, chamfer for welded)
        /// </summary>
        /// <param name="sectionType">The type of the section</param>
        void SetEdgeTypeFromSteelType(Section.SectionTypes sectionType);

        /// <summary>
        /// Calculates all the properties of the section
        /// </summary>
        void SetMechanicalProperties();

        /// <summary>
        /// Generates a mesh of the shape
        /// </summary>
        /// <param name="meshSize">The size of the elements (0: the size set by <see cref="SetMeshSize(double)"/>)</param>
        /// <param name="initialMeshOnly">True for the initial mesh only</param>
        /// <param name="recombine">True to recombine the triangles in quadrangles</param>
        /// <param name="refine">True to refine the mesh</param>
        /// <returns>The new mesh</returns>
        Mesh GetMesh(double meshSize = 0, bool initialMeshOnly = false, bool recombine = false, bool refine = false);

        /// <summary>
        /// Update the mesh size and regenerate the mesh with the new size
        /// </summary>
        /// <param name="size">The size of the mesh elements</param>
        void SetMeshSize(double size);

        /// <summary>
        /// Calculate the static moments of the section in X-Y plane
        /// </summary>
        /// <returns>The static moments respect to X and Y</returns>
        (double Sx, double Sy) CalculateStaticMoments();
    }
}
