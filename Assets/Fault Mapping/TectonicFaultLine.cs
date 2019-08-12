using UnityEngine;
using System.Collections.Generic;

namespace SCARLET.TERRA
{
    public class TectonicFaultLine : MonoBehaviour
    {
        public TectonicFaultNode NodeA;
        public TectonicFaultNode NodeB;        
        public TectonicFaultNode NodeOther(TectonicFaultNode main)
        {
            if (NodeA == main) return NodeB;
            else if (NodeB == main) return NodeA;
            else return null;
        }

        public LineRenderer LineRenderer;
        public Material Material
        {
            get => LineRenderer != null ? LineRenderer.material : null;
            set => LineRenderer.material = LineRenderer != null ? value : LineRenderer.material;
        }

        public Vector3 PointOffset = new Vector3(0,0.05f,0);
        public Vector3 CardinalPos => NodeA.CardinalPos + NodeB.CardinalPos;

        public MeshCollider MeshCollider;
        public Mesh Mesh
        {
            get => MeshCollider.sharedMesh;
            set => MeshCollider.sharedMesh = value;
        }
        
        public void UpdateLine()
        {
            if (LineRenderer != null
                && NodeA != null
                && NodeB != null)
            {
                LineRenderer.positionCount = 2;
                LineRenderer.SetPosition(0, NodeA.CardinalPos + PointOffset);
                LineRenderer.SetPosition(1, NodeB.CardinalPos + PointOffset);
            }
        }

        public override bool Equals(object other)
        {
            var line = other as TectonicFaultLine;

            return !ReferenceEquals(null, line)
                && NodeA.Equals(line.NodeA)
                && NodeB.Equals(line.NodeB);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                const int hashingBase = (int)2166136261;
                const int hashingMultiplier = 16777619;

                int hash = hashingBase;
                hash = (hash * hashingMultiplier) ^ (!ReferenceEquals(null, NodeA) ? NodeA.GetHashCode() : 0);
                hash = (hash * hashingMultiplier) ^ (!ReferenceEquals(null, NodeB) ? NodeB.GetHashCode() : 0);

                return hash;
            }
        }

    }
}