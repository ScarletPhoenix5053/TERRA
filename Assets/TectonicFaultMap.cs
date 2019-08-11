using UnityEngine;
using System.Collections.Generic;

namespace SCARLET.TERRA
{
    public class TectonicFaultMap : MonoBehaviour
    {
        #region NODES

        private Transform NodeContainer;
        private void GetNodeContainer()
        {
            NodeContainer = transform.GetChildWIthTag(Tags.TectonicNodeContainer);
            if (NodeContainer == null) Debug.LogError("Cannot find a chid tagged [" + Tags.TectonicNodeContainer + "]!");
        }
        private void ValidateNodeContainer()
        {
            if (NodeContainer == null) throw new TERRAException("NodeContainer is not set to anything");
        }

        public void AddNode(Vector3 pos)
        {
            ValidateNodeContainer();

            TectonicFaultNode node = Instantiate(
                Resources.Load<TectonicFaultNode>(ResourcePath.FaultNode),
                NodeContainer
                );

            node.name = "Fault Node #" + NodeContainer.childCount.ToString().PadLeft(3, '0');
            node.CardinalPos = pos;
        }
        public void RemoveNode(TectonicFaultNode node)
        {
            ValidateNodeContainer();
            
            for (int i = 0; i < NodeContainer.childCount; i++)
            {
                if (NodeContainer.GetChild(i).GetComponent<TectonicFaultNode>() == node)
                {
                    Destroy(NodeContainer.GetChild(i).gameObject);
                    break;
                }
            }
        }
        public void MoveNode(TectonicFaultNode node, Vector3 pos)
        {
            node.CardinalPos = pos;
        }

        #endregion
        #region LINKS

        private Transform LinkContainer;
        private void GetLinkContainer()
        {
            LinkContainer = transform.GetChildWIthTag(Tags.TectonicLinkContainer);
            if (LinkContainer == null) Debug.LogError("Cannot find a chid tagged [" + Tags.TectonicLinkContainer + "]!");
        }
        private void ValidateLinkContainer()
        {
            if (LinkContainer == null) throw new TERRAException("LinkContainer is not set to anything");
        }

        public TectonicFaultLine[] Links
        {
            get
            {
                var links = new TectonicFaultLine[LinkContainer.childCount];
                for (int i = 0; i < links.Length; i++)
                    links[i] = LinkContainer.GetChild(i).GetComponent<TectonicFaultLine>();
                
                return links;
            }
        }
        public void AddLink(TectonicFaultLine faultLine)
        {
            ValidateLinkContainer();

            faultLine.name = "Fault Line #" + LinkContainer.childCount.ToString().PadLeft(3, '0');
            faultLine.transform.parent = LinkContainer;
        }
        public void RemoveLink(TectonicFaultLine faultLine)
        {
            ValidateLinkContainer();

            for (int i = 0; i < LinkContainer.childCount; i++)
            {
                if (LinkContainer.GetChild(i).GetComponent<TectonicFaultLine>() == faultLine)
                {
                    Destroy(LinkContainer.GetChild(i).gameObject);
                    break;
                }
            }
        }

        #endregion

        private void Awake()
        {
            GetNodeContainer();
            GetLinkContainer();
        }
    }
}