using GPC.Model.FEM.Materials;
using GPC.Utilities.Attributes;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    [Serializable]
    [UI(Description = "Steel", Group = "Materials", Kind = "Material")]
    public class SteelMaterial : Material
    {
        #region Variables

        protected double _fyk;
        protected double _fu;
        protected double _epsilon0;

        #endregion 

        #region Properties

        public double Fyk => _fyk;

        public double Fu => _fu;

        public double Epsilon0 => _epsilon0;

        #endregion

        #region Constructor

        /// <summary>
        /// Default SteelMaterial constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="elasticModulus">Steel elastic modulus</param>
        /// <param name="poisson">Poissoins's Ratio</param>
        /// <param name="fyk">Yielding stress</param>
        /// <param name="fu">Ultimate stress</param>
        /// <param name="epsilon0">Yielding strain</param>
        /// <param name="density"></param>
        /// <param name="alfaThermalExpansion">Linear thermal expasion coefficient</param>
        /// <param name="guid">Guid of the material</param>
        public SteelMaterial(string name, double elasticModulus, double poisson, double fyk, double fu, double epsilon0, double density, double alfaThermalExpansion, Guid guid)
            : base(name, elasticModulus, poisson, density, alfaThermalExpansion, guid)
        {
            if (elasticModulus == 0)
                throw new ArgumentException($"{nameof(elasticModulus)} cannot be equal to zero");

            _fu = fu <= 0 ? throw new ArgumentException($"{nameof(fu)} cannot be zero or lower") : fu ;
            _fyk = fyk <= 0 ? throw new ArgumentException($"{nameof(fyk)} cannot be zero or lower") : fyk;
            _epsilon0 = epsilon0 <= 0 ? throw new ArgumentException($"{nameof(epsilon0)} cannot be zero or lower") : epsilon0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="elasticModulus">Steel elastic modulus</param>
        /// <param name="poisson">Poissoins's Ratio</param>
        /// <param name="fy">Yielding stress</param>
        /// <param name="fu">Ultimate stress</param>
        /// <param name="epsilon0">Yielding strain</param>
        /// <param name="density"></param>
        /// <remarks>Guid setted to new guid, alfaThermalExpansion setted to 0</remarks>
        public SteelMaterial(string name, double elasticModulus, double poisson, double fy, double fu, double epsilon0, double density)
            : this(name, elasticModulus, poisson, fy, fu, epsilon0, density, 0, Guid.NewGuid())
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="elasticModulus">Steel elastic modulus</param>
        /// <param name="poisson">Poissoins's Ratio</param>
        /// <param name="fy">Yielding stress</param>
        /// <param name="fu">Ultimate stress</param>
        /// <param name="density"></param>
        /// <remarks>Guid setted to empty, alfaThermalExpansion setted to 0. Epsilon0 equal to fy / E</remarks>
        public SteelMaterial(string name, double elasticModulus, double poisson, double fy, double fu, double density)
            : this(name, elasticModulus, poisson, fy, fu, fy / elasticModulus, density, 0, Guid.NewGuid())
        {

        }

        public SteelMaterial(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
            _fu = info.GetDouble("Fu");
            _fyk = info.GetDouble("Fyk");
            _epsilon0 = info.GetDouble("Epsilon0");
        }

        #endregion

        #region Public Methods

        public override IsotropicFemMaterial GetIsotropicFemMaterial()
        {
            return new IsotropicFemMaterial(E, Ni, AlfaThermalExpansion, Density);
        }

        public override OrthotropicFemMaterial GetOrthotropicFemMaterial()
        {
            return new OrthotropicFemMaterial(E, E, E, Ni, Ni, Ni, GetShearModule(), GetShearModule(), GetShearModule(), AlfaThermalExpansion, AlfaThermalExpansion, AlfaThermalExpansion, Density);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Epsilon0", _epsilon0);
            info.AddValue("Fyk", _fyk);
            info.AddValue("Fu", _fu);
        }

        #endregion 
    }
}
