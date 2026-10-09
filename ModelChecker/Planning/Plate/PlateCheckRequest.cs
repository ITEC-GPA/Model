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
    public sealed class PlateCheckRequest { public List<PlateCheckJob> Jobs { get; } = new List<PlateCheckJob>(); }
}
