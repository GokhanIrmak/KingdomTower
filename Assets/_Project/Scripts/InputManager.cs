using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KingdomTower
{
    /// <summary>
    /// Handles all player input for the drag-and-drop tower attack mechanic.
    /// Uses the new Input System for better cross-platform support.
    /// Creates persistent connections between towers for continuous unit flow.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        [Header("Raycast Settings")]
        [SerializeField] private LayerMask towerLayerMask = -1; // Everything by default
        [SerializeField] private float maxRaycastDistance = 100f;

        [Header("Visual Feedback")]
        [SerializeField] private LineRenderer dragLineRenderer;
        [SerializeField] private Color validDragColor = Color.green;
        [SerializeField] private Color invalidDragColor = Color.red;

        [Header("Line Cutting")]
        [SerializeField] private float cutDetectionRadius = 0.5f;
        [SerializeField] private Color cutTrailColor = Color.yellow;
        [SerializeField] private float cutTrailTime = 0.3f;
        [SerializeField] private float cutTrailWidth = 0.15f;

        // Input state
        private TowerController sourceTower;
        private bool isDragging;
        private bool isCuttingMode;
        private Vector3 lastCutPosition;
        private Camera mainCamera;
        private TrailRenderer cutTrail;

        // Input System references
        private bool wasPressed;
        private bool isPressed;

        // Active connections
        private List<TowerConnection> activeConnections = new List<TowerConnection>();

        #region Unity Lifecycle

        private void Awake()
        {
            mainCamera = Camera.main;

            if (mainCamera == null)
            {
                Debug.LogError("InputManager requires a MainCamera in the scene!");
            }

            SetupLineRenderer();
            SetupCutTrail();
        }

        private void Update()
        {
            HandleInput();
        }

        #endregion

        #region Input Handling

        private void HandleInput()
        {
            // Handle mouse/touch input using new Input System
            wasPressed = isPressed;

            // Check for mouse or touch input
            bool hasTouch = Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed;
            bool hasMouse = Mouse.current != null && Mouse.current.leftButton.isPressed;
            isPressed = hasTouch || hasMouse;

            bool inputDown = isPressed && !wasPressed;
            bool inputHeld = isPressed;
            bool inputUp = !isPressed && wasPressed;

            // Get input position from touch or mouse
            Vector3 inputPosition = GetInputPosition();

            // Start drag
            if (inputDown)
            {
                OnInputDown(inputPosition);
            }

            // Update drag
            if (inputHeld && isDragging)
            {
                OnInputDrag(inputPosition);
            }

            // Update cutting
            if (inputHeld && isCuttingMode)
            {
                OnCuttingDrag(inputPosition);
            }

            // End drag
            if (inputUp && isDragging)
            {
                OnInputUp(inputPosition);
            }

            // End cutting
            if (inputUp && isCuttingMode)
            {
                isCuttingMode = false;

                // Stop trail
                if (cutTrail != null)
                {
                    cutTrail.emitting = false;
                }
            }
        }

        private void OnInputDown(Vector3 screenPosition)
        {
            TowerController tower = GetTowerAtScreenPosition(screenPosition);

            if (tower != null && tower.CanSendUnits)
            {
                // Start dragging from this tower
                sourceTower = tower;
                isDragging = true;
                isCuttingMode = false;

                if (dragLineRenderer != null)
                {
                    dragLineRenderer.enabled = true;
                    dragLineRenderer.positionCount = 2;
                    dragLineRenderer.SetPosition(0, sourceTower.transform.position);
                    dragLineRenderer.SetPosition(1, sourceTower.transform.position);
                }

                Debug.Log($"Started dragging from {tower.gameObject.name}");
            }
            else
            {
                // No tower clicked - start cutting mode
                isCuttingMode = true;
                isDragging = false;
                lastCutPosition = GetWorldPositionFromScreen(screenPosition);

                // Start trail
                if (cutTrail != null)
                {
                    cutTrail.transform.position = lastCutPosition + Vector3.up * 0.5f;
                    cutTrail.Clear();
                    cutTrail.emitting = true;
                }
            }
        }

        private void OnInputDrag(Vector3 screenPosition)
        {
            if (dragLineRenderer != null && sourceTower != null)
            {
                // Update the drag line to follow the cursor
                Vector3 worldPosition = GetWorldPositionFromScreen(screenPosition);

                dragLineRenderer.SetPosition(0, sourceTower.transform.position);
                dragLineRenderer.SetPosition(1, worldPosition);

                // Check if hovering over a valid target
                TowerController targetTower = GetTowerAtScreenPosition(screenPosition);
                bool isValidTarget = targetTower != null && targetTower != sourceTower;

                // Update line color based on validity
                dragLineRenderer.startColor = isValidTarget ? validDragColor : invalidDragColor;
                dragLineRenderer.endColor = isValidTarget ? validDragColor : invalidDragColor;
            }
        }

        private void OnCuttingDrag(Vector3 screenPosition)
        {
            Vector3 currentCutPosition = GetWorldPositionFromScreen(screenPosition);

            // Update trail position
            if (cutTrail != null)
            {
                cutTrail.transform.position = currentCutPosition + Vector3.up * 0.5f;
            }

            // Check all active connections for intersection
            activeConnections.RemoveAll(c => c == null || !c.IsActive);

            for (int i = activeConnections.Count - 1; i >= 0; i--)
            {
                TowerConnection conn = activeConnections[i];
                if (conn == null) continue;

                // Check if cut line intersects with connection
                if (LineIntersectsConnection(lastCutPosition, currentCutPosition, conn))
                {
                    Debug.Log($"Cut connection between {conn.TowerA.name} and {conn.TowerB.name}");

                    // Notify towers about removed connections
                    if (conn.HasFlowFrom(conn.TowerA))
                        conn.TowerA.RemoveOutgoingConnection();
                    if (conn.HasFlowFrom(conn.TowerB))
                        conn.TowerB.RemoveOutgoingConnection();

                    conn.Deactivate(notifyTowers: false);
                    activeConnections.RemoveAt(i);
                }
            }

            lastCutPosition = currentCutPosition;
        }

        private bool LineIntersectsConnection(Vector3 cutStart, Vector3 cutEnd, TowerConnection connection)
        {
            if (connection.TowerA == null || connection.TowerB == null)
                return false;

            Vector3 lineStart = connection.TowerA.transform.position;
            Vector3 lineEnd = connection.TowerB.transform.position;

            // Use 2D intersection (ignore Y axis)
            Vector2 p1 = new Vector2(cutStart.x, cutStart.z);
            Vector2 p2 = new Vector2(cutEnd.x, cutEnd.z);
            Vector2 p3 = new Vector2(lineStart.x, lineStart.z);
            Vector2 p4 = new Vector2(lineEnd.x, lineEnd.z);

            return LinesIntersect2D(p1, p2, p3, p4);
        }

        private bool LinesIntersect2D(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            float d1 = Direction(p3, p4, p1);
            float d2 = Direction(p3, p4, p2);
            float d3 = Direction(p1, p2, p3);
            float d4 = Direction(p1, p2, p4);

            if (((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) &&
                ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0)))
            {
                return true;
            }

            return false;
        }

        private float Direction(Vector2 a, Vector2 b, Vector2 c)
        {
            return (c.x - a.x) * (b.y - a.y) - (b.x - a.x) * (c.y - a.y);
        }

        private void OnInputUp(Vector3 screenPosition)
        {
            TowerController targetTower = GetTowerAtScreenPosition(screenPosition);

            // Validate the target
            if (targetTower != null && targetTower != sourceTower)
            {
                // Check connection limit
                int maxConnections = GetMaxConnectionsForTower(sourceTower);
                int currentConnections = CountActiveConnectionsFrom(sourceTower);

                if (currentConnections >= maxConnections)
                {
                    Debug.Log($"Tower {sourceTower.gameObject.name} has reached max connections ({maxConnections})");
                    ResetDragState();
                    return;
                }

                // Create persistent connection instead of one-time send
                CreateConnection(sourceTower, targetTower);
                Debug.Log($"Created connection from {sourceTower.gameObject.name} to {targetTower.gameObject.name}");
            }
            else
            {
                Debug.Log("Invalid target - drag cancelled");
            }

            // Reset drag state
            ResetDragState();
        }

        private void CreateConnection(TowerController source, TowerController target)
        {
            // Clean up null/inactive connections first
            activeConnections.RemoveAll(c => c == null || !c.IsActive);

            // Check if connection already exists between these two towers
            foreach (var conn in activeConnections)
            {
                if (conn != null && conn.InvolvesTowers(source, target))
                {
                    // Connection exists - check if we need to add reverse flow
                    if (conn.HasFlowFrom(source))
                    {
                        Debug.Log("Flow already exists from this source!");
                        return;
                    }
                    else
                    {
                        // Add reverse flow to make it bidirectional
                        conn.AddReverseFlow(source);
                        source.AddOutgoingConnection(); // Notify tower
                        Debug.Log("Made connection bidirectional!");
                        return;
                    }
                }
            }

            // Calculate send rate multiplier based on connection count
            int connectionCount = CountActiveConnectionsFrom(source) + 1; // +1 for the new one
            float sendMultiplier = 1.0f;
            if (connectionCount == 2)
                sendMultiplier = 1.15f; // 15% slower
            else if (connectionCount >= 3)
                sendMultiplier = 1.35f; // 35% slower

            // Create new connection
            GameObject connectionObj = new GameObject($"Connection_{source.name}_{target.name}");
            TowerConnection connection = connectionObj.AddComponent<TowerConnection>();

            connection.Initialize(source, target, sendMultiplier);
            activeConnections.Add(connection);

            // Notify tower about outgoing connection
            source.AddOutgoingConnection();
        }

        private Vector3 GetInputPosition()
        {
            // Prioritize touch input for mobile
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                return Touchscreen.current.primaryTouch.position.ReadValue();
            }

            // Fall back to mouse for desktop
            if (Mouse.current != null)
            {
                return Mouse.current.position.ReadValue();
            }

            return Vector3.zero;
        }

        #endregion

        #region Connection Limits

        /// <summary>
        /// Returns the maximum number of connections a tower can have based on its HP.
        /// </summary>
        private int GetMaxConnectionsForTower(TowerController tower)
        {
            int hp = tower.CurrentHealth;
            if (hp <= 10) return 1;
            if (hp <= 30) return 2;
            return 3;
        }

        /// <summary>
        /// Counts the number of active connections from a specific tower.
        /// </summary>
        private int CountActiveConnectionsFrom(TowerController tower)
        {
            int count = 0;
            foreach (var conn in activeConnections)
            {
                if (conn != null && conn.IsActive && conn.HasFlowFrom(tower))
                {
                    count++;
                }
            }
            return count;
        }

        #endregion

        #region Raycasting

        private TowerController GetTowerAtScreenPosition(Vector3 screenPosition)
        {
            Ray ray = mainCamera.ScreenPointToRay(screenPosition);

            if (Physics.Raycast(ray, out RaycastHit hit, maxRaycastDistance, towerLayerMask))
            {
                return hit.collider.GetComponent<TowerController>();
            }

            return null;
        }

        private Vector3 GetWorldPositionFromScreen(Vector3 screenPosition)
        {
            // Project screen position onto a horizontal plane at y=0
            Ray ray = mainCamera.ScreenPointToRay(screenPosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

            if (groundPlane.Raycast(ray, out float distance))
            {
                return ray.GetPoint(distance);
            }

            return Vector3.zero;
        }

        #endregion

        #region Visual Feedback

        private void SetupLineRenderer()
        {
            if (dragLineRenderer == null)
            {
                // Create a LineRenderer if one wasn't assigned
                GameObject lineObj = new GameObject("DragLine");
                lineObj.transform.SetParent(transform);
                dragLineRenderer = lineObj.AddComponent<LineRenderer>();

                // Configure the line renderer
                dragLineRenderer.startWidth = 0.1f;
                dragLineRenderer.endWidth = 0.1f;
                dragLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                dragLineRenderer.startColor = validDragColor;
                dragLineRenderer.endColor = validDragColor;
                dragLineRenderer.enabled = false;
            }
        }

        private void ResetDragState()
        {
            isDragging = false;
            sourceTower = null;

            if (dragLineRenderer != null)
            {
                dragLineRenderer.enabled = false;
            }
        }

        private void SetupCutTrail()
        {
            GameObject trailObj = new GameObject("CutTrail");
            trailObj.transform.SetParent(transform);

            cutTrail = trailObj.AddComponent<TrailRenderer>();
            cutTrail.time = cutTrailTime;
            cutTrail.startWidth = cutTrailWidth;
            cutTrail.endWidth = 0.01f;
            cutTrail.material = new Material(Shader.Find("Sprites/Default"));
            cutTrail.startColor = cutTrailColor;
            cutTrail.endColor = new Color(cutTrailColor.r, cutTrailColor.g, cutTrailColor.b, 0f);
            cutTrail.emitting = false;
        }

        #endregion

        #region Debug Visualization

        private void OnDrawGizmos()
        {
            if (isDragging && sourceTower != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(sourceTower.transform.position, 1f);
            }
        }

        #endregion
    }
}
