using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using GPC.Geometry;
using GPC.Model.FEM;
using GPC.Model.FreedomCases;

namespace GPC.Model.Restrains
{
    public class PointRestrain : GeometryRestrain
    {
        #region Variables

        private readonly Point3d _point;

        #endregion

        #region Properties

        public Point3d Point => _point;


        #endregion

        #region Constructors

        /// <remarks><see cref="GeometryRestrain.CoordinateSystem"/> set to Global</remarks>
        public PointRestrain(Point3d point, FreedomCase freedomCase, List<DofRestrain> restrains)
            : this(point, freedomCase, CoordinateSystem.Global, restrains, Guid.NewGuid(), string.Empty)
        {

        }

        public PointRestrain(Point3d point, FreedomCase freedomCase, CoordinateSystem coordinateSystem, List<DofRestrain> restrains)
            : this(point, freedomCase, coordinateSystem, restrains, Guid.NewGuid(), string.Empty)
        {

        }

        public PointRestrain(Point3d point, FreedomCase freedomCase, CoordinateSystem coordinateSystem, List<DofRestrain> restrains, Guid guid, string name)
            : base(freedomCase, coordinateSystem, restrains, guid, name)
        {
            _point = point ?? throw new ArgumentNullException("Base point can't be null");
        }

        public PointRestrain(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            _point = (Point3d)info.GetValue("Point", typeof(Point3d));
        }


        /// <summary>
        /// Set all the <see cref="Solver.DOF"/> to restrained for the given point and freedomcase
        /// </summary>
        public static PointRestrain GetAllFixed(Point3d point, FreedomCase freedomCase, CoordinateSystem coordinateSystem) 
        {
            List<DofRestrain> restrains = new List<DofRestrain>();

            foreach (var dof in (Solver.DOF[])Enum.GetValues(typeof(Solver.DOF)))
            {
                restrains.Add(new DofRestrain(dof, true));
            }

            return new PointRestrain(point, freedomCase, coordinateSystem, restrains);                
        }

        /// <summary>
        /// Set <see cref="Solver.DOF.DX"/>, <see cref="Solver.DOF.DY"/> and <see cref="Solver.DOF.DZ"/> to restrained for the given line and freedomcase
        /// </summary>
        public static PointRestrain GetAllDisplacementFixed(Point3d point, FreedomCase freedomCase, CoordinateSystem coordinateSystem)
        {
            return new PointRestrain(point, freedomCase, coordinateSystem, new List<DofRestrain>
                                                                        {
                                                                            new DofRestrain(Solver.DOF.DX, true),
                                                                            new DofRestrain(Solver.DOF.DY, true),
                                                                            new DofRestrain(Solver.DOF.DZ, true)
                                                                        }
            );
        }

        #endregion

        #region Public methods

        public override GeometryBase GetGeometry() => _point;

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Point", _point);
        }


        #endregion


        #region Equals, hashcode, operators


        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return (obj is PointRestrain objCasted) && _point.Equals(objCasted.Point) && base.Equals(objCasted);
        }


        public override int GetHashCode()
        {
            unchecked
            {
                return -391 + _point.GetHashCode() * -17 + base.GetHashCode();
            }
        }

        public static bool operator ==(PointRestrain obj1, PointRestrain obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            return obj1.Equals(obj2);
        }


        public static bool operator !=(PointRestrain obj1, PointRestrain obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
