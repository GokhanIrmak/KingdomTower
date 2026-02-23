using UnityEngine;

namespace KingdomTower
{
    /// <summary>
    /// ScriptableObject that holds the base configuration data for towers.
    /// This allows designers to create different tower types without code changes.
    /// </summary>
    [CreateAssetMenu(fileName = "TowerStats", menuName = "KingdomTower/Tower Stats", order = 1)]
    public class TowerStatsSO : ScriptableObject
    {
        [Header("Unit Generation")]
        [Tooltip("Maximum number of units this tower can hold")]
        [SerializeField] private int maxUnits = 50;

        [Tooltip("Number of units generated per second")]
        [SerializeField] private float unitsPerSecond = 1f;

        [Tooltip("Starting units when the tower is initialized")]
        [SerializeField] private int startingUnits = 10;

        [Header("Combat")]
        [Tooltip("Health of the tower (also represents current unit count)")]
        [SerializeField] private int startingHealth = 10;

        [Header("Unit Spawning")]
        [Tooltip("Percentage of units to send when attacking (0-1)")]
        [SerializeField] [Range(0.1f, 1f)] private float unitSendPercentage = 0.5f;

        // Public getters
        public int MaxUnits => maxUnits;
        public float UnitsPerSecond => unitsPerSecond;
        public int StartingUnits => startingUnits;
        public int StartingHealth => startingHealth;
        public float UnitSendPercentage => unitSendPercentage;
    }
}
