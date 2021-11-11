using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Model.Materials;
using GPC.Model.Sections.Rebar;

namespace GPC.Model.Sections.Concrete
{

    [Serializable]
    public class ReinforcedConcreteRebar : ModelObjectId, ISerializable
    {

        protected readonly IRebarSection _rebarSection;
        protected readonly Point2d _position;
        protected readonly double _epsilonP;


        #region Properties

        public double Area => _rebarSection.Area;

        public RebarMaterial RebarMaterial => _rebarSection.RebarMaterial;

        public IRebarSection RebarSection => _rebarSection;

        public Point3d Position => _position;

        public double EpsilonP => _epsilonP;

        #endregion

        #region Public Constructors

        public ReinforcedConcreteRebar(IRebarSection section, Point2d position, double epsilonP, int id, string name, Guid guid)
            : base(id, name, guid)
        {
            _rebarSection = section ?? throw new ArgumentNullException(nameof(section));
            _position = position ?? throw new ArgumentNullException(nameof(position));
            if (epsilonP < 0.0)
                throw new ArgumentException("EpsilonP cannot be lower than 0");
            _epsilonP = epsilonP;
        }

        public ReinforcedConcreteRebar(IRebarSection section, Point2d position, double epsilonP, int id, string name = "")
            : base(id, name)
        {
            _rebarSection = section ?? throw new ArgumentNullException(nameof(section));
            _position = position ?? throw new ArgumentNullException(nameof(position));
            if (epsilonP < 0.0)
                throw new ArgumentException("EpsilonP cannot be lower than 0");
            _epsilonP = epsilonP;
        }


        public ReinforcedConcreteRebar(IRebarSection section, Point2d position, int id = IDUNASSIGNED, double epsilonP = 0.0)
            : this(section, position, epsilonP, id, "")
        {

        }



        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return obj is ReinforcedConcreteRebar rebar && base.Equals(obj)
                                                        && _rebarSection.Equals(rebar._rebarSection)
                                                        && _position.Equals(rebar._position)
                                                        && _epsilonP == rebar._epsilonP;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _rebarSection.GetHashCode();
                hashCode = hashCode * -17 + _position.GetHashCode();
                hashCode = hashCode * -17 + _epsilonP.GetHashCode();
                return hashCode;
            }

        }

        public static bool operator ==(ReinforcedConcreteRebar left, ReinforcedConcreteRebar right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ReinforcedConcreteRebar left, ReinforcedConcreteRebar right)
        {
            return !(left == right);
        }


        #endregion
    }
}
