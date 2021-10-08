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
		
		protected double _areaHomogenized;
		protected double _jxxHomogenized;
		protected double _jyyHomogenized;
		protected double _jxyHomogenized;
		protected double _jtHomogenized;
		protected double _jwHomogenized;
		protected double _j11Homogenized;
		protected double _j22Homogenized;

		protected double _angleX1Homogenized;
		protected Point2d _centroidHomogenized;

		#endregion


		#region Properties

		public ShapeEx Shape => _shapeEx;

		public ReinforcedConcreteRebar[] Rebars => _rebars;

		public ConcreteMaterial ConcreteMaterial => (ConcreteMaterial)_material;

		/// <summary>
		/// The area of the section homogenized 
		/// </summary>
		public double AreaHomogenized => _areaHomogenized;

		/// <summary>
		/// The first moment of inertia around the X-axis homogenized 
		/// </summary>
		public double JxxHomogenized => _jxxHomogenized;

		/// <summary>
		/// The first moment of inertia around the Y-axis homogenized 
		/// </summary>
		public double JyyHomogenized => _jyyHomogenized;

		/// <summary>
		/// 
		/// </summary>
		public double JxyHomogenized => _jxyHomogenized;

		/// <summary>
		/// 
		/// </summary>
		public double JtHomogenized => _jtHomogenized;

		/// <summary>
		/// 
		/// </summary>
		public double JwHomogenized => _jwHomogenized;

		/// <summary>
		/// The first moment of inertia around the 1st principal axes
		/// </summary>
		public double J11Homogenized => _j11Homogenized;

		/// <summary>
		/// The first moment of inertia around the 2nd principal axes homogenized 
		/// </summary>
		public double J22Homogenized => _j22Homogenized;

		/// <summary>
		/// The centroid of the section homogenized 
		/// </summary>
		public Point2d CentroidHomogenized =>_centroidHomogenized;

		/// <summary>
		/// The angle of rotation of the principal axis of the section homogenized 
		/// </summary>
		public double AngleX1Homogenized => _angleX1Homogenized;

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



		//protected void GetElasticTension(double nAxial, double mxx, double myy, out List<KeyValuePair<ReinforcedConcreteRebar, double>> barTensions,
		//							   out List<KeyValuePair<Point2d, double>> clsTensions, out Point2d dirNeutral,
		//							   out Point2d p0, double nOmogeneizz = 15, bool isTractionConcrete = false)
		//{

		//}

		//public void GetElasticTensionsViviani(double N, double Mx, double My)
		//{
		//	// prima iterazione. primo piano di tentativo = piano orizzontale passante per il centroide
		//	Point2d centroid = Shape.Fill.GetCentroid();
		//	double rotation = 0.0;
		//	CalculateStrainPlaneCoefficients(centroid, rotation, N, Mx, My, out double z0, out double z1, out double z2);
		//	Dictionary<Point2d, double> pointStressAssociation =  CalculateConcreteStress(z0, z1, z2);

		//	bool exit = false;
		//	do
		//	{
		//		double s0 = z0;
		//		double s1 = z1;
		//		double s2 = z2;

		//		CalculateNewNeutralAxis(pointStressAssociation, out Point2d deltaCentroid, out double deltaRotation);
		//		centroid += deltaCentroid;
		//		rotation += deltaRotation;

		//		// nuovo piano di tentativo
		//		CalculateStrainPlaneCoefficients(centroid, rotation, N, Mx, My, out z0, out z1, out z2);
		//		pointStressAssociation = CalculateConcreteStress(z0, z1, z2);

		//		exit = true;
		//		if (Math.Abs(z0 - s0) > GeometryBase.GetDefaultTolerance())
		//			exit = false; 
		//		if (Math.Abs(z1 - s1) > GeometryBase.GetDefaultTolerance())  
		//			exit = false; 
		//		if (Math.Abs(z2 - s2) > GeometryBase.GetDefaultTolerance())  
		//			exit = false; 
		//	} while (!exit);
		//}

		//public void CalculateNewNeutralAxis(Dictionary<Point2d, double> pointStressAssociation, out Point2d deltaCentroid, out double deltaRotation)
		//{
		//	double[] deltaRotationBuffer = new double[pointStressAssociation.Count];
		//	Point3d[] points = Shape.GetPoints();

		//	for (int i = 0; i < points.Count(); i++)
		//	{
		//		double deltaR;

		//		if (i != points.Count() - 1)				
		//			deltaR = points[i].DistanceTo(points[i + 1]) *
		//				pointStressAssociation[points[i]] / (pointStressAssociation[points[i]] - pointStressAssociation[points[i + 1]]);
		//		else
		//			deltaR = points[i].DistanceTo(points[0]) *
		//				pointStressAssociation[points[i]] / (pointStressAssociation[points[i]] - pointStressAssociation[points[0]]);

		//		deltaRotationBuffer[i] = deltaR;
		//	}

		//	deltaCentroid = new Point2d(0,0);
		//	deltaRotation = deltaRotationBuffer.Average();
		//}

		//public Dictionary<Point2d, double> CalculateConcreteStress(double z0, double z1, double z2)
		//{
		//	Point3d[] points = Shape.GetPoints();
		//	Dictionary<Point2d, double> kvp = new Dictionary<Point2d, double>();

		//	for (int i = 0; i < points.Count(); i++)
		//	{
		//		double stress = z0 + z1 * points[i].X + z2 * points[i].Y;
		//		kvp.Add(points[i], stress);
		//	}

		//	return kvp;
		//}

		///// <summary>
		///// Return the parametric coordinates of strain plane 
		///// </summary>
		///// <param name="centroid"></param>
		///// <param name="rotation"></param>
		///// <param name="N">Axial force</param>
		///// <param name="Mx">Bending moment around X axis</param>
		///// <param name="My">Bending moment around Y axis</param>
		///// <param name="z0"></param>
		///// <param name="z1"></param>
		///// <param name="z2"></param>
		///// <remarks>sigmaCls = z0 + x*z1 + y*z2</remarks>
		//public void CalculateStrainPlaneCoefficients(Point2d centroid, double rotation, double N, double Mx, double My, out double z0, out double z1, out double z2)
		//{
		//	CalculateStaticMoments(centroid, rotation, out double areaOm, out double Sx, out double Sy);
		//	CalculateInertiaMoments(centroid, rotation, out double Jxx, out double Jyy, out double Jxy);

		//	double area = Math.Sqrt(areaOm);
		//	double sy = Sy / area;
		//	double sx = Sx / area;

		//	double jyy = Math.Sqrt(Jyy - Math.Pow(sy, 2));
		//	double jxy = (Jxy - sx * sy) / jyy;
		//	double jxx = Math.Sqrt(Jxx - Math.Pow(sx, 2) - Math.Pow(jxy, 2));

		//	z0 = N / area;
		//	z1 = (Mx - z0 * sy) / jyy;
		//	z2 = (My - z0 * sx - z1 * jxy) / jxx;
		//	z2 /= jxx;
		//	z1 = (z1 - z2 * jxy) / jyy;
		//	z0 = (z0 - z1 * sy - z2 * sx) / area;
		//}

		//public void CalculateStaticMoments(Point2d centroid, out double AreaOmog, out double Sx, out double Sy)
		//{
		//	AreaOmog = 0.0;

		//	double aa = 0.0;
		//	double sx = 0.0;
		//	double sy = 0.0;
		//	double ix = 0.0;
		//	double iy = 0.0;
		//	double xy = 0.0;

		//	for (int i = 0; i < Rebars.Count(); i++)
		//	{
		//		AreaOmog += (N - 1) * Rebars[i].Area;
		//		sxRebar += (N - 1) * Rebars[i].Area * (Rebars[i].Position.Y - centroid.Y);
		//		syRebar += (N - 1) * Rebars[i].Area * (Rebars[i].Position.X - centroid.X);
		//	}

		//	for (int i = 0; i < Mesh.VerticesCount; i++)
		//	{
		//		if (i == Mesh.VerticesCount - 1)
		//		{
		//			double ai = (Mesh.Vertices[0].Point.X * Mesh.Vertices[i].Point.Y +
		//				Mesh.Vertices[i].Point.X * Mesh.Vertices[0].Point.Y) / 2.0;
		//			aa += ai;
		//			sx += ai * (Mesh.Vertices[i].Point.Y + Mesh.Vertices[0].Point.Y) / 3.0;
		//			sy += ai * (Mesh.Vertices[i].Point.X + Mesh.Vertices[0].Point.X) / 3.0;
		//			ix += ai * (Math.Pow(Mesh.Vertices[i].Point.Y, 2) + Mesh.Vertices[i].Point.Y * Mesh.Vertices[0].Point.Y +
		//				Math.Pow(Mesh.Vertices[i + 1].Point.Y, 2)) / 6.0;
		//			iy += ai * (Math.Pow(Mesh.Vertices[i].Point.X, 2) + Mesh.Vertices[i].Point.X * Mesh.Vertices[0].Point.X +
		//				Math.Pow(Mesh.Vertices[0].Point.X, 2)) / 6.0;
		//			xy += ai * (Mesh.Vertices[i].Point.X * Mesh.Vertices[i].Point.Y +
		//				Mesh.Vertices[i].Point.X * Mesh.Vertices[i + 1].Point.Y / 2.0 +
		//				Mesh.Vertices[0].Point.X * Mesh.Vertices[i].Point.Y / 2.0 +
		//				Mesh.Vertices[0].Point.X * Mesh.Vertices[0].Point.Y) / 6.0;
		//		}
		//		else
		//		{
		//			double ai = (Mesh.Vertices[i + 1].Point.X * Mesh.Vertices[i].Point.Y +
		//				Mesh.Vertices[i].Point.X * Mesh.Vertices[i + 1].Point.Y) / 2.0;
		//			aa += ai;
		//			sx += ai * (Mesh.Vertices[i].Point.Y + Mesh.Vertices[i + 1].Point.Y) / 3.0;
		//			sy += ai * (Mesh.Vertices[i].Point.X + Mesh.Vertices[i + 1].Point.X) / 3.0;
		//			ix += ai * (Math.Pow(Mesh.Vertices[i].Point.Y, 2) + Mesh.Vertices[i].Point.Y * Mesh.Vertices[i + 1].Point.Y +
		//				Math.Pow(Mesh.Vertices[i + 1].Point.Y, 2)) / 6.0;
		//			iy += ai * (Math.Pow(Mesh.Vertices[i].Point.X, 2) + Mesh.Vertices[i].Point.X * Mesh.Vertices[i + 1].Point.X +
		//				Math.Pow(Mesh.Vertices[i + 1].Point.X, 2)) / 6.0;
		//			xy += ai * (Mesh.Vertices[i].Point.X * Mesh.Vertices[i].Point.Y +
		//				Mesh.Vertices[i].Point.X * Mesh.Vertices[i + 1].Point.Y / 2.0 +
		//				Mesh.Vertices[i + 1].Point.X * Mesh.Vertices[i].Point.Y / 2.0 +
		//				Mesh.Vertices[i + 1].Point.X * Mesh.Vertices[i + 1].Point.Y) / 6.0;
		//		}
		//	}

		//	Sx = sxRebar + sxMesh;
		//	Sy = syRebar + syMesh;
		//}

		//public void CalculateInertiaMoments(Point2d centroid, out double Jxx, out double Jyy, out double Jxy)
		//{
		//	double jxRebar = 0.0;           // momento inerzia rispetto all'asse X Rebar
		//	double jyRebar = 0.0;           // momento inerzia rispetto all'asse Y Rebar
		//	double icRebar = 0.0;           // momento centrifugo  Rebar
		//	double jxMesh = 0.0;           // momento inerzia rispetto all'asse X cls
		//	double jyMesh = 0.0;           // momento inerzia rispetto all'asse Y cls
		//	double icMesh = 0.0;           // momento centrifugo cls

		//	for (int i = 0; i < Rebars.Count(); i++)
		//	{
		//		jxRebar += (N - 1) * (Rebars[i].RebarSection.Jxx + Rebars[i].Area * (Math.Pow((Rebars[i].Position.Y - centroid.Y), 2)));
		//		jyRebar += (N - 1) * (Rebars[i].RebarSection.Jyy + Rebars[i].Area * (Math.Pow((Rebars[i].Position.X - centroid.X), 2)));
		//		icRebar += (N - 1) * (Rebars[i].RebarSection.Jxy + Rebars[i].Area * (Rebars[i].Position.X - centroid.X) * (Rebars[i].Position.Y - centroid.Y));
		//	}

		//	for (int i = 1; i <= Mesh.FacesCount; i++)
		//	{
		//		double faceArea = Mesh.GetFaceArea(Mesh.Faces[i]);
		//		MeshEdge[] edges = Mesh.GetFaceEdges(Mesh.Faces[i]);
		//		double baseLength = Mesh.GetEdgeLength(edges[0]);
		//		double height = 2 * faceArea / baseLength;

		//		Point2d faceCentroid = Mesh.GetFaceCentroid(Mesh.Faces[i]);

		//		jxMesh += faceArea * (Math.Pow((faceCentroid.Y - centroid.Y) * Math.Cos(rotation), 2) +
		//			Math.Pow((faceCentroid.X - centroid.X) * Math.Sin(rotation), 2));
		//		jyMesh += faceArea * (Math.Pow((faceCentroid.X - centroid.X) * Math.Cos(rotation), 2) +
		//			Math.Pow((faceCentroid.Y - centroid.Y) * Math.Sin(rotation), 2));
		//		icMesh += faceArea * (faceCentroid.Y - centroid.Y) * Math.Cos(rotation) * (faceCentroid.X - centroid.X) * Math.Sin(rotation);
		//	}

		//	Jxx = jxRebar + jxMesh;
		//	Jyy = jyRebar + jyMesh;
		//	Jxy = icRebar + icMesh;
		//}

		/// <summary>
		/// Internal method to set the mechanical properties to the section
		/// </summary>
		public virtual void SetMechanicalProperties()
		{
			//_area = CalculateArea();
			//Mesh mesh = GenerateMesh();

			//CalculateStaticMoments(mesh, out double Sx, out double Sy, out double AreaHomogenized, out double SxHomogenized, out double SyHomogenized);
			//_areaHomogenized = AreaHomogenized;
			//_centroid = CalculateCentroid(Sx, Sy, Area);
			//_centroidHomogenized = CalculateCentroid(SxHomogenized, SyHomogenized, AreaHomogenized);

			//CalculateInertiaMoments(mesh, _centroid, out double Jxx, out double Jyy, out double Jxy, out double Jp,
			//	out double JxxHomogenized, out double JyyHomogenized, out double JxyHomogenized, out double JpHomogenized);
			//_jxx = Jxx;
			//_jyy = Jyy;
			//_jxy = Jxy;
			//_jxxHomogenized = JxxHomogenized;
			//_jyyHomogenized = JyyHomogenized;
			//_jxyHomogenized = JxyHomogenized;

			//_angleX1 = CalculateAngle(Jxx, Jyy, Jxy);
			//_j11 = CalculateJ11(Jxx, Jyy, Jxy);
			//_j22 = CalculateJ22(Jxx, Jyy, Jxy);

			//_angleX1Homogenized = CalculateAngle(JxxHomogenized, JyyHomogenized, JxyHomogenized);
			//_j11Homogenized = CalculateJ11(JxxHomogenized, JyyHomogenized, JxyHomogenized);
			//_j22Homogenized = CalculateJ22(JxxHomogenized, JyyHomogenized, JxyHomogenized);

			//_jw = CalculateJw();
			//_jt = CalculateJt();
			//_shearCenter = CalculateShearCenter();
			//_wel1 = CalculateWel1();
			//_wel2 = CalculateWel2();
			//_wpl1 = CalculateWpl1();
			//_wpl2 = CalculateWpl2();

		}

		//protected double CalculateArea()
		//{
		//	return Shape.GetArea();
		//}

		//protected Point2d CalculateCentroid(double Sx, double Sy, double area)
		//{		
		//	return new Point2d(Sy / area, Sx / area);
		//}

		//protected void CalculateStaticMoments(Mesh mesh, out double Sx, out double Sy, out double AreaHomog, out double SxHomogenized, out double SyHomogenized)
		//{
		//	double [] SxArray = new double[mesh.FacesCount];
		//	double[] SyArray = new double[mesh.FacesCount];

		//	Parallel.For(0, mesh.FacesCount, (i) =>
		//	{
		//		double area = mesh.GetFaceArea(mesh.Faces[i + 1]);
		//		Point3d centroid = mesh.GetFaceCentroid(mesh.Faces[i + 1]);

		//		SxArray[i] = area * centroid.Y;
		//		SyArray[i] += area * centroid.X;
		//	});

		//	Sx = SxArray.Sum();
		//	Sy = SyArray.Sum();			

		//	double[] AreaHomogArray = new double[Rebars.Count()];
		//	double[] SxHomogenizedArray = new double[Rebars.Count()];
		//	double[] SyHomogenizedArray = new double[Rebars.Count()];

		//	Parallel.For(0, Rebars.Count(), (i) =>
		//	{
		//		AreaHomogArray[i] += (CalculateN(Rebars[i]) - 1) * Rebars[i].Area;
		//		SxHomogenizedArray[i] += (CalculateN(Rebars[i]) - 1) * Rebars[i].Area * Rebars[i].Position.Y;
		//		SyHomogenizedArray[i] += (CalculateN(Rebars[i]) - 1) * Rebars[i].Area * Rebars[i].Position.X;
		//	});

		//	AreaHomog = Area + AreaHomogArray.Sum();
		//	SxHomogenized = Sx + SxHomogenizedArray.Sum();
		//	SyHomogenized = Sy + SyHomogenizedArray.Sum();
		//}

		//protected void CalculateInertiaMoments(Mesh mesh, Point3d centroid, double teta, out double Jxx, out double Jyy, out double Jxy, out double Jp, 
		//	out double JxxHomogenized, out double JyyHomogenized, out double JxyHomogenized, out double JpHomogenized)
		//{
		//	double[] JxxArray = new double[mesh.FacesCount];
		//	double[] JyyArray = new double[mesh.FacesCount];
		//	double[] JxyArray = new double[mesh.FacesCount];

		//	Parallel.For(0, mesh.FacesCount, (i) =>
		//	{
		//		double area = mesh.GetFaceArea(mesh.Faces[i + 1]);
		//		Point3d faceCentroid = mesh.GetFaceCentroid(mesh.Faces[i + 1]);

		//		JxxArray[i] = area * Math.Pow(faceCentroid.Y - centroid.Y, 2);
		//		JyyArray[i] = area * Math.Pow(faceCentroid.X - centroid.X, 2);
		//		JxyArray[i] = area * (faceCentroid.Y - centroid.Y) * (faceCentroid.X - centroid.X);
		//	});

		//	Jxx = JxxArray.Sum();
		//	Jyy = JyyArray.Sum();
		//	Jxy = JxyArray.Sum();
		//	Jp = Jxx + Jyy;

		//	double[] JxxRebarArray = new double[mesh.FacesCount];
		//	double[] JyyRebarArray = new double[mesh.FacesCount];
		//	double[] JxyRebarArray = new double[mesh.FacesCount];

		//	Parallel.For(0, Rebars.Count(), (i) =>
		//	{
		//		JxxRebarArray[i] = (CalculateN(Rebars[i]) - 1) * (Rebars[i].RebarSection.Jxx + Rebars[i].Area *
		//			(Math.Pow((Rebars[i].Position.Y - centroid.Y), 2)));
		//		JyyRebarArray[i] = (CalculateN(Rebars[i]) - 1) * (Rebars[i].RebarSection.Jyy + Rebars[i].Area *
		//			(Math.Pow((Rebars[i].Position.X - centroid.X), 2)));
		//		JxyRebarArray[i] = (CalculateN(Rebars[i]) - 1) * (Rebars[i].RebarSection.Jxy + Rebars[i].Area *
		//			(Rebars[i].Position.X - centroid.X) * (Rebars[i].Position.Y - centroid.Y));
		//	});

		//	JxxHomogenized = Jxx + JxxRebarArray.Sum();
		//	JyyHomogenized = Jyy + JxxRebarArray.Sum();
		//	JxyHomogenized = Jxy + JxxRebarArray.Sum();
		//	JpHomogenized = JxxHomogenized + JyyHomogenized;
		//}

		//protected double CalculateAngle(double Jxx, double Jyy, double Jxy)
		//{
		//	double angle = -1.0 / 2.0 * Math.Atan2(2.0 * Jxy , (Jyy - Jxx));

		//	if (Jyy < Jxx)
		//		angle += Math.PI / 2.0;

		//	if (Math.Abs(angle - Math.PI) < GeometryBase.GetDefaultAngularTolerance())
		//		return 0.0;

		//	if (Math.Abs(angle) < GeometryBase.GetDefaultAngularTolerance())
		//		return 0.0;

		//	return angle;
		//}

		//protected double CalculateJ11(double Jxx, double Jyy, double Jxy)
		//{
		//	return (Jxx + Jyy) / 2.0 + 0.5 * Math.Sqrt(Math.Pow(Jxx - Jyy, 2.0) + 4.0 * Math.Pow(Jxy, 2));
		//}

		//protected double CalculateJ22(double Jxx, double Jyy, double Jxy)
		//{
		//	return (Jxx + Jyy) / 2.0 - 0.5 * Math.Sqrt(Math.Pow(Jxx - Jyy, 2.0) + 4.0 * Math.Pow(Jxy, 2));
		//}

		///// <summary>
		///// Generate the mesh of the section. If <paramref name="size"/> not set, size is set as the default value of the minimum of the bounding box size divided by 25.
		///// </summary>
		///// <param name="size">The mesh size</param>
		///// <returns></returns>
		//protected Mesh GenerateMesh(double size = -1)
		//{
		//	if (size == -1)
		//	{
		//		BoundingBox3d bBox = Shape.GetBoundingBox();
		//		size = Math.Min(bBox.Size.X, bBox.Size.Y) / 5.0;
		//	}

		//	Mesh.GenerateOptions generateOptions = new Mesh.GenerateOptions()
		//	{
		//		Algorithm = Mesh.GenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
		//		Recombine = true,
		//		RecombinationAlgorithm = Mesh.GenerateOptions.RecombinationMeshAlgorithm.SimpleFullQuad,
		//		UseGlobalProgressID = true,

		//		MeshSize = size,
		//	};

		//	Mesh.Generate(new Shape[] { Shape }, generateOptions, out List<Mesh> meshes, out Mesh.GenerateMeshStatus _);

		//	return meshes[0];
		//}

		//protected virtual double CalculateN(ReinforcedConcreteRebar rebar)
		//{
		//	//return rebar.RebarMaterial.E / Material.E;
		//	return 15.0;
		//}

		////TODO: implementare metodi di calcolo della sezione

		//protected Point2d CalculateRotaTraslatedPoint(Point2d point, Point2d centroid, double teta)
		//{
		//	Point2d newPoint = point;
		//	newPoint.Rotate(new Point2d(0, 0), teta);
		//	newPoint.MoveTo(newPoint.X + centroid.X, newPoint.Y + centroid.Y);
		//	return newPoint;
		//}

		//protected virtual void CalculateIntegralIntertiaConcrete(Point2d p1, Point2d p2, Point2d p3, Point3d centroid, double teta, out double Jxx, out double Jyy, out double Jxy)
		//{
		//	double Jxx = 0.0;
		//	double resultantJyy = 0.0;
		//	double resultantJxy = 0.0;

		//	double[] weight = new double[] { -0.56250, 0.52083333333333, 0.52083333333333, 0.52083333333333 };

		//	double area = 0.5 * ((p3.Y + p1.Y) * (p3.X - p1.X) - (p3.Y + p2.Y) * (p3.X - p2.X) - (p2.Y + p1.Y) * (p2.X - p1.X));

		//	// primo punto semplice
		//	Point2d point1NC = new Point2d(p1.X + (p2.X - p1.X) / 3 + (p3.X - p1.X) / 3, p1.Y + (p2.Y - p1.Y) / 3 + (p3.Y - p1.Y) / 3);
		//	Point2d RTPoint1 = CalculateRotaTraslatedPoint(point1NC, centroid, teta);			
		//	double point1ValueJxx = Math.Pow(RTPoint1.Y, 2);
		//	double point1ValueJyy = Math.Pow(RTPoint1.X, 2);
		//	double point1ValueJxy = RTPoint1.X * RTPoint1.Y;

		//	Jxx += point1ValueJxx * (weight[0]);
		//	resultantJyy += point1ValueJyy * (weight[0]);
		//	resultantJxy += point1ValueJxy * (weight[0]);

		//	// secondo punto semplice
		//	Point2d point2NC = new Point2d(p1.X + (p2.X - p1.X) / 5 + (p3.X - p1.X) / 5, p1.Y + (p2.Y - p1.Y) / 5 + (p3.Y - p1.Y) / 5);
		//	Point2d RTPoint2 = CalculateRotaTraslatedPoint(point2NC, centroid, teta);
		//	double point2ValueJxx = Math.Pow(RTPoint2.Y, 2);
		//	double point2ValueJyy = Math.Pow(RTPoint2.X, 2);
		//	double point2ValueJxy = RTPoint2.X * RTPoint2.Y;

		//	Jxx += point2ValueJxx * (weight[1]);
		//	resultantJyy += point2ValueJyy * (weight[1]);
		//	resultantJxy += point2ValueJxy * (weight[1]);

		//	// terzo punto semplice
		//	Point2d point3NC = new Point2d(p1.X + 3 * (p2.X - p1.X) / 5 + (p3.X - p1.X) / 5, p1.Y + 3 * (p2.Y - p1.Y) / 5 + (p3.Y - p1.Y) / 5);
		//	Point2d RTPoint3 = CalculateRotaTraslatedPoint(point3NC, centroid, teta);
		//	double point3ValueJxx = Math.Pow(RTPoint3.Y, 2);
		//	double point3ValueJyy = Math.Pow(RTPoint3.X, 2);
		//	double point3ValueJxy = RTPoint3.X * RTPoint3.Y;

		//	Jxx += point3ValueJxx * (weight[2]);
		//	resultantJyy += point3ValueJyy * (weight[2]);
		//	resultantJxy += point3ValueJxy * (weight[2]);

		//	// quarto punto semplice
		//	Point2d point4NC = new Point2d(p1.X + (p2.X - p1.X) / 5 + 3 * (p3.X - p1.X) / 5, p1.Y + (p2.Y - p1.Y) / 5 + 3 * (p3.Y - p1.Y) / 5);
		//	Point2d RTPoint4 = CalculateRotaTraslatedPoint(point4NC, centroid, teta);
		//	double point4ValueJxx = Math.Pow(RTPoint4.Y, 2);
		//	double point4ValueJyy = Math.Pow(RTPoint4.X, 2);
		//	double point4ValueJxy = RTPoint4.X * RTPoint4.Y;

		//	Jxx += point4ValueJxx * (weight[2]);
		//	resultantJyy += point4ValueJyy * (weight[2]);
		//	resultantJxy += point4ValueJxy * (weight[2]);
		//}
	}
}
