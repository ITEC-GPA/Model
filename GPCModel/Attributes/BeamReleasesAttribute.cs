using GPC.Model.LoadCases;

namespace GPC.Model.Attributes
{
    public class BeamReleasesAttribute : Attribute
    {
        public BeamReleasesAttribute(LoadCaseBase freedomCaseName)
            : base(freedomCaseName)
        {

        }
    }
}
