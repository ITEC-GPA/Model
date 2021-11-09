using GPC.Geometry;
using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Results
{
	public interface IElementResult
	{
		ResultType[] Results { get; }
		ResultLocation[] Points { get; }
		ILoadCase Case { get; }
		CoordinateSystem CoordinateSystem { get; }
	}
}
