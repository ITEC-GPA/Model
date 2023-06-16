using GPC.Model.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Sections.Steel
{
	[Serializable]
	public class SteelSectionRectangular : SectionRectangular, ISerializable
	{
		#region Public Constructors

		public SteelSectionRectangular(double height, double width, SteelMaterial material, string name = "") 
			: this(height, width, 0, material, name)
		{
		}

		public SteelSectionRectangular(double height, double width, double angle, SteelMaterial material, string name = "") 
			: base(height, width, angle, material, name)
		{
		}

		public SteelSectionRectangular(SteelSectionRectangular section) 
			: base(section)
		{
			if (!(section.Material is SteelMaterial))
				throw new ArgumentException("Material must be SteelMaterial");
		}

		protected SteelSectionRectangular(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		#endregion

		#region Equals, hashcode, operators

		public override bool Equals(object obj)
		{
            return obj is SteelSectionRectangular &&
				   base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return -23 + base.GetHashCode();
		}

		public static bool operator ==(SteelSectionRectangular left, SteelSectionRectangular right)
		{
			return EqualityComparer<SteelSectionRectangular>.Default.Equals(left, right);
		}

		public static bool operator !=(SteelSectionRectangular left, SteelSectionRectangular right)
		{
			return !(left == right);
		}

		#endregion
	}
}
