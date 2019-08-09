using UnityEngine;
using System.Collections.Generic;

namespace SCARLET.TERRA
{
    public class TectonicFaultMap : MonoBehaviour
    {
        public void AddNode(Vector3 pos)
        {
            if (NodeContainer == null) throw new TERRAException("NodeContainer is not set to anything");

            TectonicFaultNode node = Instantiate(
                Resources.Load<TectonicFaultNode>(ResourcePath.FaultNode),
                NodeContainer
                );

            node.CardinalPos = pos;
        }

        private Transform NodeContainer;
        private void GetNodeContainer()
        {
            NodeContainer = transform.GetChildWIthTag(Tags.TectonicNodeContainer);
            if (NodeContainer == null) Debug.LogError("Cannot find a chid tagged [" + Tags.TectonicNodeContainer + "]!");
        }        

        private void Awake()
        {
            GetNodeContainer();
        }
    }
}