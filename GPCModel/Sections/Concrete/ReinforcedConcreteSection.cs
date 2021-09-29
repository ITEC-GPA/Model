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

		public double N => CalculateN();

		public Mesh Mesh => _mesh;

		#endregion


		#region Public Constructors

		public ReinforcedConcreteSection(ShapeEx shapeEx, ReinforcedConcreteRebar[] rebars, string name = "")
			: base(name)
		{
			_shapeEx = shapeEx;
			_rebars = rebars;
			_material = shapeEx.Material;
			_mesh = GenerateMesh();

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

		#endregion

		public ReinforcedConcreteSection(SerializationInfo info, StreamingContext context) :
			base(info, context)
		{
			_shapeEx = (ShapeEx)info.GetValue("ShapeEx", typeof(ShapeEx));
			_rebars = (ReinforcedConcreteRebar[])info.GetValue("ReinforcedConcreteRebar", typeof(ReinforcedConcreteRebar[]));
		}



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

		public void GetElasticTensionsViviani(double N, double Mx, double My)
		{
			// prima iterazione. primo piano di tentativo = piano orizzontale passante per il centroide
			Point2d centroid = Shape.Fill.GetCentroid();
			double rotation = 0.0;
			CalculateStrainPlaneCoefficients(centroid, rotation, N, Mx, My, out double z0, out double z1, out double z2);
			Dictionary<Point2d, double> pointStressAssociation =  CalculateConcreteStress(z0, z1, z2);

			bool exit = false;
			do
			{
				double s0 = z0;
				double s1 = z1;
				double s2 = z2;

				CalculateNewNeutralAxis(pointStressAssociation, out Point2d deltaCentroid, out double deltaRotation);
				centroid += deltaCentroid;
				rotation += deltaRotation;

				// nuovo piano di tentativo
				CalculateStrainPlaneCoefficients(centroid, rotation, N, Mx, My, out z0, out z1, out z2);
				pointStressAssociation = CalculateConcreteStress(z0, z1, z2);

				exit = true;
				if (Math.Abs(z0 - s0) > GeometryBase.GetDefaultTolerance())
					exit = false; 
				if (Math.Abs(z1 - s1) > GeometryBase.GetDefaultTolerance())  
					exit = false; 
				if (Math.Abs(z2 - s2) > GeometryBase.GetDefaultTolerance())  
					exit = false; 
			} while (!exit);
		}

		public void CalculateNewNeutralAxis(Dictionary<Point2d, double> pointStressAssociation, out Point2d deltaCentroid, out double deltaRotation)
		{
			double[] deltaRotationBuffer = new double[pointStressAssociation.Count];
			Point3d[] points = Shape.GetPoints();

			for (int i = 0; i < points.Count(); i++)
			{
				double deltaR;

				if (i != points.Count() - 1)				
					deltaR = points[i].DistanceTo(points[i + 1]) *
						pointStressAssociation[points[i]] / (pointStressAssociation[points[i]] - pointStressAssociation[points[i + 1]]);
				else
					deltaR = points[i].DistanceTo(points[0]) *
						pointStressAssociation[points[i]] / (pointStressAssociation[points[i]] - pointStressAssociation[points[0]]);

				deltaRotationBuffer[i] = deltaR;
			}

			deltaCentroid = new Point2d(0,0);
			deltaRotation = deltaRotationBuffer.Average();
		}

		public Dictionary<Point2d, double> CalculateConcreteStress(double z0, double z1, double z2)
		{
			Point3d[] points = Shape.GetPoints();
			Dictionary<Point2d, double> kvp = new Dictionary<Point2d, double>();

			for (int i = 0; i < points.Count(); i++)
			{
				double stress = z0 + z1 * points[i].X + z2 * points[i].Y;
				kvp.Add(points[i], stress);
			}

			return kvp;
		}

		/// <summary>
		/// Return the parametric coordinates of strain plane 
		/// </summary>
		/// <param name="centroid"></param>
		/// <param name="rotation"></param>
		/// <param name="N">Axial force</param>
		/// <param name="Mx">Bending moment around X axis</param>
		/// <param name="My">Bending moment around Y axis</param>
		/// <param name="z0"></param>
		/// <param name="z1"></param>
		/// <param name="z2"></param>
		/// <remarks>sigmaCls = z0 + x*z1 + y*z2</remarks>
		public void CalculateStrainPlaneCoefficients(Point2d centroid, double rotation, double N, double Mx, double My, out double z0, out double z1, out double z2)
		{
			CalculateStaticMoments(centroid, rotation, out double areaOm, out double Sx, out double Sy);
			CalculateInertiaMoments(centroid, rotation, out double Jxx, out double Jyy, out double Jxy);

			double area = Math.Sqrt(areaOm);
			double sy = Sy / area;
			double sx = Sx / area;

			double jyy = Math.Sqrt(Jyy - Math.Pow(sy, 2));
			double jxy = (Jxy - sx * sy) / jyy;
			double jxx = Math.Sqrt(Jxx - Math.Pow(sx, 2) - Math.Pow(jxy, 2));

			z0 = N / area;
			z1 = (Mx - z0 * sy) / jyy;
			z2 = (My - z0 * sx - z1 * jxy) / jxx;
			z2 /= jxx;
			z1 = (z1 - z2 * jxy) / jyy;
			z0 = (z0 - z1 * sy - z2 * sx) / area;
		}

		public void CalculateStaticMoments(Point2d centroid, out double AreaOmog, out double Sx, out double Sy)
		{
			AreaOmog = 0.0;

			double aa = 0.0;
			double sx = 0.0;
			double sy = 0.0;
			double ix = 0.0;
			double iy = 0.0;
			double xy = 0.0;

			for (int i = 0; i < Rebars.Count(); i++)
			{
				AreaOmog += (N - 1) * Rebars[i].Area;
				sxRebar += (N - 1) * Rebars[i].Area * (Rebars[i].Position.Y - centroid.Y);
				syRebar += (N - 1) * Rebars[i].Area * (Rebars[i].Position.X - centroid.X);
			}

			for (int i = 0; i < Mesh.VerticesCount; i++)
			{
				if (i == Mesh.VerticesCount - 1)
				{
					double ai = (Mesh.Vertices[0].Point.X * Mesh.Vertices[i].Point.Y +
						Mesh.Vertices[i].Point.X * Mesh.Vertices[0].Point.Y) / 2.0;
					aa += ai;
					sx += ai * (Mesh.Vertices[i].Point.Y + Mesh.Vertices[0].Point.Y) / 3.0;
					sy += ai * (Mesh.Vertices[i].Point.X + Mesh.Vertices[0].Point.X) / 3.0;
					ix += ai * (Math.Pow(Mesh.Vertices[i].Point.Y, 2) + Mesh.Vertices[i].Point.Y * Mesh.Vertices[0].Point.Y +
						Math.Pow(Mesh.Vertices[i + 1].Point.Y, 2)) / 6.0;
					iy += ai * (Math.Pow(Mesh.Vertices[i].Point.X, 2) + Mesh.Vertices[i].Point.X * Mesh.Vertices[0].Point.X +
						Math.Pow(Mesh.Vertices[0].Point.X, 2)) / 6.0;
					xy += ai * (Mesh.Vertices[i].Point.X * Mesh.Vertices[i].Point.Y +
						Mesh.Vertices[i].Point.X * Mesh.Vertices[i + 1].Point.Y / 2.0 +
						Mesh.Vertices[0].Point.X * Mesh.Vertices[i].Point.Y / 2.0 +
						Mesh.Vertices[0].Point.X * Mesh.Vertices[0].Point.Y) / 6.0;
				}
				else
				{
					double ai = (Mesh.Vertices[i + 1].Point.X * Mesh.Vertices[i].Point.Y +
						Mesh.Vertices[i].Point.X * Mesh.Vertices[i + 1].Point.Y) / 2.0;
					aa += ai;
					sx += ai * (Mesh.Vertices[i].Point.Y + Mesh.Vertices[i + 1].Point.Y) / 3.0;
					sy += ai * (Mesh.Vertices[i].Point.X + Mesh.Vertices[i + 1].Point.X) / 3.0;
					ix += ai * (Math.Pow(Mesh.Vertices[i].Point.Y, 2) + Mesh.Vertices[i].Point.Y * Mesh.Vertices[i + 1].Point.Y +
						Math.Pow(Mesh.Vertices[i + 1].Point.Y, 2)) / 6.0;
					iy += ai * (Math.Pow(Mesh.Vertices[i].Point.X, 2) + Mesh.Vertices[i].Point.X * Mesh.Vertices[i + 1].Point.X +
						Math.Pow(Mesh.Vertices[i + 1].Point.X, 2)) / 6.0;
					xy += ai * (Mesh.Vertices[i].Point.X * Mesh.Vertices[i].Point.Y +
						Mesh.Vertices[i].Point.X * Mesh.Vertices[i + 1].Point.Y / 2.0 +
						Mesh.Vertices[i + 1].Point.X * Mesh.Vertices[i].Point.Y / 2.0 +
						Mesh.Vertices[i + 1].Point.X * Mesh.Vertices[i + 1].Point.Y) / 6.0;
				}
			}

			Sx = sxRebar + sxMesh;
			Sy = syRebar + syMesh;
		}

		public void CalculateInertiaMoments(Point2d centroid, out double Jxx, out double Jyy, out double Jxy)
		{
			double jxRebar = 0.0;           // momento inerzia rispetto all'asse X Rebar
			double jyRebar = 0.0;           // momento inerzia rispetto all'asse Y Rebar
			double icRebar = 0.0;           // momento centrifugo  Rebar
			double jxMesh = 0.0;           // momento inerzia rispetto all'asse X cls
			double jyMesh = 0.0;           // momento inerzia rispetto all'asse Y cls
			double icMesh = 0.0;           // momento centrifugo cls

			for (int i = 0; i < Rebars.Count(); i++)
			{
				jxRebar += (N - 1) * (Rebars[i].RebarSection.Jxx + Rebars[i].Area * (Math.Pow((Rebars[i].Position.Y - centroid.Y), 2)));
				jyRebar += (N - 1) * (Rebars[i].RebarSection.Jyy + Rebars[i].Area * (Math.Pow((Rebars[i].Position.X - centroid.X), 2)));
				icRebar += (N - 1) * (Rebars[i].RebarSection.Jxy + Rebars[i].Area * (Rebars[i].Position.X - centroid.X) * (Rebars[i].Position.Y - centroid.Y));
			}

			for (int i = 1; i <= Mesh.FacesCount; i++)
			{
				double faceArea = Mesh.GetFaceArea(Mesh.Faces[i]);
				MeshEdge[] edges = Mesh.GetFaceEdges(Mesh.Faces[i]);
				double baseLength = Mesh.GetEdgeLength(edges[0]);
				double height = 2 * faceArea / baseLength;

				Point2d faceCentroid = Mesh.GetFaceCentroid(Mesh.Faces[i]);

				jxMesh += faceArea * (Math.Pow((faceCentroid.Y - centroid.Y) * Math.Cos(rotation), 2) +
					Math.Pow((faceCentroid.X - centroid.X) * Math.Sin(rotation), 2));
				jyMesh += faceArea * (Math.Pow((faceCentroid.X - centroid.X) * Math.Cos(rotation), 2) +
					Math.Pow((faceCentroid.Y - centroid.Y) * Math.Sin(rotation), 2));
				icMesh += faceArea * (faceCentroid.Y - centroid.Y) * Math.Cos(rotation) * (faceCentroid.X - centroid.X) * Math.Sin(rotation);
			}

			Jxx = jxRebar + jxMesh;
			Jyy = jyRebar + jyMesh;
			Jxy = icRebar + icMesh;
		}

		/// <summary>
		/// Internal method to set the mechanical properties to the section
		/// </summary>
		public virtual void SetMechanicalProperties()
		{
			_area = CalculateArea();
			//_centroid = CalculateCentroid();

			//CalculateInertiaMoments(out double Jxx, out double Jyy, out double Jxy);
			//_jxx = Jxx;
			//_jyy = Jyy;
			//_jxy = Jxy;
			//_j11 = CalculateJ11();
			//_j22 = CalculateJ22();
			//_jw = CalculateJw();
			//_jt = CalculateJt();
			//_shearCenter = CalculateShearCenter();
			//_wel1 = CalculateWel1();
			//_wel2 = CalculateWel2();
			//_wpl1 = CalculateWpl1();
			//_wpl2 = CalculateWpl2();
			//_angleX1 = CalculateAngle();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public double CalculateN()
		{
			return 15.0;
			//return Rebars.FirstOrDefault().RebarMaterial.E / ConcreteMaterial.E;
		}

		public double CalculateArea()
		{
			return Shape.GetArea();
		}

		//protected Point2d CalculateCentroid()
		//{
		//	//CalculateStaticMoments(out double Sx, out double Sy);

		//	//return new Point2d(Sx / Area, Sy / Area);
		//}

		public Mesh GenerateMesh()
		{
			BoundingBox3d bBox = Shape.GetBoundingBox();
			double size = Math.Min(bBox.Size.X, bBox.Size.Y) / 10.0;

			Mesh.GenerateOptions generateOptions = new Mesh.GenerateOptions()
			{
				Algorithm = Mesh.GenerateOptions.MeshAlgorithm.MeshAdapt,
				Recombine = false,
				UseGlobalProgressID = true,

				//MeshSize = size,
			};

			Mesh.Generate(new Shape[] { Shape }, generateOptions, out List<Mesh> meshes, out Mesh.GenerateMeshStatus status);

			return meshes[0];
		}

		//TODO: implementare metodi di calcolo della sezione

	}
}
