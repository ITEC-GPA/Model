using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GPC.Geometry;
using GPC.Model.Checker.Configuration;
using GPC.Model.Elements;
using GPC.Model.Core;
using GPC.Model.PostProcessing;
using GPC.Model.Results.ResultLocations;

namespace GPC.Model.Checker
{
    public sealed class PlateCheckJob
    {
        public string Name { get; set; }
        public PreparationRequest Preparation { get; set; }
        public ShellInputAxes AxesKind { get; set; } = ShellInputAxes.Reinforcement;
        public CoordinateSystem Axes { get; set; }
        public PlateCheckSpecification[] Checks { get; set; } = new PlateCheckSpecification[0];
        public PlateCheckerAssignment[] Assignments { get; set; } = new PlateCheckerAssignment[0];
    }
}
