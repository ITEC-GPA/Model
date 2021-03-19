using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Combinations;
using GPC.Utilities.Extensions;

namespace GPC.Model.FEM
{
    [Serializable]
    public class StageConstruction : ModelObject, ISerializable, IEquatable<StageConstruction>, ICloneable
    {

        private List<Combination> _combinations;
        private FemModel.AnalysisType _analysisType;

        private bool _morph;

        public List<Combination> Combinations => _combinations;
        public FemModel.AnalysisType AnalysisType => _analysisType;
        public bool Morph => _morph;


        public StageConstruction(string name, FemModel.AnalysisType analysisType, bool morph, List<Combination> combinations)
            : base(name)
        {
            this._analysisType = analysisType;
            this._combinations = combinations;
            this._morph = morph;
        }

        public StageConstruction(string name) 
            : base(name)
        {

        }

        public StageConstruction(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            throw new NotImplementedException();
        }


        public void AddCombination(Combination combination)
        {
            _combinations.Add(combination);
        }

        #region Interface, operators, hashcode
        public object Clone()
        {
            return new StageConstruction(_name, _analysisType, _morph, _combinations);
        }

        public bool Equals(StageConstruction sc)
        {
            if (sc is null)
                return false;

            if (ReferenceEquals(this, sc))
                return true;

            return !(sc is null) && _combinations.ScrambledEquals(sc._combinations)
                                 && _analysisType.Equals(sc._analysisType)
                                 && _morph.Equals(sc._morph)
                                 && base.Equals(sc);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as StageConstruction);
        }

        public override int GetHashCode()
        {
            int hashCode = -23;
            hashCode = hashCode * -17 + base.GetHashCode();

            foreach (var combo in _combinations)
            {
                hashCode = hashCode + EqualityComparer<Combination>.Default.GetHashCode(combo);
            }
            hashCode = hashCode + _morph.GetHashCode();
            hashCode = hashCode + _analysisType.GetHashCode();

            return hashCode;
        }

        public static bool operator ==(StageConstruction obj1, StageConstruction obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(StageConstruction obj1, StageConstruction obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
