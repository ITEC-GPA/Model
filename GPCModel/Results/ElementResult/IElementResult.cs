using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Model.LoadCases;

namespace GPC.Model.Results
{
    public interface IElementResult
    {

        ResultLocation[] ResultLocations { get; }

        ILoadCase Case { get; }

    }
}
