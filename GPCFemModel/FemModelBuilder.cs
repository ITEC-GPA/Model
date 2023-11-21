using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Geometry.Meshes.GMesh;
using GPC.Model.Loads;
using GPC.Model.Restrains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace GPC.Model.Fem
{
	[Serializable]
	public class FemModelBuilder : FemModel, ISerializable
	{
		#region Constructors

		public FemModelBuilder()
			: this(string.Empty)
		{
		}

		public FemModelBuilder(string name)
			: base(name)
		{
		}

		private FemModelBuilder(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		#endregion

		#region Mesh and shapes

		/// <summary>
		/// Generate planar mesh from a shapes. Mesh options need to be setted by <see cref="Mesh.GenerateOptions"/>
		/// </summary>
		/// <param name="shape"></param>
		/// <param name="options"></param>
		/// <param name="platePropertyName"></param>
		/// <param name="loads"></param>
		/// <param name="restrains"></param>
		public virtual void AddShape(Shape shape, string platePropertyName, GMesh.GMeshGenerateOptions options, List<Load> loads, List<GeometryRestrain> restrains)
		{
			AddShape(shape, platePropertyName, options, loads, restrains, out _, out _, out _);
		}

		/// <summary>
		/// Generate planar mesh from a shapes. Mesh options need to be setted by <see cref="Mesh.GenerateOptions"/>
		/// </summary>
		/// <param name="shape"></param>
		/// <param name="options"></param>
		/// <param name="platePropertyName"></param>
		/// <param name="loads"></param>
		/// <param name="restrains"></param>
		/// <param name="loadNodeIdMap">Map between load and node ids</param>
		/// <param name="loadPlateIdMap">Map between load and plate ids</param>
		/// <param name="restrainNodeIdMap">Map between load and restrain ids</param>
		public virtual void AddShape(Shape shape, string platePropertyName, GMesh.GMeshGenerateOptions options, List<Load> loads, List<GeometryRestrain> restrains,
			out Dictionary<Load, int[]> loadNodeIdMap, out Dictionary<Load, int[]> loadPlateIdMap, out Dictionary<GeometryRestrain, int[]> restrainNodeIdMap)
		{
			if (shape is null)
				throw new ArgumentNullException(nameof(shape));

			var embeddedGeometries = new HashSet<GeometryBase>(); // geometrie uniche da passare al meshatore

			// per ogni carico embedda la geometria nella mesh
			if (loads != null)
			{
				foreach (var load in loads)
				{
					if (load is LineLoad ll)
						embeddedGeometries.Add(ll.GetGeometry());

					else if (load is PointLoad pl)
						embeddedGeometries.Add(pl.GetGeometry());

					else if (load is AreaLoad al)
					{
						Shape geometry = al.GetGeometry();

						if (geometry != shape)
							embeddedGeometries.Add(al.GetGeometry());
					}

					else if (load is NormalAreaLoad nal)
					{
						Shape geometry = nal.GetGeometry();

						if (geometry != shape)
							embeddedGeometries.Add(nal.GetGeometry());
					}
					else
						throw new NotSupportedException($"Load type: {load.GetType()} not supported");
				}
			}

			// per ogni vincolo embedda la geometria nella mesh
			if (restrains != null)
			{
				foreach (var restrain in restrains)
				{
					if (restrain is LineRestrain lr)
					{
						embeddedGeometries.Add(lr.GetGeometry());
					}
					else if (restrain is PointRestrain pr)
					{
						embeddedGeometries.Add(pr.GetGeometry());
					}
					else
						throw new NotSupportedException($"Restrain type: {restrain.GetType()} not supported");
				}
			}

			// Genera la mesh
			bool status = GMesh.Generate(new List<Shape> { shape }, new Dictionary<Shape, GeometryBase[]>() { [shape] = embeddedGeometries.ToArray() },
				options, out List<Mesh> meshes, out GMesh.GMeshGenerateMeshStatus generateMeshStatus);

			if (!status)
			{
				throw generateMeshStatus.GetLastException();
			}

			if (meshes.Count > 1) // Non è possibile ma controlliamo lo stesso
				throw new Exception();


			// Creo associazioni fra carichi e indici elementi
			Dictionary<IPointLoad, int[]> vertexLoadMeshEntityMap = new Dictionary<IPointLoad, int[]>();
			Dictionary<ILineLoad, int[]> vertexLineLoadMeshEntityMap = new Dictionary<ILineLoad, int[]>();
			Dictionary<IAreaLoad, int[]> plateLoadMeshEntityMap = new Dictionary<IAreaLoad, int[]>();
			Dictionary<GeometryRestrain, int[]> restrainMeshEntityMap = new Dictionary<GeometryRestrain, int[]>();

			if (loads != null)
			{
				foreach (var load in loads)
				{
					if (load is IPointLoad pl)
					{
						if (generateMeshStatus.EmbeddedGeometriesVertexMap[meshes.First()].ContainsKey(pl.GetGeometry()))
							vertexLoadMeshEntityMap[pl] = generateMeshStatus.EmbeddedGeometriesVertexMap[meshes.First()][pl.GetGeometry()];
					}
					else if (load is ILineLoad ll)
					{
						if (generateMeshStatus.EmbeddedGeometriesVertexMap[meshes.First()].ContainsKey(ll.GetGeometry()))
							vertexLineLoadMeshEntityMap[ll] = generateMeshStatus.EmbeddedGeometriesVertexMap[meshes.First()][ll.GetGeometry()];
					}
					else if (load is IAreaLoad al)
					{
						if (generateMeshStatus.EmbeddedGeometriesVertexMap[meshes.First()].ContainsKey(al.GetGeometry()))
							plateLoadMeshEntityMap[al] = generateMeshStatus.EmbeddedGeometriesVertexMap[meshes.First()][al.GetGeometry()];
						else if (al.GetGeometry() == shape)
							plateLoadMeshEntityMap[al] = meshes[0].Faces.GetElementHashMap().Keys.ToArray();
					}
					else
						throw new NotSupportedException($"Load type: {load.GetType()} not supported");
				}
			}

			if (restrains != null)
			{
				foreach (var restrain in restrains)
				{
					restrainMeshEntityMap[restrain] = generateMeshStatus.EmbeddedGeometriesVertexMap[meshes.First()][restrain.GetGeometry()];
				}
			}


			bool ret = AddMesh(meshes.First(), platePropertyName, null, vertexLoadMeshEntityMap, vertexLineLoadMeshEntityMap,
				plateLoadMeshEntityMap, restrainMeshEntityMap, out Dictionary<int, int> nodesNewIndexMap,
				out Dictionary<int, int> platesNewIndexMap, out Dictionary<int, int> brickNewIndexMap);

			// Preparo mappe in uscita
			if (!ret)
				throw new ArgumentException();

			Dictionary<Load, int[]> loadNodeIdMapBuffer = new Dictionary<Load, int[]>();
			Dictionary<Load, int[]> loadPlateIdMapBuffer = new Dictionary<Load, int[]>();
			Dictionary<GeometryRestrain, int[]> restrainNodeIdMapBuffer = new Dictionary<GeometryRestrain, int[]>();


			Action<int> actionLoad = new Action<int>((index) =>
			{
				if (loads[index] is IPointLoad pl)
				{
					loadNodeIdMapBuffer[loads[index]] = vertexLoadMeshEntityMap[pl].Select(i => nodesNewIndexMap[i]).ToArray();
				}
				else if (loads[index] is ILineLoad ll)
				{
					loadNodeIdMapBuffer[loads[index]] = vertexLineLoadMeshEntityMap[ll].Select(i => nodesNewIndexMap[i]).ToArray();
				}
				else if (loads[index] is IAreaLoad al)
				{
					loadNodeIdMapBuffer[loads[index]] = plateLoadMeshEntityMap[al].Select(i => platesNewIndexMap[i]).ToArray();
				}
				else
					throw new NotSupportedException($"Load type: {loads[index].GetType()} not supported");
			});
			Action<int> actionRestrain = new Action<int>((index) =>
			{
				restrainNodeIdMapBuffer[restrains[index]] = restrainMeshEntityMap[restrains[index]].Select(i => nodesNewIndexMap[i]).ToArray();
			});

			List<Task> tasks = new List<Task>();

			if (loads != null)
			{
				tasks.Add(Task.Run(() => Parallel.ForEach(Enumerable.Range(0, loads.Count()), actionLoad)));
			}
			if (restrains != null)
			{
				tasks.Add(Task.Run(() => Parallel.ForEach(Enumerable.Range(0, restrains.Count()), actionRestrain)));
			}

			Task.WhenAll(tasks);

			loadNodeIdMap = null;
			loadPlateIdMap = null;
			restrainNodeIdMap = null;
			if (loads != null)
			{
				loadNodeIdMap = loadNodeIdMapBuffer;
				loadPlateIdMap = loadNodeIdMapBuffer;
			}
			if (restrains != null)
			{
				restrainNodeIdMap = restrainNodeIdMapBuffer;
			}

		}

		/// <summary>
		/// 
		/// </summary> 
		/// <param name="shapes"></param>
		/// <param name="options"></param>
		/// <param name="platePropertyNames"></param>
		/// <param name="loads"></param>
		/// <param name="restrains"></param>
		public virtual void AddShapes(List<Shape> shapes, List<string> platePropertyNames, GMesh.GMeshGenerateOptions options,
			List<List<Load>> loads, List<List<GeometryRestrain>> restrains)
		{
			if (shapes is null)
				throw new ArgumentNullException(nameof(shapes));

			// Garantisce la stessa lunghezza delle liste, ma non che siano liste di non nulli
			if (shapes.Count != platePropertyNames.Count)
				throw new ArgumentException($"Size of {nameof(shapes)} and {nameof(platePropertyNames)} are different");
			if (shapes.Count != loads.Count)
				throw new ArgumentException($"Size of {nameof(shapes)} and {nameof(loads)} are different");
			if (shapes.Count != restrains.Count)
				throw new ArgumentException($"Size of {nameof(shapes)} and {nameof(restrains)} are different");

			for (int i = 0; i < shapes.Count; i++)
			{
				if (shapes[i] is null)
					throw new ArgumentNullException(nameof(shapes));

				if (!string.IsNullOrEmpty(platePropertyNames[i]) || !string.IsNullOrWhiteSpace(platePropertyNames[i]))
					throw new ArgumentNullException(nameof(platePropertyNames));

				if (loads[i] is null)
					throw new ArgumentNullException(nameof(loads));

				if (restrains[i] is null)
					throw new ArgumentNullException(nameof(restrains));


				AddShape(shapes[i], platePropertyNames[i], options, loads[i], restrains[i]);
			}
		}

		#endregion

		#region Equals - HashCode - Operators

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = -391 + base.GetHashCode();
				return hashCode;
			}
		}

		public override bool Equals(object obj)
		{
			return obj is FemModelBuilder model && base.Equals(obj);
		}

		#endregion
	}
}
