using GPC.Model.FEM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;

namespace GPC.Model.FEM
{
    public class Node
    {
        #region Variables
        protected Point3d _position;
        protected int _nodeIndex;
        protected string _nodeLabel;
        protected FEMNodeDoF _doF;
        protected Guid _guid;
        #endregion

        #region Properties
        public Point3d Position => _position;
        protected int NodeIndex => _nodeIndex;
        public string NodeLabel => _nodeLabel;
        public FEMNodeDoF DoF => _doF;
        protected Guid Guid => _guid;
        #endregion

        #region Public Constructors
        public Node(Guid guid, Point3d position, int nodeIndex, int[] doFid, int[] activeDoF) 
        {
            _guid = guid;
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

        #endregion
    }
}
