using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FEM.Elements;
using MathNet.Numerics.LinearAlgebra;

namespace FEM
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Node> nodesPlate1 = new List<Node>();
            nodesPlate1.Add(new Node(  0,   0, 0, "1"));
            nodesPlate1.Add(new Node(  100, 0, 0, "2"));
            nodesPlate1.Add(new Node(0, 100, 0, "3"));

            List<Node> nodesPlate2 = new List<Node>();
            nodesPlate2.Add(new Node(100, 0, 0, "2"));
            nodesPlate2.Add(new Node(0, 100, 0, "3"));
            nodesPlate2.Add(new Node(100, 100, 0, "4"));

            List<Elements.FiniteElement> elements = new List<FiniteElement>();

            elements.Add(new TriangularMembranal(nodesPlate1));
            elements.Add(new TriangularMembranal(nodesPlate2));
            
            FEMModel fem = new FEMModel(elements.ToArray());

            //get results

            Console.ReadKey();
        }
    }
}
