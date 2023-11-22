using GPC.Geometry;
using GPC.Model.Fem;
using GPC.Model.FreedomCases;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Restrains
{
    public class LineRestrain : GeometryRestrain
    {
        #region Variables

        private Line3d _line;

        #endregion

        #region Properties

        public Line3d Line { get => _line; set => _line = value; }

        #endregion

        #region Public Constructors

        /// <remarks><see cref="GeometryRestrain.CoordinateSystem"/> set to Global</remarks>
        public LineRestrain(Line3d line, FreedomCase freedomCase, List<DofRestrain> restrains, string name = "")
            : this(line, freedomCase, CoordinateSystem.Global, restrains, name)
        {

        }

        public LineRestrain(Line3d line, FreedomCase freedomCase, CoordinateSystem coordinateSystem, List<DofRestrain> restrains, string name = "")
            : base(freedomCase, coordinateSystem, restrains, name)
        {
            _line = line ?? throw new ArgumentNullException("Base line is null");
        }

        protected LineRestrain(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _line = (Line3d)info.GetValue("Line", typeof(Line3d));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Set all the <see cref="Solver.DOF.DX"/>, <see cref="Solver.DOF.DY"/>, <see cref="Solver.DOF.DZ"/> and <see cref="Solver.DOF.RX"/>, <see cref="Solver.DOF.RY"/> and <see cref="Solver.DOF.RZ"/> to restrained for the given line and freedomcase
        /// </summary>
        public static LineRestrain GetAllFixed(Line3d line, FreedomCase freedomCase, CoordinateSystem coordinateSystem)
        {
            return new LineRestrain(line, freedomCase, coordinateSystem, new List<DofRestrain>
                                                                        {
                                                                            new DofRestrain(Solver.DOF.DX),
                                                                            new DofRestrain(Solver.DOF.DY),
                                                                            new DofRestrain(Solver.DOF.DZ),
                                                                            new DofRestrain(Solver.DOF.RX),
                                                                            new DofRestrain(Solver.DOF.RY),
                                                                            new DofRestrain(Solver.DOF.RZ)
                                                                        }
            );
        }

        /// <summary>
        /// Set <see cref="Solver.DOF.DX"/>, <see cref="Solver.DOF.DY"/> and <see cref="Solver.DOF.DZ"/> to restrained for the given line and freedomcase
        /// </summary>
        public static LineRestrain GetAllDisplacementFixed(Line3d line, FreedomCase freedomCase, CoordinateSystem coordinateSystem)
        {
            return new LineRestrain(line, freedomCase, coordinateSystem, new List<DofRestrain>
                                                                            {
                                                                                new DofRestrain(Solver.DOF.DX),
                                                                                new DofRestrain(Solver.DOF.DY),
                                                                                new DofRestrain(Solver.DOF.DZ)
                                                                            }
            );
        }

        public override GeometryBase GetGeometry() => _line;

        #endregion

        #region Equals, hashcode, operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Line", _line);
        }

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
