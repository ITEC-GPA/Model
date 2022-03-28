using System;
using System.Collections.Generic;
using System.Diagnostics;
using GPC.Model.FEM.Attributes;
using GPC.Model.FEM.Properties;
using GPC.Model.Results;
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

        public bool IsTriangular => Nodes.Length == 6 ? true : false;

        public bool IsQuadrangular => Nodes.Length == 8 ? true : false;

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

        public void AddResult(BrickResult result)
        {
            base.AddResult(result);
        }

        public override void AddResult(FiniteElementResult result)
        {
            if (result is BrickResult)
            {
                base.AddResult(result);
            }
            else
            {
                throw new ArgumentException();
            }
        }


        public virtual bool AddLoadCaseAttribute(IBrickLoadCaseAttribute attribute, out bool replaced)
        {
            return _attributesLoadCase.Add((LoadCaseAttribute)attribute, out replaced);
        }

        public virtual bool AddLoadCaseAttribute(IBrickLoadCaseAttribute attribute)
        {
            return _attributesLoadCase.Add((LoadCaseAttribute)attribute);
        }

        //public virtual void AddFreedomCaseAttribute(Ibri attribute)
        //{
        //    AddFreedomCaseAttribute(attribute);
        //}


        //TODO: Da ottimizzare/scrivere
        /*public override void GetNodesResults(double[] globalDisplacementsNodes, out mnl.Matrix<double>[] gloabalPseudoDeformation, out mnl.Matrix<double>[] localPseudoDeformation, out mnl.Matrix<double>[] globalForces, out mnl.Matrix<double>[] localForces, out mnl.Matrix<double>[] globalStress, out mnl.Matrix<double>[] localStress, out mnl.Matrix<double>[] globalEpsilon, out mnl.Matrix<double>[] localEpsilon)
        {
            throw new NotImplementedException();
        }*/
        #endregion

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
