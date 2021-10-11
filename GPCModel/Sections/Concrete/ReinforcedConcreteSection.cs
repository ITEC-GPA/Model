using GPC.Geometry;
using GPC.Model.Elements;
using GPC.Model.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry.Meshes;

namespace GPC.Model.Sections.Concrete
{
	public class ReinforcedConcreteSection : Section, IConcreteSection
	{
		#region Variables

		protected readonly ShapeEx _shapeEx;
		protected readonly ReinforcedConcreteRebar[] _rebars;
		protected readonly Mesh _mesh;

		#endregion


		#region Properties

		public ShapeEx Shape => _shapeEx;

		public ReinforcedConcreteRebar[] Rebars => _rebars;

		public ConcreteMaterial ConcreteMaterial => (ConcreteMaterial)_material;

		Shape IConcreteSection.Shape => _shapeEx;



		#endregion


		#region Public Constructors

		public ReinforcedConcreteSection(ShapeEx shapeEx, ReinforcedConcreteRebar[] rebars, string name = "")
			: base(name)
		{
			_shapeEx = shapeEx ?? throw new ArgumentNullException(nameof(shapeEx));
			_rebars = rebars ?? throw new ArgumentNullException(nameof(rebars));
			_material = shapeEx.Material;
			
			#region Input check

			Plane pln = _shapeEx.GetPlane();

			foreach(ReinforcedConcreteRebar rebar in _rebars)
			{
				if (!pln.IsPointOnPlane(rebar.Position))
					throw new ArgumentException("Rebars must be on the plane of the section");
				if(!_shapeEx.IsPointInside(rebar.Position))
					throw new ArgumentException("Rebars must be inside the section");
			}

			#endregion

			_mesh = GenerateMesh();
			SetMechanicalProperties();
		}
		
		public ReinforcedConcreteSection(SerializationInfo info, StreamingContext context) :
			base(info, context)
		{
			_shapeEx = (ShapeEx)info.GetValue("ShapeEx", typeof(ShapeEx));
			_rebars = (ReinforcedConcreteRebar[])info.GetValue("ReinforcedConcreteRebar", typeof(ReinforcedConcreteRebar[]));
		}

		#endregion


		#region Field Serialization

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("ShapeEx", _shapeEx);
			info.AddValue("ReinforcedConcreteRebar", _rebars);
		}

		#endregion


		/// <summary>
		/// Internal method to set the mechanical properties to the section
		/// </summary>
		public virtual void SetMechanicalProperties()
		{
			_area = CalculateArea();
			
			CalculateStaticMoments(_mesh, out double Sx, out double Sy);
			_centroid = CalculateCentroid(Sx, Sy, Area);
			
			CalculateInertiaMoments(_mesh, _centroid, out double Jxx, out double Jyy, out double Jxy, out double _);
			_jxx = Jxx;
			_jyy = Jyy;
			_jxy = Jxy;			
			_angleX1 = CalculateAngle(Jxx, Jyy, Jxy);
			_j11 = CalculateJ11(Jxx, Jyy, Jxy);
			_j22 = CalculateJ22(Jxx, Jyy, Jxy);

			//TODO: implementare metodi di calcolo della sezione
			_jw = CalculateJw();
			_jt = CalculateJt();
			_shearCenter = CalculateShearCenter();
			_wel1 = CalculateWel1();
			_wel2 = CalculateWel2();
			_wpl1 = CalculateWpl1();
			_wpl2 = CalculateWpl2();
		}


		#region Public Methods

		/// <summary>
		/// Return all homogenized mechanical properties with default value of homogenized factor n
		/// </summary>
		/// <param name="areaH">The homogeneized area</param>
		/// <param name="SxH">The first moment of area calculated respect input X-axis of the homogeneized section</param>
		/// <param name="SyH">The first moment of area calculated respect input Y-axis of the homogeneized section</param>
		/// <param name="centroidH">The centroid of homogeneized section</param>
		/// <param name="JxxH">The first moment of area calculated respect X-axis passing throw the centroid of the homogeneized section</param>
		/// <param name="JyyH">The first moment of area calculated respect Y-axis passing throw the centroid of the homogeneized section</param>
		/// <param name="JxyH"></param>
		/// <param name="JpH"></param>
		/// <param name="J11H">The first moment of area calculated respect the first principal axis 
		/// passing throw the centroid of only concrete section of the homogeneized section</param>
		/// <param name="J22H">The first moment of area calculated respect the second principal axis 
		/// passing throw the centroid of only concrete section of the homogeneized section</param>
		/// <param name="angleX">The angle of rotation of the principal axis respect the X-Axis</param>
		public void GetHomogeneizedMechanicalProperties(out double areaH, out double SxH, out double SyH, out Point3d centroidH,
			out double JxxH, out double JyyH, out double JxyH, out double JpH, out double J11H, out double J22H, out double angleX)
		{
			areaH = GetHomogenizedArea();
			centroidH = GetHomogenizedCentroid(out SxH, out SyH);
			CalculateHomogeneizedInertiaMoments(centroidH, Jxx, Jyy, Jxy,
			out JxxH, out JyyH, out JxyH, out JpH);
			J11H = CalculateJ11(JxxH, JyyH, JxyH);
			J22H = CalculateJ22(JxxH, JyyH, JxyH);
			angleX = CalculateAngle(JxxH, JyyH, JxyH);
		}

		/// <summary>
		/// Return all homogenized mechanical properties with homogeneized factor <paramref name="n"/>
		/// </summary>
		/// <param name="n">The homogeneized factor</param>
		/// <param name="areaH">The homogeneized area</param>
		/// <param name="SxH">The first moment of area calculated respect input X-axis of the homogeneized section</param>
		/// <param name="SyH">The first moment of area calculated respect input Y-axis of the homogeneized section</param>
		/// <param name="centroidH">The centroid of homogeneized section</param>
		/// <param name="JxxH">The first moment of area calculated respect X-axis passing throw the centroid of the homogeneized section</param>
		/// <param name="JyyH">The first moment of area calculated respect Y-axis passing throw the centroid of the homogeneized section</param>
		/// <param name="JxyH"></param>
		/// <param name="JpH"></param>
		/// <param name="J11H">The first moment of area calculated respect the first principal axis 
		/// passing throw the centroid of only concrete section of the homogeneized section</param>
		/// <param name="J22H">The first moment of area calculated respect the second principal axis 
		/// passing throw the centroid of only concrete section of the homogeneized section</param>
		/// <param name="angleX">The angle of rotation of the principal axis respect the X-Axis</param>
		public void GetHomogeneizedMechanicalProperties(double n, out double areaH, out double SxH, out double SyH, out Point3d centroidH,
			out double JxxH, out double JyyH, out double JxyH, out double JpH, out double J11H, out double J22H, out double angleX)
		{
			areaH = GetHomogenizedArea(n);
			centroidH = GetHomogenizedCentroid(n, out SxH, out SyH);
			CalculateHomogeneizedInertiaMoments(n, centroidH, Jxx, Jyy, Jxy,
			out JxxH, out JyyH, out JxyH, out JpH);
			J11H = CalculateJ11(JxxH, JyyH, JxyH);
			J22H = CalculateJ22(JxxH, JyyH, JxyH);
			angleX = CalculateAngle(JxxH, JyyH, JxyH);
		}

		/// <summary>
		/// The centroid of the homogenized section with default value of homogenized factor n
		/// </summary>
		/// <param name="SxHomog">The first moment of area respect X-Axis</param>
		/// <param name="SyHomog">The first moment of area respect Y-Axis</param>
		/// <returns>The centroid</returns>
		public Point3d GetHomogenizedCentroid(out double SxHomog, out double SyHomog)
		{
			CalculateStaticMoments(_mesh, out double Sx, out double Sy);

			double[] AreaHomogArray = new double[Rebars.Count()];
			double[] SxHomogenizedArray = new double[Rebars.Count()];
			double[] SyHomogenizedArray = new double[Rebars.Count()];

			Parallel.For(0, Rebars.Count(), (i) =>
			{
				SxHomogenizedArray[i] += (CalculateN(Rebars[i]) - 1) * Rebars[i].Area * Rebars[i].Position.Y;
				SyHomogenizedArray[i] += (CalculateN(Rebars[i]) - 1) * Rebars[i].Area * Rebars[i].Position.X;
			});

			SxHomog = Sx + SxHomogenizedArray.Sum();
			SyHomog = Sy + SyHomogenizedArray.Sum();

			return CalculateCentroid(SxHomog, SyHomog, GetHomogenizedArea());
		}

		/// <summary>
		/// The centroid of the homogenized section with homogenized factor <paramref name="n"/>
		/// </summary>
		/// <param name="n">The homogenized factor</param>
		/// <param name="SxHomog">The first moment of area respect X-Axis</param>
		/// <param name="SyHomog">The first moment of area respect Y-Axis</param>
		/// <returns></returns>
		public Point3d GetHomogenizedCentroid(double n, out double SxHomog, out double SyHomog)
		{
			CalculateStaticMoments(_mesh, out double Sx, out double Sy);

			double[] SxHomogenizedArray = new double[Rebars.Count()];
			double[] SyHomogenizedArray = new double[Rebars.Count()];

			Parallel.For(0, Rebars.Count(), (i) =>
			{
				SxHomogenizedArray[i] += (n - 1) * Rebars[i].Area * Rebars[i].Position.Y;
				SyHomogenizedArray[i] += (n - 1) * Rebars[i].Area * Rebars[i].Position.X;
			});

			SxHomog = Sx + SxHomogenizedArray.Sum();
			SyHomog = Sy + SyHomogenizedArray.Sum();

			return CalculateCentroid(SxHomog, SyHomog, GetHomogenizedArea(n));
		}

		/// <summary>
		/// The homogenized area with default value of homogenized factor n
		/// </summary>
		/// <returns>The homogenized area</returns>
		public double GetHomogenizedArea()
		{
			double[] AreaHomogArray = new double[Rebars.Count()];

			Parallel.For(0, Rebars.Count(), (i) =>
			{
				AreaHomogArray[i] += (CalculateN(Rebars[i]) - 1) * Rebars[i].Area;
			});

			return Area + AreaHomogArray.Sum();
		}

		/// <summary>
		/// The homogenized area with homogenized factor <paramref name="n"/>
		/// </summary>
		/// <param name="n"></param>
		/// <returns>The homogenized area</returns>
		public double GetHomogenizedArea(double n)
		{
			double[] AreaHomogArray = new double[Rebars.Count()];

			Parallel.For(0, Rebars.Count(), (i) =>
			{
				AreaHomogArray[i] += (n - 1) * Rebars[i].Area;
			});

			return Area + AreaHomogArray.Sum();
		}

		public double GetHomogeneizedJ11(double n)
		{
			Point2d centroidH = GetHomogenizedCentroid(n, out _, out _);
			CalculateHomogeneizedInertiaMoments(n, centroidH, Jxx, Jyy, Jxy, out double JxxH, out double JyyH, out double JxyH, out double _);
			return CalculateJ11(JxxH, JyyH, JxyH);
		}

		public double GetHomogeneizedJ11()
		{
			Point2d centroidH = GetHomogenizedCentroid(out _, out _);
			CalculateHomogeneizedInertiaMoments(centroidH, Jxx, Jyy, Jxy, out double JxxH, out double JyyH, out double JxyH, out double _);
			return CalculateJ11(JxxH, JyyH, JxyH);
		}

		public double GetHomogeneizedJ22(double n)
		{
			Point2d centroidH = GetHomogenizedCentroid(n, out _, out _);
			CalculateHomogeneizedInertiaMoments(n, centroidH, Jxx, Jyy, Jxy, out double JxxH, out double JyyH, out double JxyH, out double _);
			return CalculateJ22(JxxH, JyyH, JxyH);
		}

		public double GetHomogeneizedJ22()
		{
			Point2d centroidH = GetHomogenizedCentroid(out _, out _);
			CalculateHomogeneizedInertiaMoments(centroidH, Jxx, Jyy, Jxy, out double JxxH, out double JyyH, out double JxyH, out double _);
			return CalculateJ22(JxxH, JyyH, JxyH);
		}

		#endregion

		#region Protected Methods

		protected double CalculateArea()
		{
			return Shape.GetArea();
		}

		protected void CalculateStaticMoments(Mesh mesh, out double Sx, out double Sy)
		{
			double[] SxArray = new double[mesh.FacesCount];
			double[] SyArray = new double[mesh.FacesCount];

			Parallel.For(0, mesh.FacesCount, (i) =>
			{
				double area = mesh.GetFaceArea(mesh.Faces[i + 1]);
				Point3d centroid = mesh.GetFaceCentroid(mesh.Faces[i + 1]);

				SxArray[i] = area * centroid.Y;
				SyArray[i] += area * centroid.X;
			});

			Sx = SxArray.Sum();
			Sy = SyArray.Sum();
		}

		protected void CalculateInertiaMoments(Mesh mesh, Point3d centroid, out double Jxx, out double Jyy, out double Jxy, out double Jp)
		{
			double[] JxxArray = new double[mesh.FacesCount];
			double[] JyyArray = new double[mesh.FacesCount];
			double[] JxyArray = new double[mesh.FacesCount];

			Parallel.For(0, mesh.FacesCount, (i) =>
			{
				double area = mesh.GetFaceArea(mesh.Faces[i + 1]);
				Point3d faceCentroid = mesh.GetFaceCentroid(mesh.Faces[i + 1]);

				JxxArray[i] = area * Math.Pow(faceCentroid.Y - centroid.Y, 2);
				JyyArray[i] = area * Math.Pow(faceCentroid.X - centroid.X, 2);
				JxyArray[i] = area * (faceCentroid.Y - centroid.Y) * (faceCentroid.X - centroid.X);
			});

			Jxx = JxxArray.Sum();
			Jyy = JyyArray.Sum();
			Jxy = JxyArray.Sum();
			Jp = Jxx + Jyy;
		}

		protected void CalculateHomogeneizedInertiaMoments(Point3d centroid, double Jxx, double Jyy, double Jxy,
			out double JxxHomogenized, out double JyyHomogenized, out double JxyHomogenized, out double JpHomogenized)
		{
			double[] JxxRebarArray = new double[Rebars.Count()];
			double[] JyyRebarArray = new double[Rebars.Count()];
			double[] JxyRebarArray = new double[Rebars.Count()];

			Parallel.For(0, Rebars.Count(), (i) =>
			{
				JxxRebarArray[i] = (CalculateN(Rebars[i]) - 1) * (Rebars[i].RebarSection.Jxx + Rebars[i].Area *
					(Math.Pow((Rebars[i].Position.Y - centroid.Y), 2)));
				JyyRebarArray[i] = (CalculateN(Rebars[i]) - 1) * (Rebars[i].RebarSection.Jyy + Rebars[i].Area *
					(Math.Pow((Rebars[i].Position.X - centroid.X), 2)));
				JxyRebarArray[i] = (CalculateN(Rebars[i]) - 1) * (Rebars[i].RebarSection.Jxy + Rebars[i].Area *
					(Rebars[i].Position.X - centroid.X) * (Rebars[i].Position.Y - centroid.Y));
			});

			JxxHomogenized = Jxx + JxxRebarArray.Sum();
			JyyHomogenized = Jyy + JyyRebarArray.Sum();
			JxyHomogenized = Jxy + JxyRebarArray.Sum();
			JpHomogenized = JxxHomogenized + JyyHomogenized;

			JxxHomogenized += Math.Pow(Centroid.Y - centroid.Y, 2) * Area;
			JyyHomogenized += Math.Pow(Centroid.X - centroid.X, 2) * Area;
			JxyHomogenized += (Centroid.X - centroid.X) * (Centroid.Y - centroid.Y) * Area;
		}

		protected void CalculateHomogeneizedInertiaMoments(double n, Point3d centroid, double Jxx, double Jyy, double Jxy,
			out double JxxHomogenized, out double JyyHomogenized, out double JxyHomogenized, out double JpHomogenized)
		{
			double[] JxxRebarArray = new double[Rebars.Count()];
			double[] JyyRebarArray = new double[Rebars.Count()];
			double[] JxyRebarArray = new double[Rebars.Count()];

			Parallel.For(0, Rebars.Count(), (i) =>
			{
				JxxRebarArray[i] = (n - 1) * (Rebars[i].RebarSection.Jxx + Rebars[i].Area *
					(Math.Pow((Rebars[i].Position.Y - centroid.Y), 2)));
				JyyRebarArray[i] = (n - 1) * (Rebars[i].RebarSection.Jyy + Rebars[i].Area *
					(Math.Pow((Rebars[i].Position.X - centroid.X), 2)));
				JxyRebarArray[i] = (n - 1) * (Rebars[i].RebarSection.Jxy + Rebars[i].Area *
					(Rebars[i].Position.X - centroid.X) * (Rebars[i].Position.Y - centroid.Y));
			});

			JxxHomogenized = Jxx + JxxRebarArray.Sum();
			JyyHomogenized = Jyy + JyyRebarArray.Sum();
			JxyHomogenized = Jxy + JxyRebarArray.Sum();
			JpHomogenized = JxxHomogenized + JyyHomogenized;

			JxxHomogenized += Math.Pow(Centroid.Y - centroid.Y, 2) * Area;
			JyyHomogenized += Math.Pow(Centroid.X - centroid.X, 2) * Area;
			JxyHomogenized += (Centroid.X - centroid.X) * (Centroid.Y - centroid.Y) * Area;

			// NOTA: ci siamo ricondotti a momenti d'inerzia rispetto al baricentro della sezione di solo calcestruzzo
		}

		protected Point2d CalculateCentroid(double Sx, double Sy, double area)
		{
			return new Point2d(Sy / area, Sx / area);
		}

		protected double CalculateAngle(double Jxx, double Jyy, double Jxy)
		{
			double angle = -1.0 / 2.0 * Math.Atan2(2.0 * Jxy , (Jyy - Jxx));

			if (Jyy < Jxx)
				angle += Math.PI / 2.0;

			if (Math.Abs(angle - Math.PI) < GeometryBase.GetDefaultAngularTolerance())
				return 0.0;

			if (Math.Abs(angle) < GeometryBase.GetDefaultAngularTolerance())
				return 0.0;

			return angle;
		}

		protected double CalculateJ11(double Jxx, double Jyy, double Jxy)
		{
			return (Jxx + Jyy) / 2.0 + 0.5 * Math.Sqrt(Math.Pow(Jxx - Jyy, 2.0) + 4.0 * Math.Pow(Jxy, 2));
		}

		protected double CalculateJ22(double Jxx, double Jyy, double Jxy)
		{
			return (Jxx + Jyy) / 2.0 - 0.5 * Math.Sqrt(Math.Pow(Jxx - Jyy, 2.0) + 4.0 * Math.Pow(Jxy, 2));
		}

		/// <summary>
		/// Generate the mesh of the section. If <paramref name="size"/> not set, size is set as the default value of the minimum of the bounding box size divided by 10.
		/// </summary>
		/// <param name="size">The mesh size</param>
		/// <returns></returns>
		protected Mesh GenerateMesh(double size = -1)
		{
			if (size == -1)
			{
				BoundingBox3d bBox = Shape.GetBoundingBox();
				size = Math.Min(bBox.Size.X, bBox.Size.Y) / 20.0;
			}

			Mesh.GenerateOptions generateOptions = new Mesh.GenerateOptions()
			{
				Algorithm = Mesh.GenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
				Recombine = true,
				RecombinationAlgorithm = Mesh.GenerateOptions.RecombinationMeshAlgorithm.SimpleFullQuad,
				UseGlobalProgressID = true,

				MeshSize = size,
			};

			Mesh.Generate(new Shape[] { Shape }, generateOptions, out List<Mesh> meshes, out Mesh.GenerateMeshStatus _);

			return meshes[0];
		}

		protected virtual double CalculateN(ReinforcedConcreteRebar rebar)
		{
			return rebar.RebarMaterial.E / Material.E;
			//return 15.0;
		}

		protected double CalculateWpl2()
		{
			return 0;
			throw new NotImplementedException();
		}

		protected double CalculateWpl1()
		{
			return 0;
			throw new NotImplementedException();
		}

		protected double CalculateWel2()
		{
			return 0;
			throw new NotImplementedException();
		}

		protected double CalculateWel1()
		{
			return 0;
			throw new NotImplementedException();
		}

		protected double CalculateJt()
		{
			return 0;
			throw new NotImplementedException();
		}

		protected Point2d CalculateShearCenter()
		{
			return new Point2d(0,0);
			throw new NotImplementedException();
		}

		protected double CalculateJw()
		{
			return 0;
			throw new NotImplementedException();
		}

		#endregion

	}
}
