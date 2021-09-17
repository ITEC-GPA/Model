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
using GPC.Model.Sections.Rebar;

namespace GPC.Model.Elements
{
    [Serializable]
    public class Rebar : Element
    {
        public const double PRESTRESSED_LIMIT = 0.0000001;

        #region Variables

        protected IRebarSection _rebarSection;
        protected Point3d _startPosition;
        protected Point3d _endPosition;
        protected double _epsilonP;
        protected double _tensionP;

        #endregion

        #region Properties

        public double EffectiveArea => _rebarSection.Area;

        public Point3d StartPosition => _startPosition;

        public Point3d EndPosition => _endPosition;

        public RebarMaterial RebarMaterial => _rebarSection.RebarMaterial;

        public double EpsilonP => _epsilonP;

        public double TensionP => _tensionP;

        public bool IsPrestressed => _tensionP > 0.0;

        #endregion

        #region Public Constructors

        public Rebar(IRebarSection section, Point3d startPosition, Point3d endPosition, double epsilonP, double tensionP, int id, Guid guid)
            : base(guid)    
        {
            _rebarSection = section;
            _startPosition = startPosition;
            _endPosition = endPosition;
            _epsilonP = epsilonP;
            _tensionP = tensionP;
            _id = id;
        }

        public Rebar(IRebarSection section, Point3d startPosition, Point3d endPosition, double epsilonP = 0.0, double tensionP = 0.0, int id = IDUNASSIGNED)
            : this(section, startPosition, endPosition, epsilonP, tensionP, id, new Guid())
        {
        }

        public Rebar(IRebarSection section, Point3d startPosition, Point3d endPosition, int id = IDUNASSIGNED, double epsilonP = 0.0, double tensionP = 0.0)
            : this(section, startPosition, endPosition, epsilonP, tensionP, id, new Guid())
        {
        }

        public Rebar(IRebarSection section, Point2d position, int id = IDUNASSIGNED) 
            : this(section, position, position, 0.0, 0.0, id)
        {
        }

        public Rebar(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
            _rebarSection = (IRebarSection)info.GetValue("Section", typeof(IRebarSection));
            _startPosition = (Point3d)info.GetValue("StartPosition", typeof(Point3d));
            _endPosition = (Point3d)info.GetValue("EndPosition", typeof(Point3d));
            _epsilonP = info.GetDouble("EpsilonP");
            _tensionP = info.GetDouble("TensionP");
        }

        #endregion


        #region Public Methods Specific

        public void AddPrestress(double tensionP)
        {
            _tensionP = tensionP;
        }

        #endregion


        #region Field Serialization

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Section", _rebarSection);
            info.AddValue("StartPosition", _startPosition);
            info.AddValue("EndPosition", _endPosition);
            info.AddValue("EpsilonP", _epsilonP);
            info.AddValue("TensionP", _tensionP);
        }

		#endregion
	}
}
