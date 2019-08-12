using UnityEngine;
using System.Collections.Generic;

namespace SCARLET.TERRA
{
    public class TectonicFaultMap : MonoBehaviour
    {
        #region NODES

        public List<TectonicFaultNode> Nodes = new List<TectonicFaultNode>();
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

            Nodes.Add(node);
        }
        public void RemoveNode(TectonicFaultNode node)
        {
            ValidateNodeContainer();
            
            for (int i = 0; i < NodeContainer.childCount; i++)
            {
                var thisNode = NodeContainer.GetChild(i).GetComponent<TectonicFaultNode>();
                if (thisNode == node)
                {
                    Nodes.Remove(thisNode);
                    
                    var thisNodeLinks = thisNode.Links;
                    Debug.Log(thisNodeLinks.Count);
                    for (int j = thisNodeLinks.Count - 1; j >= 0; j--)
                    {
                        // Get affected nodes and remove their references to links
                        var otherNode = thisNodeLinks[j].NodeOther(thisNode);
                        otherNode.Links.Remove(thisNodeLinks[j]);

                        // Remove the link
                        RemoveLink(thisNodeLinks[j]);
                    }
                    
                    Destroy(thisNode.gameObject);
                    thisNode = null;
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

        public List<TectonicFaultLine> FaultLines = new List<TectonicFaultLine>();
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

        public TectonicFaultLine[] FindLinks()
        {
            var links = new TectonicFaultLine[LinkContainer.childCount];
            for (int i = 0; i < links.Length; i++)
                links[i] = LinkContainer.GetChild(i).GetComponent<TectonicFaultLine>();
                
            return links;
        }
        public void AddLink(TectonicFaultLine faultLine)
        {
            ValidateLinkContainer();

            faultLine.name = "Fault Line #" + LinkContainer.childCount.ToString().PadLeft(3, '0');
            faultLine.transform.parent = LinkContainer;

            FaultLines.Add(faultLine);
        }
        public void RemoveLink(TectonicFaultLine faultLine)
        {
            ValidateLinkContainer();

            for (int i = 0; i < LinkContainer.childCount; i++)
            {
                var thisLine = LinkContainer.GetChild(i).GetComponent<TectonicFaultLine>();
                if (thisLine == faultLine)
                {
                    thisLine.NodeA?.Links.Remove(thisLine);
                    thisLine.NodeB?.Links.Remove(thisLine);

                    FaultLines.Remove(thisLine);
                    Destroy(thisLine.gameObject);
                    break;
                }
            }
        }

        #endregion
        #region PLATES

        internal struct TectonicPlate
        {
            internal List<TectonicFaultLine> BoundaryLines;
            internal Vector3 GetDirToBoundary(int i)
            {
                return BoundaryLines[i].CardinalPos - CardinalPos;
            }

            internal Vector3 CardinalPos
            {
                get
                {
                    var pos = Vector3.zero;
                    foreach (TectonicFaultLine boundaryLine in BoundaryLines)
                        pos += boundaryLine.CardinalPos;

                    return pos;
                }
            }
        }
        internal List<TectonicPlate> Plates { get; private set; }
        private void CheckPlates()
        {
            // Create new set of plates from scratch
            Plates = new List<TectonicPlate>();

            
        }

        #endregion

        private void Awake()
        {
            GetNodeContainer();
            GetLinkContainer();
        }
    }
}