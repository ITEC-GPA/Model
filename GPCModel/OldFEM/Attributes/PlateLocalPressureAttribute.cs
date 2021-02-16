using GPC.Model.LoadCases;
using System;

namespace GPC.Model.FEM.Attributes
{
    public class PlateLocalPressureAttribute : Attribute, IPlateFemAttribute
    {
        private double _pressure;

        public double Pressure => _pressure;


        public PlateLocalPressureAttribute(LoadCase loadCase, double pressure) : base(loadCase)
        {
            this._pressure = pressure;
        }
    }
}
