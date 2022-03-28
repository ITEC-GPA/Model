using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Model.FEM.FiniteElements;
using GPC.Model.FreedomCases;
using GPC.Model.Restrains;

namespace GPC.Model.FEM.Attributes
{
    public class BeamReleasesAttribute : FreedomCaseAttribute, IBeamFreedomCaseAttribute
    {
        Beam.EndSide _endBeam;
        HashSet<Beam.LocalDOF> _localDOFs = new HashSet<Beam.LocalDOF>(); //non permetto di avere duplicati

        public Beam.EndSide EndBeam => _endBeam;
        public Beam.LocalDOF[] LocalDOFReleased => _localDOFs.ToArray();

        public BeamReleasesAttribute(int indexEndBeam, HashSet<Beam.LocalDOF> releases, string freedomCaseName, string name)
            : base(freedomCaseName, name)
        {
            if (indexEndBeam == 1)
            {
                _endBeam = Beam.EndSide.End1;
            }
            else if (indexEndBeam == 2)
            {
                _endBeam = Beam.EndSide.End2;
            }
            else
            {
                throw new IndexOutOfRangeException("Release can be applicated in End 1 or End 2");
            }
            _localDOFs = releases;
        }

        public BeamReleasesAttribute(Beam.EndSide end, HashSet<Beam.LocalDOF> releases, string freedomCaseName, string name)
            : base(freedomCaseName, name)
        {
            {
                _endBeam = end;
                _localDOFs = releases;
            }
        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
