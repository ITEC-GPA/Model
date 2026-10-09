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

    /// <summary>Mass in kg; rotational inertia in kg mm^2; cross terms kg mm. No implicit conversion to weight.</summary>
    [Serializable]
    public sealed class NodalMass
    {
        public CoordinateSystem Axes { get; set; }
        public double[] Matrix6x6RowMajor { get; set; }
        public string SourceRecord { get; set; }
        public bool? IncludedInSourceTotal { get; set; }
    }
}
