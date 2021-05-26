using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.FEM;
using GPC.Model.FreedomCases;

namespace GPC.Model.Restrains
{
    public class LineRestrain : GeometryRestrain
    {
        #region Variables

        private readonly Line3d _line;

        #endregion

        #region Properties

        public Line3d Line => _line;

        #endregion

        #region Public Constructors


        /// <remarks><see cref="GeometryRestrain.CoordinateSystem"/> set to Global</remarks>
        public LineRestrain(Line3d line, FreedomCase freedomCase, List<DofRestrain> restrains)
            : this(line, freedomCase, CoordinateSystem.Global, restrains, Guid.NewGuid(), string.Empty)
        {

        }

        public LineRestrain(Line3d line, FreedomCase freedomCase, CoordinateSystem coordinateSystem, List<DofRestrain> restrains) 
            : this(line, freedomCase, coordinateSystem, restrains, Guid.NewGuid(), string.Empty)
        {

        }

        public LineRestrain(Line3d line, FreedomCase freedomCase, CoordinateSystem coordinateSystem, List<DofRestrain> restrains, Guid guid, string name) 
            : base(freedomCase, coordinateSystem, restrains, guid, name)
        {
            this._line = line ?? throw new ArgumentNullException("Base line is null");
        }

        /// <summary>
        /// Set all the <see cref="Solver.DOF"/> to restrained for the given line and freedomcase
        /// </summary>
        public static LineRestrain GetAllFixed(Line3d line, FreedomCase freedomCase, CoordinateSystem coordinateSystem)
        {
            List<DofRestrain> restrains = new List<DofRestrain>();

            foreach (var dof in (Solver.DOF[])Enum.GetValues(typeof(Solver.DOF)))
            {
                restrains.Add(new DofRestrain(dof, true));
            }

            return new LineRestrain(line, freedomCase, coordinateSystem, restrains);
        }

        /// <summary>
        /// Set <see cref="Solver.DOF.DX"/> <see cref="Solver.DOF.DX"/>, <see cref="Solver.DOF.DY"/> and <see cref="Solver.DOF.DZ"/> to restrained for the given line and freedomcase
        /// </summary>
        public static LineRestrain GetAllDisplacementFixed(Line3d line, FreedomCase freedomCase, CoordinateSystem coordinateSystem)
        {
            return new LineRestrain(line, freedomCase, coordinateSystem, new List<DofRestrain>
                        {
                            new DofRestrain(Solver.DOF.DX, true),
                            new DofRestrain(Solver.DOF.DY, true),
                            new DofRestrain(Solver.DOF.DZ, true)
                        }
            );
        }

        public LineRestrain(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _line = (Line3d)info.GetValue("Line", typeof(Line3d));
        }


        #endregion

      
        public override GeometryBase GetGeometry() => _line;


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Line", _line);
        }



        #region Equals, hashcode, operators


        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return (obj is LineRestrain objCasted) && _line.Equals(objCasted.Line) && base.Equals(objCasted);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (-391 + base.GetHashCode()) * -17 + _line.GetHashCode();
            }
        }

        public static bool operator ==(LineRestrain obj1, LineRestrain obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            return obj1.Equals(obj2);
        }


        public static bool operator !=(LineRestrain obj1, LineRestrain obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion

    }
}
