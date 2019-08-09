using UnityEngine;
using System.Collections;

namespace SCARLET.TERRA
{
    public class TectonicFaultNode : MonoBehaviour
    {
        public Vector3 CardinalPos { get => transform.position; set { transform.position = value; } }
        public Vector3 SphericalPos { get => throw new TER_NotImplimentedException(); set { throw new TER_NotImplimentedException(); } }
        public Material Material { get => GetComponent<MeshRenderer>().sharedMaterial; set { GetComponent<MeshRenderer>().sharedMaterial = value; } }
    }
}