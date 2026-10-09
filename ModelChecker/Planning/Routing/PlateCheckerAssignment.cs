using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GPC.Geometry;
using GPC.Model.Checker.Configuration;
using GPC.Model.Elements;
using GPC.Model.Persistence;
using GPC.Model.PostProcessing;
using GPC.Model.Results.ResultLocations;

namespace GPC.Model.Checker
{
    public sealed class PlateCheckerAssignment
    {
        public string EngineId { get; set; }
        public IPlateChecker Checker { get; set; }
        public ElementSelection Selection { get; set; }
        public CheckMechanism[] Mechanisms { get; set; }
        public string[] CheckIds { get; set; }
    }
}
