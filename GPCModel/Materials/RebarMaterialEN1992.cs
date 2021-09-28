using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Standards;

namespace GPC.Model.Materials
{
	public class RebarMaterialEN1992 : RebarMaterial
	{
		public static RebarMaterialEN1992 B450C => new RebarMaterialEN1992(new StandardEn1992p11(), "B450C", 20000, 0.28, 450, 510, 0.075, 0.007850, 12 * 1e-6, new Guid());

		#region Variables

		protected StandardEn1992p11 _standard;

		#endregion

		#region Properties

		public StandardEn1992p11 Standard => _standard;

		#endregion


		/// <summary>
		/// Default rebar material constructor
		/// </summary>
		/// <param name="standard">Standard EN 1992-1-1 or a relative national annex</param>
		/// <param name="name">Name of material</param>
		/// <param name="elasticModulus">Steel elastic modulus</param>
		/// <param name="poisson">Poissoins's Ratio</param>
		/// <param name="fy">Yielding stress</param>
		/// <param name="fu">Ultimate stress</param>
		/// <param name="epsilonU">Ultimate strain</param>
		/// <param name="density"></param>
		/// <param name="alfaThermalExpansion">Linear thermal expasion coefficient</param>
		/// <param name="guid">Guid of the material</param>
		public RebarMaterialEN1992(StandardEn1992p11 standard, string name, double elasticModulus, double poisson,
            double fy, double fu, double epsilonU, double density, double alfaThermalExpansion, Guid guid)
            : base(name, elasticModulus, poisson, fy, fu, epsilonU, density, alfaThermalExpansion, guid)
		{
			_standard = standard;
		}

		/// <summary>
		/// 
		/// </summary>
		/// /// <param name="standard">Standard EN 1992-1-1 or a relative national annex</param>
		/// <param name="elasticModulus">Steel elastic modulus</param>
		/// <param name="fy">Yielding stress</param>
		/// <param name="fu">Ultimate stress</param>
		/// <param name="poisson">Poissoins's Ratio</param>
		/// <param name="density"></param>
		/// <param name="alfaThermalExpansion">Linear thermal expasion coefficient</param>
		/// <remarks>Name is empty</remarks>
		public RebarMaterialEN1992(StandardEn1992p11 standard, double elasticModulus, double fy, double fu, double poisson = 0.28, double density = 0.007850, double alfaThermalExpansion = 12 * 1e-6)
			: this(standard, "", elasticModulus, poisson, fy, fu, 0.075, density, alfaThermalExpansion, new Guid())
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// /// <param name="standard">Standard EN 1992-1-1 or a relative national annex</param>
		/// <param name="fyk">Yielding stress</param>        
		/// <remarks>Guid setted to new guid. StressStrainDiagram is set to ElastoPlastic. alfaThermalExpansion setted to 0. Epsilon0 equal to fy / E
		/// E = 200GPa, ni = 0.28. Epsilon U is set as 0.075 and fu is set as fyk</remarks>
		public RebarMaterialEN1992(StandardEn1992p11 standard, double fyk)
			: this(standard, 200000, fyk, fyk)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="fyk">Yielding stress</param>        
		/// <remarks>Guid setted to new guid. StressStrainDiagram is set to ElastoPlastic. alfaThermalExpansion setted to 0. Epsilon0 equal to fy / E
		/// E = 200GPa, ni = 0.28. Epsilon U is set as 0.075 and fu is set as fyk.
		/// Standard set is Standard EN 1992-1-1</remarks>
		public RebarMaterialEN1992(double fyk)
			: this(new StandardEn1992p11(), 200000, fyk, fyk)
		{
		}

		public RebarMaterialEN1992(SerializationInfo info, StreamingContext context) :
			base(info, context)
		{
			_standard = (StandardEn1992p11)info.GetValue("Standard", typeof(StandardEn1992p11));
		}



		#region Field Serialization

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("Standard", _standard);
		}

		#endregion

	}
}
