using System;
using System.Collections.Generic;
using GPC.Geometry;
using GPC.Model.LoadCases;

namespace GPC.Model.FEM.Attributes
{
    /// <summary>
    /// Represent an acceleration attribute of a <see cref="FiniteElements.Plate"/>
    /// </summary>
    /// <remarks>The visibility of this class is internal since only the <see cref="FemModel"/> is responsibile to apply it to each <see cref="FiniteElements.Plate"/> </remarks>
    internal sealed class PlateAccelerationAttribute : ModelAccelerationAttribute, IEquatable<PlateAccelerationAttribute>, IPlateLoadCaseAttribute
    {

        internal PlateAccelerationAttribute(string loadCaseName, CoordinateSystem coordinateSystem, double a1, double a2, double a3)
            : base(loadCaseName, coordinateSystem, a1, a2, a3)
        {

        }


        internal PlateAccelerationAttribute(PlateAccelerationAttribute beamAccelerationAttribute)
            : this(beamAccelerationAttribute.LoadCaseName, beamAccelerationAttribute.CoordinateSystem,
                  beamAccelerationAttribute.A1, beamAccelerationAttribute.A2, beamAccelerationAttribute.A3)
        {

        }

        public bool Equals(PlateAccelerationAttribute other)
        {
            return base.Equals(other);
        }
    }
}