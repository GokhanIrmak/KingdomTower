using UnityEngine;
using TMPro;

namespace KingdomTower
{
    /// <summary>
    /// Tower state based on HP level.
    /// </summary>
    public enum TowerState
    {
        Destroyed,  // 0 HP
        Repairing,  // 1-4 HP
        Level1,     // 5-9 HP
        Level2,     // 10-19 HP
        Level3      // 20+ HP
    }

    /// <summary>
    /// Controls an individual tower's behavior including unit generation,
    /// health management, team ownership, and visual representation.
    /// </summary>
    [RequireComponent(typeof(MeshRenderer))]
    public class TowerController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private TowerStatsSO stats;
        [SerializeField] private Team startingTeam = Team.Neutral;

        [Header("Unit Prefab")]
        [SerializeField] private GameObject unitPrefab;

        [Header("Visual Settings")]
        [SerializeField] private Color neutralColor = Color.gray;
        [SerializeField] private Color blueTeamColor = Color.blue;
        [SerializeField] private Color redTeamColor = Color.red;

        [Header("Level Scaling")]
        [SerializeField] private float level1Scale = 1.0f;
        [SerializeField] private float level2Scale = 1.3f;
        [SerializeField] private float level3Scale = 1.6f;
        [SerializeField] private float scaleTransitionSpeed = 2f;

        [Header("HP Display")]
        [SerializeField] private TextMeshPro hpText;
        [SerializeField] private Vector3 hpTextOffset = new Vector3(0, 2.5f, 0);

        [Header("HP Limits")]
        [SerializeField] private int maxHP = 30;

        [Header("HP Regeneration")]
        [SerializeField] private float idleRegenInterval = 3f; // 3 seconds per HP when idle

        // Current state
        private Team currentTeam;
        private int currentHealth;
        private float unitGenerationTimer;
        private MeshRenderer meshRenderer;
        private TowerState currentState;
        private Vector3 targetScale = Vector3.one;
        private int activeOutgoingConnections = 0;

        // Properties
        public Team CurrentTeam => currentTeam;
        public int CurrentHealth => currentHealth;
        public bool CanSendUnits => currentTeam != Team.Neutral;
        public bool HasOutgoingConnections => activeOutgoingConnections > 0;

        #region Unity Lifecycle

        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            ValidateConfiguration();
        }

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            if (currentTeam != Team.Neutral)
            {
                GenerateUnits();
            }

            // Smooth scale transition
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleTransitionSpeed);
        }

        #endregion

        #region Initialization

        private void Initialize()
        {
            currentTeam = startingTeam;
            currentHealth = stats.StartingHealth;
            unitGenerationTimer = 0f;

            CreateHPText();
            UpdateVisuals();
            UpdateHPDisplay();
            UpdateTowerState();
        }

        private void CreateHPText()
        {
            if (hpText != null) return;

            // Create HP text dynamically if not assigned
            GameObject textObj = new GameObject("HPText");
            textObj.transform.SetParent(transform);
            textObj.transform.localPosition = hpTextOffset;

            hpText = textObj.AddComponent<TextMeshPro>();
            hpText.alignment = TextAlignmentOptions.Center;
            hpText.fontSize = 8;
            hpText.fontStyle = FontStyles.Bold;

            // Make text always face camera
            textObj.AddComponent<LookAtCamera>();
        }

        private void ValidateConfiguration()
        {
            if (stats == null)
            {
                Debug.LogError($"TowerController on {gameObject.name} is missing TowerStatsSO reference!", this);
            }

            if (unitPrefab == null)
            {
                Debug.LogWarning($"TowerController on {gameObject.name} is missing unit prefab reference!", this);
            }
        }

        #endregion

        #region Unit Generation

        private void GenerateUnits()
        {
            // No HP regen if tower has outgoing connections (attacking)
            if (activeOutgoingConnections > 0)
            {
                return;
            }

            // Max HP reached
            if (currentHealth >= maxHP)
            {
                return;
            }

            unitGenerationTimer += Time.deltaTime;

            // Idle regen: 1 HP per idleRegenInterval seconds (default 3 sec)
            if (unitGenerationTimer >= idleRegenInterval)
            {
                currentHealth++;
                unitGenerationTimer = 0f;
                UpdateHPDisplay();
                UpdateTowerState();
            }
        }

        #endregion

        #region Connection Management

        /// <summary>
        /// Called by InputManager when a connection is created from this tower.
        /// </summary>
        public void AddOutgoingConnection()
        {
            activeOutgoingConnections++;
        }

        /// <summary>
        /// Called by InputManager when a connection from this tower is removed.
        /// </summary>
        public void RemoveOutgoingConnection()
        {
            activeOutgoingConnections = Mathf.Max(0, activeOutgoingConnections - 1);
        }

        #endregion

        #region Tower State Management

        /// <summary>
        /// Returns the tower state based on current HP.
        /// </summary>
        private TowerState GetTowerStateFromHP()
        {
            if (currentHealth <= 0) return TowerState.Destroyed;
            if (currentHealth <= 4) return TowerState.Repairing;
            if (currentHealth <= 9) return TowerState.Level1;
            if (currentHealth <= 19) return TowerState.Level2;
            return TowerState.Level3;
        }

        /// <summary>
        /// Updates the tower state and triggers visual changes if state changed.
        /// </summary>
        private void UpdateTowerState()
        {
            TowerState newState = GetTowerStateFromHP();

            if (newState != currentState)
            {
                currentState = newState;
                UpdateStateVisuals();
            }
        }

        /// <summary>
        /// Updates visual appearance based on current state.
        /// </summary>
        private void UpdateStateVisuals()
        {
            switch (currentState)
            {
                case TowerState.Destroyed:
                    targetScale = Vector3.one * 0.8f;
                    break;
                case TowerState.Repairing:
                    targetScale = Vector3.one * 0.9f;
                    break;
                case TowerState.Level1:
                    targetScale = Vector3.one * level1Scale;
                    break;
                case TowerState.Level2:
                    targetScale = Vector3.one * level2Scale;
                    break;
                case TowerState.Level3:
                    targetScale = Vector3.one * level3Scale;
                    break;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds HP to this tower (used when capturing or reinforcing).
        /// </summary>
        public void AddUnits(int amount)
        {
            currentHealth = Mathf.Min(currentHealth + amount, stats.MaxUnits);
            UpdateHPDisplay();
            UpdateTowerState();
        }

        /// <summary>
        /// Applies damage to the tower from an attacking team.
        /// If health reaches zero, ownership changes to the attacker.
        /// </summary>
        public void TakeDamage(int amount, Team attackerTeam)
        {
            if (attackerTeam == Team.Neutral)
            {
                Debug.LogWarning("Neutral team cannot attack towers!");
                return;
            }

            // If same team, add HP (up to max)
            if (attackerTeam == currentTeam)
            {
                currentHealth = Mathf.Min(currentHealth + amount, maxHP);
                UpdateHPDisplay();
                UpdateTowerState();
                return;
            }

            currentHealth -= amount;

            // Check if tower should change ownership
            if (currentHealth <= 0)
            {
                ChangeOwnership(attackerTeam);
            }

            UpdateVisuals();
            UpdateHPDisplay();
            UpdateTowerState();
        }

        private void UpdateHPDisplay()
        {
            if (hpText == null) return;

            hpText.text = currentHealth.ToString();

            // Color based on team
            hpText.color = currentTeam switch
            {
                Team.BlueTeam => blueTeamColor,
                Team.RedTeam => redTeamColor,
                Team.Neutral => neutralColor,
                _ => Color.white
            };
        }

        /// <summary>
        /// Sends units from this tower to attack a target tower.
        /// Called by the InputManager when drag-and-drop is completed.
        /// </summary>
        public void SendUnitsToTower(TowerController targetTower)
        {
            if (!CanSendUnits)
            {
                Debug.LogWarning($"Tower {gameObject.name} cannot send units!");
                return;
            }

            if (targetTower == null || targetTower == this)
            {
                Debug.LogWarning("Invalid target tower!");
                return;
            }

            // Calculate how many units to send
            int unitsToSend = Mathf.FloorToInt(currentHealth * stats.UnitSendPercentage);
            unitsToSend = Mathf.Max(1, unitsToSend); // Send at least 1 unit

            if (unitsToSend > currentHealth)
            {
                unitsToSend = currentHealth;
            }

            // Deduct from HP
            currentHealth -= unitsToSend;
            UpdateHPDisplay();
            UpdateTowerState();

            // Spawn the units and send them to the target
            SpawnUnits(unitsToSend, targetTower);
        }

        /// <summary>
        /// Sends a single unit to target tower. Used by TowerConnection for continuous flow.
        /// Units are unlimited - HP only changes when unit arrives at target.
        /// </summary>
        public void SendSingleUnit(TowerController targetTower)
        {
            if (targetTower == null || targetTower == this)
                return;

            // Units are unlimited - just spawn and send
            // HP only changes when unit arrives at destination (TakeDamage)
            SpawnUnits(1, targetTower);
        }

        #endregion

        #region Unit Spawning

        private void SpawnUnits(int count, TowerController targetTower)
        {
            if (unitPrefab == null)
            {
                Debug.LogError($"Cannot spawn units: unitPrefab is not assigned on {gameObject.name}!");
                return;
            }

            for (int i = 0; i < count; i++)
            {
                // Spawn with slight offset to prevent units stacking
                Vector3 randomOffset = new Vector3(
                    Random.Range(-0.5f, 0.5f),
                    0f,
                    Random.Range(-0.5f, 0.5f)
                );

                Vector3 spawnPosition = transform.position + randomOffset + Vector3.up * 0.5f;
                GameObject unitObj = Instantiate(unitPrefab, spawnPosition, Quaternion.identity);

                // Initialize the unit
                UnitController unit = unitObj.GetComponent<UnitController>();
                if (unit != null)
                {
                    unit.Initialize(targetTower, currentTeam);
                }
                else
                {
                    Debug.LogError($"Unit prefab is missing UnitController component!");
                    Destroy(unitObj);
                }
            }
        }

        #endregion

        #region Ownership & Visuals

        private void ChangeOwnership(Team newTeam)
        {
            currentTeam = newTeam;

            // Reset health to a small positive value when captured
            currentHealth = Mathf.Abs(currentHealth);
            if (currentHealth == 0)
            {
                currentHealth = 1;
            }

            UpdateVisuals();
            UpdateTowerState();

            Debug.Log($"{gameObject.name} captured by {newTeam}!");
        }

        private void UpdateVisuals()
        {
            if (meshRenderer == null) return;

            Color targetColor = currentTeam switch
            {
                Team.Neutral => neutralColor,
                Team.BlueTeam => blueTeamColor,
                Team.RedTeam => redTeamColor,
                _ => Color.white
            };

            // Create a new material instance to avoid affecting the shared material
            if (meshRenderer.material != null)
            {
                meshRenderer.material.color = targetColor;
            }
        }

        #endregion

        #region Debug Visualization

        private void OnDrawGizmosSelected()
        {
            // Draw a sphere showing the tower's range (optional for future features)
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 1f);
        }

        #endregion
    }
}
