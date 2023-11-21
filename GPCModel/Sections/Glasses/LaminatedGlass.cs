using GPC.Utilities.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Sections.Glass
{
	/// <summary>
	/// Laminated glass. This represent a multilayer glass panel. Between each layer there is an interlayer
	/// </summary>
	[Serializable]
	[UI(Description = "Laminated", Group = "Glasses", Kind = "Glass")]
	public sealed class LaminatedGlass : Glass, IGlassPanel, IEquatable<LaminatedGlass>
	{
		#region Variables

		private MonolithicGlass[] _monolithicGlasses;

		private Interlayer[] _interlayers;

		#endregion Variables

		#region Properties

		public MonolithicGlass[] MonolithicGlasses { get => _monolithicGlasses; set => _monolithicGlasses = value; }

		public Interlayer[] Interlayers { get => _interlayers; set => _interlayers = value; }

		public double TotalThickness => _monolithicGlasses.Sum(glass => glass.Thickness) + _interlayers.Sum(interlayer => interlayer.Thickness);

		public int GlassLayerCount => _monolithicGlasses.Count();

		public int InterlayerCount => _interlayers.Count();

		#endregion Properties

		#region Public Constructors

		/// <summary>
		/// Initialize the Laminated glass with empty arrays of monolithics and interlayers.
		/// Used in UI to create an empty laminated that the user will interactively define.
		/// </summary>
		/// <param name="name"></param>
		/// <remarks>Order of the glass panels is from external to internal</remarks>
		public LaminatedGlass(string name)
			: base(name)
		{
			_monolithicGlasses = new MonolithicGlass[0];
			_interlayers = new Interlayer[0];
		}

		/// <param name="name"></param>
		/// <param name="monolithicGlasses">Monolithic glasses composing the laminated panel</param>
		/// <param name="interlayers">Interlayers between monolithic glasses, number of interlayer must be equal to glass number - 1</param>
		/// <remarks>Order of the glass panels is from external to internal</remarks>
		public LaminatedGlass(string name, MonolithicGlass[] monolithicGlasses, Interlayer[] interlayers)
			: base(name)
		{
			if (monolithicGlasses.Length < 2)
				throw new ArgumentException("Number of monolithic glasses should be greater than one");
			if (interlayers == null || interlayers.Length == 0)
				throw new ArgumentException("No interlayer provided");
			if (monolithicGlasses.Length - 1 != interlayers.Length)
				throw new ArgumentException("MonolithicGlasses.Length - 1 != interlayers.Length");

			_monolithicGlasses = monolithicGlasses.Where(i => i == null).Count() > 0 ? throw new ArgumentNullException("Monolithic glasses cannot be null") : monolithicGlasses;
			_interlayers = interlayers.Where(i => i == null).Count() > 0 ? throw new ArgumentNullException("Interlayers cannot be null") : interlayers; ;
		}

		private LaminatedGlass(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			_monolithicGlasses = (MonolithicGlass[])info.GetValue("MonolithicGlasses", typeof(MonolithicGlass[]));
			_interlayers = (Interlayer[])info.GetValue("Interlayers", typeof(Interlayer[]));
		}

		#endregion

		#region Public methods - Getter

		/// <returns>Return an array of <see cref="MonolithicGlass"/> and <see cref="Interlayer"/> rapresenting the glass package composition</returns>
		/// <remarks>The first layer is the first monolithic glass that has been added
		/// <para>Order of the glass panels is from external to internal</para> </remarks>
		public IGlassPackage[] GetGlassPackage()
		{
			IGlassPackage[] package = new IGlassPackage[_monolithicGlasses.Length + _interlayers.Length];

			int index = 0;
			for (int i = 0; i < _monolithicGlasses.Length + _interlayers.Length; i++)
			{
				if (i % 2 == 0)
				{
					package[i] = _monolithicGlasses[index];
				}
				else
				{
					package[i] = _interlayers[index];
					index++;
				}
			}

			return package;
		}

		/// <inheritdoc cref="IGlassPanel.GetElasticModulus()"/>
		public double GetElasticModulus()
		{
			return _monolithicGlasses.Select(i => i.Material.E).Min();
		}

		/// <inheritdoc cref="IGlassPanel.GetPoissonRatios()"/>
		public double GetPoissonRatios()
		{
			return _monolithicGlasses.Select(i => i.Material.Ni).Min();
		}

		/// <inheritdoc cref="IGlassPanel.GetSelfWeightPerUnitArea()"/>
		public double GetSelfWeightPerUnitArea()
		{
			return _monolithicGlasses.Select(i => i.Thickness * i.Material.Density).Sum() + _interlayers.Select(i => i.Thickness * i.Material.Density).Sum();
		}

		/// <inheritdoc cref="IGlassPanel.GetDensity()"/>
		public double GetDensity()
		{
			return GetSelfWeightPerUnitArea() / TotalThickness;
		}

		#endregion

		#region Equals - HashCode - Operators

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("MonolithicGlasses", _monolithicGlasses);
			info.AddValue("Interlayers", _interlayers);
		}

		public bool Equals(LaminatedGlass other)
		{
			if (other is null)
				return false;

			if (ReferenceEquals(this, other))
				return true;

			return other._interlayers.SequenceEqual(_interlayers) &&
				other._monolithicGlasses.SequenceEqual(_monolithicGlasses) &&
				base.Equals(other);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as LaminatedGlass);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = -23;
				hashCode = hashCode * -17 + base.GetHashCode();
				hashCode = hashCode * -17 + EqualityComparer<MonolithicGlass[]>.Default.GetHashCode(_monolithicGlasses);
				hashCode = hashCode * -17 + EqualityComparer<Interlayer[]>.Default.GetHashCode(_interlayers);
				return hashCode;
			}
		}

		public static bool operator ==(LaminatedGlass obj1, LaminatedGlass obj2)
		{
			if (ReferenceEquals(obj1, obj2))
				return true;

			if (obj1 is null || obj2 is null)
				return false;

			return obj1.Equals(obj2);
		}

		public static bool operator !=(LaminatedGlass obj1, LaminatedGlass obj2)
		{
			return !(obj1 == obj2);
		}

		#endregion
	}
}
