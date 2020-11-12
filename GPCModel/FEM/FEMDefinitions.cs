using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.FEM
{
    public enum PlateAnalysisType
    {
        UnknownAnalysis = -1,
        NormalAnalysis = 0,
        PlaneStress,
        PlaneStrain,
        Axisymmetric
    };

    public enum PlateBendingPlate
    {
        UnknownBendPlate = -1,
        ThinAnalysis = 0,
        ThickAnalysis,
    };

    public enum CSystemType
    {
        SystTypeCartesian,
        SystTypeCylindrical,
        SystTypeSpherical,
        SystTypeToroidal,
    };
}
