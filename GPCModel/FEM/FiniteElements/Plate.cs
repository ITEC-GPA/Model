using System;
using System.Collections.Generic;
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
    public class Plate : FiniteElement
    {
        protected List<IPlateLoadCaseAttribute> _attributesLoadCase;

        public bool IsTriangle => Nodes.Length == 3 ? true : false;

        public bool IsQuad => Nodes.Length == 4 ? true : false;

        public new PlateProperty Property => (PlateProperty)_property;

        public List<IPlateLoadCaseAttribute> AttributesLoadCase => _attributesLoadCase;


        public Plate(Node[] nodes, PlateProperty property, int id) 
            : base (nodes, property, id)
        {
            _attributesLoadCase = new List<IPlateLoadCaseAttribute>();
        }


        public virtual void AddAttribute(IPlateLoadCaseAttribute attribute)
        {
            _attributesLoadCase.Add(attribute);
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

        public override void GetResults(double[] displacementsNodes, bool displacementsInGlobalCoordinates = true)
        {
            throw new NotImplementedException();
        }
    }
}
