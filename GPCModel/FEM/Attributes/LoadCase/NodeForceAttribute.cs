using GPC.Geometry;
using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.FEM.Attributes
{
    public sealed class NodeForceAttribute : LoadCaseAttribute, INodeFemAttribute, IEquatable<NodeForceAttribute>
    {
        private double _fX;
        private double _fY;
        private double _fZ;
        private double _mX;
        private double _mY;
        private double _mZ;
        private CoordinateSystem _cSys;

        public double Fx => _fX;
        public double Fy => _fY;
        public double Fz => _fZ;
        public double Mx => _mX;
        public double My => _mY;
        public double Mz => _mZ;

        public NodeForceAttribute(LoadCase loadCase, CoordinateSystem cSys, double fx, double fy, double fz, double mx, double my, double mz) : base(loadCase)
        {
            _fX = fx;
            _fY = fy;
            _fZ = fz;
            _mX = mx;
            _mY = my;
            _mZ = mz;
            _cSys = cSys;
        }
    }
}
