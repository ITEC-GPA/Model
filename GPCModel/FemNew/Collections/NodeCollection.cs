using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using GPC.Model.Fem.FemObjects;

namespace GPC.Model.Fem.Collections
{
    [Serializable]
    public class NodeCollection : SortedCollection<Node>
    {
        protected class PositionComparer : IComparer<Node>
        {
            public int Compare(Node x, Node y)
            {
                if (Math.Abs(x.Position.X - y.Position.X) > GPC.Geometry.GeometryBase.GetDefaultTolerance())
                    return x.Position.X < y.Position.X ? -1 : 1;
                if (Math.Abs(x.Position.Y - y.Position.Y) > GPC.Geometry.GeometryBase.GetDefaultTolerance())
                    return x.Position.Y < y.Position.Y ? -1 : 1;
                if (Math.Abs(x.Position.Z - y.Position.Z) > GPC.Geometry.GeometryBase.GetDefaultTolerance())
                    return x.Position.Z < y.Position.Z ? -1 : 1;
                return 0;
            }
        }

        public NodeCollection()
            : base()
        {
        }

        protected NodeCollection(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        protected static PositionComparer _positionComparer = new PositionComparer();

        public override IComparer<Node> Comparer => _positionComparer;
    }
}
