using GPC.Model.Results.Processing;
using System;
using System.Collections.Generic;
using GPC.Model.Elements;

namespace GPC.Model.PostProcessing
{
    public enum DataStatus { Ready, Insufficient, NotSupported, MissingDependency, Stale }
}
