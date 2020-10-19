using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.FEM
{
    public class FEMNodeDoF : FEMObject
    {
        #region Variables
        protected int[] _globalIncidence;
        protected int _doF;
        protected int _numDoF;
        protected int _maxNumDoF;
        protected List<FEMDoF> _femDoFs;
        protected const int _maxDoFNum = 6;
        #endregion

        #region Properties
        public int MaxNumDoF => _maxNumDoF;
        public List<FEMDoF> FEMDoFs => _femDoFs;
        public int MaxDoFNum => _maxDoFNum;
        public int[] GlobalIncidence => _globalIncidence;
        #endregion

        #region Public Constructors
        public FEMNodeDoF(int[] activeDoF, int[] freeDoF)
        {
            _maxNumDoF = activeDoF.Length;
            _femDoFs = new List<FEMDoF>(_maxNumDoF);
            RegisterDoF(activeDoF, freeDoF);
        }
        #endregion

        #region Public Methods Specific
        public virtual void RegisterDoF(int[] idDoF, int[] activeDoF)
        {
            for(int i = 0; i < idDoF.Length; i++)
            {
                FEMDoF dof = new FEMDoF(idDoF[i], activeDoF[i]);
                _femDoFs.Add(dof);
            }
        }

        public void FormIncidence(ref int globalNum, ref int reactionNum)
        {
            _globalIncidence = new Int32[_maxNumDoF];
            //Numera i gdl partendo da globalNum(inizialmente =1 e poi aggiornato) 
            // e ne inserisce il n°nell'array m_GlobalIncidence.

            for (int i = 0; i < _maxNumDoF; i++)
            {
                // Convenzione restituita da GetDoF: 0 (libero) -1 bloccato
                // Se il g.d.l. è bloccato viene marcato con -1

                _globalIncidence[i] = _femDoFs[i].Active == 1 ? --reactionNum : globalNum++;                
            }
        }
        public void UpdateReactionNode(ref int globalNum)
        {
            int tmpglobalNum = globalNum;
            for (int i = 0; i < _maxNumDoF; i++)
            {
                if (_globalIncidence[i] < 0)
                    _globalIncidence[i] -= tmpglobalNum;
            }
        }
        #endregion
    }
}

