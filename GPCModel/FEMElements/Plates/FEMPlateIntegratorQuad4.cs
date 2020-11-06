using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using System.IO;
using GPC.Model.Sections;

namespace GPC.Model.FEM
{
    public class FEMPlateIntegratorQuad4 : FEMPlateIntegrator
    {
        #region Variables 
        protected FEMShape _shape;
        #endregion

        #region Properties
        public FEMShape Shape => _shape;
        #endregion

        #region Public Constructors
        public FEMPlateIntegratorQuad4(Guid guid, FEMShape shape)
            : base(guid)
        {
            _shape = shape;
        }
        public FEMPlateIntegratorQuad4(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        #endregion
        #region Public Methods Override
        #endregion

        #region Public Methods Specific
        public override void StartIntegration(FEMElement element)
        {
            Plate plate = element as Plate;   
            BuildD(plate);

            for(int nd = 0; nd < plate.Nodes.Length; nd++)
            {

            }
        }
        public override void BuildD(FEMElement element)
        {
            Plate plate = element as Plate;

            _DPlane = Matrix<double>.Build.Dense(3, 3, 0);
            _DBending = Matrix<double>.Build.Dense(3, 3, 0);

            double E = plate.Property.Material.ElasticModulus;
            double ni = plate.Property.Material.Poisson;
            double tb = plate.Property.Tb;
            double tm = plate.Property.Tm;

            double c, cc;
            c = E / (1 - Math.Pow(ni, 2.0));
            _DPlane[0, 0] = c;
            _DPlane[1, 1] = c;
            _DPlane[2, 2] = 0.5 * c * (1.0 - ni);
            _DPlane[0, 1] = ni * c;
            _DPlane[1, 0] = _DPlane[0, 1];

            cc = c * Math.Pow(tb, 3.0) / 12.0;
            _DBending[0, 0] = cc;
            _DBending[1, 1] = cc;
            _DBending[2, 2] = 0.5 * cc * (1.0 - ni);
            _DBending[0, 1] = ni * cc;
            _DBending[1, 0] = _DBending[0, 1];
        }

        public override void BuildJ()
        {
        }
        public override void BuildK(FEMElement element)
        {
            Plate plate = element as Plate;
        }
        public override void BuildT()
        {
        }
        public override void BuildF()
        {
        }
        public override void BuildB()
        {
        }
        public override void RegisterDoF(Node node)
        {
        }
        public override void BuildTrfMatrix(FEMElement element)
        {
            Plate plate = element as Plate;
        }      
        #endregion

        #region Private Methods Specific
        #endregion
    }
}
