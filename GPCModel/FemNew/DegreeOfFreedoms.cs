using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Fem
{
    public static class DegreeOfFreedoms
    {


        public enum LocalDegreeOfFreedoms
        {
            D1,
            D2,
            D3,
            R1,
            R2,
            R3,
        }

        public enum GlobalDegreeOfFreedoms
        {
            DX,
            DY,
            DZ,
            RX,
            RY,
            RZ,
        }


        public static GlobalDegreeOfFreedoms ToGlobal(this LocalDegreeOfFreedoms local)
        {
            switch (local)
            {
                case LocalDegreeOfFreedoms.D3:
                    return GlobalDegreeOfFreedoms.DX;

                case LocalDegreeOfFreedoms.D2:
                    return GlobalDegreeOfFreedoms.DY;

                case LocalDegreeOfFreedoms.D1:
                    return GlobalDegreeOfFreedoms.DZ;

                case LocalDegreeOfFreedoms.R3:
                    return GlobalDegreeOfFreedoms.RX;

                case LocalDegreeOfFreedoms.R2:
                    return GlobalDegreeOfFreedoms.RY;

                case LocalDegreeOfFreedoms.R1:
                    return GlobalDegreeOfFreedoms.RZ;

                default:
                    throw new NotImplementedException();
            }
        }


        public static LocalDegreeOfFreedoms ToLocal(this GlobalDegreeOfFreedoms local)
        {
            switch (local)
            {

                case GlobalDegreeOfFreedoms.DX:
                    return LocalDegreeOfFreedoms.D3;

                case GlobalDegreeOfFreedoms.DY:
                    return LocalDegreeOfFreedoms.D2;

                case GlobalDegreeOfFreedoms.DZ:
                    return LocalDegreeOfFreedoms.D1;

                case GlobalDegreeOfFreedoms.RX:
                    return LocalDegreeOfFreedoms.R3;

                case GlobalDegreeOfFreedoms.RY:
                    return LocalDegreeOfFreedoms.R2;

                case GlobalDegreeOfFreedoms.RZ:
                    return LocalDegreeOfFreedoms.R1;

                default:
                    throw new NotImplementedException();
            }
        }
    }


}
