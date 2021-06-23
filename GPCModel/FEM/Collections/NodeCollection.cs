using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace GPC.Model.FEM.Collections
{
    [Serializable]
    public class NodeCollection : SortedCollection<Node>
    {
        protected class PositionComparer : IComparer<Node>
        {
            public int Compare(Node x, Node y)
            {
                if (Math.Abs(x.Position.X - y.Position.X) > 0.001)
                    return x.Position.X < y.Position.X ? -1 : 1;
                if (Math.Abs(x.Position.Y - y.Position.Y) > 0.001)
                    return x.Position.Y < y.Position.Y ? -1 : 1;
                if (Math.Abs(x.Position.Z - y.Position.Z) > 0.001)
                    return x.Position.Z < y.Position.Z ? -1 : 1;
                return 0;
            }
        }

        protected static PositionComparer _positionComparer = new PositionComparer();

        public override IComparer<Node> Comparer => _positionComparer;
    }
}
