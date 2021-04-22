using GPC.Geometry;
using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;


namespace GPC.Model.FEM.Attributes
{
    internal class BrickAccelerationAttribute : ModelAccelerationAttribute, IEquatable<BrickAccelerationAttribute>, IBrickLoadCaseAttribute
    {

        internal BrickAccelerationAttribute(LoadCase loadCase, CoordinateSystem coordinateSystem, double a1, double a2, double a3)
            : base(loadCase, coordinateSystem, a1, a2, a3)
        {

        }


        internal BrickAccelerationAttribute(BrickAccelerationAttribute brickAccelerationAttribute)
            : this(brickAccelerationAttribute.LoadCase, brickAccelerationAttribute.CoordinateSystem, brickAccelerationAttribute.A1, brickAccelerationAttribute.A2, brickAccelerationAttribute.A3)
        {

        }

        public bool Equals(BrickAccelerationAttribute other)
        {
            return base.Equals(other);
        }
    }
}
