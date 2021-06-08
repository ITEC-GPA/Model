using GPC.Utilities.Attributes;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    /// <summary>
    /// Glass Material for ASTM standard.
    /// according ot NCSEA - Engineering structural glass design guide
    /// </summary>
    [Serializable]
    [UI(Description = "Glass ASTM", Group = "Materials", Kind = "Material")]
    public sealed class GlassMaterialAstm : GlassMaterial, IEquatable<GlassMaterialAstm>
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
        /// <param name="name"></param>
        /// <param name="elasticModulus">Elastic modulus of the glass [MPa]</param>
        /// <param name="poisson">poisson ratio's of the glass</param>
        /// <param name="psiSurface">psi coefficient of surface according to NCSEA</param>
        /// <param name="nGlassCoefficient">psi coefficient of the glass according to NCSEA</param>
        /// <param name="surfaceBaseStress">Surface base stress according to NCSEA [MPa]</param>
        /// <param name="surfaceBaseEdgeStress">Surface base edge stress according to NCSEA [MPa]</param>
        /// <param name="probabiltyOfBreakage">Probability of breakage according to NCSEA</param>
        /// <param name="density">Density of the material [T/mm^3]</param>
        /// <param name="alfaThermalExpansion">Alfa linear thermal expansion coefficient</param>
        public GlassMaterialAstm(string name, double elasticModulus, double poisson, double psiSurface, double nGlassCoefficient, double surfaceBaseStress, 
                                    double surfaceBaseEdgeStress, double probabiltyOfBreakage, double density, double alfaThermalExpansion)
            : this(name, elasticModulus, poisson, psiSurface, nGlassCoefficient, surfaceBaseStress, surfaceBaseEdgeStress, probabiltyOfBreakage, density, alfaThermalExpansion, Guid.NewGuid())
        {

        }


        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        /// <param name="elasticModulus">Elastic modulus of the glass [MPa]</param>
        /// <param name="poisson">poisson ratio's of the glass</param>
        /// <param name="psiSurface">psi coefficient of surface according to NCSEA</param>
        /// <param name="nGlassCoefficient">psi coefficient of the glass according to NCSEA</param>
        /// <param name="surfaceBaseStress">Surface base stress according to NCSEA [MPa]</param>
        /// <param name="surfaceBaseEdgeStress">Surface base edge stress according to NCSEA [MPa]</param>
        /// <param name="probabiltyOfBreakage">Probability of breakage according to NCSEA</param>
        /// <param name="density">Density of the material [T/mm^3]</param>
        /// <param name="alfaThermalExpansion">Alfa linear thermal expansion coefficient</param>
        /// <param name="guid"></param>
        public GlassMaterialAstm(string name, double elasticModulus, double poisson, double psiSurface, double nGlassCoefficient, 
                                double surfaceBaseStress, double surfaceBaseEdgeStress, double probabiltyOfBreakage, double density, double alfaThermalExpansion, Guid guid)
            : base(name, elasticModulus, poisson, density, alfaThermalExpansion, guid)
        {

            if (psiSurface <= 0.001)
            {
                throw new ArgumentException($"{nameof(psiSurface)} cannot be zero or lower");
            }
            if (nGlassCoefficient <= 0.001)
            {
                throw new ArgumentException($"{nameof(nGlassCoefficient)} cannot be zero or lower");
            }
            if (surfaceBaseStress <= 0.001)
            {
                throw new ArgumentException($"{nameof(surfaceBaseStress)} cannot be zero or lower");
            }
            if (surfaceBaseEdgeStress <= 0.001)
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
            _probabiltyOfBreakage = info.GetDouble("ProbabiltyOfBreakage");
        }

        #endregion

        #region Private Methods

        private double GetLoadDurationFactor(double loadDuration)
        {
            if (_nGlassCoefficient == 0)
                throw new ArgumentException();

            if (loadDuration < 3.00)
                loadDuration = 3;

            return 1.0 / Math.Pow(loadDuration / 3.0, 1.0 / _nGlassCoefficient);
        }

        private double GetProbabiltyOfBreakageFactor()
        {
            return Math.Pow(_probabiltyOfBreakage / 0.008, 1.0 / 7.0);
        }


        #endregion


        #region Public method override 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="edgeResistance">if true give the resistance on edge</param>
        /// <param name="loadDuration">load duration [seconds]</param>
        /// <returns>The glass resistance according to NCSEA §3.5</returns>
        /// <exception cref="ArgumentException">If <paramref name="loadDuration"/> is lower than zero</exception>
        public override double GetGlassResistance(bool edgeResistance, double loadDuration)
        {
            if (loadDuration < 0)
                throw new ArgumentException("Load duration lower than zero");

            double loadDurationFactor = GetLoadDurationFactor(loadDuration);
            double probabiltyOfBreakageFactor = GetProbabiltyOfBreakageFactor();

            if (edgeResistance)
                return _surfaceBaseEdgeStress * loadDurationFactor * probabiltyOfBreakageFactor * _psiSurface;

            return _surfaceBaseStress * loadDurationFactor * probabiltyOfBreakageFactor * _psiSurface;
        }


        #endregion

        #region Equals - haschode - operators - serialization

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("PsiSurface", _psiSurface);
            info.AddValue("NGlassCoefficient", _nGlassCoefficient);
            info.AddValue("SigmaBase", _surfaceBaseStress);
            info.AddValue("SigmaBaseEdge", _surfaceBaseEdgeStress);
            info.AddValue("ProbabiltyOfBreakage", _probabiltyOfBreakage);
        }

        public bool Equals(GlassMaterialAstm other)
        {
            if (ReferenceEquals(this, other))
                return true;
            return !(other is null) && other._psiSurface.Equals(_psiSurface) &&
                                        other._nGlassCoefficient.Equals(_nGlassCoefficient) &&
                                        other._surfaceBaseStress.Equals(_surfaceBaseStress) &&
                                        other._surfaceBaseEdgeStress.Equals(_surfaceBaseEdgeStress) &&
                                        other._probabiltyOfBreakage.Equals(_probabiltyOfBreakage) &&
                                        base.Equals(other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;
            return Equals(obj as GlassMaterialAstm);
        }

        public override int GetHashCode()
        {
            int hashCode = 23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + _psiSurface.GetHashCode();
            hashCode = hashCode * -17 + _nGlassCoefficient.GetHashCode();
            hashCode = hashCode * -17 + _surfaceBaseStress.GetHashCode();
            hashCode = hashCode * -17 + _surfaceBaseEdgeStress.GetHashCode();
            hashCode = hashCode * -17 + _probabiltyOfBreakage.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(GlassMaterialAstm obj1, GlassMaterialAstm obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(GlassMaterialAstm obj1, GlassMaterialAstm obj2)
        {
            return !(obj1 == obj2);
        }
        #endregion
    }
}
