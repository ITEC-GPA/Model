using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Elements;
using GPC.Model.FEM.Attributes;
using GPC.Model.FEM.Properties;
using MathNet.Numerics.LinearAlgebra;
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

        public override mnl.Matrix<double> GetB(double csi = 0, double eta = 0, double zeta = 0)
        {
            throw new NotImplementedException();
        }

        public override void GetNodesResults(double[] globalDisplacementsNodes, out double[] localDisplacements, out mnl.Matrix<double>[] gloabalPseudoDeformation, out mnl.Matrix<double>[] localPseudoDeformation, out mnl.Matrix<double>[] globalForces, out mnl.Matrix<double>[] localForces, out mnl.Matrix<double>[] globalStress, out mnl.Matrix<double>[] localStress, out mnl.Matrix<double>[] globalEpsilon, out mnl.Matrix<double>[] localEpsilon)
        {
            throw new NotImplementedException();
        }

        public override void GetResultPositionNaturalCoordinates(double csi, double eta, double zeta, double[] globalDisplacementsNodes, out double x, out double y, out double z, out double[] localDisplacements, out Matrix<double> gloabalPseudoDeformation, out Matrix<double> localPseudoDeformation, out Matrix<double> globalForces, out Matrix<double> localForces, out Matrix<double> globalStress, out Matrix<double> localStress, out Matrix<double> globalEpsilon, out Matrix<double> localEpsilon)
        {
            throw new NotImplementedException();
        }

        private string GetDebuggerDisplay()
        {
            return $"Plate, Id: {Id}, PropertyName: {Property.Name}";
        }



        // GetNodalDisplacement()

        // GetGaussPointStress() => List<ResultPLateStress> [ngauspoint * 3facce]

        // GetNodalStress() => List<ResultPLateStress> [ngauspoint * 3facce]

        /// <summary>
        /// Matrice stato piano di tensione da materiale elastico lineare isotropo
        /// </summary>
        /// <param name="E"></param>
        /// <param name="ni"></param>
        /// <returns></returns>
        public static mnl.Matrix<double> DPlaneStress(double E, double ni)
        {
            mnl.Matrix<double> D = mnl.Matrix<double>.Build.Dense(3, 3);
            D[0, 0] = 1.0;
            D[0, 1] = ni;
            D[1, 0] = ni;
            D[1, 1] = 1.0;
            D[2, 2] = (1.0 - ni) / 2.0;
            D = E / (1.0 - ni * ni) * D;
            return D;
        }
    }
}
