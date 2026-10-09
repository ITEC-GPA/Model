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
    public sealed class RestrainAssignment
    {
        public NodeRestrain Restrain { get; set; }
        public ILoadCase Case { get; set; }
        public string Phase { get; set; }
        public string SourceRecord { get; set; }
    }
}
