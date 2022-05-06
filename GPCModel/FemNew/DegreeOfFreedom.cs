using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Fem;
using GPC.Utilities.Extensions;


namespace GPC.Model.Fem
{

    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public struct DegreeOfFreedom : IEquatable<DegreeOfFreedom>
    {

        public enum DegreeOfFreedomTypes
        {
            [Description("D")]
            Displacement,
            [Description("R")]
            Rotation,
            [Description("T")]
            Temperature,
            [Description("F")]
            HeatFlux,
        }

        public enum DegreeOfFreedomLocalDirections
        {
            [Description("")]
            None,
            [Description("1")]
            Axis1,
            [Description("2")]
            Axis2,
            [Description("3")]
            Axis3
        }

        public DegreeOfFreedomTypes DegreeOfFreedomType { get; }
        public DegreeOfFreedomLocalDirections DegreeOfFreedomLocalDirection { get; }

        public DegreeOfFreedom(DegreeOfFreedomTypes degreeOfFreedomType, DegreeOfFreedomLocalDirections degreeOfFreedomLocalDirection)
        {
            DegreeOfFreedomType = degreeOfFreedomType;
            DegreeOfFreedomLocalDirection = degreeOfFreedomLocalDirection;
        }

        private string GetDebuggerDisplay()
        {
            return $"{DegreeOfFreedomType.GetDescription()}{DegreeOfFreedomLocalDirection.GetDescription()}";
        }

        public override bool Equals(object obj)
        {
            return obj is DegreeOfFreedom freedom && Equals(freedom);
        }

        public bool Equals(DegreeOfFreedom other)
        {
            return DegreeOfFreedomType == other.DegreeOfFreedomType && DegreeOfFreedomLocalDirection == other.DegreeOfFreedomLocalDirection;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 17;
                hashCode = hashCode * -23 + DegreeOfFreedomType.GetHashCode();
                hashCode = hashCode * -23 + DegreeOfFreedomLocalDirection.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(DegreeOfFreedom left, DegreeOfFreedom right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DegreeOfFreedom left, DegreeOfFreedom right)
        {
            return !(left == right);
        }


    }


    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public struct GlobalDegreeOfFreedom : IEquatable<GlobalDegreeOfFreedom>
    {

        public enum DegreeOfFreedomGlobalDirections
        {
            [Description("")]
            None,
            [Description("X")]
            X,
            [Description("Y")]
            Y,
            [Description("Z")]
            Z
        }

        public DegreeOfFreedom.DegreeOfFreedomTypes DegreeOfFreedomType { get; }
        public DegreeOfFreedomGlobalDirections DegreeOfFreedomGlobalDirection { get; }


        public GlobalDegreeOfFreedom(DegreeOfFreedom degreeOfFreedom)
        {
            DegreeOfFreedomType = degreeOfFreedom.DegreeOfFreedomType;

            switch (degreeOfFreedom.DegreeOfFreedomLocalDirection)
            {
                case DegreeOfFreedom.DegreeOfFreedomLocalDirections.None:
                    DegreeOfFreedomGlobalDirection = DegreeOfFreedomGlobalDirections.None;
                    break;

                case DegreeOfFreedom.DegreeOfFreedomLocalDirections.Axis1:
                    DegreeOfFreedomGlobalDirection = DegreeOfFreedomGlobalDirections.Z;
                    break;

                case DegreeOfFreedom.DegreeOfFreedomLocalDirections.Axis2:
                    DegreeOfFreedomGlobalDirection = DegreeOfFreedomGlobalDirections.Y;
                    break;

                case DegreeOfFreedom.DegreeOfFreedomLocalDirections.Axis3:
                    DegreeOfFreedomGlobalDirection = DegreeOfFreedomGlobalDirections.X;
                    break;

                default:
                    throw new NotImplementedException();
            }

        }

        public GlobalDegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes degreeOfFreedomType, DegreeOfFreedomGlobalDirections degreeOfFreedomGlobalDirection)
        {
            DegreeOfFreedomType = degreeOfFreedomType;
            DegreeOfFreedomGlobalDirection = degreeOfFreedomGlobalDirection;
        }

        private string GetDebuggerDisplay()
        {
            return ToString();
        }

        public override bool Equals(object obj)
        {
            return obj is GlobalDegreeOfFreedom freedom && Equals(freedom);
        }

        public bool Equals(GlobalDegreeOfFreedom other)
        {
            return DegreeOfFreedomType == other.DegreeOfFreedomType && DegreeOfFreedomGlobalDirection == other.DegreeOfFreedomGlobalDirection;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 17;
                hashCode = hashCode * -23 + DegreeOfFreedomType.GetHashCode();
                hashCode = hashCode * -23 + DegreeOfFreedomGlobalDirection.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"{DegreeOfFreedomType.GetDescription()}{DegreeOfFreedomGlobalDirection.GetDescription()}";
        }

        public static bool operator ==(GlobalDegreeOfFreedom left, GlobalDegreeOfFreedom right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GlobalDegreeOfFreedom left, GlobalDegreeOfFreedom right)
        {
            return !(left == right);
        }


    }

}
