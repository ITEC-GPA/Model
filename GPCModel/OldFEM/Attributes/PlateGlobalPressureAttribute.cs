using GPC.Model.LoadCases;
using System;

namespace GPC.Model.FEM.Attributes
{
    public class PlateGlobalPressureAttribute : Attribute, IPlateFemAttribute
    {
        private double _pX;

        private double _pY;

        private double _pZ;



        public double Px => _pX;

        public double Py => _pY;

        public double Pz => _pZ;



        public PlateGlobalPressureAttribute(LoadCase loadCase, double px, double py, double pz) : base(loadCase)
        {
            this._pX = px;
            this._pY = py;
            this._pZ = pz;
        }
    }
}
