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
                
        #endregion 

        #region Properties
               

		#endregion

		public ConcreteMaterialACI318(string name, double fck)
			: base(name, fck)
		{
            if (fck < 17.0)
                throw new ArgumentException("fc' less than the minimum fc' permitted. See §19.2.1.1");

			SetProperties();
		}

        public ConcreteMaterialACI318(double fck)
            : this("", fck)
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

		#endregion
	}
}
