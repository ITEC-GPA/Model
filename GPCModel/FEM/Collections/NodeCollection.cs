using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.FEM.Collections
{
    [Serializable]
    public class NodeCollection : IEnumerable<Node>, ISerializable
    {
        protected class PositionComparer : IComparer<Node>
        {
            public int Compare(Node x, Node y)
            {
                if (Math.Abs(x.Position.X - y.Position.X) > 0.001)
                    return x.Position.X < y.Position.X ? -1 : 1;
                if (Math.Abs(x.Position.Y - y.Position.Y) > 0.001)
                    return x.Position.Y < y.Position.Y ? -1 : 1;
                if (Math.Abs(x.Position.Z - y.Position.Z) > 0.001)
                    return x.Position.Z < y.Position.Z ? -1 : 1;
                return 0;
            }
        }

        protected readonly List<Node> _collection;
        protected int _lastId;
        protected static PositionComparer _positionComparer = new PositionComparer();

        /// <summary>
        /// Get an item by his id
        /// </summary>
        /// <param name="id">The Id of the item to retrieve</param>
        /// <returns>The node foud or null if it not exists</returns>
        public Node this[int id] => GetById(id)?.Duplicate();

        public int Count => _collection.Count;

        public NodeCollection()
        {
            _collection = new List<Node>();
            _lastId = 1;
        }

        public NodeCollection(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info can't be null");

            _collection = (List<Node>)info.GetValue("Collection", typeof(List<Node>));
            _lastId = info.GetInt32("LastId");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info can't be null");
            info.AddValue("Collection", _collection, typeof(List<Node>));
            info.AddValue("LastId", _lastId);
        }

        /// <summary>
        /// Get the node index by his id
        /// </summary>
        /// <param name="id">The node Id</param>
        /// <returns></returns>
        public int GetIndexById(int id)
        {
            int pos = -1;
            Parallel.For(0, _collection.Count, (i, state) =>
            {
                if (_collection[i].Id == id)
                {
                    pos = i;
                    state.Stop();
                }
            });
            return pos;
        }

        /// <summary>
        /// Search a node by his position (the id is not considered).
        /// </summary>
        /// <param name="item">The node to search</param>
        /// <returns>The index of the node if found, or a negative value if the node does not exist</returns>
        protected int GetIndexByValue(Node item)
        {
            return _collection.BinarySearch(0, 1, item, _positionComparer);
        }

        /// <summary>
        /// Get a node by his Id.
        /// </summary>
        /// <param name="id">The id of the node to get</param>
        /// <returns>The node or null if not exists</returns>
        public Node GetById(int id)
        {
            int pos = GetIndexById(id);
            if (pos >= 0)
                return _collection[pos];
            return null;
        }

        /// <summary>
        /// Get a node by his index. 
        /// </summary>
        /// <param name="index">The node index</param>
        /// <returns>The note or null if out of range</returns>
        public Node GetByIndex(int index)
        {
            if (index > _collection.Count - 1)
                return null;
            return _collection[index];
        }

        /// <summary>
        /// Tell if a node exists or not (the id is not considered)
        /// </summary>
        /// <param name="item">The node to check</param>
        /// <returns>True if the collection contains the given node</returns>
        public bool Contains(Node item)
        {
            return _collection.BinarySearch(item, _positionComparer) >= 0;
        }

        /// <summary>
        /// Reorder the collection based on the items value
        /// </summary>
        public void Sort()
        {
            Node[] nodes = _collection.ToArray();
            Array.Sort(nodes, _positionComparer);
            _collection.Clear();
            _collection.AddRange(nodes);
        }

        /// <summary>
        /// Remove all the items from the collection
        /// </summary>
        public void Clear()
        {
            _collection.Clear();
        }

        /// <summary>
        /// Copy the ordered collection in the given array
        /// </summary>
        /// <param name="array">The array to fill with the nodes of the collection</param>
        /// <param name="arrayIndex">The 0 based index where to start the copy</param>
        public void CopyTo(Node[] array, int arrayIndex)
        {
            _collection.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Returns the inxed of a node
        /// </summary>
        /// <param name="item">The item</param>
        /// <returns>The index of the node or -1 if not exist</returns>
        public int IndexOf(Node item)
        {
            int pos = _collection.BinarySearch(item, _positionComparer);
            return pos >= 0 ? pos : -1;
        }

        /// <summary>
        /// Adds a new node and returns the new Id. If the node already exists return his Id
        /// </summary>
        /// <param name="item">The node to add</param>
        /// <returns>The node Id</returns>
        public int Add(Node item)
        {
            int pos = _collection.BinarySearch(item, _positionComparer);
            if (pos < 0) // New not existing item
            {
                item.Id = _lastId++;

                if (pos == -_collection.Count - 1)
                    _collection.Add(item); // Append to the end of the collection
                else
                    _collection.Insert(-pos - 1, item); // Insert inside to keep the collection ordered
                return item.Id;
            }
            return _collection[pos].Id;
        }

        /// <summary>
        /// Replace a node with another and reposition it basing on its value
        /// </summary>
        /// <param name="oldItem">The node to replace</param>
        /// <param name="newItem">The new node</param>
        /// <returns>The new node index</returns>
        public int Replace(Node oldItem, Node newItem)
        {
            if (oldItem.Id != newItem.Id)
                return -1;
            return 0;
        }

        /// <summary>
        /// Update the given node and return his new index position
        /// </summary>
        /// <param name="item">The node to update</param>
        /// <returns></returns>
        public int Update(Node item)
        {
            int oldPos = GetIndexById(item.Id);
            _collection.RemoveAt(oldPos);
            int newPos = _collection.BinarySearch(item, _positionComparer);
            if (newPos == -_collection.Count - 1)
            {
                _collection.Add(item); // Append to the end of the collection
                return -newPos - 1;
            }
            else
            {
                _collection.Insert(-newPos - 1, item); // Insert inside to keep the collection ordered           
                return newPos + 1;
            }            
        }

        /// <summary>
        /// Remove the given node from the collection
        /// </summary>
        /// <param name="item">The node to remove</param>
        /// <returns>True id success</returns>
        public bool Remove(Node item)
        {
            int pos = _collection.BinarySearch(item, _positionComparer);
            if (pos >= 0)
            {
                _collection.RemoveAt(pos);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Remove the node at the given index
        /// </summary>
        /// <param name="index">The index of the node to remove</param>
        public void RemoveAt(int index)
        {
            _collection.RemoveAt(index);
        }

        /// <summary>
        /// Remove a node by his id
        /// </summary>
        /// <param name="id">The id of th node to remove</param>
        /// <returns>True id success</returns>
        public bool RemoveById(int id)
        {
            int pos = GetIndexById(id);
            if (pos >= 0)
            {
                _collection.RemoveAt(pos);
                return true;
            }
            return false;
        }

        public IEnumerator<Node> GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _collection.GetEnumerator();
        }
    }
}
