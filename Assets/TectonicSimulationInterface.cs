using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SCARLET.TERRA
{
    public class TectonicSimulationInterface : MonoBehaviour
    {
        #region FAULT MAP INTERACTIONS

        public TectonicFaultMap FaultMap;

        private TectonicFaultNode selectedNode;
        private void SelectNode(TectonicFaultNode node)
        {
            Selection = SelectionState.SingleNode;

            selectedNode = node;
            selectedNode.Material = Resources.Load<Material>(ResourcePath.FaultMaterial2);

        }
        private void DeselectNode()
        {
            Selection = SelectionState.Nothing;
            if (selectedNode == null) return;

            selectedNode.Material = Resources.Load<Material>(ResourcePath.FaultMaterial1);
            selectedNode = null;
        }

        public int ScatterNodesMax = 16;
        public float ScatterNodesMinSpacing = 1f;
        public Vector3 ScatterNodesArea = new Vector3(10, 0, 10);

        private static List<string> DetectionLayerNames_Default = new List<string>()
        {
            LayerNames.InteractactableBase,
            LayerNames.Node
        };
        public List<string> DetectionLayerNames = DetectionLayerNames_Default;
        public LayerMask DetectionLayers;
        private void UpdateDetectionLayers() => DetectionLayers = LayerMask.GetMask(DetectionLayerNames.ToArray());
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
        private void RestoreDetectionLayers()
        {
            DetectionLayerNames = DetectionLayerNames_Default;
            UpdateDetectionLayers();
        }

        public void GenerateNodeField()
        {
            // Build and array of points, limited by a maxCount and minDist
            var points = new List<Vector3>();
            points.Add(new Vector3(
                Random.Range(0, ScatterNodesArea.x),
                Random.Range(0, ScatterNodesArea.y),
                Random.Range(0, ScatterNodesArea.z)
                ));
            for (int i = 1; i < ScatterNodesMax; i++)
            {
                var newPoint = new Vector3(
                    Random.Range(0, ScatterNodesArea.x),
                    Random.Range(0, ScatterNodesArea.y),
                    Random.Range(0, ScatterNodesArea.z)
                    );

                var shortestDist = float.MaxValue;
                for (int j = 0; j < points.Count; j++)
                {
                    if (Vector3.Distance(newPoint, points[j]) < shortestDist)
                        shortestDist = Vector3.Distance(newPoint, points[j]);

                    if (shortestDist <= ScatterNodesMinSpacing) break;
                }
                if (shortestDist > ScatterNodesMinSpacing) points.Add(newPoint);
            }

            // Create nodes at each of these points
            foreach (Vector3 point in points)
            {
                FaultMap.AddNode(point - (ScatterNodesArea / 2));
            }

        }

        #endregion

        #region Input - MOVE TO INPUT OBJECT LATER

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

        #endregion

        #region INTERFACE STATE

        public enum Tool
        {
            EditNodes,
            FormLinks
        }
        public Tool ActiveTool = Tool.EditNodes;
        public void SetTool(Tool newTool)
        {
            ActiveTool = newTool;
            DeselectNode();
            RestoreDetectionLayers();
        }
        public void SetTool(string toolName)
        {
            if (Enum.TryParse(toolName, out Tool newTool))
            {
                SetTool(newTool);
            }
        }

        public enum SelectionState
        {
            Nothing,
            SingleNode,
            SingleLink
        }
        public SelectionState Selection = SelectionState.Nothing;

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
                    if (hitData.transform.tag == Tags.TectonicLink) targetType = TargetType.Line;
                    else continue;

                    return targetType;
                }
                // did not find a layer of interest, must have been a base layer
                return TargetType.Empty;
            }
            // else must not have found any targets.
            else return targetType;
        }

        #endregion

        #region Runtime Messages

        private void Start()
        {
            GenerateNodeField();
        }
        private void Update()
        {
            // Get target
            var mouseTarget = GetMouseTarget(
                out RaycastHit hit
                );

            switch (ActiveTool)
            {
                #region Node Editor

                case Tool.EditNodes:
                    switch (Selection)
                    {
                        case SelectionState.Nothing:
                            if (PrimaryInput.Down)
                            {
                                if (mouseTarget == TargetType.Node)
                                {
                                    SelectNode(hit.transform.GetComponent<TectonicFaultNode>());
                                    RemoveDetectionLayer(LayerNames.Node);
                                }
                                else if (mouseTarget == TargetType.Empty) FaultMap.AddNode(hit.point);
                            }
                            break;

                        case SelectionState.SingleNode:
                            if (PrimaryInput.Held)
                            {
                                if (PrimaryInput.Down)
                                {
                                    // Adjust selected node if a new node is clicked on, or deselect if nothing clicked
                                    if (mouseTarget != TargetType.Node)
                                    {
                                        DeselectNode();
                                        AddDetectionLayer(LayerNames.Node);
                                    }
                                    else
                                    {
                                        var queryNode = hit.transform.GetComponent<TectonicFaultNode>();
                                        if (queryNode.CardinalPos != selectedNode.CardinalPos)
                                        {
                                            selectedNode = queryNode;
                                        }
                                    }
                                }
                                else
                                {
                                    // Move selected node around
                                    FaultMap.MoveNode(selectedNode, hit.point);
                                }
                            }
                            else if (SecondaryInput.Down)
                            {
                                DeselectNode();
                                AddDetectionLayer(LayerNames.Node);
                            }
                            else if (DelInput.Down)
                            {
                                FaultMap.RemoveNode(selectedNode);
                                DeselectNode();
                                AddDetectionLayer(LayerNames.Node);
                            }
                            break;

                        default:
                            Debug.LogWarning("Passed though undefined selection case: " + Selection);
                            break;
                    }
                    break;

                #endregion
                #region Link Creator

                case Tool.FormLinks:
                    switch (Selection)
                    {
                        case SelectionState.Nothing:
                            if (PrimaryInput.Down)
                            {
                                if (mouseTarget == TargetType.Node) SelectNode(hit.transform.GetComponent<TectonicFaultNode>());
                            }
                            break;

                        case SelectionState.SingleNode:
                            if (PrimaryInput.Down)
                            {
                                // Desel if clicked on nothing
                                if (PrimaryInput.Down && mouseTarget != TargetType.Node)
                                    DeselectNode(); 

                                if (mouseTarget == TargetType.Node)
                                {
                                    var otherNode = hit.transform.GetComponent<TectonicFaultNode>();
                                    var newLink = selectedNode.LinkTo(otherNode);
                                    FaultMap.AddLink(newLink);
                                    DeselectNode();
                                    SelectNode(otherNode);
                                }
                            }
                            break;

                        default:
                            Debug.LogWarning("Passed though undefined selection case: " + Selection);
                            break;
                    }
                    break;

                #endregion

                default:
                    break;                   
            }

        }

        #endregion
    }
}