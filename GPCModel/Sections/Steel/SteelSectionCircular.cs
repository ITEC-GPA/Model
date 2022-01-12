using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Materials;

namespace GPC.Model.Sections.Steel
{
    [Serializable]
    public class SteelSectionCircular : SectionCircular
    {
		#region Public Constructors

		public SteelSectionCircular(double diameter, SteelMaterial material, string name = "")
            : base(diameter, material, name)
        {
        }

        public SteelSectionCircular(SectionCircular sectionCircular)
            : base(sectionCircular)
        {
            if (!(sectionCircular.Material is SteelMaterial))
                throw new ArgumentException("Material must be SteelMaterial");
        }

		protected SteelSectionCircular(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		#endregion

		#region Equals, hashcode, operators

		public override bool Equals(object obj)
		{
			return obj is SteelSectionCircular circular &&
				   base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return 624022166 + base.GetHashCode();
		}

		public static bool operator ==(SteelSectionCircular left, SteelSectionCircular right)
		{
			return EqualityComparer<SteelSectionCircular>.Default.Equals(left, right);
		}

		public static bool operator !=(SteelSectionCircular left, SteelSectionCircular right)
		{
			return !(left == right);
		}

		#endregion
	}
}
