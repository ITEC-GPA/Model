using GPC.Model.FEM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;

namespace GPC.Model.FEM
{
    public class Node : FEMObject
    {
        #region Variables

        protected Point3d _position;
        protected int _nodeIndex;
        protected string _nodeLabel;
        protected FEMNodeDoF _doF;
        #endregion

        #region Properties
        public Point3d Position => _position;
        public int NodeIndex => _nodeIndex;
        public string NodeLabel => _nodeLabel;
        public FEMNodeDoF DoF => _doF;
        #endregion

        #region Public Constructors
        public Node(Guid guid, Point3d position, int nodeIndex, int[] doFid, int[] activeDoF) 
            : base (guid, "")
        {
            _guid = guid;
            _doF = new FEMNodeDoF(doFid, activeDoF);
            _position = position;
            _nodeIndex = nodeIndex;
        }
        public Node(Guid guid, Point3d position, int nodeIndex, FEMNodeDoF doF)
            : base(guid, "")
        {
            _guid = guid;
            _doF = doF;
            _position = position;
            _nodeIndex = nodeIndex;
        }
        public Node(Guid guid, string name, Point3d position, int nodeIndex, int[] doFid, int[] activeDoF)
             : base(guid, name)
        {
            _guid = guid;
            _doF = new FEMNodeDoF(doFid, activeDoF);
            _position = position;
            _nodeIndex = nodeIndex;
        }
        public Node(Point3d position, int nodeIndex, int[] doFid, int[] activeDoF)
            : base(Guid.Empty, "")
        {
            _doF = new FEMNodeDoF(doFid, activeDoF);
            _position = position;
            _nodeIndex = nodeIndex;
        }
        public Node(Node node)
        {
            //_nodeLabel = node.NodeLabel;
            //_position = node.Position;
            //_doF = node.DoF;
            //_guid = node.Guid;
        }
        #endregion
        #region Public Methods Specific
        public void SetPosition(Point3d newPos)
        {
            _position = new Point3d(newPos.X, newPos.Y, newPos.Z);
        }
        #endregion
    }
}
