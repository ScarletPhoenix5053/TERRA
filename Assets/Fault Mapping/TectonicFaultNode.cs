using UnityEngine;
using System.Collections.Generic;

namespace SCARLET.TERRA
{
    public class TectonicFaultNode : MonoBehaviour
    {
        public Vector3 CardinalPos {
            get => transform.position;
            set
            {
                transform.position = value;
                UpdateLinks();
            }
        }
        public Vector3 SphericalPos { get => throw new TER_NotImplimentedException(); set { throw new TER_NotImplimentedException(); } }
        public Material Material { get => GetComponent<MeshRenderer>().sharedMaterial; set { GetComponent<MeshRenderer>().sharedMaterial = value; } }

        public List<TectonicFaultLine> Links { get; set; } = new List<TectonicFaultLine>();
        public TectonicFaultLine LinkTo(TectonicFaultNode other)
        {
            var line = Instantiate(Resources.Load<TectonicFaultLine>(ResourcePath.FaultLine));

            line.NodeA = this;
            line.NodeB = other;

            line.NodeA.Links.Add(line);
            line.NodeB.Links.Add(line);

            line.UpdateLine();
            return line;
        }
        private void UpdateLinks()
        {
            if (Links == null) return;
            for (int i = 0; i < Links.Count; i++)
            {
                var link = Links[i];

                if (link != null)
                    link.UpdateLine();
                else
                    Links.Remove(link);
            }
        }
        

        public override bool Equals(object other)
        {
            var node = other as TectonicFaultNode;

            return !ReferenceEquals(null, node)
                && CardinalPos.Equals(node.CardinalPos);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                const int hashBase = (int)2166136261;
                const int hashMult = 16777619;

                int hash = hashBase;
                hash = (hash * hashMult) ^ (!ReferenceEquals(null, CardinalPos) ? CardinalPos.GetHashCode() : 0);

                return hash;
            }
        }
    }
}