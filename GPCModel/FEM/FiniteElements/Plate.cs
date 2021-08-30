using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using GPC.Model.FEM.Attributes;
using GPC.Model.FEM.Properties;
using GPC.Model.Results;
using mnl = MathNet.Numerics.LinearAlgebra;
using GPC.Geometry;

namespace GPC.Model.FEM.FiniteElements
{
    // TODO: Classe Plate: farla diventare abstract
    /// <summary>
    /// Va messa abstract una volta che è stabile il fem
    /// </summary>
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    [System.ComponentModel.Description("Verrà messa abstract una volta che il fem è stabile")]
    public class Plate : FiniteElement
    {
        // TODO: rendere abstract

        public enum Face
        {
            Top,
            Middle,
            Bottom
        }

        //contains Material information of the element
        protected mnl.Matrix<double> _d;

        /// <summary>
        /// F,M = [D] * (epsilon, curvature...)
        /// </summary>
        public mnl.Matrix<double> D => _d;

        public bool IsTriangle => Nodes.Length == 3 ? true : false;

        public bool IsQuad => Nodes.Length == 4 ? true : false;


        public Plate(Node[] nodes) : base(nodes)
        {

        }

        public override FiniteElement Duplicate(ElementProperty property, List<LoadCaseAttribute> lcAttributes, List<FreedomCaseAttribute> fdAttributes)
        {
            var plate = new Plate(_nodesGlobal);
            plate.SetProperty(property);
            plate.SetId(Id);

            if(lcAttributes != null)
            { 
                foreach(LoadCaseAttribute attribute in lcAttributes)
                { 
                    if (attribute is IPlateLoadCaseAttribute plca)
                    {
                        plate.AddLoadCaseAttribute(plca);
                    }
                } 
            }

            if (fdAttributes != null)
            {
                foreach (FreedomCaseAttribute attribute in fdAttributes)
                {
                    if (attribute is IPlateFreedomCaseAttribute pfca)
                    {
                        plate.AddFreedomCaseAttribute(pfca);
                    }
                }
            }

            return plate;
        }

        public override FiniteElement Duplicate()
        {
            throw new NotImplementedException();
        }


        public virtual void AddLoadCaseAttribute(IPlateLoadCaseAttribute attribute)
        {
            AddLoadCaseAttribute((LoadCaseAttribute)attribute);
        }


        public virtual void AddFreedomCaseAttribute(IPlateFreedomCaseAttribute attribute)
        {
            AddFreedomCaseAttribute((FreedomCaseAttribute)attribute);
        }

        public void AddResult(PlateResult result)
        {
            base.AddResult(result);
        }

        public override void AddResult(FiniteElementResult result)
        {
            if (result is PlateResult)
            {
                base.AddResult(result);
            }
            else
            {
                throw new ArgumentException();
            }
        }

        protected override mnl.Vector<double> BuildFLocalCoord()
        {
            throw new NotImplementedException();
        }

        public override void BuildMatrix()
        {
            throw new NotImplementedException();
        }

        private string GetDebuggerDisplay()
        {
            var prop = Property != null ? Property.Name : String.Empty;
            return $"Plate, Id: {Id}, PropertyName: {prop}";
        }

        //TODO: ottimizzare
        public double GetArea()
        {
            if (IsQuad == true)
            {
                var pts = Quad4Element.GetLocalNodes(_nodesGlobal, out CoordinateSystem sys).Select(x => x.Position).ToList();

                double a1 = Tri3Element.GetArea(new Point3d[] { pts[0], pts[1], pts[2] });
                double a2 = Tri3Element.GetArea(new Point3d[] { pts[0], pts[2], pts[3] });
                return a1 + a2;

            } else if (IsTriangle == true)
            {
                var pts = Tri3Element.GetLocalNodes(_nodesGlobal, out CoordinateSystem sys).Select(x => x.Position).ToList();

                return Tri3Element.GetArea(new Point3d[] { pts[0], pts[1], pts[2] });
            } else
            {
                throw new NotImplementedException("This plate have nr of nodes different than 3 or 4");
            }
        }

        // GetNodalDisplacement()

        // GetGaussPointStress() => List<ResultPLateStress> [ngauspoint * 3facce]

        // GetNodalStress() => List<ResultPLateStress> [ngauspoint * 3facce]

    }
}
