using GPC.Model.Fem.Materials;
using GPC.Model.Fem.Properties;
using GPC.Model.Sections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;

namespace GPC.Model.Fem.Properties
{
	[DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
	[Serializable]
	public class BeamProperty : ElementProperty, IBeamProperty, ISerializable
	{
		#region Variables

		protected ISectionShape _shape;

		protected FemMaterial _material;

		#endregion

		#region Properties

		public ISectionShape GeometricShape => _shape;

		public FemMaterial FemMaterial => _material;

		#endregion

		public BeamProperty(string name, int id = IDUNASSIGNED)
			: base(name, id)
		{

		}

		public BeamProperty(ISectionShape shape, FemMaterial femMaterial, string name, int id = IDUNASSIGNED) 
			: base(name, id)
		{
			_shape = shape;
			_material = femMaterial;
		}

		public BeamProperty(SerializationInfo info, StreamingContext context) 
			: base(info, context)
		{
			_material = (FemMaterial)info.GetValue("FemMaterial", typeof(FemMaterial));
			_shape = (ISectionShape)info.GetValue("GeometricShape", typeof(ISectionShape));
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("GeometricShape", _shape);
			info.AddValue("FemMaterial", _material);
		}

		private string GetDebuggerDisplay()
		{
			return $"BeamProperty: {_name}";
		}

		public override bool Equals(object obj)
		{
			return (obj is BeamProperty objCasted)
				&& _shape == objCasted._shape
				&& _material == objCasted._material
				&& base.Equals(objCasted);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 23;
				hashCode = hashCode * -17 + base.GetHashCode();
				hashCode = hashCode * -17 + _shape.GetHashCode();
				hashCode = hashCode * -17 + EqualityComparer<FemMaterial>.Default.GetHashCode(_material);
				return hashCode;
			}
		}
	}
}
