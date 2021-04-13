using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Elements;
using GPC.Model.FEM.Attributes;
using GPC.Model.FEM.Properties;
using GPC.Model.FreedomCases;
using GPC.Model.Sections;
using MathNet.Numerics.LinearAlgebra;
using mnl = MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.FEM.FiniteElements
{
    /// <summary>
    /// Based on Finite Element by Rao - Chapter 9
    /// </summary>
    public abstract class Beam : FiniteElement
    {
        public Beam(Node[] nodes) : base(nodes) { }
    }
}
