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

        protected double _diameter;
        protected RebarMaterial _material;
        protected Point2d _position;
        protected double _epsilonP;
        protected double _tensionP;

        private static int SerializationVersion = 1;

        #endregion

        #region Properties

        public double Diameter => _diameter;

        public double EffectiveArea => CalculateEffectiveArea();

        public Point2d Position => _position;

        public RebarMaterial Material => _material;

        public double EpsilonP => _epsilonP;

        public double TensionP => _tensionP;

        public bool IsPrestressed => _tensionP > 0.0;

        #endregion

        #region Public Constructors

        public Rebar(double diameter, Point2d position, RebarMaterial material, double epsilonP, double tensionP, Guid guid)
            : base(guid)
        {
            _diameter = diameter;            
            _position = position;
            _material = material;
            _epsilonP = epsilonP;
            _tensionP = tensionP;
        }

        public Rebar(double diameter, Point2d position, RebarMaterial material, double epsilonP = 0.0, double tensionP = 0.0)
            : this(diameter, position, material, epsilonP, tensionP, new Guid())
        {
        }

        public Rebar(double diameter, Point2d position, RebarMaterial material) :
            this(diameter, position, material, 0.0, 0.0, new Guid())
        {
        }

        public Rebar(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
            SerializationVersion = info.GetInt32("SerializationVersion");
            _diameter = info.GetDouble("Diameter");            
            _position = (Point2d)info.GetValue("Position", typeof(Point2d));
            _material = (RebarMaterial)info.GetValue("Material", typeof(RebarMaterial));
            _epsilonP = info.GetDouble("EpsilonP");
            _tensionP = info.GetDouble("TensionP");
        }

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

        protected double CalculateEffectiveArea()
		{
            return Diameter * Diameter * Math.PI / 4;
		}

        #endregion

        #region Field Serialization

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("SerializationVersion", SerializationVersion);
            info.AddValue("Diameter", _diameter);
            info.AddValue("Position", _position);
            info.AddValue("Material", _material);
            info.AddValue("EpsilonP", _epsilonP);
            info.AddValue("TensionP", _tensionP);
        }

        #endregion 
    }

    public class Rebars : IEnumerable<Rebar>
    {
        private readonly List<Rebar> _bars;

        public Rebars()
        {
            _bars = new List<Rebar>();
        }

        public Rebars(List<Rebar> rebars)
        {
            _bars = new List<Rebar>(rebars);
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

        public double TotalArea => _bars.Select(i => i.EffectiveArea).Distinct().Sum();

        public Rebar this[int index] => _bars[index];

        public void AddRebar(double diameter, Point2d position, RebarMaterial material, Guid guid)
        {
            _bars.Add(new Rebar(diameter, position, material, 0.0, 0.0, guid));
        }

        public void AddRebar(double diameter, Point2d position, RebarMaterial material)
        {
            _bars.Add(new Rebar(diameter, position, material));
        }

        public void AddRebar(Rebar rebar)
        {
            _bars.Add(rebar);
        }
    }
}
