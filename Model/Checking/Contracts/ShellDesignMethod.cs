using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GPC.Geometry;
using GPC.Model.Elements;
using GPC.Model.Results;
using GPC.Model.Results.Locations;
using GPC.Model.Sections.Concrete;
using GPC.Model.Structure.Assignments;

namespace GPC.Model.Checking.Contracts
{
    /// <summary>Structural design method must be provided and benchmarked separately from axis and unit conversions.</summary>
    public interface IShellDesignActionMethod
    {
        string NameAndVersion { get; }
        ResultPlateForces DesignActions(ResultPlateForces normalized, ShellAssignments reinforcement);
    }
}
