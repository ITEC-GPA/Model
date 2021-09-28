using GPC.Model.Standards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Materials
{
	public class ConcreteMaterialACI318 : ConcreteMaterial
	{
        #region Variables

        protected StandardACI318 _standard;

        #endregion 

        #region Properties

        public StandardACI318 Standard => _standard;

		#endregion

		public ConcreteMaterialACI318(string name, double fck, StandardACI318 standard)
			: base(name, fck)
		{
            if (fck < 17.0)
                throw new ArgumentException("fc' less than the minimum fc' permitted. See §19.2.1.1");

            _standard = standard ?? throw new ArgumentNullException(nameof(standard));
			SetProperties();
		}

        public ConcreteMaterialACI318(string name, double fck)
            : this(name, fck, new StandardACI318())
		{

		}

        #region Public Methods

        #endregion

        #region Protected Methods

        protected virtual void SetProperties()
        {
            _elasticModulus = CalculateEc();
        }

        protected virtual double CalculateEc()
		{
            return 4700 * Math.Sqrt(_fck);
		}

		public override double CalculateSigmaC(double epsilonC)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
