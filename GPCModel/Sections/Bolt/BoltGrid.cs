using GPC.Geometry;
using GPC.Model.Materials;
using GPC.Model.Results;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GPC.Model.Sections.Bolt
{
    /// <summary>
    /// Support class for the list of bolts with their locations.
    /// </summary>
    public partial class BoltGrid : ModelObject
    {
        protected List<BoltPosition> _bolts;

        #region Properties

        public List<BoltPosition> Bolts { get => _bolts; set => _bolts = value; }

        #endregion

        #region Public Constructors

        public BoltGrid(IEnumerable<BoltPosition> bolts, string name = "")
            : this(name)
        {
            _bolts.AddRange(bolts);
        }

        public BoltGrid(string name = "")
            : base(name)
        {
            _bolts = new List<BoltPosition>();
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
            if (Area <= GeometryBase.Tolerance)
                return new Point2d(0.0, 0.0);

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
                I_X += /*b.BoltDef.J11 +*/ b_area * Math.Pow(b.Position.Y, 2);
                I_Y += /*b.BoltDef.J22 +*/ b_area * Math.Pow(b.Position.X, 2);
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
        public Dictionary<BoltPosition, ResultBeamForces> CalculateShearForcesElastic(in ResultBeamForces Soll)
        {
            var retForces = new Dictionary<BoltPosition, ResultBeamForces>();

            // Special case
            if (Bolts.Count == 0)
                return retForces;

            // Calculate parameters of the whole group of bolts.
            double Area = CalculateArea();
            Point2d G = CalculateBarycenter();
            double I_P0 = CalculatePolarMoment(); // Polar moment.

            // BoltSection group coordinate system.
            var PlateSystem = new CoordinateSystem(G, Vector3d.XAxis, Vector3d.YAxis);

            // Special case
            if (Bolts.Count == 1)
            {
                retForces[Bolts.First()] = new ResultBeamForces(0, Soll.V1, Soll.V2, 0, 0, 0,
                    new CoordinateSystem(new Point3d(Bolts.First().Position), Vector3d.XAxis, Vector3d.YAxis), Soll.Id);
                return retForces;
            }

            // Move sollecitation to barycenter.
            ResultBeamForces SollLoc;
            SollLoc = Soll.ToCoordinateSystemWithEccentricity(PlateSystem);

            // List of stresses to return.
            foreach (var b in Bolts)
            {
                double soll_X = b.BoltDef.Area * (SollLoc.V1 / Area + (G.Y - b.Position.Y) * SollLoc.T / I_P0);
                double soll_Y = b.BoltDef.Area * (SollLoc.V2 / Area + (b.Position.X - G.X) * SollLoc.T / I_P0);
                retForces[b] = new ResultBeamForces(0, soll_X, soll_Y, 0, 0, 0,
                    new CoordinateSystem(new Point3d(b.Position), Vector3d.XAxis, Vector3d.YAxis), Soll.Id);
            }

            return retForces;
        }

        /// <summary>
        /// Checking the previous calculation in <see cref="CalculateShearForcesElastic(in ResultBeamForces)"/>.
        /// </summary>
        /// <param name="ForceList">Output of <see cref="CalculateShearForcesElastic(in ResultBeamForces)"/>.</param>
        /// <param name="Soll">Total stress in input in <see cref="CalculateShearForcesElastic(in ResultBeamForces)"/>.</param>
        /// <returns>True if values are correct.</returns>
        public bool CheckShearForcesElastic(in Dictionary<BoltPosition, ResultBeamForces> ForceList, in ResultBeamForces Soll)
        {
            if (Soll is null || ForceList is null)
                return false;

            var GSys = new CoordinateSystem(CalculateBarycenter(), Vector3d.XAxis, Vector3d.YAxis);
            ResultBeamForces totForce = new ResultBeamForces(0, 0, 0, 0, 0, 0, GSys);
            foreach (var fl in ForceList)
                totForce += fl.Value;

            var SollG = Soll.ToCoordinateSystemWithEccentricity(GSys);

            return totForce == SollG;
        }

        public void RemoveBoltById(int id)
        {
            _bolts.RemoveAll(bp => bp.Id == id);
        }

        public BoltPosition AddBolt(double posX, double posY, double diameter, SteelMaterial mat, Hole hole = null)
        {
            // Check that it does not intersect another bolt.
            // The distance must be greater than the sum of the radii.
            var pos = new Point2d(posX, posY);
            foreach (var bp in _bolts)
                if (bp.Position.DistanceTo(pos) <= 0.5 * (diameter + bp.BoltDef.Diameter) + GeometryBase.Tolerance)
                    return null;

            if (hole is null)
                hole = new Hole(diameter + 1.0);

            // Find the next index.
            int nextIndex = 1;
            if (_bolts.Count > 0)
                nextIndex = _bolts.Max(bp => bp.Id) + 1;

            var newBoltPos = new BoltPosition(pos, new BoltSection(diameter, mat), hole, nextIndex);
            _bolts.Add(newBoltPos);
            return newBoltPos;
        }

        /// <summary>
        /// Add a rectangular grid of bolts.
        /// </summary>
        /// <param name="stepX">Steps in X.</param>
        /// <param name="stepY">Steps in Y.</param>
        /// <param name="diameter"></param>
        /// <param name="mat"></param>
        /// <param name="origin">Starting point, bottom right corner.</param>
        public List<BoltPosition> AddBoltsRectangularGrid(IEnumerable<double> stepX, IEnumerable<double> stepY, double diameter, SteelMaterial mat, Point2d origin = null)
        {
            var boltList = new List<BoltPosition>();

            if (origin == null)
                origin = Point2d.Origin;

            // Create list of absolute cooridnates.
            var absX = new List<double>();
            var absY = new List<double>();
            absX.Add(origin.X);
            absY.Add(origin.Y);
            foreach (double x in stepX)
                absX.Add(absX.Last() + x);
            foreach (double y in stepY)
                absY.Add(absY.Last() + y);

            // Add bolts respecting a rectangular grid.
            foreach (double posX in absX)
            {
                foreach (double posY in absY)
                {
                    var addedBolt = AddBolt(posX, posY, diameter, mat);
                    if (addedBolt is null)
                        continue;
                    boltList.Add(addedBolt);
                }
            }
            return boltList;
        }

        #endregion
    }
}
