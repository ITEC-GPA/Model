using GPC.Model.FEM;
using GPC.Model.FEM.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Examples
{
    class Program
    {
        static void Main(string[] args)
        {
            Node n1 = new Node(0, 0, 0, 1);
            Node n2 = new Node(0, 0, 0, 2);
            Console.WriteLine(n1 == n2);

            List <Node> nodesPlate1 = new List<Node>();
            nodesPlate1.Add(new Node(0, 0, 0, -1, "1"));
            nodesPlate1.Add(new Node(100, 0, 0, -1, "2"));
            nodesPlate1.Add(new Node(0, 100, 0, -1, "3"));

            List<Node> nodesPlate2 = new List<Node>();
            nodesPlate2.Add(new Node(100, 0, 0, -1, "2"));
            nodesPlate2.Add(new Node(0, 100, 0, -1, "3"));
            nodesPlate2.Add(new Node(100, 100, 0, -1, "4"));

            List<Node> nodesVoidElement = new List<Node>();
            nodesVoidElement.Add(new Node(0, 100, 0, -1, "3"));

            List<FiniteElement> elements = new List<FiniteElement>();

            elements.Add(new TriangularMembranal(nodesPlate1, 1));
            elements.Add(new TriangularMembranal(nodesPlate2, 2));
            //elements.Add(new VoidElement(nodesVoidElement));

            FEMModel fem = new FEMModel(elements.ToArray());

            //get results
            Console.ReadKey();
        }
    }
}