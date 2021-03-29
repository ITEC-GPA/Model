using GPC.Model.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Model.FEM.Properties;
using MathNet.Numerics.LinearAlgebra;
using GPC.Geometry;

namespace GPC.Model.FEMOld
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
        protected CoordinateSystem _coordSys;
        protected ElementProperty _property;

        protected int _index;
        #endregion

        #region Properties
        public FEMIntegrator Integrator => _integrator;
        public int[,] ElIncidence => _elIncidence;
        public Node[] NodesGlobal => _nodesGlobal;
        public Node[] NodesLocal => _nodesLocal;
        public CoordinateSystem CoordSys => _coordSys;
        public ElementProperty Property => _property;

        public int Index => _index;
        #endregion

        #region Public Constructors

        protected FEMElement(Guid guid, FEMIntegrator integrator, ElementProperty property, int index)
            : base(guid)
        {
            _integrator = integrator;
            _property = property;
            _index = index;
        }

        protected FEMElement(Guid guid, ElementProperty property, int index)
            : this(guid, null, property, index)
        {

        }

        protected FEMElement(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        #endregion

        #region Public Methods Override

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

        }

        #endregion

        #region Public Methods Specific

        public virtual void BuildElementDoF()
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
        }

        public virtual void BuildElementDoFIncidence()
        {
            //int totalDoF = 0;
            //int totalActiveDoF = 0;

            ///// Get Total Active Nodes
            //for (int nd = 0; nd < _nodesGlobal.Length; nd++)
            //{
            //    /// Loop on DoF
            //    for (int i = 0; i < _nodesGlobal[nd].DoF.FEMDoFs.Count; i++)
            //    {
            //        if (_nodesGlobal[nd].DoF.FEMDoFs[i].Active == 0) { totalActiveDoF++; }
            //        totalDoF++;
            //    }
            //}
            //_integrator.NumTotDoF = totalDoF;
            //_integrator.NumTotActiveDoF = totalActiveDoF;


            /// Initialize Incidence Vector
            _elIncidence = new int[2, _integrator.NumTotDoF];

            // Loop on Nodes
            /// Creazione incidenza locale elementi
            int k0 = 0;
            int k1 = 0;
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
            PutInGlobal(_integrator.KMatrix, ref Kg);
        }

        public virtual void TInGlobal()
        {

        }

        public virtual void MInGlobal(ref Matrix<double> Mg)
        {
            PutInGlobal(_integrator.MassMatrix, ref Mg);
        }

        public virtual void FInGlobal()
        {

        }

        public virtual void ChooseIntegrator()
        {

        }

        protected abstract void SetLocalCoordinateSystem(double rotationAngle);

        protected abstract void SetElement(Node[] arrayNode);

        public virtual void PutInGlobal(Matrix<double> localMatrix, ref Matrix<double> Kg)
        {
            int er;
            int ec;
            int r;
            int c;

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
                        Kg[r, c] += localMatrix[er, ec];
                    }
                }
            }
        }
        #endregion

    }
}

