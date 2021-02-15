using GPC.Geometry;
using GPC.Model.Elements;
using GPC.Model.FEM.Attributes;
using GPC.Model.Results;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.FEM
{
    public abstract class Plate : FEMElement, ISerializable, IEquatable<Plate>, ICloneable
    {
        #region Variables

        protected PlateAnalysisType _analysisType;

        protected double _A;

        protected List<IPlateFemAttribute> _attributes;

        #endregion Variables

        #region Properties

        public PlateAnalysisType AnalysisType => _analysisType;

        public double A => _A;

        public bool IsTriangle => _nodesGlobal.Length == 3 ? true : false;

        public bool IsQuad => _nodesGlobal.Length == 4 ? true : false;

        public List<IPlateFemAttribute> Attributes => _attributes;

        #endregion Properties

        #region Public Contructors

        //public Plate(Guid guid, IGlassProperty property, int plateIndex, Node[] nodes)
        //    : base(guid, integrator, property, plateIndex)
        //{
        //    PlateSetUp(nodes, integrator);
        //}

        public Plate(Guid guid, IPlateProperty property, int plateIndex, Node[] nodes)
            : this(guid, property, null, plateIndex, nodes)
        {

        }

        //public Plate(Guid guid, LaminatedGlassProperty property, int plateIndex, Node[] nodes)
        //    : this(guid, property, null, plateIndex, nodes)
        //{

        //}


        //public Plate(Guid guid, LaminatedGlassProperty property, FEMPlateIntegrator integrator, int plateIndex, Node[] nodes)
        //    : base(guid, integrator, property, plateIndex)
        //{
        //    PlateSetUp(nodes, integrator);
        //}

        public Plate(Guid guid, IPlateProperty property, FEMPlateIntegrator integrator, int plateIndex, Node[] nodes)
            : base(guid, integrator, (ElementProperty)property, plateIndex)
        {
            _attributes = new List<IPlateFemAttribute>();

            SetElement(nodes);
            SetLocalCoordinateSystem(0.0);

            if (integrator != null)
            {
                _integrator = integrator;
                BuildElementDoF();
                _integrator.StartIntegration(this);
            }
        }

        //public Plate(Guid guid, PlateProperty property, FEMPlateIntegrator integrator, int plateIndex, Node[] nodes)
        //    : base(guid, integrator, property, plateIndex)
        //{
        //    PlateSetUp(nodes, integrator);
        //}

        private void PlateSetUp(Node[] nodes, FEMPlateIntegrator integrator)
        {
            _attributes = new List<IPlateFemAttribute>();

            SetElement(nodes);
            SetLocalCoordinateSystem(0.0);

            if (integrator != null)
            {
                _integrator = integrator;
                BuildElementDoF();
                _integrator.StartIntegration(this);
            }
        }


        protected Plate(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            throw new NotImplementedException();
        }

        #endregion Public Contructors

        #region Private methods

        protected override void SetElement(Node[] arrayNode)
        {
            _nodesGlobal = new Node[arrayNode.Length];
            _nodesLocal = new Node[arrayNode.Length];
            _nodesGlobal = arrayNode;
        }

        protected override void SetLocalCoordinateSystem(double rotationAngle)
        {
            _coordSys = new CoordinateSystem(_nodesGlobal[0].Position, _nodesGlobal[1].Position, _nodesGlobal[2].Position, rotationAngle, string.Empty, Guid.NewGuid());
            for (int nd = 0; nd < _nodesGlobal.Length; nd++)
            {
                var pointLocal = _coordSys.ToLocal(_nodesGlobal[nd].Position);
                _nodesLocal[nd] = new Node(new Guid(), pointLocal, _nodesGlobal[nd].NodeIndex, _nodesGlobal[nd].DoF);
            }
        }

        #endregion Private methods

        #region Public Methods Override

        public void AddAttribute(IPlateFemAttribute attribute)
        {
            _attributes.Add(attribute);
        }

        public int[] GetConnection()
        {
            if (IsQuad)
                return new int[4] { NodesGlobal[0].NodeIndex, NodesGlobal[1].NodeIndex, NodesGlobal[2].NodeIndex, NodesGlobal[3].NodeIndex };
            else
                return new int[3] { NodesGlobal[0].NodeIndex, NodesGlobal[1].NodeIndex, NodesGlobal[2].NodeIndex };
        }

        public virtual void CalcArea(double A)
        {
            _A = A;
        }

        public bool Equals(Plate other)
        {
            return !(other is null) && _index == other._index && _property == other._property && NodesGlobal[0] == other.NodesGlobal[0] && NodesGlobal[1] == other.NodesGlobal[1] && NodesGlobal[2] == other.NodesGlobal[2] && NodesGlobal[3] == other.NodesGlobal[3];
        }

        public abstract object Clone();

        #endregion Public Methods Override

        #region Operators overrides

        public static bool operator ==(Plate plate1, Plate plate2)
        {
            if (ReferenceEquals(plate1, plate2))
                return true;
            return plate1.Equals(plate2);
        }

        public static bool operator !=(Plate plate1, Plate plate2)
        {
            return !(plate1 == plate2);
        }

        #endregion Operators overrides

        #region Public Methods Override

        public override bool Equals(object obj)
        {
            if (obj is Plate plate)
            {
                return Equals(plate);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            int hashCode = -44831239;
            hashCode = hashCode * -1521134295 + _index.GetHashCode();
            hashCode = hashCode * -1521134295 + _property.GetHashCode();
            hashCode = hashCode * -1521134295 + NodesGlobal[0].GetHashCode();
            hashCode = hashCode * -1521134295 + NodesGlobal[1].GetHashCode();
            hashCode = hashCode * -1521134295 + NodesGlobal[2].GetHashCode();
            hashCode = hashCode * -1521134295 + NodesGlobal[3].GetHashCode();
            return hashCode;
        }

        #endregion Public Methods Override
    }
}