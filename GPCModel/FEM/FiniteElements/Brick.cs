using System;
using System.Collections.Generic;
using System.Diagnostics;
using GPC.Model.FEM.Attributes;
using GPC.Model.FEM.Properties;
using mnl = MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.FEM.FiniteElements
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public class Brick : FiniteElement
    {
        //contains Material information of the element
        protected mnl.Matrix<double> _d;

        /// <summary>
        /// F,M = [D] * (epsilon, curvature...)
        /// </summary>
        public mnl.Matrix<double> D => _d;

        public Brick(Node[] nodes) 
            : base(nodes)
        {

        }

        /// <summary>
        /// Convert attribute in node forces
        /// </summary>
        /// <returns></returns>
        protected override mnl.Vector<double> BuildFLocalCoord()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create K matrix, B matrix etc
        /// </summary>
        public override void BuildMatrix()
        {
            throw new NotImplementedException();
        }

        public virtual mnl.Matrix<double> GetB(double csi, double eta, double zeta)
        {
            throw new NotImplementedException();
        }

        #region Result
        //TODO: Da ottimizzare/scrivere
        /*public override void GetNodesResults(double[] globalDisplacementsNodes, out mnl.Matrix<double>[] gloabalPseudoDeformation, out mnl.Matrix<double>[] localPseudoDeformation, out mnl.Matrix<double>[] globalForces, out mnl.Matrix<double>[] localForces, out mnl.Matrix<double>[] globalStress, out mnl.Matrix<double>[] localStress, out mnl.Matrix<double>[] globalEpsilon, out mnl.Matrix<double>[] localEpsilon)
        {
            throw new NotImplementedException();
        }*/
        #endregion

        /// <summary>
        /// reference eq. 11.10 - Finite element method by Rao
        /// </summary>
        /// <param name="E"></param>
        /// <param name="poisson"></param>
        /// <returns></returns>
        public static mnl.Matrix<double> GetD (double E, double poisson)
        {
            double factor = E / ((1.0 + poisson) * (1.0 - 2.0 * poisson));

            mnl.Matrix<double> d = mnl.Matrix<double>.Build.Dense(6, 6);

            d[0, 0] = 1.0 - poisson;
            d[0, 1] = poisson;
            d[0, 2] = poisson;

            d[1, 0] = poisson;
            d[1, 1] = 1.0 - poisson;
            d[1, 2] = poisson;

            d[2, 0] = poisson;
            d[2, 1] = poisson;
            d[2, 2] = (1.0 - poisson);

            d[3, 3] = (1.0 - 2.0 * poisson) / 2.0;

            d[4, 4] = (1.0 - 2.0 * poisson) / 2.0;

            d[5, 5] = (1.0 - 2.0 * poisson) / 2.0;

            /*Console.WriteLine("D");
            Util.WriteMatrix(factor * d);*/
            return factor * d;
        }

        public override FiniteElement Duplicate(ElementProperty property, List<LoadCaseAttribute> attributes, List<FreedomCaseAttribute> fdAttributes)
        {
            throw new NotImplementedException();
        }

        public override FiniteElement Duplicate()
        {
            throw new NotImplementedException();
        }

        private string GetDebuggerDisplay()
        {
            return $"Brick, Id: {Id}, PropertyName: {Property.Name}";
        }
    }
}
