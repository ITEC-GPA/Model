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
    /// <summary>
    /// This class rapresent a rebar object in the 3d space, not a rebar in a concrete section 
    /// </summary>
    [Serializable]
    public class Rebar : Element
    {
        // public const double PRESTRESSED_LIMIT = 0.0000001;

        #region Variables

        protected IRebarSection _rebarSection;
        protected Point3d _startPosition;
        protected Point3d _endPosition;
        // protected double _epsilonP;

        #endregion

        #region Properties

        public double Area => _rebarSection.Area;

        public Point3d StartPosition => _startPosition;

        public Point3d EndPosition => _endPosition;

        public RebarMaterial RebarMaterial => _rebarSection.RebarMaterial;

        // public double EpsilonP => _epsilonP;

        // public double TensionP => _tensionP;

        // public bool IsPrestressed => _tensionP > 0.0;

        #endregion

        #region Public Constructors

        public Rebar(IRebarSection section, Point3d startPosition, Point3d endPosition, int id, Guid guid)
            : base(guid)    
        {
            _rebarSection = section;
            _startPosition = startPosition;
            _endPosition = endPosition;
            //_epsilonP = epsilonP;
            _id = id;
        }

        public Rebar(IRebarSection section, Point3d startPosition, Point3d endPosition, int id = IDUNASSIGNED)
            : this(section, startPosition, endPosition, id, new Guid())
        {
        }

        public Rebar(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
            _rebarSection = (IRebarSection)info.GetValue("Section", typeof(IRebarSection));
            _startPosition = (Point3d)info.GetValue("StartPosition", typeof(Point3d));
            _endPosition = (Point3d)info.GetValue("EndPosition", typeof(Point3d));
            //_epsilonP = info.GetDouble("EpsilonP");
            //_tensionP = info.GetDouble("TensionP");
        }

        #endregion

        #region Field Serialization

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Section", _rebarSection);
            info.AddValue("StartPosition", _startPosition);
            info.AddValue("EndPosition", _endPosition);
            //info.AddValue("EpsilonP", _epsilonP);
            //info.AddValue("TensionP", _tensionP);
        }

		#endregion
	}
}
