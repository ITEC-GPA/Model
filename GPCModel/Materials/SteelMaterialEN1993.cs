using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Standards;

namespace GPC.Model.Materials
{
	public class SteelMaterialEN1993 : SteelMaterial
	{
        /// <summary>
        /// Default Steel S235 according to EN1993
        /// </summary>
        public static SteelMaterialEN1993 S235 => new SteelMaterialEN1993("S235", 210000, 0.3, 235, 360, 0.05, 0.007850, 12 * 1e-6, new Guid());

        /// <summary>
        /// Default Steel S275 according to EN1993
        /// </summary>
        public static SteelMaterialEN1993 S275 => new SteelMaterialEN1993("S275", 210000, 0.3, 275, 430, 0.05, 0.007850, 12 * 1e-6, new Guid());

        /// <summary>
        /// Default Steel S355 according to EN1993
        /// </summary>
        public static SteelMaterialEN1993 S355 => new SteelMaterialEN1993("S355", 210000, 0.3, 355, 510, 0.05, 0.007850, 12 * 1e-6, new Guid());


        /// <summary>
        /// Default SteelMaterial constructor
        /// </summary>
        /// <param name="standard">Standard EN 1992-1-1 or a relative national annex</param>
        /// <param name="name"></param>
        /// <param name="elasticModulus">Steel elastic modulus</param>
        /// <param name="poisson">Poissoins's Ratio</param>
        /// <param name="fyk">Yielding stress</param>
        /// <param name="fu">Ultimate stress</param>
        /// <param name="density">Density of material</param>
        /// <param name="alfaThermalExpansion">Linear thermal expasion coefficient</param>        
        public SteelMaterialEN1993(string name, double elasticModulus, double poisson, double fyk, double fu, double density, double alfaThermalExpansion)
            : this(name, elasticModulus, poisson, fyk, fu, 0.05, density, alfaThermalExpansion, new Guid())
        {
            if (elasticModulus == 0)
                throw new ArgumentException($"{nameof(elasticModulus)} cannot be equal to zero");

            _fu = fu <= 0 ? throw new ArgumentException($"{nameof(fu)} cannot be zero or lower") : fu;
            _fyk = fyk <= 0 ? throw new ArgumentException($"{nameof(fyk)} cannot be zero or lower") : fyk;
        }

        /// <summary>
        /// 
        /// </summary>
        /// /// <param name="standard">Standard EN 1992-1-1 or a relative national annex</param>
        /// <param name="name"></param>
        /// <param name="elasticModulus">Steel elastic modulus</param>
        /// <param name="poisson">Poissoins's Ratio</param>
        /// <param name="fyk">Yielding stress</param>
        /// <param name="fu">Ultimate stress</param>
        /// <param name="density">Density of material</param>
        /// <remarks>Guid setted to new guid, alfaThermalExpansion setted to 12 * 1e-6</remarks>
        public SteelMaterialEN1993(string name, double elasticModulus, double poisson, double fyk, double fu, double density)
            : this(name, elasticModulus, poisson, fyk, fu, 0.05, density, 12 * 1e-6, Guid.NewGuid())
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// /// <param name="standard">Standard EN 1992-1-1 or a relative national annex</param>
        /// <param name="name"></param>
        /// <param name="fyk">Yielding stress</param>
        /// <param name="fu">Ultimate stress</param>
        /// <param name="density">Density of material</param>
        /// <remarks>Guid setted to empty, alfaThermalExpansion setted to 12 * 1e-6. Epsilon0 equal to fy / E</remarks>
        public SteelMaterialEN1993(string name, double fyk, double fu, double density = 0.007850)
            : this(name, 210000.0, 0.30, fyk, fu, 0.05, density, 0, Guid.NewGuid())
        {

        }

        /// <summary>
        /// Protected steelMaterial constructor 
        /// </summary>
        /// /// <param name="standard">Standard EN 1992-1-1 or a relative national annex</param>
        /// <param name="name"></param>
        /// <param name="elasticModulus">Steel elastic modulus</param>
        /// <param name="poisson">Poissoins's Ratio</param>
        /// <param name="fyk">Yielding stress</param>
        /// <param name="fu">Ultimate stress</param>
        /// <param name="epsilonU">The ultimate strain</param>
        /// <param name="density">Density of material</param>
        /// <param name="alfaThermalExpansion">Linear thermal expasion coefficient</param>
        /// <param name="guid">Guid of the material</param>
        protected SteelMaterialEN1993(string name, double elasticModulus, double poisson, double fyk,
            double fu, double epsilonU, double density, double alfaThermalExpansion, Guid guid)
            : base(name, elasticModulus, poisson, fyk, fu, epsilonU, density, alfaThermalExpansion, guid)
        {

        }

        public SteelMaterialEN1993(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {

        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
