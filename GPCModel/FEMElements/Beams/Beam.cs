using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Materials;
using GPC.Model.Sections;
using MathNet.Numerics.LinearAlgebra;
using GPC.Geometry;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;
using System.IO;

namespace GPC.Model.FEM
{
    public class Beam : FEMElement
    {
        #region Variables
        protected Node _node1;
        protected Node _node2;
        protected Section _section;
        protected Material _material;
        protected FEMIntegrator _Integrator;
        protected GPC.Geometry.CoordinateSystem _CoordSys;
        #endregion

        #region Properties
        public Node Node1 => _node1;
        public Node Node2 => _node2;
        public double Length => Node1.Position.DistanceTo(Node2.Position);
        public Section Section => _section;
        public Material Material => _material;
        public FEMIntegrator Integrator => _Integrator;
        #endregion

        #region Public Constructors
        public Beam(Guid guid, Section section, Material material, FEMIntegrator integrator, Node[] nodes)
            : base(guid, integrator)
        {
            _guid = guid;
            _node1 = nodes[0];
            _node2 = nodes[1];
            _nodes = new Node[nodes.Length];
            _nodes = nodes;
            _section = section;
            _material = material;
            _Integrator = integrator;
            SetLocalCoordinateSystem(0.0);
        }

        protected Beam(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region Public Methods Override
        public override void ElementIncidence()
        {
            int totalDoF = 0;
            int totalActiveDoF = 0;

            /// Get Total Active Nodes
            for (int nd = 0; nd < _nodes.Length; nd++)
            {
                /// Loop on DoF
                for (int i = 0; i < _nodes[nd].DoF.FEMDoFs.Count; i++)
                {
                    if(_nodes[nd].DoF.FEMDoFs[i].Active == 0) { totalActiveDoF++; }
                    totalDoF++;
                }
            }
            _Integrator.NumTotDoF = totalDoF;
            _Integrator.NumTotActiveDoF = totalActiveDoF;

            /// Initialize Incidence Vector
            _elIncidence = new int[2, totalDoF];

            int k = 0;
            // Loop on Nodes
            /// Creazione incidenza locale elementi
            int k0 = 0;
            int k1 = 0;
            int dofPos = 0;
            for (int nd = 0; nd < _nodes.Length; nd++)
            {
                /// Loop on DoFs
                for (int i = 0; i < _nodes[nd].DoF.FEMDoFs.Count; i++)
                {
                    //k0++;
                    //k1++;
                    int degree = _nodes[nd].DoF.GlobalIncidence[i];
                    //if (_nodes[nd].DoF.FEMDoFs[i].Active == 0)
                    if (degree >= 0)
                    {
                        _elIncidence[0, k0++] = k1; // incidenza locale dei gradi liberi
                    }
                    _elIncidence[1, k1++] = degree; // incidenza globale
                }
            }
        }
        public override void KInGlobal(ref Matrix<double> Kg)
        {
            /// Built Local Stiffness Matrix
            _integrator.BuildK(this);

            int er = 0;
            int ec = 0;
            int r = 0;
            int c = 0;

            /// Lettura della Matrice Locale
            for (int i = 0; i < _Integrator.NumTotActiveDoF; i++)
            {
                for (int j = 0; j < _Integrator.NumTotActiveDoF; j++)
                {
                    er = _elIncidence[0,i]; //Locale(elementi riga)
                    ec = _elIncidence[0,j]; //Locale(elementi colonna)
                    r = _elIncidence[1,er]; //Globale(elementi riga)
                    c = _elIncidence[1,ec]; //Globale(elementi colonna)

                    if(_elIncidence[1, ec] >= 0.0 && _elIncidence[1, er] >= 0.0)
                    {
                        double val = _integrator.StiffnessMatrix[er, ec];
                        Kg[r, c] += _integrator.StiffnessMatrix[er, ec];
                    }
                }
            }
        }
        public override void TInGlobal()
        {
        }
        public override void MInGlobal()
        {
        }
        public override void FInGlobal()
        {
        }
        public override void ChooseIntegrator()
        {
        }
        #endregion

        #region Public Methods Specific
        #endregion

        #region Private Methods Specific
        protected void SetLocalCoordinateSystem(double rotationAngle)
        {
            Vector3d ZAxis = new Vector3d(0, 0, 1);
            Vector3d v11 = new Vector3d(1, 0, 0);
            Vector3d v22 = new Vector3d(0, 1, 0);
            Vector3d v33 = new Vector3d((_node2.Position.X - _node1.Position.X), (_node2.Position.Y - _node1.Position.Y), (_node2.Position.Z - _node1.Position.Z));
            double checkVert = ZAxis.CrossProduct(v33).Length;

            /// Create Local Transformation Matrix
            Matrix<double> tfrMatrix1 = Matrix<double>.Build.Dense(3, 3, 0);


            if (checkVert < 1.0E-12)
            {
                v11 = new Vector3d(0, 1, 0);
                v22 = new Vector3d(0, 1, 0);
                v33 = new Vector3d(0, 0, 1);

                v11.Unitize();
                v22.Unitize();
                v33.Unitize();

                double CX = v33.X;
                double CY = v33.Y;
                double CZ = v33.Z;
                double sen = Math.Sin(rotationAngle * 3.14159 / 180);
                double cos = Math.Cos(rotationAngle * 3.14159 / 180);

                tfrMatrix1[0, 0] = 0;
                tfrMatrix1[0, 1] = 0;
                tfrMatrix1[0, 2] = CZ;

                tfrMatrix1[1, 0] = -Math.Pow(CZ,2.0);
                tfrMatrix1[1, 1] = 0;
                tfrMatrix1[1, 2] = 0;

                tfrMatrix1[2, 0] = 0;
                tfrMatrix1[2, 1] = CZ;
                tfrMatrix1[2, 2] = 0;
            }
            else
            {
                v11 = ZAxis.CrossProduct(v33);
                v22 = v33.CrossProduct(v11);
                v11.Unitize();
                v22.Unitize();
                v33.Unitize();

                double CX = v33.X;
                double CY = v33.Y;
                double CZ = v33.Z;
                double d = Math.Sqrt(Math.Pow(CX, 2.0) + Math.Pow(CY, 2.0));
                double sen = Math.Sin(rotationAngle * 3.14159 / 180);
                double cos = Math.Cos(rotationAngle * 3.14159 / 180);

                tfrMatrix1[0, 0] = CX;
                tfrMatrix1[0, 1] = CY;
                tfrMatrix1[0, 2] = CZ;

                tfrMatrix1[1, 0] = -CY / d;
                tfrMatrix1[1, 1] = CX / d;
                tfrMatrix1[1, 2] = 0;

                tfrMatrix1[2, 0] = -CX * CZ / d;
                tfrMatrix1[2, 1] = -CY * CZ / d;
                tfrMatrix1[2, 2] = (Math.Pow(CX, 2.0) + Math.Pow(CY, 2.0)) / d;
            }



            //if (rotationAngle != 0.0)
            //{
            //    double s = Math.Sin(rotationAngle * 3.14159 / 180);
            //    double c = Math.Cos(rotationAngle * 3.14159 / 180);
            //    double t = 1 - c;
            //    Matrix<double> rotMatrix = Matrix<double>.Build.Dense(3, 3, 0);
            //    Vector3d tv = t * v33;
            //    Vector3d sv = s * v33;

            //    rotMatrix[0, 0] = tv.X * v33.X + c;
            //    rotMatrix[0, 1] = tv.X * v33.Y - sv.Z;
            //    rotMatrix[0, 2] = tv.X * v33.Z + sv.Y;

            //    rotMatrix[1, 0] = tv.X * v33.Y + sv.Z;
            //    rotMatrix[1, 1] = tv.Y * v33.Y + c;
            //    rotMatrix[1, 2] = tv.Z * v33.Y - sv.X;

            //    rotMatrix[2, 0] = tv.X * v33.Z - sv.Y;
            //    rotMatrix[2, 1] = tv.Y * v33.Z + sv.X;
            //    rotMatrix[2, 2] = tv.Z * v33.Z + c;

            //    //   m_V11.Cx = PurgeValue(m1(0,0));  m_V11.Cy = PurgeValue(m1(1,0));  m_V11.Cz = PurgeValue(m1(2,0));
            //    //   m_V22.Cx = PurgeValue(m1(0,1));  m_V22.Cy = PurgeValue(m1(1,1));  m_V22.Cz = PurgeValue(m1(2,1));
            //    //   m_V33.Cx = PurgeValue(m1(0,2));  m_V33.Cy = PurgeValue(m1(1,2));  m_V33.Cz = PurgeValue(m1(2,2));
            //    v11 = new Vector3d(
            //        v11.X * rotMatrix[0, 0] + v11.Y * rotMatrix[0, 1] + v11.Z * rotMatrix[0, 2],
            //        v11.X * rotMatrix[1, 1] + v11.Y * rotMatrix[1, 1] + v11.Z * rotMatrix[1, 2],
            //        v11.X * rotMatrix[2, 1] + v11.Y * rotMatrix[2, 1] + v11.Z * rotMatrix[2, 2]);

            //    v22 = new Vector3d(
            //        v22.X * rotMatrix[0, 0] + v22.Y * rotMatrix[0, 1] + v22.Z * rotMatrix[0, 2],
            //        v22.X * rotMatrix[1, 1] + v22.Y * rotMatrix[1, 1] + v22.Z * rotMatrix[1, 2],
            //        v22.X * rotMatrix[2, 1] + v22.Y * rotMatrix[2, 1] + v22.Z * rotMatrix[2, 2]); 
            //    v33 = v11 ^ v22;
            //}

            _CoordSys = new GPC.Geometry.CoordinateSystem(v11, v22, v33);
            //_CoordSys.RotationAngle = rotationAngle;

            /// Set Transformation Matrix for beam Element
            _integrator.TransformationMatrix = Matrix<double>.Build.Dense(12, 12, 0);
            //Matrix<double> BeamTrfMatrix = Matrix<double>.Build.Dense(3, 3, 0);

            //BeamTrfMatrix[0, 0] = v11.X;
            //BeamTrfMatrix[0, 1] = v11.Y;
            //BeamTrfMatrix[0, 2] = v11.Z;
            //BeamTrfMatrix[1, 0] = v22.X;
            //BeamTrfMatrix[1, 1] = v22.Y;
            //BeamTrfMatrix[1, 2] = v22.Z;
            //BeamTrfMatrix[2, 0] = v33.X;
            //BeamTrfMatrix[2, 1] = v33.Y;
            //BeamTrfMatrix[2, 2] = v33.Z;

            for (int i = 0; i < 4; i++)
            {
                for (int r = 0; r < 3; r++)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        _integrator.TransformationMatrix[i * 3 + r, i * 3 + c] = tfrMatrix1[r,c];
                    }
                }                      
            }


            string path = "C:\\Users\\r.vochescu\\Desktop\\" + "TRANF-MATRIX" + this.Guid.ToString() + ".txt";
            // This text is added only once to the file.
            if (File.Exists(path) == true)
            {
                File.Delete(path);
            }
             if (!File.Exists(path))
            {
                // Create a file to write to.
                //string createText = "Hello and Welcome" + Environment.NewLine;
                File.WriteAllText(path, _integrator.TransformationMatrix.ToString());
            }


            Matrix<double> TEST = _integrator.TransformationMatrix;
        }
        #endregion
    }
}
