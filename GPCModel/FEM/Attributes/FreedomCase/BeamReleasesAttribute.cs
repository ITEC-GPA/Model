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

        int _indexEndBeam; //node 1 or node 2 of the beam
        HashSet<Beam.LocalDOF> _localDOFs = new HashSet<Beam.LocalDOF>(); //non permetto di avere duplicati

        public int EndBeam => _indexEndBeam;
        public Beam.LocalDOF[] LocalDOFReleased => _localDOFs.ToArray();

        public BeamReleasesAttribute(int indexEndBeam, HashSet<Beam.LocalDOF> releases, FreedomCase freedomCase, string name) 
            : base(freedomCase, name)
        {
            if (indexEndBeam != 1 && indexEndBeam != 2)
            {
                throw new Exception("Release can be applicated in End 1 or End 2");
            } else
            {
                _indexEndBeam = indexEndBeam;
                _localDOFs = releases;
            }
        }
        public override object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
