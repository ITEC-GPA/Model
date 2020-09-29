using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model
{
    public class Material
    {
        #region FIELD_CONSTRUCTORS
        public Material(double elasticModulus, double poisson, double ftk, double fck, double alfa = 0.0) 
        {
            _elasticModulus = elasticModulus;
            _poisson = poisson;
            _ftk = ftk;
            _fck = fck;
            _alfa = alfa;
        }
        #endregion

        #region FIELD_DECONSTRUCTORS
        #endregion

        #region FIELD_COMMANDS
        #endregion

        #region FIELD_METHODS

        #endregion

        #region FIELD_VARIABLES
        /// <summary>
        /// </summary>
        /// <param name="_elasticModulus"> Elastic Modulus </param>
        /// <param name="_shearModulus"> Elastic Shear Modulus </param>
        /// <param name="_poisson"> Poissoins's Ratio </param>
        /// <param name="_ftk"> Tensile strength design value</param>
        /// <param name="_fck"> Compression strength design value </param>
        /// <param name="_alfa"> Thermal expansion coefficient </param>
        /// <param name="_guid"> Guid of the object </param>
        protected double _elasticModulus;
        protected double _shearModulus;
        protected double _poisson;
        protected double _ftk;
        protected double _fck;
        protected double _alfa;
        protected Guid _guid;
        #endregion

        #region FIELD_PROPERTIES
        public Guid Guid => _guid; 
        public double ElasticModulus  => _elasticModulus;
        public double ShearModulus => _shearModulus; 
        public double Poisson => _poisson;
        public double Ftk  => _ftk;
        public double Fck => _fck;
        #endregion
    }


    public class ConcreteMaterial : Material
    {
        #region FIELD_CONSTRUCTORS
        public ConcreteMaterial(double elasticModulus, double poisson, double ftk, double fck, double alfa = 0.00001) : base(elasticModulus, poisson, ftk, fck, alfa)
        {
        }
        #endregion

        #region FIELD_DECONSTRUCTORS
        #endregion

        #region FIELD_COMMANDS
        #endregion

        #region FIELD_METHODS

        #endregion

        #region FIELD_VARIABLES
        /// <summary>
        /// </summary>
        #endregion

        #region FIELD_PROPERTIES
        #endregion
    }

    public class SteelMaterial : Material
    {
        #region FIELD_CONSTRUCTORS
        public SteelMaterial(double elasticModulus, double poisson, double ftk, double fck, double ftu, double epsilon0, double alfa = 0.0000115) : base(elasticModulus, poisson, ftk, fck, alfa)
        {
           _ftu = ftu;
            _epsilon0 = epsilon0;
        }
        #endregion

        #region FIELD_DECONSTRUCTORS
        #endregion

        #region FIELD_COMMANDS
        #endregion

        #region FIELD_METHODS

        #endregion

        #region FIELD_VARIABLES
        /// <summary>
        /// </summary>
        /// <param name="Ftu"> Tensile resistance </param>
        /// <param name="Epsilon0"> Maximum Strain </param>
        protected double _ftu;
        protected double _epsilon0;
        #endregion

        #region FIELD_PROPERTIES
        public double Ftu => _ftu;
        public double Epsilon0 => _epsilon0;
        #endregion
    }

    public class RebarMaterial : SteelMaterial
    {
        #region FIELD_CONSTRUCTORS
        public RebarMaterial(double elasticModulus, double poisson, double ftk, double fck, double ftu, double epsilon0, double alfa = 0.0000115) : base(elasticModulus, poisson, ftk, fck, ftu, epsilon0, alfa)
        {
        }
        #endregion

        #region FIELD_DECONSTRUCTORS
        #endregion

        #region FIELD_COMMANDS
        #endregion

        #region FIELD_METHODS

        #endregion

        #region FIELD_VARIABLES
        #endregion

        #region FIELD_PROPERTIES
        #endregion
    }
}
