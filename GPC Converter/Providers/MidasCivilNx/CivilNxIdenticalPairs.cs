using System;
using GPC.Model.Sections;
using GPC.Converter.MidasCivil;

namespace GPC.Converter.CivilNx
{
    public sealed partial class CivilNxModelProfile
    {
        private sealed partial class Reader
        {
            private void Pair(SectionRecord section, string shape, double[] size, SectionValues computed)
            {
                section.Values = computed;
                try
                {
                    var definition = MidasIdenticalPairs.Definition(shape, size, length);
                    var component = (Section)PropertyMapper.Parametric(new SectionRecord
                        { Id = section.Id + "/component", Shape = definition.ComponentShape, Dimensions = definition.ComponentDimensions }, "component");
                    var pair = definition.Create(component, section.Name ?? section.Id);
                    bool Close(double a, double b) => b > 0 && Math.Abs(a - b) <= .005 * Math.Abs(b);
                    // Geometric MIDAS y/z, never sorted principal axes. A supplied table is an independent consistency guard.
                    if (computed != null && (!Close(pair.Area, computed.Area) || !Close(pair.Jxx, computed.I11) || !Close(pair.Jyy, computed.I22)))
                        throw new ArgumentException("Computed section properties disagree with the standard pair dimensions.");
                    section.IdenticalPair = definition;
                    Warn("CivilNxIdenticalPairGeometry", section.Record,
                        "Two identical back-to-back C/L components from the native-tested H,B,tw,tf,C convention. H is the facing leg/web and C the clear gap. Supplied SECTIONALL values are checked. Packing and connection action require separate qualification.");
                    return;
                }
                catch (ArgumentException) { }
                Warn("CivilNxIdenticalPairUnresolved", section.Record,
                    "Double section geometry not qualified: explicit valid H,B,tw,tf,C are required and any supplied SECTIONALL area/Iyy/Izz must agree. Source data retained without guessing radii or orientation.");
            }
        }
    }
}
