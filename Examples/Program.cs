using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Model.Elements;
using GPC.Model.FEM;
using GPC.Model.FEM.Attributes;
using GPC.Model.FEM.FiniteElements;
using GPC.Model.FreedomCases;
using GPC.Model.LoadCases;
using GPC.Model.Materials;

namespace Examples
{
    class Program
    {
        static void Main(string[] args)
        {
            Material mat = new SteelMaterial("steel", 200000, 0.2, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat, 0, 1);

            //LoadCase myLoadCase = new LoadCase("myLoadCase", new Guid());
            FreedomCase fc = new FreedomCase("freedomCase1");

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            NodeRestrainAttribute DXDYDZ = new NodeRestrainAttribute(fc, sys);
            DXDYDZ.AddRestrain(FEMModel.DOF.DX);
            DXDYDZ.AddRestrain(FEMModel.DOF.DY);
            DXDYDZ.AddRestrain(FEMModel.DOF.DZ);

            NodeRestrainAttribute DZ = new NodeRestrainAttribute(fc, sys);
            DZ.AddRestrain(FEMModel.DOF.DZ);

            List<Node> nodesPlate1 = new List<Node>();
            Node nd1 = new Node(0, 0, 0, 1, "1");
            Node nd2 = new Node(0, 100, 0, 2, "2");
            Node nd3 = new Node(100, 0, 0, 3, "3");
            

            nd1.AddAttribute(DXDYDZ);
            nd2.AddAttribute(DXDYDZ);

            nodesPlate1.Add(nd1);
            nodesPlate1.Add(nd2);
            nodesPlate1.Add(nd3);

            List<Node> nodesPlate2 = new List<Node>();
            Node nd2copy = new Node(0, 100, 0, 2, "2");
            Node nd3copy = new Node(100, 0, 0, 3, "3");
            Node nd4 = new Node(100, 100, 0, 4, "4");

            nd2copy.AddAttribute(DZ);
            nd3copy.AddAttribute(DZ);
            nd4.AddAttribute(DZ);

            nodesPlate2.Add(nd2copy);
            nodesPlate2.Add(nd3copy);
            nodesPlate2.Add(nd4);
            
            /*List<Node> nodesVoidElement = new List<Node>();
            nodesVoidElement.Add(new Node(0, 100, 0, -1, "3"));*/

            List<FiniteElement> elements = new List<FiniteElement>();
            elements.Add(new TriangularMembranal(nodesPlate1.ToArray(), prop, 1));
            elements.Add(new TriangularMembranal(nodesPlate2.ToArray(), prop, 2));
            //elements.Add(new VoidElement(nodesVoidElement));

            FEMModel fem = new FEMModel(elements.ToArray());

            //get results
            Console.ReadKey();
        }
    }
}