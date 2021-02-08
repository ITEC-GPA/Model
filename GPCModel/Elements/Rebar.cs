using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.Materials;

namespace GPC.Model.Elements
{
    [Serializable]
    public class Rebar : Element
    {
        public const double PRESTRESSED_LIMIT = 0.0000001;

        #region Variables
        /// <summary>
        /// <param name="_diameter">rebar diameter [mm]</param>
        /// <param name="_effectiveArea">Area to be used for calculations [mm2]</param>
        /// <param name="_p1"> Start Point of the rebar</param>
        /// <param name="_p2"> End Point of the rebar</param>
        /// <param name="_material"> Material of the rebar</param>
        /// </summary>
        protected double _diameter;
        protected double _effectiveArea;
        protected Point2d _startPoint;
        protected Point2d _endPoint;
        protected RebarMaterial _material;
        protected Point2d _position;
        private static int SerializationVersion = 1;
        protected double _epsilonP;
        protected double _tensionP;
        protected bool _isPrestressed;
        #endregion

        #region Properties
        public double Diameter => _diameter;
        public double EffectiveArea => _effectiveArea;
        public Point2d StartPoint => _startPoint;
        public Point2d EndPoint => _endPoint;
        public Point2d Position => _position;
        public RebarMaterial Material => _material;
        public double EpsilonP => _epsilonP;
        public double TensionP => _tensionP;
        public bool IsPrestressed => _isPrestressed;
        #endregion

        #region Public Constructors
        public Rebar(double diameter, double effectiveArea, Point2d startPoint, Point2d endPoint, Point2d position, RebarMaterial material, Guid guid, double epsilonP, double tensionP) :
            base(guid)
        {
            _diameter = diameter;
            _effectiveArea = effectiveArea;
            _startPoint = new Point2d(startPoint);
            _endPoint = new Point2d(endPoint);
            _position = new Point2d(position);
            _material = material;
            _epsilonP = epsilonP;
            _tensionP = tensionP;

            _isPrestressed = false;
            if (_tensionP > 0)
            {
                _isPrestressed = true;
            }
        }

        public Rebar(double diameter, double effectiveArea, Point2d startPoint, Point2d endPoint, Point2d position, RebarMaterial material, Guid guid) :
             this(diameter, effectiveArea, startPoint, endPoint, position, material, guid, 0.0, 0.0)
        {
        }

        public Rebar(double diameter, double effectiveArea, Point2d position, RebarMaterial material) :
            this(diameter, effectiveArea, new Point2d(0, 0), new Point2d(0, 0), position, material, Guid.NewGuid())
        {
        }

        public Rebar(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
            SerializationVersion = info.GetInt32("SerializationVersion");
            _diameter = info.GetDouble("Diameter");
            _effectiveArea = info.GetDouble("EffectiveArea");
            _startPoint = (Point2d)info.GetValue("StartPoint", typeof(Point2d));
            _endPoint = (Point2d)info.GetValue("EndPoint", typeof(Point2d));
            _position = (Point2d)info.GetValue("Position", typeof(Point2d));
            _material = (RebarMaterial)info.GetValue("Material", typeof(RebarMaterial));
            _epsilonP = info.GetDouble("EpsilonP");
            _tensionP = info.GetDouble("TensionP");
        }

        #endregion

        #region Public Methods Override
        #endregion

        #region Public Methods Specific
        public void ChangeDiameter(double newDiamter)
        {
            _diameter = newDiamter;
        }
        public void ChangeMaterial(RebarMaterial newMaterial)
        {
            _material = newMaterial;
        }
        public void AddPrestress(double tensionP)
        {
            _tensionP = tensionP;
        }
        #endregion

        #region Private Methods Specific
        #endregion


        #region FIELD_SERIALIZATION
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("SerializationVersion", SerializationVersion);
            info.AddValue("Diameter", _diameter);
            info.AddValue("EffectiveArea", _effectiveArea);
            info.AddValue("StartPoint", _startPoint);
            info.AddValue("EndPoint", _endPoint);
            info.AddValue("Position", _position);
            info.AddValue("Material", _material);
            info.AddValue("EpsilonP", _epsilonP);
            info.AddValue("TensionP", _tensionP);
        }
        #endregion 

        #region FIELD_VARIABLES

        //private double _x;
        //private double _y;

        #endregion
    }

    public class Rebars : IEnumerable<Rebar>
    {
        private readonly List<Rebar> _bars;

        public Rebars()
        {
            _bars = new List<Rebar>();
        }

        #region IEnumerable

        public IEnumerator<Rebar> GetEnumerator()
        {
            return _bars.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_bars).GetEnumerator();
        }

        #endregion IEnumerable

        public int Count => _bars.Count;

        public Rebar this[int index] => _bars[index];

        public void AddRebar(double diameter, double effectiveArea, Point2d startPoint, Point2d endPoint, Point2d position, RebarMaterial material, Guid guid)
        {
            _bars.Add(new Rebar(diameter, effectiveArea, startPoint, endPoint, position, material, guid));
        }
        public void AddRebar(Rebar rebar)
        {
            _bars.Add(rebar);
        }
    }
}
