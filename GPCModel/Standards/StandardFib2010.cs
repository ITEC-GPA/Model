using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Standards
{
	public class StandardFib2010 : StandardEN1992p11
	{
        #region Variables

        private readonly double _gammaF;

        #endregion

        /// <summary>
        /// Partial safety factor for FRC in tension (residual strength)
        /// </summary>
        public double GammF => _gammaF;

        public StandardFib2010()
        {
            _gammaF = 1.5;
        }
    }
}
