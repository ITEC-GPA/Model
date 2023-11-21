using GPC.Model.Fem.Materials;
using GPC.Utilities.Attributes;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
	[Serializable]
	[UI(Description = "Silicon", Group = "Materials", Kind = "Material")]
	public class SiliconMaterial : Material
	{
		private double _adhesiveStress;

		public double AdhesiveStress { get => _adhesiveStress; set => _adhesiveStress = value; }

		public SiliconMaterial(double adhesiveStress, double density, double alfaThermalExpansion)
			: base("", 0, 0, density, alfaThermalExpansion)
		{
			_adhesiveStress = adhesiveStress <= 0.001 ? throw new ArgumentException($"{nameof(adhesiveStress)} cannot be zero or lower") : adhesiveStress;
		}

		protected SiliconMaterial(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			_adhesiveStress = info.GetDouble("AdhesiveStress");
		}

		#region FemMaterial

		public override IsotropicFemMaterial GetIsotropicFemMaterial()
		{
			// da impostare il valore corretto di E e di NI
			return new IsotropicFemMaterial(E, Ni, AlfaThermalExpansion, Density);
		}

		public override OrthotropicFemMaterial GetOrthotropicFemMaterial()
		{
			// da impostare il valore corretto di E e di NI e di G

			return new OrthotropicFemMaterial(E, E, E, Ni, Ni, Ni, GetShearModule(), GetShearModule(), GetShearModule(), AlfaThermalExpansion, AlfaThermalExpansion, AlfaThermalExpansion, Density);
		}

		#endregion

		#region PUBLIC METHODS

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("AdhesiveStress", _adhesiveStress);
		}

		#endregion PUBLIC METHODS
	}
}