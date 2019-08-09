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
        private void UpdateLinks()
        {
            if (Links == null) return;
            foreach (TectonicFaultLine link in Links)
                link.UpdateLine();
        }

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
    }
}