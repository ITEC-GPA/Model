using GPC.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Model.FEM.Attributes;
using GPC.Model.Elements;

namespace GPC.Model.FEM
{
    public class Node : FEMObject
    {
        #region Variables

        protected Point3d _position;

        protected int _nodeIndex;

        protected string _nodeLabel;

        protected FEMNodeDoF _doF;

        private List<INodeFemAttribute> _attributes;

        #endregion 

        #region Properties

        public Point3d Position => _position;

        public int NodeIndex => _nodeIndex;

        public string NodeLabel => _nodeLabel;

        public FEMNodeDoF DoF => _doF;

        public List<INodeFemAttribute> Attributes => _attributes;

        #endregion

        #region Public Constructors

        public Node(Guid guid, Point3d position, int nodeIndex, Restrain restrain)
            : base(guid, "")
        {
            int[] doFid = new int[6];
            int[] activeDoF = new int[6];

            if (restrain != null)
            {
                doFid[0] = restrain.D1 == true ? 1 : 0;
                doFid[1] = restrain.D2 == true ? 1 : 0;
                doFid[2] = restrain.D3 == true ? 1 : 0;
                doFid[3] = restrain.R1 == true ? 1 : 0;
                doFid[4] = restrain.R2 == true ? 1 : 0;
                doFid[5] = restrain.R3 == true ? 1 : 0;
            }
            else
            {
                doFid[0] = 0;
                doFid[1] = 0;
                doFid[2] = 0;
                doFid[3] = 0;
                doFid[4] = 0;
                doFid[5] = 0;
            }

            _doF = new FEMNodeDoF(doFid, activeDoF);
            _position = position ?? throw new ArgumentNullException("Node position cannot be null");
            _nodeIndex = nodeIndex >= 0 ? nodeIndex : throw new ArgumentException("Node index cannot be lower than zero");
        }


        public Node(Guid guid, Point3d position, int nodeIndex, int[] doFid, int[] activeDoF)
            : this(guid, "", position, nodeIndex, doFid, activeDoF)
        {

        }

        public Node(Guid guid, Point3d position, int nodeIndex, FEMNodeDoF doF)
            : base(guid, "")
        {
            _doF = doF;
            _position = position ?? throw new ArgumentNullException("Node position cannot be null");
            _nodeIndex = nodeIndex >= 0 ? nodeIndex : throw new ArgumentException("Node index cannot be lower than zero");
        }

        public Node(Guid guid, string name, Point3d position, int nodeIndex, int[] doFid, int[] activeDoF)
             : base(guid, name)
        {
            _doF = new FEMNodeDoF(doFid, activeDoF);
            _position = position ?? throw new ArgumentNullException("Node position cannot be null");
            _nodeIndex = nodeIndex >= 0 ? nodeIndex : throw new ArgumentException("Node index cannot be lower than zero");
        }

        public Node(Point3d position, int nodeIndex, int[] doFid, int[] activeDoF)
            : this(Guid.Empty, "", position, nodeIndex, doFid, activeDoF)
        {

        }

        #endregion 

        #region Public Methods Specific

        public Restrain GetRestrain()
        {
            return _doF.GetRestrain();
        }

        public void SetPosition(Point3d newPos)
        {
            _position = new Point3d(newPos.X, newPos.Y, newPos.Z);
        }

        public void AddAttribute(INodeFemAttribute attribute)
        {
            _attributes.Add(attribute);
        }

        #endregion 
    }
}