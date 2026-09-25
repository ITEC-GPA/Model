using GPC.Geometry;
using GPC.Model.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Restrains
{
    /// <summary>
    /// Restrains of the degrees of freedom of a geometry (node, line), in a coordinate system
    /// </summary>
    public abstract class GeometryRestrain : Attributes.Attribute
    {
        /// <summary>
        /// The degrees of freedom of a node
        /// </summary>
        public enum DOF
        {
            /// <summary>
            /// Translation along the first axis (0)
            /// </summary>
            DX,
            /// <summary>
            /// Translation along the second axis (1)
            /// </summary>
            DY,
            /// <summary>
            /// Translation along the third axis (2)
            /// </summary>
            DZ,
            /// <summary>
            /// Rotation around the first axis (3)
            /// </summary>
            RX,
            /// <summary>
            /// Rotation around the second axis (4)
            /// </summary>
            RY,
            /// <summary>
            /// Rotation around the third axis (5)
            /// </summary>
            RZ,
            /// <summary>
            /// Additional translation along the first axis, used by the laminated glass plate elements (6)
            /// </summary>
            DDX,
            /// <summary>
            /// Additional translation along the second axis, used by the laminated glass plate elements (7)
            /// </summary>
            DDY,
            /// <summary>
            /// Additional translation along the third axis, used by the laminated glass plate elements (8)
            /// </summary>
            DDZ,
        }

        #region Variables

        /// <summary>
        /// The coordinate system of the restrain
        /// </summary>
        protected CoordinateSystem _coordinateSystem;
        /// <summary>
        /// The restrains of the degrees of freedom
        /// </summary>
        protected List<DofRestrain> _restrains;

        #endregion

        #region Properties

        /// <summary>
        /// The coordinate system of the restrain
        /// </summary>
        public CoordinateSystem CoordinateSystem { get => _coordinateSystem; set => _coordinateSystem = value; }

        /// <summary>
        /// List of restrain, one for each direction
        /// </summary>
        public List<DofRestrain> Restrains { get => _restrains; set => _restrains = value; }

        #endregion

        #region Constructor

        /// <summary>
        /// Creates the restrain
        /// </summary>
        /// <param name="coordinateSystem">The coordinate system</param>
        /// <param name="restrains">The restrains of the degrees of freedom (null: an empty list)</param>
        /// <param name="name">The name</param>
        /// <param name="id">The id</param>
        /// <exception cref="ArgumentNullException">If <paramref name="coordinateSystem"/> is null</exception>
        public GeometryRestrain(CoordinateSystem coordinateSystem, List<DofRestrain> restrains, string name = "", int id = IDUNASSIGNED)
            : base(name, id)
        {
            _coordinateSystem = coordinateSystem ?? throw new ArgumentNullException(nameof(coordinateSystem));
            _restrains = restrains ?? new List<DofRestrain>();
        }

        /// <summary>
        /// Deserialization constructor (not implemented)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        /// <exception cref="NotImplementedException">Always</exception>
        protected GeometryRestrain(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the coordinate system
        /// </summary>
        /// <param name="coordinateSystem">The coordinate system</param>
        public void SetCoordinateSystem(CoordinateSystem coordinateSystem)
        {
            _coordinateSystem = coordinateSystem;
        }

        /// <summary>
        /// Adds the restrain of a degree of freedom (a degree of freedom already present is not replaced)
        /// </summary>
        /// <param name="dofRestrain">The restrain</param>
        public void AddRestain(DofRestrain dofRestrain)
        {
            _restrains.Add(dofRestrain);
        }

        /// <summary>
        /// The first axis of the coordinate system
        /// </summary>
        /// <returns>The axis</returns>
        public Vector3d GetV1() => _coordinateSystem.V1;

        /// <summary>
        /// The second axis of the coordinate system
        /// </summary>
        /// <returns>The axis</returns>
        public Vector3d GetV2() => _coordinateSystem.V2;

        /// <summary>
        /// The third axis of the coordinate system
        /// </summary>
        /// <returns>The axis</returns>
        public Vector3d GetV3() => _coordinateSystem.V3;

        /// <summary>
        /// The origin of the coordinate system
        /// </summary>
        /// <returns>The origin</returns>
        public Point3d GetCoordinateSystemOrigin() => _coordinateSystem.Origin;

        /// <summary>
        /// The restrained geometry
        /// </summary>
        /// <returns>The geometry</returns>
        public abstract GeometryBase GetGeometry();

        /// <summary>
        /// The restrained element
        /// </summary>
        /// <returns>The element</returns>
        public abstract Element GetElement();

        /// <summary>
        /// The restrained degrees of freedom
        /// </summary>
        /// <returns>Dictionary of each restrained DOF where <see cref="DofRestrain.IsRestrained"/> is <see langword="true"/></returns>
        public Dictionary<DOF, bool> GetRestrains()
        {
            Dictionary<DOF, bool> kvp = new Dictionary<DOF, bool>();

            for (int i = 0; i < _restrains.Count; i++)
            {
                if (_restrains[i].IsRestrained)
                {
                    if (kvp.ContainsKey(_restrains[i].Dof))
                    {
                        kvp[_restrains[i].Dof] = kvp[_restrains[i].Dof] | kvp[_restrains[i].Dof]; // in teoria ritorna questo: V V = V, V F = V, F F = F. Cioè se è vincolato almeno una volta resta vincolato
                    }
                    else
                    {
                        kvp[_restrains[i].Dof] = _restrains[i].IsRestrained;
                    }
                }
            }

            return kvp;
        }

        /// <summary>
        /// The stiffnesses of the degrees of freedom, summed by degree of freedom (a DOF not yet in the dictionary throws
        /// <see cref="KeyNotFoundException"/>: it always throws if a stiffness is not zero)
        /// </summary>
        /// <returns>Dictionary of each DOF where <see cref="DofRestrain.Stiffness"/> is != 0</returns>
        public Dictionary<DOF, double> GetStiffnesses()
        {
            Dictionary<DOF, double> kvp = new Dictionary<DOF, double>();

            for (int i = 0; i < _restrains.Count; i++)
            {
                if (_restrains[i].Stiffness != 0)
                {
                    kvp[_restrains[i].Dof] += _restrains[i].Stiffness;
                }
            }

            return kvp;
        }

        /// <summary>
        /// The imposed displacements of the degrees of freedom, summed by degree of freedom (a DOF not yet in the dictionary throws
        /// <see cref="KeyNotFoundException"/>: it always throws if a displacement is not zero)
        /// </summary>
        /// <returns>Dictionary of each DOF where <see cref="DofRestrain.ImposedDisplacement"/> is != 0</returns>
        public Dictionary<DOF, double> GetImposedDisplacement()
        {
            Dictionary<DOF, double> kvp = new Dictionary<DOF, double>();

            for (int i = 0; i < _restrains.Count; i++)
            {
                if (_restrains[i].ImposedDisplacement != 0)
                {
                    kvp[_restrains[i].Dof] += _restrains[i].ImposedDisplacement;
                }
            }

            return kvp;
        }

        /// <summary>
        /// Replaces the restrains with <see cref="DOF.DX"/>, <see cref="DOF.DY"/>, <see cref="DOF.DZ"/>, <see cref="DOF.RX"/>,
        /// <see cref="DOF.RY"/> and <see cref="DOF.RZ"/> restrained
        /// </summary>
        public void FixAll()
        {
            Restrains = new List<DofRestrain>() {
                new DofRestrain(DOF.DX), new DofRestrain(DOF.DY), new DofRestrain(DOF.DZ),
                new DofRestrain(DOF.RX), new DofRestrain(DOF.RY), new DofRestrain(DOF.RZ) };
        }

        /// <summary>
        /// Replaces the restrains with <see cref="DOF.DX"/>, <see cref="DOF.DY"/> and <see cref="DOF.DZ"/> restrained and <see cref="DOF.RX"/>,
        /// <see cref="DOF.RY"/> and <see cref="DOF.RZ"/> released
        /// </summary>
        public void FixDisplacement()
        {
            Restrains = new List<DofRestrain>() {
                new DofRestrain(DOF.DX), new DofRestrain(DOF.DY), new DofRestrain(DOF.DZ),
                new DofRestrain(DOF.RX, false), new DofRestrain(DOF.RY, false), new DofRestrain(DOF.RZ, false) };
        }

        /// <summary>
        /// Replaces the restrains with <see cref="DOF.DX"/>, <see cref="DOF.DY"/>, <see cref="DOF.DZ"/>, <see cref="DOF.RX"/>,
        /// <see cref="DOF.RY"/> and <see cref="DOF.RZ"/> released
        /// </summary>
        public void ReleaseAll()
        {
            Restrains = new List<DofRestrain>() {
                new DofRestrain(DOF.DX, false), new DofRestrain(DOF.DY, false), new DofRestrain(DOF.DZ, false),
                new DofRestrain(DOF.RX, false), new DofRestrain(DOF.RY, false), new DofRestrain(DOF.RZ, false) };
        }

        #endregion

        #region Equals, HasCode and operators

        /// <summary>
        /// Serialization (not implemented)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        /// <exception cref="NotImplementedException">Always</exception>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Equality of the coordinate systems, of the restrains (in the same order) and of the name
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal restrain</returns>
        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return (obj is GeometryRestrain objCasted) &&
                _coordinateSystem.Equals(objCasted._coordinateSystem) &&
                _restrains.SequenceEqual(objCasted._restrains) &&
                base.Equals(objCasted);
        }

        /// <summary>
        /// The hash code of the coordinate system and of the list of the restrains (as instance)
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -23;
                hashCode = hashCode * -17 + EqualityComparer<CoordinateSystem>.Default.GetHashCode(_coordinateSystem);
                hashCode = hashCode * -17 + EqualityComparer<List<DofRestrain>>.Default.GetHashCode(_restrains);
                return hashCode;
            }
        }

        #endregion
    }
}
