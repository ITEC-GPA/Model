using GPC.Model.Elements;
using GPC.Model.Restrains;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GPC.Model.Costrains
{
    public class RigidLink : Costrain
    {
        #region constructor

        public RigidLink(NodeElement node1, NodeElement[] nodes)
            : base(node1, nodes)
        {
            _startNode = node1;
            _endNodes = nodes;

            int nNodes = nodes.Count();
            _links = new MultiPointsCostrain[nNodes * 6];
            List<MultiPointsCostrain> links = new List<MultiPointsCostrain>();
            for (int i = 0; i < nNodes; i++)
            {
                MultiPointsCostrain[] linksPoint = GetRigidLink(node1, nodes[i]);
                links.AddRange(linksPoint);
            }
            _links = links.ToArray();
        }

        public RigidLink(NodeElement node1, NodeElement node2)
            : this(node1, new NodeElement[] { node2 })
        {

        }

        #endregion

        #region PublicFunctions

        /// <summary>
        /// Ritorna le equazioni per rigid link tra 2 nodi
        /// </summary>
        /// <param name="node1"></param>
        /// <param name="node2"></param>
        /// <returns></returns>
        public static MultiPointsCostrain[] GetRigidLink(NodeElement node1, NodeElement node2)
        {
            double LX = Math.Abs(node1.Position.X - node2.Position.X);
            double LY = Math.Abs(node1.Position.Y - node2.Position.Y);
            double LZ = Math.Abs(node1.Position.Z - node2.Position.Z);

            double factorRX;
            double factorRY;
            double factorRZ;

            //Y' = Y + cos(Theta) * L
            //0 = Y' + Y + cos(Theta) * L = Y' + Y + 1.0 * L

            #region traslations
            MultiPointsCostrain.Equation[] eqtnDX = new MultiPointsCostrain.Equation[4];
            factorRY = node2.Position.Z - node1.Position.Z < 0 ? -1.0 : 1.0;
            factorRZ = node2.Position.Y - node1.Position.Y < 0 ? 1.0 : -1.0;

            eqtnDX[0] = new MultiPointsCostrain.Equation(node2, GeometryRestrain.DOF.DX, -1.0);
            eqtnDX[1] = new MultiPointsCostrain.Equation(node1, GeometryRestrain.DOF.DX, 1.0);
            eqtnDX[2] = new MultiPointsCostrain.Equation(node1, GeometryRestrain.DOF.RY, factorRY * LZ);
            eqtnDX[3] = new MultiPointsCostrain.Equation(node1, GeometryRestrain.DOF.RZ, factorRZ * LY);

            MultiPointsCostrain.Equation[] eqtnDY = new MultiPointsCostrain.Equation[4];
            factorRX = node2.Position.Z - node1.Position.Z < 0 ? 1.0 : -1.0;
            factorRZ = node2.Position.X - node1.Position.X < 0 ? -1.0 : 1.0;

            eqtnDY[0] = new MultiPointsCostrain.Equation(node2, GeometryRestrain.DOF.DY, -1.0);
            eqtnDY[1] = new MultiPointsCostrain.Equation(node1, GeometryRestrain.DOF.DY, 1.0);
            eqtnDY[2] = new MultiPointsCostrain.Equation(node1, GeometryRestrain.DOF.RX, factorRX * LZ);
            eqtnDY[3] = new MultiPointsCostrain.Equation(node1, GeometryRestrain.DOF.RZ, factorRZ * LX);

            MultiPointsCostrain.Equation[] eqtnDZ = new MultiPointsCostrain.Equation[4];
            factorRX = node2.Position.Y - node1.Position.Y < 0 ? -1.0 : 1.0;
            factorRY = node2.Position.X - node1.Position.X < 0 ? 1.0 : -1.0;
            eqtnDZ[0] = new MultiPointsCostrain.Equation(node2, GeometryRestrain.DOF.DZ, -1.0);
            eqtnDZ[1] = new MultiPointsCostrain.Equation(node1, GeometryRestrain.DOF.DZ, 1.0);
            eqtnDZ[2] = new MultiPointsCostrain.Equation(node1, GeometryRestrain.DOF.RX, factorRX * LY);
            eqtnDZ[3] = new MultiPointsCostrain.Equation(node1, GeometryRestrain.DOF.RY, factorRY * LX);
            #endregion

            #region rotation
            //RX node1 = RX node2
            MultiPointsCostrain.Equation[] eqtnRX = new MultiPointsCostrain.Equation[2];
            eqtnRX[0] = new MultiPointsCostrain.Equation(node2, GeometryRestrain.DOF.RX, -1.0);
            eqtnRX[1] = new MultiPointsCostrain.Equation(node1, GeometryRestrain.DOF.RX, 1.0);

            //RY node1 = RY node2
            MultiPointsCostrain.Equation[] eqtnRY = new MultiPointsCostrain.Equation[2];
            eqtnRY[0] = new MultiPointsCostrain.Equation(node2, GeometryRestrain.DOF.RY, -1.0);
            eqtnRY[1] = new MultiPointsCostrain.Equation(node1, GeometryRestrain.DOF.RY, 1.0);

            //RZ node1 = RZ node2
            MultiPointsCostrain.Equation[] eqtnRZ = new MultiPointsCostrain.Equation[2];
            eqtnRZ[0] = new MultiPointsCostrain.Equation(node2, GeometryRestrain.DOF.RZ, -1.0);
            eqtnRZ[1] = new MultiPointsCostrain.Equation(node1, GeometryRestrain.DOF.RZ, 1.0);
            #endregion

            #region equations
            MultiPointsCostrain[] links = new MultiPointsCostrain[6];
            links[0] = new MultiPointsCostrain(eqtnDX, 0.0);
            links[1] = new MultiPointsCostrain(eqtnDY, 0.0);
            links[2] = new MultiPointsCostrain(eqtnDZ, 0.0);
            links[3] = new MultiPointsCostrain(eqtnRX, 0.0);
            links[4] = new MultiPointsCostrain(eqtnRY, 0.0);
            links[5] = new MultiPointsCostrain(eqtnRZ, 0.0);
            #endregion

            return links;
        }
        #endregion
    }
}
