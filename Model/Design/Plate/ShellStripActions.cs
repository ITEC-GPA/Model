using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GPC.Geometry;
using GPC.Model.Elements;
using GPC.Model.Results;
using GPC.Model.Results.Locations;
using GPC.Model.Sections.Concrete;
using GPC.Model.Core.Coordinates;

namespace GPC.Model.Design.Plate
{
    public static class ShellStripActions
    {
        // Explicit uniaxial strip extraction from already determined design actions. No Wood-Armer/Baumann assumption.
        public static ResultBeamForces StripDirection1(ResultPlateForces designActions, double widthMm, CoordinateSystem sectionAxes)
        {
            if (NumericGuard.Finite(widthMm, nameof(widthMm)) <= 0) throw new ArgumentOutOfRangeException(nameof(widthMm));
            Axes.Validate(sectionAxes);
            return new ResultBeamForces(designActions.Fxx * widthMm, 0, designActions.Fxz * widthMm, 0, designActions.Mxx * widthMm, 0, sectionAxes);
        }
    }
}
