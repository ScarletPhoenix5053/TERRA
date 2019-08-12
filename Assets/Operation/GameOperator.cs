using UnityEngine;
using System.Collections.Generic;

namespace SCARLET.TERRA
{
    public class GameOperator : MonoBehaviour
    {
        #region SINGLETON

        public static GameOperator Instance;
        private void ValidateSingleton()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(this);
        }

        #endregion

        #region TOOL SYSTEM

        public void SetActiveTool(string toolName)
        {
            if (System.Enum.TryParse(toolName, ignoreCase: true, out ToolType newTool))
            {
                SetActiveTool(newTool);
            }
        }
        public void SetActiveTool(ToolType newTool)
        {
            ActiveTool = newTool;
        }
        public ToolType ActiveTool = ToolType.None;
        private ToolAction[] ToolActions = new ToolAction[]
        {
            TS_RunNodeEditor,
            TS_RunLineEditor,
        };

        private void RunActiveTool()
        {
            if (ActiveTool == ToolType.None) return;        // Do nothing if no active tool

            ToolActions[(int)ActiveTool]();
        }

        #endregion

        #region OBJECT DETECTION

        [Header("Object Detection")]
        public float DetectionRadius = 0.5f;

        private static TectonicFaultLine lastDetectedLink;

        /*
        private static List<string> DetectionLayerNames_Default = new List<string>()
        {
            LayerNames.DetectionLayer
        };
        public List<string> DetectionLayerNames = DetectionLayerNames_Default;
        */

        public LayerMask DetectionLayers;
        public LayerMask InteractbleLayers;

        /*
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
        */

        private Collider[] sphereOverlapResults = new Collider[3];
        
        private enum TargetType
        {
            Invalid,
            DetectionObj,
            Node,
            Line,
            Plate
        }
        private struct TargetData
        {
            internal TargetType Type;
            internal Transform Transform;
            internal Vector3 HitPosition;
        }
        private TargetData GetMouseTarget()
        {
            TargetData targetData = new TargetData();

            // Use a physics raycast to check for a detection object
            var screenToPoint = Camera.main.ScreenPointToRay(Input.mousePosition);
            var castHits = Physics.Raycast(screenToPoint, out RaycastHit hit, Mathf.Infinity, DetectionLayers);

            List<Transform> targetTransforms = new List<Transform>();

            if (castHits) // Look for objects of interest
            {
                // Track hit pos
                targetData.HitPosition = hit.point;

                // Physics check
                int physicsHits = Physics.OverlapSphereNonAlloc(hit.point, DetectionRadius, sphereOverlapResults, InteractbleLayers);
                if (physicsHits > 0)
                {
                    foreach (Collider overlap in sphereOverlapResults)
                    {
                        if (overlap != null)
                            targetTransforms.Add(overlap.transform);
                    }
                }

                // Custom fault line check
                var links = TectonicSimulation.FaultMap.FaultLines;
                var selectionSphere = new Sphere(hit.point, DetectionRadius);
                foreach (TectonicFaultLine link in links)
                {
                    var linkLine = new Line(link.NodeA.CardinalPos, link.NodeB.CardinalPos);
                    if (Geometry.LineIntersectsSphere(linkLine, selectionSphere))
                    {
                        targetTransforms.Add(link.transform);
                    }
                }


                // Determine closest and apply data to targetData
                if (targetTransforms.Count == 1) 
                {
                    targetData.Transform = targetTransforms[0];
                    targetData.Type = GetTargetTypeOf(targetData.Transform);
                }
                else if (targetTransforms.Count > 1)
                {
                    Transform closest = targetTransforms[0];
                    float closestDist = Vector3.Distance(hit.point, targetTransforms[0].position);
                    for (int i = 1; i < targetTransforms.Count; i++)
                    {
                        closest =
                            (Vector3.Distance(hit.point, targetTransforms[i].position) < closestDist) 
                            ? targetTransforms[i] 
                            : closest;
                    }
                    targetData.Transform = closest;
                    targetData.Type = GetTargetTypeOf(closest);
                }

                // If no target could be found, Use detection object as target
                if (targetTransforms.Count == 0 || targetData.Type == TargetType.Invalid)
                {
                    targetData.Transform = hit.transform;
                    targetData.Type = TargetType.DetectionObj;
                }

            }
            else // Didn't find anything
            {
                targetData.Type = TargetType.Invalid;
            }

            return targetData;
        }
        private TargetType GetTargetTypeOf(Transform transform)
        {
            if (transform.tag == Tags.TectonicNode)             return TargetType.Node;
            else if (transform.tag == Tags.TectonicLink)        return TargetType.Line;

            else                                                return TargetType.Invalid;
        }

        #endregion


        #region OPERATIONS

        #region Tectonic Simulation

        [Header("Tectonics")]
        public TectonicSimulation tectonicSimulation;
        private static TectonicSimulation TectonicSimulation => Instance.tectonicSimulation;

        private static TargetData targetData;
        
        private static void TS_RunNodeEditor()
        {
            var selection = TectonicSimulation.Selection;
            switch (selection)
            {
                #region Case: Nothing Seleted

                case TERRA.TectonicSimulation.SelectionState.Nothing:
                    if (InputOperator.PrimaryInput.Down)
                    {
                        if (targetData.Type == TargetType.Node)
                        {
                            TectonicSimulation.SelectNode(targetData.Transform.GetComponent<TectonicFaultNode>());
                        }
                        else if (targetData.Type == TargetType.DetectionObj)
                        {
                            TectonicSimulation.FaultMap.AddNode(targetData.HitPosition);
                        }
                    }
                    break;

                #endregion
                #region Case: One Node Selected

                case TERRA.TectonicSimulation.SelectionState.SingleNode:
                    if (InputOperator.PrimaryInput.Held)
                    {
                        if (InputOperator.PrimaryInput.Down)
                        {
                            // Adjust selected node if a new node is clicked on
                            if (targetData.Type != TargetType.Node)
                            {
                                TectonicSimulation.DeselectNode();
                            }
                            // Select new node if hit another node is clicked on
                            else
                            {
                                var queryNode = targetData.Transform.GetComponent<TectonicFaultNode>();
                                if (queryNode.CardinalPos !=TectonicSimulation.SelectedNode.CardinalPos)
                                {
                                   TectonicSimulation.DeselectNode();
                                   TectonicSimulation.SelectNode(queryNode);
                                }
                            }
                        }
                        else
                        {
                            // Move selected node around
                           TectonicSimulation.FaultMap.MoveNode(TectonicSimulation.SelectedNode, targetData.HitPosition);
                        }
                    }
                    else if (InputOperator.SecondaryInput.Down)
                    {
                       TectonicSimulation.DeselectNode();
                    }
                    else if (InputOperator.DelInput.Down)
                    {
                       TectonicSimulation.FaultMap.RemoveNode(TectonicSimulation.SelectedNode);
                       TectonicSimulation.DeselectNode();
                    }
                    break;

                #endregion

                default:
                    Debug.LogWarning(("Passed though undefined selection case: " + selection));
                    break;
            }
        }
        private static void TS_RunLineEditor()
        {
            var selection = TectonicSimulation.Selection;
            switch (selection)
            {
                #region CASE: Nothing Selected

                case TERRA.TectonicSimulation.SelectionState.Nothing:
                    if (InputOperator.PrimaryInput.Down)
                    {
                        if (targetData.Type == TargetType.Node)TectonicSimulation.SelectNode(targetData.Transform.GetComponent<TectonicFaultNode>());
                        else if (targetData.Type == TargetType.Line)TectonicSimulation.SelectLink(targetData.Transform.GetComponent<TectonicFaultLine>());
                    }
                    break;

                #endregion
                #region CASE: One Node Selected

                case TERRA.TectonicSimulation.SelectionState.SingleNode:
                    if (InputOperator.PrimaryInput.Down)
                    {
                        // Desel if clicked on nothing
                        if (InputOperator.PrimaryInput.Down && targetData.Type != TargetType.Node)
                           TectonicSimulation.DeselectNode();

                        if (targetData.Type == TargetType.Node)
                        {
                            var otherNode = targetData.Transform.GetComponent<TectonicFaultNode>();
                            var newLink =TectonicSimulation.SelectedNode.LinkTo(otherNode);
                           TectonicSimulation.FaultMap.AddLink((TectonicFaultLine)newLink);
                           TectonicSimulation.DeselectNode();
                           TectonicSimulation.SelectNode(otherNode);
                        }
                    }
                    else if (InputOperator.SecondaryInput.Down)
                    {
                       TectonicSimulation.DeselectNode();
                       TectonicSimulation.DeselectLink();
                    }
                    break;

                #endregion
                #region CASE : One Link Selected

                case TERRA.TectonicSimulation.SelectionState.SingleLink:
                    if (InputOperator.PrimaryInput.Down || InputOperator.SecondaryInput.Down)
                    {
                        // Desel
                        if (InputOperator.PrimaryInput.Down && targetData.Type != TargetType.Node)
                           TectonicSimulation.DeselectLink();
                    }
                    else if (InputOperator.SecondaryInput.Down)
                    {
                       TectonicSimulation.DeselectLink();
                    }
                    else if (InputOperator.DelInput.Down)
                    {
                       TectonicSimulation.FaultMap.RemoveLink(TectonicSimulation.SelectedLink);
                       TectonicSimulation.DeselectLink();
                    }
                    break;

                #endregion

                default:
                    Debug.LogWarning(("Passed though undefined selection case: " + selection));
                    break;
            }
        }

        #endregion
        
        #endregion

        private void Awake()
        {
            ValidateSingleton();
        }

        private void Update()
        {
            targetData = GetMouseTarget();
            RunActiveTool();
        }
    }

    public enum ToolType
    {
        None = -1,

        TS_NodeEditor = 0,
        TS_FaultEditor,
        TS_PlateEditor
    }
    public delegate void ToolAction();
}