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
    public sealed class ShellAssignments
    {
        public double? PhysicalThickness { get; set; }
        public double? Offset { get; set; }
        public CoordinateSystem LayerAxes { get; set; }
        public List<ShellRebarLayer> Layers { get; private set; } = new List<ShellRebarLayer>();
        public string ReinforcementZone { get; set; }
    }
}
