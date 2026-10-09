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
    public sealed class ShellRebarLayer
    {
        public string PhysicalFace { get; set; }
        public SteelMaterial Steel { get; set; }
        public double Diameter { get; set; }
        public double Pitch { get; set; }
        public double AxisPositionThroughThickness { get; set; }
        public double DirectionRadians { get; set; }
        public int Order { get; set; }
    }
}
