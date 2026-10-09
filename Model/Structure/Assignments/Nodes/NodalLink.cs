using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.LoadCases;
using GPC.Model.Restrains;
using GPC.Model.Sections.Concrete;
using GPC.Model.Materials;

namespace GPC.Model.PostProcessing
{

    [Serializable]
    public sealed class NodalLink
    {
        public int OtherNodeId { get; set; }
        public SpringMatrix Spring { get; set; }
        // Rows express C_i u_i + C_j u_j = rhs. This is not an artificial stiff beam.
        public double[] KinematicCoefficients { get; set; }
        public double[] RightHandSide { get; set; }
        public Vector3d LeverArm { get; set; }
        public string SourceRecord { get; set; }
        public string UnsupportedLaw { get; set; }
    }
}
