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
        protected IRebarSection _rebarSection;
        protected Point2d _position;
        protected double _epsilonP;


        public double Area => _rebarSection.Area;

        public RebarMaterial RebarMaterial => _rebarSection.RebarMaterial;

        public IRebarSection RebarSection => _rebarSection;

        public Point2d Position => _position;

        public double EpsilonP => _epsilonP;


        #region Public Constructors

        public ReinforcedConcreteRebar(IRebarSection section, Point2d position, double sigmaP, int id, string name, Guid guid)
            : base(id, name, guid)
        {
            _rebarSection = section ?? throw new ArgumentNullException(nameof(section));
            _position = position ?? throw new ArgumentNullException(nameof(position));
            if (sigmaP < 0.0)
                throw new ArgumentException("SigmaP cannot be lower than 0");
            if (sigmaP > RebarMaterial.Fu)
                throw new ArgumentException("SigmaP cannot be greater than Fu");

            _epsilonP = GetEpsilonP(sigmaP);
        }

        public ReinforcedConcreteRebar(IRebarSection section, Point2d position, double sigmaP = 0.0, int id = IDUNASSIGNED, string name = "")
            : this(section, position, sigmaP, id, name, new Guid())
        {

        }

        protected ReinforcedConcreteRebar(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _rebarSection = (IRebarSection)info.GetValue("RebarSection", typeof(IRebarSection));
            _position = (Point2d)info.GetValue("Position", typeof(Point2d));
            _epsilonP = info.GetDouble("EpsilonP");
        }


        #endregion

        protected double GetEpsilonP(double sigmaP)
        {
            if (sigmaP < 0.0)
                throw new ArgumentException("SigmaP must be greater than 0");

            if (sigmaP == 0.0)
                return 0.0;

            if (sigmaP <= RebarMaterial.Fyk)
                return Utilities.Maths.Interpolation.GetLinearInterpolation(0.0, RebarMaterial.Fyk, 0.0,
                    RebarMaterial.StrainY, sigmaP);
            else
                return RebarMaterial.StrainY + Utilities.Maths.Interpolation.GetLinearInterpolation(RebarMaterial.Fyk, RebarMaterial.Fu,
                    RebarMaterial.StrainY, RebarMaterial.StrainU, sigmaP - RebarMaterial.Fyk);
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

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("RebarSection", _guid, typeof(IRebarSection));
            info.AddValue("Position", _name, typeof(Point2d));
            info.AddValue("EpsilonP", _name);
        }

        public static bool operator ==(ReinforcedConcreteRebar left, ReinforcedConcreteRebar right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ReinforcedConcreteRebar left, ReinforcedConcreteRebar right)
        {
            return !(left == right);
        }


        public class ReinforcedConcreteRebarComparer : IEqualityComparer<ReinforcedConcreteRebar>
        {

            /// <returns>
            /// <para> true if both <paramref name="x"/> and <paramref name="y"/> are null </para>
            /// </returns>
            /// <remarks> Only <see cref="ModelObjectId.Id"/> is used as equality parameter </remarks>
            bool IEqualityComparer<ReinforcedConcreteRebar>.Equals(ReinforcedConcreteRebar x, ReinforcedConcreteRebar y)
            {
                if (ReferenceEquals(x, y))
                    return true;

                if (x == null || y == null)
                    return false;

                if (x.Position.Equals(y.Position))
                    return true;

                return false;
            }


            /// <remarks> Only <see cref="ModelObjectId.Id"/> is used as equality parameter </remarks>
            int IEqualityComparer<ReinforcedConcreteRebar>.GetHashCode(ReinforcedConcreteRebar obj)
            {
                unchecked
                {
                    return - 391 * obj.Position.GetHashCode(); 
                }
            }

        }


    }
}
