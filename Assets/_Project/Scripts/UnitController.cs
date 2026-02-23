using UnityEngine;

namespace KingdomTower
{
    /// <summary>
    /// Controls a single unit's movement toward a target tower.
    /// Simple state machine: Move -> Reach Target -> Deal Damage -> Destroy Self
    /// Units can collide with enemy units on the path and destroy each other.
    /// </summary>
    public class UnitController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float arrivalThreshold = 0.5f;

        [Header("Combat Settings")]
        [SerializeField] private int damageAmount = 1;
        [SerializeField] private float collisionRadius = 0.3f;

        [Header("Visual Settings")]
        [SerializeField] private Color blueTeamColor = Color.cyan;
        [SerializeField] private Color redTeamColor = Color.red;

        // State
        private TowerController targetTower;
        private Team ownerTeam;
        private bool hasReachedTarget;
        private bool isDestroyed;
        private MeshRenderer meshRenderer;

        // Properties
        public Team OwnerTeam => ownerTeam;
        public bool IsAlive => !isDestroyed && !hasReachedTarget;

        #region Initialization

        /// <summary>
        /// Initializes the unit with its target and team affiliation.
        /// Called by TowerController when the unit is spawned.
        /// </summary>
        public void Initialize(TowerController target, Team team)
        {
            targetTower = target;
            ownerTeam = team;
            hasReachedTarget = false;

            // Cache renderer component
            meshRenderer = GetComponent<MeshRenderer>();

            // Set visual appearance based on team
            UpdateVisuals();

            // Validate initialization
            if (targetTower == null)
            {
                Debug.LogError("UnitController initialized with null target tower!");
                Destroy(gameObject);
            }
        }

        #endregion

        #region Unity Lifecycle

        private void Update()
        {
            if (isDestroyed || hasReachedTarget || targetTower == null)
            {
                return;
            }

            MoveTowardsTarget();
            CheckEnemyCollision();
            CheckArrival();
        }

        #endregion

        #region Movement

        private void MoveTowardsTarget()
        {
            Vector3 targetPosition = targetTower.transform.position;
            Vector3 currentPosition = transform.position;

            // Move towards target using simple linear interpolation
            Vector3 newPosition = Vector3.MoveTowards(
                currentPosition,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            transform.position = newPosition;

            // Rotate to face the direction of movement
            Vector3 direction = (targetPosition - currentPosition).normalized;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        private void CheckArrival()
        {
            float distanceToTarget = Vector3.Distance(
                transform.position,
                targetTower.transform.position
            );

            if (distanceToTarget <= arrivalThreshold)
            {
                OnReachTarget();
            }
        }

        #endregion

        #region Combat

        private void CheckEnemyCollision()
        {
            // Find all colliders in collision radius
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, collisionRadius);

            foreach (var hitCollider in hitColliders)
            {
                // Skip self
                if (hitCollider.gameObject == gameObject)
                    continue;

                // Check if it's another unit
                UnitController otherUnit = hitCollider.GetComponent<UnitController>();
                if (otherUnit == null)
                    continue;

                // Skip if same team or other unit is already destroyed
                if (otherUnit.OwnerTeam == ownerTeam || !otherUnit.IsAlive)
                    continue;

                // Enemy unit found - destroy both!
                otherUnit.DestroyUnit();
                DestroyUnit();
                return;
            }
        }

        public void DestroyUnit()
        {
            if (isDestroyed) return;

            isDestroyed = true;
            Destroy(gameObject);
        }

        private void OnReachTarget()
        {
            hasReachedTarget = true;

            // Deal damage to the target tower
            if (targetTower != null)
            {
                targetTower.TakeDamage(damageAmount, ownerTeam);
            }

            // Destroy this unit after dealing damage
            Destroy(gameObject);
        }

        #endregion

        #region Visuals

        private void UpdateVisuals()
        {
            if (meshRenderer == null) return;

            Color targetColor = ownerTeam switch
            {
                Team.BlueTeam => blueTeamColor,
                Team.RedTeam => redTeamColor,
                _ => Color.white
            };

            // Create material instance to avoid affecting shared material
            if (meshRenderer.material != null)
            {
                meshRenderer.material.color = targetColor;
            }
        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            // Clean up any resources if needed in the future
        }

        #endregion

        #region Debug Visualization

        private void OnDrawGizmos()
        {
            if (targetTower != null && !hasReachedTarget)
            {
                // Draw a line to show the unit's path to its target
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, targetTower.transform.position);
            }
        }

        #endregion
    }
}
