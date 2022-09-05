using GPC.Model.Materials;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Sections.Rebar
{
	[Serializable]
	public class RebarSectionCircular : SectionCircular, IRebarSection, ISerializable
	{

		public SteelMaterial RebarMaterial => (SteelMaterial)_material;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="name">The name of section</param>
		/// <param name="diameter">Th diameter</param>
		/// <param name="rebarMaterial">The material</param>
		/// <param name="id">The unique id</param>
		public RebarSectionCircular(string name, double diameter, SteelMaterial rebarMaterial, int id = IDUNASSIGNED)
			: base(diameter, rebarMaterial, name)
		{
			_id = id;
		}

		public RebarSectionCircular(SectionCircular sectionCircular)
			: base(sectionCircular)
		{
		}

		public RebarSectionCircular(double diameter, SteelMaterial material)
			: this("", diameter, material)
		{
		}


		protected RebarSectionCircular(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{

		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
		}
	}
}
