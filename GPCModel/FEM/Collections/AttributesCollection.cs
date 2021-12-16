using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.FEM.Attributes;

namespace GPC.Model.FEM.Collections
{
    [Serializable]
    public class AttributesCollection<T> : ModelObjectSet<T> where T : Attributes.Attribute
    {

        public AttributesCollection()
        {
            _collection = new HashSet<T>(new AttributeEqualityComparer());
        }


        public AttributesCollection(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        /// <returns>True if the element has been added
        /// <para>False if the element has not been added.</para>
        /// </returns>
        /// <remarks>This is a O(1) operation. 
        /// <para>If an <paramref name="item"/> with the same <see cref="Attributes.Attribute.CaseName"/> already exist. It will be replaced</para>
        /// </remarks>
        /// <param name="item"></param>
        /// <param name="replaced"><see langword="True"/> if the <paramref name="item"/> replace an existing one </param>
        /// <inheritdoc cref="ModelObjectSet{T}.Add(T)"/>
        public bool Add(T item, out bool replaced)
        {
            lock (_locker)
            {
                if (base.Add(item))
                {
                    // caseName nuovo
                    // l'elemento è stato aggiunto
                    replaced = false;
                    return true;
                }
                else
                {
                    // esiste già un elemento con lo stesso caseName

                    // rimuoviamo quello già presente                    
                    _collection = _collection.Except(_collection.Where(i => i.CaseName == item.CaseName).ToList()).ToHashSet();
                    
                    replaced = true;

                    return base.Add(item); // dovrebbe sempre tornare vero, se torna falso è successo qualcosa di anomalo
                }
            }
        }

        /// <returns>True if the element has been added
        /// <para>False if the element has not been added.</para>
        /// </returns>
        /// <remarks>This is a O(1) operation. 
        /// <para>If an <paramref name="item"/> with the same <see cref="Attributes.Attribute.CaseName"/> already exist. It will be replaced</para>
        /// </remarks>
        /// <inheritdoc cref="ModelObjectSet{T}.Add(T)"/>
        public override bool Add(T item)
        {
            lock (_locker)
            {
                if (base.Add(item))
                {
                    // caseName nuovo
                    // l'elemento è stato aggiunto
                    return true;
                }
                else
                {
                    // esiste già un elemento con lo stesso caseName

                    // rimuoviamo quello già presente                    
                    _collection = _collection.Except(_collection.Where(i => i.CaseName == item.CaseName).ToList()).ToHashSet();

                    return base.Add(item); // dovrebbe sempre tornare vero, se torna falso è successo qualcosa di anomalo
                }
            }
        }


        /// <inheritdoc cref="ModelObjectSet{T}.Add(T)"/>
        /// <param name="items"></param>
        /// <param name="replaced"><see langword="True"/> if the <paramref name="items"/> replace an existing one </param>
        public bool AddRange(IEnumerable<T> items, out bool[] replaced)
        {
            replaced = new bool[items.Count()];

            if (items != null)
            {
                int index = 0;
                foreach (var item in items)
                {
                    var ret = this.Add(item, out bool rep);

                    replaced[index] = rep;

                    if (!ret)
                        return false;
                    
                    index++;
                }
                return true;
            }
            return false;
        }

        /// <inheritdoc cref="ModelObjectSet{T}.Add(T)"/>
        public override bool AddRange(IEnumerable<T> items)
        {
            if (items != null)
            {
                foreach (var item in items)
                {
                    if (!this.Add(item))
                        return false;
                }
                return true;
            }
            return false;
        }


        public Attributes.Attribute GetElementByCaseName(string caseName)
        {
            return _collection.Where(i => i.CaseName.Equals(caseName)).FirstOrDefault();
        }

        public bool ContainsCaseName(string caseName)
        {
            return _collection.Where(i => i.CaseName.Equals(caseName)).Count() > 0;
        }

    }
}
