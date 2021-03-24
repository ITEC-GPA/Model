using GPC.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mnl = MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.FEM
{
    public static class GaussIntegration
    {
        public static mnl.Matrix<double> IntegrationQuadrangular(Func<double, double, mnl.Matrix<double>> GetM, Func<double, double, mnl.Matrix<double>> Jacob, int nrPoints)
        {
            GaussPoint[] gaussPoints = GetPointsRectangular(nrPoints);
            #region
            //primo giro per determinare dimensioni della matrice di risultato
            double csi = gaussPoints[0].Point.X;
            double eta = gaussPoints[0].Point.Y;

            mnl.Matrix<double> ris = gaussPoints[0].Weight * Jacob(csi, eta).Determinant() * GetM(csi, eta);
            #endregion
            for (int i = 1; i < gaussPoints.Length; i++) //trhough the gauss points "variable i START FROM 1 NOT FROM 0!!!"
            {
                csi = gaussPoints[i].Point.X;
                eta = gaussPoints[i].Point.Y;
                ris = ris + gaussPoints[i].Weight * Jacob(csi, eta).Determinant() * GetM(csi, eta);
            }
            return ris;
        }

        public static GaussPoint[] GetPointsLinear(int nPoints)
        {
            GaussPoint[] pts = new GaussPoint[nPoints];
          
            switch (nPoints)
            {
                case 1:
                    pts[0] = new GaussPoint(0, 0, 0, 2.0);
                    break;
                case 2:
                    pts[0] = new GaussPoint(-1.0 / Math.Sqrt(3.0), 0.0, 0.0, 1.0);
                    pts[1] = new GaussPoint(+1.0 / Math.Sqrt(3.0), 0.0, 0.0, 1.0);
                    break;
                case 3:
                    pts[0] = new GaussPoint(-Math.Sqrt(3.0 / 5.0), 0.0, 0.0, 5.0 / 9.0);
                    pts[1] = new GaussPoint(0.0, 0.0, 0.0, 8.0 / 9.0);
                    pts[2] = new GaussPoint(+Math.Sqrt(3.0 / 5.0), 0.0, 0.0, 5.0 / 9.0);
                    break;
                default:
                    throw new Exception("Actually nr of possible gauss points = 1, 2 or 3");
            }
            return pts;
        }
        /// <summary>
        /// Get position and weigth of gauss points for a rectangular domain
        /// </summary>
        /// <param name="points">nr of gauss points for integration</param>
        /// <returns></returns>
        public static GaussPoint[] GetPointsRectangular(int nPoints)
        {
            GaussPoint[] pts = new GaussPoint[nPoints];
               
            switch (nPoints)
            {
                case 1:
                    pts[0] = new GaussPoint(0, 0, 0, 2.0);
                    break;
                case 4:
                    pts[0] = new GaussPoint(-1.0 / Math.Sqrt(3.0), -1.0 / Math.Sqrt(3.0), 0.0, 1.0);
                    pts[1] = new GaussPoint(+1.0 / Math.Sqrt(3.0), -1.0 / Math.Sqrt(3.0), 0.0, 1.0);
                    pts[2] = new GaussPoint(-1.0 / Math.Sqrt(3.0), +1.0 / Math.Sqrt(3.0), 0.0, 1.0);
                    pts[3] = new GaussPoint(+1.0 / Math.Sqrt(3.0), +1.0 / Math.Sqrt(3.0), 0.0, 1.0);
                    break;
                case 9:
                    pts[0] = new GaussPoint(-Math.Sqrt(3.0 / 5.0), -Math.Sqrt(3.0 / 5.0), 0.0, 25.0 / 81.0);
                    pts[1] = new GaussPoint(                  0.0, -Math.Sqrt(3.0 / 5.0), 0.0, 40.0 / 81.0);
                    pts[2] = new GaussPoint(+Math.Sqrt(3.0 / 5.0), -Math.Sqrt(3.0 / 5.0), 0.0, 25.0 / 81.0);
                    pts[3] = new GaussPoint(-Math.Sqrt(3.0 / 5.0),                   0.0, 0.0, 40.0 / 81.0);
                    pts[4] = new GaussPoint(                    0,                   0.0, 0.0, 64.0 / 81.0);
                    pts[5] = new GaussPoint(+Math.Sqrt(3.0 / 5.0),                   0.0, 0.0, 40.0 / 81.0);
                    pts[6] = new GaussPoint(-Math.Sqrt(3.0 / 5.0), +Math.Sqrt(3.0 / 5.0), 0.0, 25.0 / 81.0);
                    pts[7] = new GaussPoint(                  0.0, +Math.Sqrt(3.0 / 5.0), 0.0, 40.0 / 81.0);
                    pts[8] = new GaussPoint(+Math.Sqrt(3.0 / 5.0), +Math.Sqrt(3.0 / 5.0), 0.0, 25.0 / 81.0);
                    break;
                default:
                    throw new Exception("Actually nr of possible gauss points = 1, 4 or 9");
            }
            return pts;
        }

        /// <summary>
        /// Get position and weigth of gauss points for a triangular domain
        /// </summary>
        /// <param name="points">nr of gauss points for integration</param>
        /// <returns></returns>
        public static GaussPoint[] GetPointsTriangular(int nPoints)
        {
            GaussPoint[] pts = new GaussPoint[nPoints];
            
            switch (nPoints)
            {
                case 1:
                    pts[0] = new GaussPoint(1.0 / 3.0, 1.0 / 3.0, 0, 0.5);
                    break;
                case 3:
                    pts[0] = new GaussPoint(0.5, 0.5, 0.0, 1.0 / 6.0);
                    pts[1] = new GaussPoint(0.0, 0.5, 0.0, 1.0 / 6.0);
                    pts[2] = new GaussPoint(0.5, 0.0, 0.0, 1.0 / 6.0);

                    //alternative
                    /*pts[0] = new GaussPoint(1.0 / 6.0, 1.0 / 6.0, 0.0, 1.0 / 6.0));
                    pts[1] = new GaussPoint(2.0 / 3.0, 1.0 / 6.0, 0.0, 1.0 / 6.0));
                    pts[2] = new GaussPoint(1.0 / 6.0, 2.0 / 3.0, 0.0, 1.0 / 6.0));*/
                    break;
                case 4:
                    pts[0] = new GaussPoint(1.0 / 3.0, 1.0 / 3.0, 0.0, -27.0 / 96.0);
                    pts[1] = new GaussPoint(1.0 / 5.0, 1.0 / 5.0, 0.0, 25.0 / 96.0);
                    pts[2] = new GaussPoint(3.0 / 5.0, 1.0 / 5.0, 0.0, 25.0 / 96.0);
                    pts[3] = new GaussPoint(1.0 / 5.0, 1.0 / 3.0, 0.0, 25.0 / 96.0);
                    break;
                default:
                    throw new Exception("Actually nr of possible gauss points = 1, 3 or 4");
            }
                
            return pts;
        }

        public struct GaussPoint
        {
            public Point3d Point;
            public double Weight;
            public GaussPoint(double csi, double eta, double zeta, double weight)
            {
                Point = new Point3d(csi, eta, zeta);
                Weight = weight;
            }
        }
    }
}
