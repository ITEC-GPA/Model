using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Combinations
{
    /// <summary>
    /// This class collects all the coefficient of the ASCE7-16 Standard
    /// </summary>
    /// <remarks>Reference: ASCE7-16</remarks>
    public class StandardASCE16 : Standard
    {
        #region PUBLIC ENUMS        

        /// <summary>
        /// The limit states. Reference: ASCE7-16
        /// </summary>
        public enum LimitStates
        {
            LFRD,
            ASD
        }

        #endregion


        #region VARIABLES

        // LFRD
        private double _lfrd1PermCoef1;

        private double _lfrd2PermCoef1;
        private double _lfrd2VarCoef1;
        private double _lfrd2VarCoef2;

        private double _lfrd3PermCoef1;
        private double _lfrd3VarCoef1;
        private double _lfrd3VarCoef2L;
        private double _lfrd3VarCoef2W;

        private double _lfrd4PermCoef1;
        private double _lfrd4VarCoef1;
        private double _lfrd4VarCoef2;
        private double _lfrd4VarCoef3;

        private double _lfrd5PermCoef1;
        private double _lfrd5VarCoef1;

        private double _lfrd6PermCoef1;
        private double _lfrd6VarCoef1;
        private double _lfrd6VarCoef2;
        private double _lfrd6VarCoef3;

        private double _lfrd7PermCoef1;
        private double _lfrd7VarCoef1;
        private double _lfrd7VarCoef2;


        // ASD
        private double _asd1PermCoef1;

        private double _asd2PermCoef1;
        private double _asd2VarCoef1;

        private double _asd3PermCoef1;
        private double _asd3VarCoef1;

        private double _asd4PermCoef1;
        private double _asd4VarCoef1;
        private double _asd4VarCoef2;

        private double _asd5PermCoef1;
        private double _asd5VarCoef1;

        private double _asd6PermCoef1;
        private double _asd6VarCoef1;
        private double _asd6VarCoef2;
        private double _asd6VarCoef3;

        private double _asd7PermCoef1;
        private double _asd7VarCoef1;

        private double _asd8PermCoef1;
        private double _asd8VarCoef1;
        private double _asd8VarCoef2;

        private double _asd9PermCoef1;
        private double _asd9VarCoef1;
        private double _asd9VarCoef2;
        private double _asd9VarCoef3;
        private double _asd9VarCoef4;

        private double _asd10PermCoef1;
        private double _asd10VarCoef1;
        private double _asd10VarCoef2;



        // LFRD
        // combo1
        public double Lfrd1PermCoef1  =>  _lfrd1PermCoef1;
        // combo2
        public double Lfrd2PermCoef1  =>  _lfrd2PermCoef1;
        public double Lfrd2VarCoef1   =>  _lfrd2VarCoef1;
        public double Lfrd2VarCoef2   =>  _lfrd2VarCoef2;
        // combo3                
        public double Lfrd3PermCoef1  =>  _lfrd3PermCoef1;
        public double Lfrd3VarCoef1   =>  _lfrd3VarCoef1;
        public double Lfrd3VarCoef2L  =>  _lfrd3VarCoef2L;
        public double Lfrd3VarCoef2W  =>  _lfrd3VarCoef2W;
        // combo4      
        public double Lfrd4PermCoef1  =>  _lfrd4PermCoef1;
        public double Lfrd4VarCoef1   =>  _lfrd4VarCoef1;
        public double Lfrd4VarCoef2   =>  _lfrd4VarCoef2;
        public double Lfrd4VarCoef3   =>  _lfrd4VarCoef3;
        // combo5                          
        public double Lfrd5PermCoef1  =>  _lfrd5PermCoef1;
        public double Lfrd5VarCoef1   =>  _lfrd5VarCoef1;
        // combo6         
        public double Lfrd6PermCoef1  =>  _lfrd6PermCoef1;
        public double Lfrd6VarCoef1   =>  _lfrd6VarCoef1;
        public double Lfrd6VarCoef2   =>  _lfrd6VarCoef2;
        public double Lfrd6VarCoef3   =>  _lfrd6VarCoef3;
        // combo7            
        public double Lfrd7PermCoef1  =>  _lfrd7PermCoef1;
        public double Lfrd7VarCoef1   =>  _lfrd7VarCoef1;
        public double Lfrd7VarCoef2   =>  _lfrd7VarCoef2;

        // ASD
        // combo1
        public double Asd1PermCoef1 => _asd1PermCoef1;
        // combo2              
        public double Asd2PermCoef1 => _asd2PermCoef1;
        public double Asd2VarCoef1 => _asd2VarCoef1;
        // combo3     
        public double Asd3PermCoef1 => _asd3PermCoef1;
        public double Asd3VarCoef1 => _asd3VarCoef1;
        // combo4     
        public double Asd4PermCoef1 => _asd4PermCoef1;
        public double Asd4VarCoef1 => _asd4VarCoef1;
        public double Asd4VarCoef2 => _asd4VarCoef2;
        // combo5     
        public double Asd5PermCoef1 => _asd5PermCoef1;
        public double Asd5VarCoef1 => _asd5VarCoef1;
        // combo6     
        public double Asd6PermCoef1 => _asd6PermCoef1;
        public double Asd6VarCoef1 => _asd6VarCoef1;
        public double Asd6VarCoef2 => _asd6VarCoef2;
        public double Asd6VarCoef3 => _asd6VarCoef3;
        // combo7     
        public double Asd7PermCoef1 => _asd7PermCoef1;
        public double Asd7VarCoef1 => _asd7VarCoef1;
        // combo8     
        public double Asd8PermCoef1 => _asd8PermCoef1;
        public double Asd8VarCoef1 => _asd8VarCoef1;
        public double Asd8VarCoef2 => _asd8VarCoef2;
        // combo9     
        public double Asd9PermCoef1 => _asd9PermCoef1;
        public double Asd9VarCoef1 => _asd9VarCoef1;
        public double Asd9VarCoef2 => _asd9VarCoef2;
        public double Asd9VarCoef3 => _asd9VarCoef3;
        public double Asd9VarCoef4 => _asd9VarCoef4;
        // combo10     
        public double Asd10PermCoef1 => _asd10PermCoef1;
        public double Asd10VarCoef1 => _asd10VarCoef1;
        public double Asd10VarCoef2 => _asd10VarCoef2;

        #endregion


        #region PUBLIC CONSTRUCTOR

        public StandardASCE16()
        {
            // LFRD
            _lfrd1PermCoef1 = 1.4;

            _lfrd2PermCoef1 = 1.2;
            _lfrd2VarCoef1 = 1.6;
            _lfrd2VarCoef2 = 0.5;

            _lfrd3PermCoef1 = 1.2;
            _lfrd3VarCoef1 = 1.6;
            _lfrd3VarCoef2L = 1;
            _lfrd3VarCoef2W = 0.5;

            _lfrd4PermCoef1 = 1.2;
            _lfrd4VarCoef1 = 1.0;
            _lfrd4VarCoef2 = 1.0;
            _lfrd4VarCoef3 = 0.5;

            _lfrd5PermCoef1 = 0.9;
            _lfrd5VarCoef1 = 1.0;

            _lfrd6PermCoef1 = 1.2;
            _lfrd6VarCoef1 = 1.0;
            _lfrd6VarCoef2 = 1.0;
            _lfrd6VarCoef3 = 0.2;

            _lfrd7PermCoef1 = 0.9;
            _lfrd7VarCoef1 = -1;
            _lfrd7VarCoef2 = +1;

            // ASD
            _asd1PermCoef1 = 1.0;

            _asd2PermCoef1 = 1.0;
            _asd2VarCoef1 = 1.0;

            _asd3PermCoef1 = 1.0;
            _asd3VarCoef1 = 1.0;

            _asd4PermCoef1 = 1.0;
            _asd4VarCoef1 = 0.75;
            _asd4VarCoef2 = 0.75;

            _asd5PermCoef1 = 1;
            _asd5VarCoef1 = 0.6;

            _asd6PermCoef1 = 1.0;
            _asd6VarCoef1 = 0.75;
            _asd6VarCoef2 = 0.75 * 0.6;
            _asd6VarCoef3 = 0.75;

            _asd7PermCoef1 = 0.6;
            _asd7VarCoef1 = 0.6;

            _asd8PermCoef1 = 1;
            _asd8VarCoef1 = 0.7;
            _asd8VarCoef2 = 0.7;

            _asd9PermCoef1 = 1.0;
            _asd9VarCoef1 = 0.525;
            _asd9VarCoef2 = 0.525;
            _asd9VarCoef3 = 0.75;
            _asd9VarCoef4 = 0.75;

            _asd10PermCoef1 = 0.6;
            _asd10VarCoef1 = -0.7;
            _asd10VarCoef2 = 0.7;
        }

        #endregion


        public override CombinationsCollection CreateCombinations<T>(LoadCaseBase[] loadCases, T options)
        {
            throw new NotImplementedException();
        }
    }
}
