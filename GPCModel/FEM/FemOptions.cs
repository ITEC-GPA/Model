using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.FEM
{
    // https://csharpindepth.com/articles/singleton

    /// <summary>
    /// This is a singleton class that collects options related to the fem model 
    /// </summary>
    public sealed class FemOptions : ModelObject
    {

        #region Singleton setup

        private static readonly FemOptions instance = new FemOptions();

        public static FemOptions Instance
        {
            get
            {
                return instance;
            }
        } 

        #endregion



        private double _interlayerPoissonValue;


        #region Properties

        /// <summary>
        /// Rapresent the value of the elastic modulus to be used to replace zero in case numerical singularity must be avoided 
        /// </summary>
        public double ZeroElasticModulus { get; set; }

        /// <summary>
        /// Rapresent the value of the shear modulus to be used to replace zero in case numerical singularity must be avoided 
        /// </summary>
        public double ZeroShearModulus { get; set; }


        /// <summary>
        /// Rapresent the value of the poisson value used to define the <see cref="Properties.InterlayerBrickProperty"/> 
        /// </summary>
        /// <remarks>Value must be higher than 0 and lower to 0.50</remarks>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public double InterlayerPoissonValue
        {
            get => _interlayerPoissonValue;
            set
            {
                if (value < 0.50 && value > 0) // non mettere minore uguale a 0.50 prima di averne discusso con tutto il team, può dare problemi numerici al fem
                {
                    _interlayerPoissonValue = value;
                }
                else
                    throw new ArgumentOutOfRangeException("Interlayer poisson Value must be lower than 0.5 and higher than 0");
            }
        }




        #endregion


        // Explicit static constructor to tell C# compiler not to mark type as beforefieldinit. non toccare
        static FemOptions()
        {

        }

        private FemOptions()
        {
            ZeroElasticModulus = 0.001;
            ZeroShearModulus = 0.01;
            InterlayerPoissonValue = 0.49;
        }

    }

}
