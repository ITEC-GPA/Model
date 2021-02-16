using GPC.Model.LoadCases;
using System;

namespace GPC.Model.FEMOld.Attributes
{
    public class NodeGlobalForceAttribute : Attribute, INodeFemAttribute
    {
        private double _fX;
        private double _fY;
        private double _fZ;
        private double _mX;
        private double _mY;
        private double _mZ;

        public double Fx => _fX;
        public double Fy => _fY;
        public double Fz => _fZ;
        public double Mx => _mX;
        public double My => _mY;
        public double Mz => _mZ;



        public NodeGlobalForceAttribute(LoadCase loadCase, double fx, double fy, double fz, double mx, double my, double mz) : base(loadCase)
        {
            _fX = fx;
            _fY = fy;
            _fZ = fz;
            _mX = mx;
            _mY = my;
            _mZ = mz;
        }
    }
}
