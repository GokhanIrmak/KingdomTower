using UnityEngine;

namespace KingdomTower
{
    /// <summary>
    /// Manages a persistent connection between two towers.
    /// Supports bidirectional flow when both towers send units to each other.
    /// Uses a single line with gradient colors for bidirectional connections.
    /// </summary>
    public class TowerConnection : MonoBehaviour
    {
        [Header("Flow Settings")]
        [SerializeField] private float unitSendInterval = 0.5f;

        [Header("Visual Settings")]
        [SerializeField] private float lineWidth = 0.15f;

        // Connection state
        private TowerController towerA;
        private TowerController towerB;
        private LineRenderer lineRenderer;
        private bool isActive;

        // Bidirectional flow state
        private bool flowAtoB;
        private bool flowBtoA;
        private float sendTimerAtoB;
        private float sendTimerBtoA;

        // Properties
        public TowerController TowerA => towerA;
        public TowerController TowerB => towerB;
        public bool IsActive => isActive;
        public bool IsBidirectional => flowAtoB && flowBtoA;

        #region Initialization

        public void Initialize(TowerController source, TowerController target, float sendRateMultiplier = 1f)
        {
            towerA = source;
            towerB = target;
            isActive = true;

            // Apply the multiplier to send interval (higher multiplier = slower sending)
            unitSendInterval = 0.5f * sendRateMultiplier;

            // Initial flow direction
            flowAtoB = true;
            flowBtoA = false;
            sendTimerAtoB = 0f;
            sendTimerBtoA = 0f;

            SetupLineRenderer();
            UpdateLinePositions();
            UpdateLineColor();
        }

        private void SetupLineRenderer()
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.positionCount = 2;
        }

        #endregion

        #region Unity Lifecycle

        private void Update()
        {
            if (!isActive) return;

            // Check if connection is still valid
            if (!IsConnectionValid())
            {
                Deactivate();
                return;
            }

            UpdateLinePositions();
            UpdateLineColor();
            HandleUnitFlow();
        }

        #endregion

        #region Connection Logic

        private bool IsConnectionValid()
        {
            if (towerA == null || towerB == null)
                return false;

            // Connection invalid if no flow in either direction
            if (!flowAtoB && !flowBtoA)
                return false;

            // Check A to B flow validity
            if (flowAtoB)
            {
                // Only stop if source tower becomes neutral
                if (towerA.CurrentTeam == Team.Neutral)
                {
                    flowAtoB = false;
                    towerA.RemoveOutgoingConnection();
                }
            }

            // Check B to A flow validity
            if (flowBtoA)
            {
                // Only stop if source tower becomes neutral
                if (towerB.CurrentTeam == Team.Neutral)
                {
                    flowBtoA = false;
                    towerB.RemoveOutgoingConnection();
                }
            }

            return flowAtoB || flowBtoA;
        }

        private void HandleUnitFlow()
        {
            // Handle A to B flow (unlimited units)
            if (flowAtoB)
            {
                sendTimerAtoB += Time.deltaTime;
                if (sendTimerAtoB >= unitSendInterval)
                {
                    sendTimerAtoB = 0f;
                    towerA.SendSingleUnit(towerB);
                }
            }

            // Handle B to A flow (unlimited units)
            if (flowBtoA)
            {
                sendTimerBtoA += Time.deltaTime;
                if (sendTimerBtoA >= unitSendInterval)
                {
                    sendTimerBtoA = 0f;
                    towerB.SendSingleUnit(towerA);
                }
            }
        }

        #endregion

        #region Visuals

        private void UpdateLinePositions()
        {
            if (lineRenderer == null || towerA == null || towerB == null)
                return;

            lineRenderer.SetPosition(0, towerA.transform.position);
            lineRenderer.SetPosition(1, towerB.transform.position);
        }

        private void UpdateLineColor()
        {
            if (lineRenderer == null) return;

            Color colorA = GetTeamColor(towerA != null ? towerA.CurrentTeam : Team.Neutral);
            Color colorB = GetTeamColor(towerB != null ? towerB.CurrentTeam : Team.Neutral);

            if (IsBidirectional)
            {
                // Gradient from A's color to B's color
                lineRenderer.startColor = colorA;
                lineRenderer.endColor = colorB;
            }
            else if (flowAtoB)
            {
                // Solid color of A
                lineRenderer.startColor = colorA;
                lineRenderer.endColor = colorA;
            }
            else if (flowBtoA)
            {
                // Solid color of B
                lineRenderer.startColor = colorB;
                lineRenderer.endColor = colorB;
            }
        }

        private Color GetTeamColor(Team team)
        {
            return team switch
            {
                Team.BlueTeam => Color.cyan,
                Team.RedTeam => Color.red,
                _ => Color.white
            };
        }

        #endregion

        #region Public Methods

        public void Deactivate(bool notifyTowers = true)
        {
            isActive = false;

            // Notify towers about remaining flows being removed
            if (notifyTowers)
            {
                if (flowAtoB && towerA != null)
                {
                    towerA.RemoveOutgoingConnection();
                    flowAtoB = false;
                }
                if (flowBtoA && towerB != null)
                {
                    towerB.RemoveOutgoingConnection();
                    flowBtoA = false;
                }
            }

            if (lineRenderer != null)
            {
                lineRenderer.enabled = false;
            }

            Destroy(gameObject, 0.1f);
        }

        /// <summary>
        /// Check if this connection involves both towers (in either direction)
        /// </summary>
        public bool InvolvesTowers(TowerController a, TowerController b)
        {
            return (towerA == a && towerB == b) || (towerA == b && towerB == a);
        }

        /// <summary>
        /// Add reverse flow direction (makes connection bidirectional)
        /// </summary>
        public void AddReverseFlow(TowerController source)
        {
            if (source == towerA && !flowAtoB)
            {
                flowAtoB = true;
                sendTimerAtoB = 0f;
                Debug.Log($"Added A->B flow to connection");
            }
            else if (source == towerB && !flowBtoA)
            {
                flowBtoA = true;
                sendTimerBtoA = 0f;
                Debug.Log($"Added B->A flow to connection");
            }

            UpdateLineColor();
        }

        /// <summary>
        /// Check if flow exists from source
        /// </summary>
        public bool HasFlowFrom(TowerController source)
        {
            if (source == towerA) return flowAtoB;
            if (source == towerB) return flowBtoA;
            return false;
        }

        #endregion
    }
}
