using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Utilities.Extensions;

namespace GPC.Model.Sections.Concrete
{
    [Serializable]
    public class RebarCollection : ModelObjectIdSet<ReinforcedConcreteRebar>, ISerializable
    {
        #region Variables

        protected int _maxId;

		/// <summary>
		/// Set di ID unici, l'indice d'ingresso non è garantito essere quello di uscita
		/// </summary>
		protected HashSet<int> _ids = new HashSet<int>();

		#endregion

		#region Properties

		public int MaxId => _maxId;

		#endregion

		#region Constructor

		public RebarCollection()
            : base(new ReinforcedConcreteRebar.ReinforcedConcreteRebarComparer())
        {
            _maxId = 0;
            _ids = new HashSet<int>();
        }

        protected RebarCollection(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _maxId = info.GetInt32("MaxId");
            _ids = (HashSet<int>)info.GetValue("Ids", typeof(HashSet<int>));
        }

		#endregion

		#region Add

		public override bool Add(ReinforcedConcreteRebar item)
        {
            lock (_locker)
            {
                if (_collection.Contains(item))
                {
                    // stessa posizione. => rimpiazza barra in quella posizione e assegno lo stesso id

                    HashSet<ReinforcedConcreteRebar> collection = (HashSet<ReinforcedConcreteRebar>)_collection;


                    ((HashSet<ReinforcedConcreteRebar>)_collection).TryGetValue(item, out ReinforcedConcreteRebar itemFound);

                    item.Id = itemFound.Id;
                    collection.Remove(itemFound);
                    collection.Add(item);
                }
                else
                {
                    // posizione diversa => Aggiunge barra

                    if (item.Id <= ModelObjectId.IDUNASSIGNED || item.Id == 0)
                    {
                        // se l'id non è stato assegnato, lo assegno e aggiungo id e barra
                        item.Id = ++_maxId;

                        _collection.Add(item);
                        _ids.Add(item.Id);

                        return true;
                    }
                    else
                    {
                        if (_ids.Contains(item.Id))
                        {
                            // se l'id c'è già, lo cambio

                            item.Id = ++_maxId;

                            _collection.Add(item);
                            _ids.Add(item.Id);

                            return true;
                        }
                        else
                        {
                            // l'id non c'è già, aggiungo.

                            if (item.Id > _maxId)
                                _maxId = item.Id;

                            _collection.Add(item);
                            _ids.Add(item.Id);

                            return true;
                        }
                    }

                }

                return false;
            }
        }

        public override bool AddRange(IEnumerable<ReinforcedConcreteRebar> items)
        {
            foreach (var item in items)
            {
                if (!Add(item))
                {
                    return false;
                }
            }
            return true;
        }

        #endregion

        #region Get

        public override void CopyTo(ReinforcedConcreteRebar[] array, int arrayIndex)
        {
            base.CopyTo(array, arrayIndex);
        }

        public override bool GetItem(ReinforcedConcreteRebar item, out ReinforcedConcreteRebar itemFound)
        {
            return base.GetItem(item, out itemFound);
        }

        /// <summary><inheritdoc cref="Enumerable.SingleOrDefault{TSource}(IEnumerable{TSource})"/></summary>
        /// <returns><inheritdoc cref="Enumerable.SingleOrDefault{TSource}(IEnumerable{TSource})"/></returns>
        /// <exception cref="InvalidOperationException" ></exception>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <remarks>This is a O(1) operation</remarks>
        public virtual ReinforcedConcreteRebar GetById(int id)
        {
            // l'Add non fa aggiungere oggetti con id duplicato.
            // se le istanze variano dopo che sono stati aggiunti e trova un duplicato va in eccezione

            lock (_locker)
            {
                if (Contains(id))
                {
                    return _collection.SingleOrDefault(i => i.Id == id);
                }
                else
                    throw new KeyNotFoundException($"Collection does not contain a element with id: {id}");
            }
        }

        #endregion

        #region Check

        /// <returns><see langword="True" /> if <paramref name="item"/> id already contained in the collection </returns>
        public override bool Contains(ReinforcedConcreteRebar item)
        {
            lock (_locker)
            {
                return _collection.Contains(item);
            }
        }

        /// <returns><see langword="True" /> if all the <paramref name="items"/> id already contained in the collection </returns>
        public bool ContainsRange(IEnumerable<ReinforcedConcreteRebar> items)
        {
            lock (_locker)
            {
                foreach (var item in items)
                {
                    if (!_collection.Contains(item))
                        return false;
                }
                return true;
            }
        }

        /// <returns><see langword="True" /> if this collection contains an element with <see cref="ModelObjectId.Id"/> equals to <paramref name="id"/> </returns>
        /// <remarks>This is an O(1) operation</remarks>
        public bool Contains(int id)
        {
            lock (_locker)
            {
                return _ids.Contains(id);
            }
        }

        /// <returns><see langword="True" /> if all the <paramref name="ids"/> are contained into the collection </returns>
        /// <remarks>This is an O(1*n) operation</remarks>
        public bool ContainsRange(IEnumerable<int> ids)
        {
            lock (_locker)
            {
                foreach (var item in ids)
                {
                    if (!_ids.Contains(item))
                        return false;
                }
                return true;
            }
        }

        #endregion

        #region Edit

        public override void Clear()
        {
            lock (_locker)
            {
                _collection.Clear();
                _ids.Clear();
                _maxId = 0;
            };
        }

        public bool Remove(int id)
        {
            lock (_locker)
            {
                if (_ids.Contains(id))
                {
                    _collection.Remove(GetById(id));
                    _ids.Remove(id);

                    if (id == _maxId)
					{
                        if (_ids.Count == 0)
                            _maxId = 0;
                        else
                            _maxId = _ids.Max();
                    }

                    return true;

                }
                return false;
            }
        }

        public override bool Remove(ReinforcedConcreteRebar item)
        {
            lock (_locker)
            {
                if (_ids.Contains(item.Id))
                {
                    _collection.Remove(GetById(item.Id));
                    _ids.Remove(item.Id);

                    if (item.Id == _maxId)
                    {
                        if (_ids.Count == 0)
                            _maxId = 0;
                        else
                            _maxId = _ids.Max();
                    }

                    return true;
                }

                return false;
            }
        }

        public override bool RemoveRange(IEnumerable<ReinforcedConcreteRebar> items)
        {
            lock (_locker)
            {
                foreach (var item in items)
                {
                    if (!Remove(item))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public bool RemoveRange(IEnumerable<int> ids)
        {
            lock (_locker)
            {
                foreach (var item in ids)
                {
                    if (!Remove(item))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public bool Replace(ReinforcedConcreteRebar itemToReplace, ReinforcedConcreteRebar newItem)
        {
            if (itemToReplace is null || newItem is null)
                return false;

            bool status = Remove(itemToReplace);

            if (status)
            {
                return Add(newItem);
            }

            return false;
        }

        #endregion

        #region Equals - hashcode - Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("MaxId", _maxId);
            info.AddValue("Ids", _ids);
        }

        public override bool Equals(object obj)
        {
            lock (_locker)
            {
                return obj is RebarCollection collection && _collection.ScrambledEquals(collection._collection);
            }
        }

        public override int GetHashCode()
        {
            lock (_locker)
            {
                unchecked
                {
                    return -391 + base.GetHashCode() + _collection.GetHashCodeScrambled();
                }
            }
        }

        public static bool operator ==(RebarCollection obj1, RebarCollection obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(RebarCollection obj1, RebarCollection obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion 
    }
}
