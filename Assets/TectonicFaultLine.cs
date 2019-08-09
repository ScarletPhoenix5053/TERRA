using UnityEngine;
using System.Collections;

namespace SCARLET.TERRA
{
    public class TectonicFaultLine : MonoBehaviour
    {
        public TectonicFaultNode NodeA;
        public TectonicFaultNode NodeB;        

        public LineRenderer LineRenderer;
        public Vector3 PointOffset = new Vector3(0,0.05f,0);

        

        public Material Material {
            get => LineRenderer != null ? LineRenderer.material : null;
            set => LineRenderer.material = LineRenderer != null ? value : LineRenderer.material;
        }

        public void UpdateLine()
        {
            if (NodeA != null
                && NodeB != null)
            {
                LineRenderer.positionCount = 2;
                LineRenderer.SetPosition(0, NodeA.CardinalPos + PointOffset);
                LineRenderer.SetPosition(1, NodeB.CardinalPos + PointOffset);
            }
        }
    }
}