using System;
using System.Collections.Generic;
using System.Diagnostics;
using GPC.Model.FEM.Attributes;
using GPC.Model.FEM.Properties;
using mnl = MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.FEM.FiniteElements
{
    // TODO: Classe Plate: farla diventare abstract
    /// <summary>
    /// Va messa abstract una volta che è stabile il fem
    /// </summary>
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    [System.ComponentModel.Description("Verrà messa abstract una volta che il fem è stabile")]
    public class Plate : FiniteElement
    {
        // TODO: rendere abstract

        //contains Material information of the element
        protected mnl.Matrix<double> _d;

        /// <summary>
        /// F,M = [D] * (epsilon, curvature...)
        /// </summary>
        public mnl.Matrix<double> D => _d;

        public bool IsTriangle => Nodes.Length == 3 ? true : false;

        public bool IsQuad => Nodes.Length == 4 ? true : false;

        public new PlateProperty Property => (PlateProperty)_property;


        public Plate(Node[] nodes) : base(nodes)
        {

        }

        public override FiniteElement Duplicate(ElementProperty property, List<LoadCaseAttribute> lcAttributes, List<FreedomCaseAttribute> fdAttributes)
        {
            var plate = new Plate(_nodesGlobal);
            plate.SetProperty(property);
            plate.SetId(Id);

            if(lcAttributes != null)
            { 
                foreach(LoadCaseAttribute attribute in lcAttributes)
                { 
                    if (attribute is IPlateLoadCaseAttribute plca)
                    {
                        plate.AddLoadCaseAttribute(plca);
                    }
                } 
            }

            if (fdAttributes != null)
            {
                foreach (FreedomCaseAttribute attribute in fdAttributes)
                {
                    if (attribute is IPlateFreedomCaseAttribute pfca)
                    {
                        plate.AddFreedomCaseAttribute(pfca);
                    }
                }
            }

            return plate;
        }

        public override FiniteElement Duplicate()
        {
            throw new NotImplementedException();
        }


        public virtual void AddLoadCaseAttribute(IPlateLoadCaseAttribute attribute)
        {
            _attributesLoadCase.Add((LoadCaseAttribute)attribute);
        }


        public virtual void AddFreedomCaseAttribute(IPlateFreedomCaseAttribute attribute)
        {
            _attributesFreedomCase.Add((FreedomCaseAttribute)attribute);
        }


        protected override mnl.Vector<double> BuildFLocalCoord()
        {
            throw new NotImplementedException();
        }

        public override void BuildMatrix()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// usually = B : derivative of ShapeFunctions, need for epsilon = [B] * q with q = node displacements vector
        /// </summary>
        /// <param name="csi">natural coordinate -1 to 1</param>
        /// <param name="eta">natural coordinate -1 to 1</param>
        /// <returns></returns>
        public virtual mnl.Matrix<double> GetB(double csi = 0, double eta = 0)
        {
            throw new NotImplementedException();
        }

        //TODO: Da ottimizzare/scrivere
        public new void GetNodesResults(double[] globalDisplacementsNodes, out mnl.Matrix<double>[] gloabalPseudoDeformation, out mnl.Matrix<double>[] localPseudoDeformation, out mnl.Matrix<double>[] globalForces, out mnl.Matrix<double>[] localForces, out mnl.Matrix<double>[] globalStress, out mnl.Matrix<double>[] localStress, out mnl.Matrix<double>[] globalEpsilon, out mnl.Matrix<double>[] localEpsilon)
        {
            throw new NotImplementedException();
        }

        private string GetDebuggerDisplay()
        {
            var prop = Property != null ? Property.Name : String.Empty;
            return $"Plate, Id: {Id}, PropertyName: {prop}";
        }



        // GetNodalDisplacement()

        // GetGaussPointStress() => List<ResultPLateStress> [ngauspoint * 3facce]

        // GetNodalStress() => List<ResultPLateStress> [ngauspoint * 3facce]

    }
}
