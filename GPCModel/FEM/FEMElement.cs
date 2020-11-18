using GPC.Model.Elements;
using GPC.Model.FEM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using GPC.Model.CoordinateSystems;

namespace GPC.Model.FEM
{
    public abstract class FEMElement : Element
    {
        #region Variables

        protected FEMIntegrator _integrator;
        protected Node[] _nodesGlobal;
        protected Node[] _nodesLocal;
        protected int[,] _elIncidence;
        protected int[] _elIncidenceLocal;
        protected int[] _elIncidenceGlobal;
        protected GPC.Model.CoordinateSystems.CoordinateSystem _coordSys;
        //protected CoordinateSystem _cSys;
        #endregion

        #region Properties
        public FEMIntegrator Integrator => _integrator;
        public int[,] ElIncidence => _elIncidence;
        public Node[] NodesGlobal => _nodesGlobal;
        public Node[] NodesLocal => _nodesLocal;
        public GPC.Model.CoordinateSystems.CoordinateSystem CoordSys => _coordSys;
        //public CoordinateSystem CSys => _cSys;
        #endregion

        #region Public Constructors
        protected FEMElement(Guid guid, FEMIntegrator integrator)
            : this(guid)
        {
            _integrator = integrator;
        }

        protected FEMElement(Guid guid)
                 : base(guid)
        {
            _integrator = null;
        }

        protected FEMElement(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        #endregion

        #region Public Methods Override
        /*public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }*/
        #endregion

        #region Public Methods Specific
        public virtual void ElementIncidence()
        {
            int totalDoF = 0;
            int totalActiveDoF = 0;

            /// Get Total Active Nodes
            for (int nd = 0; nd < _nodesGlobal.Length; nd++)
            {
                /// Loop on DoF
                for (int i = 0; i < _nodesGlobal[nd].DoF.FEMDoFs.Count; i++)
                {
                    if (_nodesGlobal[nd].DoF.FEMDoFs[i].Active == 0) { totalActiveDoF++; }
                    totalDoF++;
                }
            }
            _integrator.NumTotDoF = totalDoF;
            _integrator.NumTotActiveDoF = totalActiveDoF;

            /// Initialize Incidence Vector
            _elIncidence = new int[2, totalDoF];

            int k = 0;
            // Loop on Nodes
            /// Creazione incidenza locale elementi
            int k0 = 0;
            int k1 = 0;
            int dofPos = 0;
            for (int nd = 0; nd < _nodesGlobal.Length; nd++)
            {
                /// Loop on DoFs
                for (int i = 0; i < _nodesGlobal[nd].DoF.FEMDoFs.Count; i++)
                {
                    //k0++;
                    //k1++;
                    int degree = _nodesGlobal[nd].DoF.GlobalIncidence[i];
                    //if (_nodes[nd].DoF.FEMDoFs[i].Active == 0)
                    if (degree >= 0)
                    {
                        _elIncidence[0, k0++] = k1; // incidenza locale dei gradi liberi
                    }
                    _elIncidence[1, k1++] = degree; // incidenza globale
                }
            }
        }
        public virtual void KInGlobal(ref Matrix<double> Kg)
        {
            /// Built Local Stiffness Matrix
            _integrator.BuildK(this);

            int er = 0;
            int ec = 0;
            int r = 0;
            int c = 0;

            /// Lettura della Matrice Locale
            for (int i = 0; i < _integrator.NumTotActiveDoF; i++)
            {
                for (int j = 0; j < _integrator.NumTotActiveDoF; j++)
                {
                    er = _elIncidence[0, i]; //Locale(elementi riga)
                    ec = _elIncidence[0, j]; //Locale(elementi colonna)
                    r = _elIncidence[1, er]; //Globale(elementi riga)
                    c = _elIncidence[1, ec]; //Globale(elementi colonna)

                    if (_elIncidence[1, ec] >= 0.0 && _elIncidence[1, er] >= 0.0)
                    {
                        double val = _integrator.KMatrix[er, ec];
                        Kg[r, c] += _integrator.KMatrix[er, ec];
                    }
                }
            }
        }
        public virtual void TInGlobal()
        {
        }
        public virtual void MInGlobal()
        {
        }
        public virtual void FInGlobal()
        {
        }
        public virtual void ChooseIntegrator()
        {
        }
        protected abstract void SetLocalCoordinateSystem(double rotationAngle);

        protected abstract void SetElement(Node[] arrayNode);

        #endregion

        #region Private Methods Specific
        #endregion
    }
}

