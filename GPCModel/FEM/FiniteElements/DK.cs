using GPC.Geometry;
using GPC.Model.FEM.Materials;
using GPC.Model.FEM.Properties;
using mnl = MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.FEM.FiniteElements
{
    public abstract class DK : Plate
    {
        protected DK(Node[] nodes) : base(nodes)
        {
        }

        #region Results
        /// <summary>
        /// 
        /// </summary>
        /// <param name="csi"></param>
        /// <param name="eta"></param>
        /// <param name="globalDisplacements"></param>
        /// <returns>tensor curvature xx, yy, xy</returns>
        private mnl.Matrix<double> GetCurvaturesLocalCoordinates(double csi, double eta, double[] globalDisplacements)
        {
            var localDisplacements = GetLocalDisplacementVector(globalDisplacements);

            //curvature = B * U
            //contains curvature xx, yy, xy
            mnl.Vector<double> curvatureLocal = GetB(csi, eta) * localDisplacements;

            mnl.Matrix<double> curvatureTensor = mnl.Matrix<double>.Build.Dense(3, 3);
            curvatureTensor[0, 0] = curvatureLocal[0]; //xx

            curvatureTensor[0, 1] = curvatureLocal[2]; //xy
            curvatureTensor[1, 0] = curvatureLocal[2]; //yx

            curvatureTensor[1, 1] = curvatureLocal[1]; //yy

            return curvatureTensor;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="csi"></param>
        /// <param name="eta"></param>
        /// <param name="globalDisplacements"></param>
        /// <param name="newSys">if null, local axis system are used</param>
        /// <returns></returns>
        public mnl.Matrix<double> GetCurvatures(double csi, double eta, double[] globalDisplacements, CoordinateSystem newSys = null)
        {
            var localCurvatures = GetCurvaturesLocalCoordinates(csi, eta, globalDisplacements);

            return FEMUtilities.RotateTensor(localCurvatures, _localCoordinateSystem, newSys);
        }

        public mnl.Matrix<double> GetBending(double csi, double eta, double[] globalDisplacements, CoordinateSystem newSys = null)
        {
            var localCurvatures = GetCurvaturesLocalCoordinates(csi, eta, globalDisplacements);

            var vecLocalCurvatures = mnl.Vector<double>.Build.Dense(new double[] { localCurvatures[0, 0], localCurvatures[1, 1], localCurvatures[1, 0] });

            var localBending = _d * vecLocalCurvatures;

            var tensorLocalBending = mnl.Matrix<double>.Build.Dense(3, 3);
            tensorLocalBending[0, 0] = localBending[0]; //mxx

            tensorLocalBending[0, 1] = localBending[2]; //mxy
            tensorLocalBending[1, 0] = localBending[2]; //myx

            tensorLocalBending[1, 1] = localBending[1]; //myy

            return FEMUtilities.RotateTensor(tensorLocalBending, _localCoordinateSystem, newSys);
        }

        public mnl.Matrix<double> GetStrains(Face face, double csi, double eta, double[] globalDisplacements, CoordinateSystem newSys = null)
        {
            double h = ((PlateProperty)Property).BendingThickness;

            var localCurvatures = GetCurvaturesLocalCoordinates(csi, eta, globalDisplacements);

            var newCurvatures = FEMUtilities.RotateTensor(localCurvatures, _localCoordinateSystem, newSys);

            var strain = mnl.Matrix<double>.Build.Dense(3, 3);
            if (face == Face.Top)
            {
                return h / 2.0 * newCurvatures;
            }
            else if (face == Face.Bottom)
            {
                return -h / 2.0 * newCurvatures;
            }
            else
            {
                return strain;
            }
        }

        public mnl.Matrix<double> GetStress(Plate.Face face, double csi, double eta, double[] globalDisplacements, CoordinateSystem newSys = null)
        {
            var strains = GetStrains(face, csi, eta, globalDisplacements, newSys);

            var strainVec = mnl.Vector<double>.Build.Dense(3);
            strainVec[0] = strains[0, 0]; //exx
            strainVec[1] = strains[1, 1]; //eyy
            strainVec[2] = strains[0, 1]; //exy

            var planeStressMatrix = ((IsotropicFemMaterial)((PlateProperty)_property).Material).GetPlaneStress();
            var stressVector = planeStressMatrix * strainVec;

            var tensorStress = mnl.Matrix<double>.Build.Dense(3, 3);
            tensorStress[0, 0] = stressVector[0];

            tensorStress[0, 1] = stressVector[2];
            tensorStress[1, 0] = stressVector[2];

            tensorStress[1, 1] = stressVector[1];

            return tensorStress;
        }
        #endregion
    }
}
