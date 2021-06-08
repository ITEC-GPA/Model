using GPC.Geometry;
using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;

namespace GPC.Model.FEM.Attributes
{

    /// <summary>
    /// Represent an acceleration attribute of a <see cref="FiniteElements.Beam"/>
    /// </summary>
    /// <remarks>The visibility of this class is internal since only the <see cref="FemModel"/> is responsibile to apply it to each <see cref="FiniteElements.Beam"/> </remarks>
    internal sealed class BeamAccelerationAttribute : ModelAccelerationAttribute, IEquatable<BeamAccelerationAttribute>, IBeamLoadCaseAttribute
    {

        internal BeamAccelerationAttribute(string loadCaseName, CoordinateSystem coordinateSystem, double a1, double a2, double a3) 
            : base(loadCaseName, coordinateSystem, a1, a2, a3)
        {

        }


        internal BeamAccelerationAttribute(BeamAccelerationAttribute beamAccelerationAttribute)
            : this(beamAccelerationAttribute.LoadCaseName, beamAccelerationAttribute.CoordinateSystem, beamAccelerationAttribute.A1, beamAccelerationAttribute.A2, beamAccelerationAttribute.A3)
        {

        }

        public bool Equals(BeamAccelerationAttribute other)
        {
            return base.Equals(other);
        }
    }
}
