using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Fem.FemObjects.FiniteElements;
using GPC.Model.Fem.FemObjects;
using GPC.Model.Fem;
using GPC.Model.Materials;
using GPC.Model.Sections;
using GPC.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPC.Model.Fem.Attributes;

namespace UnitTestFem
{

    [TestClass]
    public class FemModelBeamTest : UnitTestBase
    {

        private SectionRectangular GetSectionRectangular(string name, double height = 500, double width = 100)
        {
            return new SectionRectangular(height, width, SteelMaterial.S355, name); ;
        }


        private EulerBeam GetBeam(Node nodeStart, Node nodeEnd, double angle = 0)
        {
            var beam = new EulerBeam(nodeEnd, nodeStart, angle);

            return beam;
        }


        [TestMethod]
        public void FemModelTest01()
        {
            FemModel model = new FemModel();

            Node node1 = new Node(0, 0, 0);
            Node node2 = new Node(0, 1500, 0);
            Node node3 = new Node(1500, 1500, 0);
            Node node4 = new Node(1500, 0, 0);

            node1.AddAttribute(new NodeForceAttribute("F", 100, 0, 0));

            var beam1 = GetBeam(node1, node2);
            var beam2 = GetBeam(node2, node3);
            var beam3 = GetBeam(node3, node4);

            string propertyName = "rect";
            var section = GetSectionRectangular(propertyName);

            model.AddProperty(section);

            model.AddFiniteElement(beam1, propertyName);
            model.AddFiniteElement(beam2, propertyName);
            model.AddFiniteElement(beam3, propertyName);

            model.Solve();

        }


    }
}
