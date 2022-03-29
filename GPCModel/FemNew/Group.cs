using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Fem.FemObjects;

namespace GPC.Model.Fem
{
    /// <summary>
    /// This class to be used to group some <see cref="FemObject"/> togethers.
    /// </summary>
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public sealed class Group : ModelObjectId, IEquatable<Group>
    {
        // eventuali opzioni

        // costruttore internal. Solo la classe fem model deve poter essere in grado di instaliazzare i gruppi
        internal Group(string name) : base(name)
        {

        }

        private Group(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        public bool Equals(Group other)
        {
            return base.Equals(other);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is Group group && Equals(group);
        }

        public static bool operator ==(Group obj1, Group obj2)
        {
            return obj1.Equals(obj2);
        }

        public static bool operator !=(Group obj1, Group obj2)
        {
            return !(obj1 == obj2);
        }

        private string GetDebuggerDisplay()
        {
            return ToString();
        }

        public override string ToString()
        {
            if (Name != string.Empty)
                return $"{Id}-{Name}";
            else
                return $"{Id}";
        }
    }
}