using GPC.Geometry;
using GPC.Model.FEM;
using GPC.Model.FEM.Attributes;
using GPC.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FemTest
{
    [TestClass]
    public class FemModelAttributesTest : UnitTestBase
    {

        [TestMethod]
        public void FemAttributeTest1()
        {

            GhostPlate gp = new GhostPlate(new Node[] { new Node(0, 0, 0)});

            gp.AddLoadCaseAttribute(new PlateNormalPressureAttribute("lc1", 1, "lc1"));
            gp.AddLoadCaseAttribute(new PlateNormalPressureAttribute("lc1", 1.5, "lc2"));


            Assert.IsTrue(gp.AttributesLoadCase.Count == 1);

            Assert.IsTrue(gp.AttributesLoadCase.ContainsCaseName("lc1"));
            Assert.IsTrue(gp.AttributesLoadCase.GetElementByCaseName("lc1").Name == "lc2");
            Assert.IsFalse(gp.AttributesLoadCase.GetElementByCaseName("lc1").Name == "lc1");

            
        }


        /// <summary>
        /// This class is only a kind of Moq class for testing since Plate is abstract
        /// </summary>
        internal class GhostPlate : GPC.Model.FEM.FiniteElements.Plate
        {

            public GhostPlate(Node[] nodes) : base(nodes)
            {

            }
        }
    }
}
