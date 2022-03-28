using System;
using System.Collections.Generic;
using GPC.Geometry;
using GPC.Model.LoadCases;


namespace GPC.Model.FEM.Attributes
{
    /// <summary>
    /// Represent an acceleration attribute of a <see cref="FiniteElements.Brick"/>
    /// </summary>
    /// <remarks>The visibility of this class is internal since only the <see cref="FemModel"/> is responsibile to apply it to each <see cref="FiniteElements.Brick"/> </remarks>
    internal class BrickAccelerationAttribute : ModelAccelerationAttribute, IEquatable<BrickAccelerationAttribute>, IBrickLoadCaseAttribute
    {

        internal BrickAccelerationAttribute(string loadCaseName, CoordinateSystem coordinateSystem, double a1, double a2, double a3)
            : base(loadCaseName, coordinateSystem, a1, a2, a3)
        {

        }


        internal BrickAccelerationAttribute(BrickAccelerationAttribute brickAccelerationAttribute)
            : this(brickAccelerationAttribute.LoadCaseName, brickAccelerationAttribute.CoordinateSystem, brickAccelerationAttribute.A1, brickAccelerationAttribute.A2, brickAccelerationAttribute.A3)
        {

        }

        public bool Equals(BrickAccelerationAttribute other)
        {
            return base.Equals(other);
        }
    }
}
