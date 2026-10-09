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
    public sealed class PreservedAssignment
    {
        public string Kind { get; set; }
        public string SourceRecord { get; set; }
        public string RawData { get; set; }
        public string UnitsAndAxes { get; set; }
        public string UnsupportedReason { get; set; }
    }
}
