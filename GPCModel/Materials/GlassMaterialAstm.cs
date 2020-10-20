using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    [Serializable]
    public class GlassMaterialAstm : GlassMaterial
    {
        #region VARIABLES

        private double _psiSurface;
        private double _nGlassCoefficient;
        private double _surfaceBaseStress;
        private double _surfaceBaseEdgeStress;
        private double _probabiltyOfBreakage;

        #endregion 

        #region PROPERTIES

        public double PsiSurface => _psiSurface;
        public double NGlassCoefficient => _nGlassCoefficient;
        public double SurfaceBaseStress => _surfaceBaseStress;
        public double SurfaceBaseEdgeStress => _surfaceBaseEdgeStress;
        public double ProbabiltyOfBreakage => _probabiltyOfBreakage;
        #endregion 

        #region PUBLIC CONSTRUCTORS

        /// <summary>
        ///
        /// </summary>
        /// <param name="elasticModulus">Elastic modulus of the glass</param>
        /// <param name="poisson">poisson ratio's of the glass</param>
        /// <param name="psiSurface">psi coefficient of surface according to NCSEA</param>
        /// <param name="nGlassCoefficient">psi coefficient of the glass according to NCSEA</param>
        /// <param name="surfaceBaseStress">Surface base stress according to NCSEA</param>
        /// <param name="surfaceBaseEdgeStress">Surface base edge stress according to NCSEA</param>
        /// <param name="probabiltyOfBreakage">Probability of breakage according to NCSEA</param>
        /// <param name="density">Density of the material</param>
        /// <param name="alfaThermalExpansion">Alfa linear thermal expansion coefficient</param>
        public GlassMaterialAstm(double elasticModulus, double poisson, double psiSurface, double nGlassCoefficient, double surfaceBaseStress, double surfaceBaseEdgeStress, double probabiltyOfBreakage, double density, double alfaThermalExpansion)
            : this(elasticModulus, poisson, density, psiSurface, nGlassCoefficient, surfaceBaseStress, surfaceBaseEdgeStress, probabiltyOfBreakage, alfaThermalExpansion, Guid.Empty)
        {

        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="elasticModulus">Elastic modulus of the glass</param>
        /// <param name="poisson">poisson ratio's of the glass</param>
        /// <param name="psiSurface">psi coefficient of surface according to NCSEA</param>
        /// <param name="nGlassCoefficient">psi coefficient of the glass according to NCSEA</param>
        /// <param name="surfaceBaseStress">Surface base stress according to NCSEA</param>
        /// <param name="surfaceBaseEdgeStress">Surface base edge stress according to NCSEA</param>
        /// <param name="probabiltyOfBreakage">Probability of breakage according to NCSEA</param>
        /// <param name="density">Density of the material</param>
        /// <param name="alfaThermalExpansion">Alfa linear thermal expansion coefficient</param>
        public GlassMaterialAstm(double elasticModulus, double poisson, double psiSurface, double nGlassCoefficient, double surfaceBaseStress, double surfaceBaseEdgeStress, double probabiltyOfBreakage, double density, double alfaThermalExpansion, Guid guid)
            : base(elasticModulus, poisson, density, alfaThermalExpansion, guid)
        {
            if (psiSurface <= 0)
            {
                throw new ArgumentException($"{nameof(psiSurface)} cannot be zero or lower");
            }
            if (nGlassCoefficient <= 0)
            {
                throw new ArgumentException($"{nameof(nGlassCoefficient)} cannot be zero or lower");
            }
            if (surfaceBaseStress <= 0)
            {
                throw new ArgumentException($"{nameof(surfaceBaseStress)} cannot be zero or lower");
            }
            if (surfaceBaseEdgeStress <= 0)
            {
                throw new ArgumentException($"{nameof(surfaceBaseEdgeStress)} cannot be zero or lower");
            }

            if (probabiltyOfBreakage < 0.001)
            {
                throw new ArgumentException($"{nameof(probabiltyOfBreakage)} cannot be lower than 0.001");
            }
            else if (probabiltyOfBreakage > 0.01)
            {
                throw new ArgumentException($"{nameof(probabiltyOfBreakage)} cannot be greater than 0.01");
            }

            this._psiSurface = psiSurface;
            this._nGlassCoefficient = nGlassCoefficient;
            this._surfaceBaseStress = surfaceBaseStress;
            this._surfaceBaseEdgeStress = surfaceBaseEdgeStress;
            this._probabiltyOfBreakage = probabiltyOfBreakage;
        }

        public GlassMaterialAstm(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _psiSurface = info.GetDouble("PsiSurface");
            _nGlassCoefficient = info.GetDouble("NGlassCoefficient");
            _surfaceBaseStress = info.GetDouble("SigmaBase");
            _surfaceBaseEdgeStress = info.GetDouble("SigmaBaseEdge");
        }

        #endregion

        #region PUBLIC METHODS

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("PsiSurface", _psiSurface);
            info.AddValue("NGlassCoefficient", _nGlassCoefficient);
            info.AddValue("SigmaBase", _surfaceBaseStress);
            info.AddValue("SigmaBaseEdge", _surfaceBaseEdgeStress);
        }

        #endregion 
    }
}