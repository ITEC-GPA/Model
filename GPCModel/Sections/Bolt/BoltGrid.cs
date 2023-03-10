using GPC.Geometry;
using GPC.Model.Materials;
using GPC.Model.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GPC.Model.Sections.Bolt
{
    /// <summary>
    /// Support class for the list of bolts with their locations.
    /// </summary>
    public class BoltGrid
    {
        public class BoltPosition : ModelObjectId
        {
            public Point2d Position;
            public BoltSection BoltDef;

            public BoltPosition(Point2d _pos, BoltSection _bol)
            {
                Position = _pos;
                BoltDef = _bol;
            }

            public override bool Equals(object obj)
            {
                return obj is BoltPosition other &&
                       EqualityComparer<Point2d>.Default.Equals(Position, other.Position) &&
                       EqualityComparer<BoltSection>.Default.Equals(BoltDef, other.BoltDef);
            }

            public override int GetHashCode()
            {
                int hashCode = -1030903623;
                hashCode = hashCode * -1521134295 + EqualityComparer<Point2d>.Default.GetHashCode(Position);
                hashCode = hashCode * -1521134295 + EqualityComparer<BoltSection>.Default.GetHashCode(BoltDef);
                return hashCode;
            }
        }

        #region Properties

        public UniqueIdCollection<BoltPosition> Bolts { get; }

        #endregion

        #region Public Constructors

        public BoltGrid() :
            this(new double[] { 50, 50, 50 }, new double[] { 50, 50 }, 16)
        { }

        /// <summary>
        /// Creates a rectangular grid of bolts.
        /// </summary>
        /// <param name="stepX">Steps in X.</param>
        /// <param name="stepY">Steps in Y.</param>
        /// <param name="diameter"></param>
        /// <param name="Mat"></param>
        public BoltGrid(IEnumerable<double> stepX, IEnumerable<double> stepY, double diameter, SteelMaterial Mat = null)
        {
            if (Mat == null)
                Mat = new SteelMaterial("10.9", 200000, 940, 1040, 0.3, SteelMaterial.SteelTypes.Structural);

            // Create list of absolute cooridnates.
            var absX = new List<double>();
            var absY = new List<double>();
            absX.Add(0.0);
            absY.Add(0.0);
            foreach (var x in stepX)
                absX.Add(absX.Last() + x);
            foreach (var y in stepY)
                absY.Add(absY.Last() + y);

            // Add bolts respecting a rectangular grid.
            Bolts = new UniqueIdCollection<BoltPosition>();
            foreach (var x in absX)
                foreach (var y in absY)
                    Bolts.Add(new BoltPosition(new Point2d(x, y), new BoltSection(diameter, Mat)));
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Calculate area of the whole group of bolts.
        /// </summary>
        /// <returns>Area.</returns>
        public double CalculateArea()
        {
            double Area = 0;
            foreach (var b in Bolts)
                Area += b.BoltDef.Area;
            return Area;
        }

        /// <summary>
        /// Calculate barycenter of the whole group of bolts.
        /// </summary>
        /// <returns>Barycenter.</returns>
        public Point2d CalculateBarycenter()
        {
            double Area = CalculateArea();
            double S_X = 0; // Static moments.
            double S_Y = 0;

            foreach (var b in Bolts)
            {
                var b_area = b.BoltDef.Area;
                S_X += b_area * b.Position.Y;
                S_Y += b_area * b.Position.X;
            }
            return new Point2d(S_Y / Area, S_X / Area);
        }

        /// <summary>
        /// Calculate polar moment of the whole group of bolts
        /// </summary>
        /// <returns></returns>
        public double CalculatePolarMoment()
        {
            double Area = CalculateArea();
            double I_X = 0; // Inertia moments.
            double I_Y = 0;

            foreach (var b in Bolts)
            {
                var b_area = b.BoltDef.Area;
                I_X += b.BoltDef.J11 + b_area * Math.Pow(b.Position.Y, 2);
                I_Y += b.BoltDef.J22 + b_area * Math.Pow(b.Position.X, 2);
            }
            var G = CalculateBarycenter();

            // Translation on the barycenter.
            double I_X0 = I_X - Area * G.Y * G.Y;
            double I_Y0 = I_Y - Area * G.X * G.X;
            return I_X0 + I_Y0; // Polar moment.
        }

        /// <summary>
        /// Given a stress with point of application, determines a distribution of shear forces in the elastic field
        /// from shear and torsion stresses.
        /// Theory in "\\studio\Software_Development\01 Theory\08 BoltSection\Shear on bolts - elastic distribution.docx".
        /// </summary>
        /// <param name="Soll">Stresses per bolt with reference to bolt ID.</param>
        /// <param name="ForceOnBarycenter">True if Soll is already on barycenter.
        /// The position or reference system of the applied force will be chosen from interface by the user,
        /// normally it will be the center of gravity.</param>
        /// <returns></returns>
        public Dictionary<int, ResultBeamForces> CalculateShearForcesElastic(in ResultBeamForces Soll)
        {
            // Calculate parameters of the whole group of bolts.
            double Area = CalculateArea();
            Point2d G = CalculateBarycenter();
            double I_P0 = CalculatePolarMoment(); // Polar moment.

            // BoltSection group coordinate system.
            var PlateSystem = new CoordinateSystem(G, Vector3d.XAxis, Vector3d.YAxis);
            // Move sollecitation to barycenter.
            ResultBeamForces SollLoc;
            SollLoc = Soll.ToCoordinateSystem(PlateSystem);

            // List of stresses to return.
            var retForces = new Dictionary<int, ResultBeamForces>();
            foreach (var b in Bolts)
            {
                double soll_X = b.BoltDef.Area * (SollLoc.V1 / Area + (b.Position.Y - G.Y) * SollLoc.T / I_P0);
                double soll_Y = b.BoltDef.Area * (SollLoc.V2 / Area + (b.Position.X - G.X) * SollLoc.T / I_P0);
                retForces[b.Id] = new ResultBeamForces(0, soll_X, soll_Y, 0, 0, 0,
                    new CoordinateSystem(new Point3d(b.Position), Vector3d.XAxis, Vector3d.YAxis));
            }

            return retForces;
        }

        /// <summary>
        /// Checking the previous calculation in <see cref="CalculateShearForcesElastic(in ResultBeamForces)"/>.
        /// </summary>
        /// <param name="ForceList">Output of <see cref="CalculateShearForcesElastic(in ResultBeamForces)"/>.</param>
        /// <param name="Soll">Total stress in input in <see cref="CalculateShearForcesElastic(in ResultBeamForces)"/>.</param>
        /// <returns>True if values are correct.</returns>
        public bool CheckShearForcesElastic(in Dictionary<int, ResultBeamForces> ForceList, in ResultBeamForces Soll)
        {
            if (Soll is null || ForceList is null)
                return false;

            ResultBeamForces totForce = new ResultBeamForces(0, 0, 0, 0, 0, 0, Soll.CoordinateSystem);
            foreach (var fl in ForceList)
                totForce += fl.Value;

            return totForce == Soll;
        }

        #endregion
    }
}
