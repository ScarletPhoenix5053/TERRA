using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCARLET.TERRA
{
    public class TectonicSimulationInterface : MonoBehaviour
    {
        public TectonicFaultMap FaultMap;

        public List<string> DetectionLayerNames = new List<string>() { LayerNames.InteractactableBase };
        public LayerMask DetectionLayers;
        private void AddDetectionLayer(string layerName)
        {
            if (!DetectionLayerNames.Contains(layerName)) DetectionLayerNames.Add(layerName);
            UpdateDetectionLayers();
        }
        private void RemoveDetectionLayer(string layerName)
        {
            if (DetectionLayerNames.Contains(layerName)) DetectionLayerNames.Remove(layerName);
            UpdateDetectionLayers();
        }
        private void UpdateDetectionLayers() => DetectionLayers = LayerMask.GetMask(DetectionLayerNames.ToArray());

        public enum SelectionState
        {
            Nothing,
            SingleNode,
            SingleLink
        }
        public SelectionState Selection = SelectionState.Nothing;

        private struct PlayerInput
        {
            public KeyCode inputKey;
            public bool Held => Input.GetKey(inputKey);
            public bool Down => Input.GetKeyDown(inputKey);

            public PlayerInput(KeyCode keyCode)
            {
                inputKey = keyCode;
            }
        }
        private PlayerInput PrimaryInput = new PlayerInput(KeyCode.Mouse0);
        private PlayerInput SecondaryInput = new PlayerInput(KeyCode.Mouse1);
        private PlayerInput DelInput = new PlayerInput(KeyCode.Backspace);

        private enum TargetType
        {
            Invalid,
            Empty,
            Node,
            Line,
            Plate
        }
        private TargetType GetMouseTarget(out RaycastHit hitData)
        {
            hitData = default;
            TargetType targetType = TargetType.Invalid;

            var screenToPoint = Camera.main.ScreenPointToRay(Input.mousePosition);
            var castHits = Physics.RaycastAll(screenToPoint, Mathf.Infinity, DetectionLayers);

            // If anything was hit
            if (castHits.Length > 0)
            {
                for (int i = 0; i < castHits.Length; i++)
                {
                    hitData = castHits[i];

                    // Determine type of target hit
                    if (hitData.transform.tag == Tags.TectonicNode) targetType = TargetType.Node;
                    else continue;

                    return targetType;
                }
                // did not find a layer of interest, must have been a base layer
                return TargetType.Empty;
            }
            // else must not have found any targets.
            else return targetType;
        }

        private TectonicFaultNode selectedNode;
        private void SelectNode(TectonicFaultNode node)
        {
            Debug.Log("select");
            Selection = SelectionState.SingleNode;            

            selectedNode = node;
            selectedNode.Material = Resources.Load<Material>(ResourcePath.FaultMaterial2);
            
            RemoveDetectionLayer(LayerNames.Node);
        }
        private void DeselectNode()
        {
            Debug.Log("deselect");
            Selection = SelectionState.Nothing;

            selectedNode.Material = Resources.Load<Material>(ResourcePath.FaultMaterial1);
            selectedNode = null;

            AddDetectionLayer(LayerNames.Node);
        }

        private void Update()
        {
            // Get target
            var mouseTarget = GetMouseTarget(
                out RaycastHit hit
                );
            //Debug.Log(mouseTarget.ToString());

            switch (Selection)
            {
                case SelectionState.Nothing:
                    if (PrimaryInput.Down)
                    {
                        if (mouseTarget == TargetType.Node) SelectNode(hit.transform.GetComponent<TectonicFaultNode>());
                        else if (mouseTarget == TargetType.Empty) FaultMap.AddNode(hit.point);
                    }
                    break;

                case SelectionState.SingleNode:
                    if (PrimaryInput.Held)
                    {
                        // Move
                        selectedNode.CardinalPos = hit.point;
                    }
                    else if (SecondaryInput.Down)
                    {
                        // Desel
                        DeselectNode();
                    }
                    else if (DelInput.Down)
                    {
                        // remove
                    }
                    break;

                default:
                    Debug.LogWarning("Passed though undefined selection case: " + Selection);
                    break;
            }
        }

    }
}