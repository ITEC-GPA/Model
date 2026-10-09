using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Model.Elements;

namespace GPC.Model.Structure.Members
{
    /// <summary>A named subset of complete FEM plates. It does not own a second copy of their section properties.</summary>
    [Serializable]
    [System.Runtime.Serialization.DataContract(Name = "SurfaceZoneDefinition", Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.Structure")]
    public sealed class SurfaceZoneDefinition
    {
        [System.Runtime.Serialization.DataMember(IsRequired = true)]
        private readonly int[] _elements;
        [field: System.Runtime.Serialization.DataMember(Name = "<Id>k__BackingField", IsRequired = true)]
        public string Id { get; private set; }
        [field: System.Runtime.Serialization.DataMember(Name = "<Name>k__BackingField", IsRequired = true)]
        public string Name { get; private set; }
        public IReadOnlyList<int> ElementIds => Array.AsReadOnly(_elements);
        public SurfaceZoneDefinition(string id, IEnumerable<int> elementIds, string name=null)
        { Id=id; Name=name??id; _elements=elementIds?.ToArray(); Validate(); }
        internal void Validate()
        {
            if(string.IsNullOrWhiteSpace(Id) || _elements==null || _elements.Length==0 || _elements.Distinct().Count()!=_elements.Length)
                throw new ArgumentException("InvalidSurfaceZone");
        }
        [OnDeserialized] private void OnDeserialized(StreamingContext context) { Validate(); }
    }

    /// <summary>A physical wall/slab assembled from complete FEM plates. It can be curved; each plate retains its own axes and one property.
    /// Zones partition explicit element IDs, without inferring boundaries from a group or generating mesh elements.</summary>
    [Serializable]
    [System.Runtime.Serialization.DataContract(Name = "PhysicalSurfaceDefinition", Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.Structure")]
    public sealed class PhysicalSurfaceDefinition
    {
        [System.Runtime.Serialization.DataMember(IsRequired = true)]
        private readonly int[] _elements;
        [System.Runtime.Serialization.DataMember(IsRequired = true)]
        private readonly SurfaceZoneDefinition[] _zones;
        [field: System.Runtime.Serialization.DataMember(Name = "<Id>k__BackingField", IsRequired = true)]
        public string Id { get; private set; }
        [field: System.Runtime.Serialization.DataMember(Name = "<Name>k__BackingField", IsRequired = true)]
        public string Name { get; private set; }
        [field: System.Runtime.Serialization.DataMember(Name = "<Source>k__BackingField", IsRequired = true)]
        public string Source { get; private set; }
        public IReadOnlyList<int> ElementIds => Array.AsReadOnly(_elements);
        public IReadOnlyList<SurfaceZoneDefinition> Zones => Array.AsReadOnly(_zones);
        public PhysicalSurfaceDefinition(string id, IEnumerable<int> elementIds, IEnumerable<SurfaceZoneDefinition> zones=null, string name=null, string source=null)
        { Id=id; Name=name??id; Source=source; _elements=elementIds?.ToArray(); _zones=zones?.ToArray()??Array.Empty<SurfaceZoneDefinition>(); Validate(); }
        private void Validate()
        {
            if(string.IsNullOrWhiteSpace(Id) || _elements==null || _elements.Length==0 || _elements.Distinct().Count()!=_elements.Length
                || _zones==null || _zones.Any(z=>z==null) || _zones.Select(z=>z.Id).Distinct(StringComparer.Ordinal).Count()!=_zones.Length)
                throw new ArgumentException("InvalidPhysicalSurface");
            var assigned=new HashSet<int>(); var members=new HashSet<int>(_elements);
            foreach(var zone in _zones)
            {
                zone.Validate();
                foreach(int id in zone.ElementIds)
                {
                    if(!members.Contains(id)) throw new ArgumentException("SurfaceZoneOutsideSurface");
                    if(!assigned.Add(id)) throw new ArgumentException("OverlappingSurfaceZones");
                }
            }
        }
        [OnDeserialized] private void OnDeserialized(StreamingContext context) { Validate(); }
        public string ZoneAt(int elementId)
        {
            if(!_elements.Contains(elementId)) throw new ArgumentException("PlateOutsidePhysicalSurface");
            return _zones.SingleOrDefault(z=>z.ElementIds.Contains(elementId))?.Id;
        }
        public IReadOnlyList<AreaElement> ResolveElements(Models.Model model, string zoneId=null)
        {
            if(model==null) throw new ArgumentNullException(nameof(model)); Validate();
            var elements=new List<AreaElement>();
            foreach(int id in _elements)
            {
                if(!model.AreaElements.TryGetValue(id,out var element) || element==null || element.Nodes.Count<3 || element.Nodes.Count>4
                    || element.Nodes.Any(n=>n==null)
                    || element.Nodes.Select(n=>n.Id).Distinct().Count()!=element.Nodes.Count
                    || element.Nodes.Any(n=>!model.NodesElements.TryGetValue(n.Id,out var registered) || !ReferenceEquals(n,registered)))
                    throw new ArgumentException("MissingSurfacePlateOrConnectivity");
                elements.Add(element);
            }
            // A physical surface must form one edge-connected mesh patch. Corner-only contact is insufficient.
            var edges=new Dictionary<string,List<int>>(StringComparer.Ordinal);
            for(int i=0;i<elements.Count;i++)
            {
                var nodes=elements[i].Nodes;
                for(int j=0;j<nodes.Count;j++)
                {
                    int a=nodes[j].Id,b=nodes[(j+1)%nodes.Count].Id;
                    string key=Math.Min(a,b).ToString(System.Globalization.CultureInfo.InvariantCulture)+":"+Math.Max(a,b).ToString(System.Globalization.CultureInfo.InvariantCulture);
                    if(!edges.TryGetValue(key,out var owners)) edges[key]=owners=new List<int>();
                    owners.Add(i); if(owners.Count>2) throw new ArgumentException("NonManifoldPhysicalSurface");
                }
            }
            var neighbours=elements.Select(e=>new List<int>()).ToArray();
            foreach(var owners in edges.Values.Where(v=>v.Count==2))
            { neighbours[owners[0]].Add(owners[1]); neighbours[owners[1]].Add(owners[0]); }
            var visited=new HashSet<int>{0}; var queue=new Queue<int>(); queue.Enqueue(0);
            while(queue.Count>0) foreach(int next in neighbours[queue.Dequeue()]) if(visited.Add(next)) queue.Enqueue(next);
            if(visited.Count!=elements.Count) throw new ArgumentException("DisconnectedPhysicalSurface");
            if(zoneId==null) return elements.AsReadOnly();
            var zone=_zones.SingleOrDefault(z=>z.Id==zoneId) ?? throw new ArgumentException("UnknownSurfaceZone");
            return elements.Where(e=>zone.ElementIds.Contains(e.Id)).ToArray();
        }
    }

}
