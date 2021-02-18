using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Model.FEM;
using GPC.Model.FEM.FiniteElements;

namespace Examples
{
    class Program
    {
        static void Main(string[] args)
        {
            GPC.Model.LoadCases.LoadCase myLoadCase = new GPC.Model.LoadCases.LoadCase("myLoadCase", new Guid());

            List <Node> nodesPlate1 = new List<Node>();
            nodesPlate1.Add(new Node(0, 0, 0, 1, "1"));
            nodesPlate1.Add(new Node(0, 100, 0, 2, "2"));
            nodesPlate1.Add(new Node(100, 0, 0, 3, "3"));

            List<Node> nodesPlate2 = new List<Node>();
            nodesPlate2.Add(new Node(100, 0, 0, 2, "2"));
            nodesPlate2.Add(new Node(0, 100, 0, 3, "3"));
            Node node4 = new Node(100, 100, 0, 4, "4");
            nodesPlate2.Add(node4);

            /*List<Node> nodesVoidElement = new List<Node>();
            nodesVoidElement.Add(new Node(0, 100, 0, -1, "3"));*/

            List<FiniteElement> elements = new List<FiniteElement>();

            elements.Add(new TriangularMembranal(nodesPlate1.ToArray(), 1));
            //elements.Add(new TriangularMembranal(nodesPlate2.ToArray(), 2));
            //elements.Add(new VoidElement(nodesVoidElement));

            FEMModel fem = new FEMModel(elements.ToArray());

            //get results
            Console.ReadKey();
        }
    }
}