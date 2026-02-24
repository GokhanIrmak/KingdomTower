using System;
using UnityEngine;

namespace KingdomTower
{
    /// <summary>
    /// Represents the outcome of a finished game.
    /// </summary>
    public enum GameResult
    {
        Victory,  // Player (BlueTeam) captured all towers
        Defeat    // Enemy (RedTeam) captured all towers
    }

    /// <summary>
    /// Main game manager that handles overall game state using the singleton pattern.
    /// For the MVP, this manages basic game flow (play/pause) and can be extended for level management.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        // Singleton instance
        private static GameManager instance;
        public static GameManager Instance
        {
            get
            {
                if (instance == null)
                {
                    Debug.LogError("GameManager instance is null! Ensure a GameManager exists in the scene.");
                }
                return instance;
            }
        }

        [Header("Game State")]
        [SerializeField] private bool isGameActive = true;

        [Header("Victory/Defeat")]
        [SerializeField] private float endCheckDelay = 0.5f; // Small delay to let state settle

        // Event fired when game ends — UI and other systems can subscribe
        public event Action<GameResult> OnGameEnded;

        // Properties
        public bool IsGameActive => isGameActive;

        #region Unity Lifecycle

        private void Awake()
        {
            // Implement singleton pattern
            if (instance != null && instance != this)
            {
                Debug.LogWarning("Multiple GameManager instances detected! Destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            instance = this;

            // Optional: Persist across scenes (uncomment if needed for multi-scene game)
            // DontDestroyOnLoad(gameObject);

            Initialize();
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        #endregion

        #region Initialization

        private void Initialize()
        {
            Debug.Log("GameManager initialized");

            // Additional initialization logic can go here
            // For example: loading saved data, setting up game systems, etc.
        }

        #endregion

        #region Game State Control

        /// <summary>
        /// Starts or resumes the game.
        /// </summary>
        public void StartGame()
        {
            isGameActive = true;
            Time.timeScale = 1f;
            Debug.Log("Game started");
        }

        /// <summary>
        /// Pauses the game.
        /// </summary>
        public void PauseGame()
        {
            isGameActive = false;
            Time.timeScale = 0f;
            Debug.Log("Game paused");
        }

        /// <summary>
        /// Ends the current game session (manual / external call).
        /// </summary>
        public void EndGame()
        {
            if (!isGameActive) return;

            isGameActive = false;
            Debug.Log("Game ended (manual)");
        }

        /// <summary>
        /// Restarts the current level.
        /// </summary>
        public void RestartLevel()
        {
            Debug.Log("Restarting level...");

            // Reload the current scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
            );
        }

        #endregion

        #region Victory/Defeat Conditions

        /// <summary>
        /// Called by TowerController when a tower changes ownership.
        /// Schedules a game-end check after a short delay so state can settle.
        /// </summary>
        public void CheckGameEndCondition()
        {
            if (!isGameActive) return;

            // Use Invoke to avoid checking mid-frame during cascading ownership changes
            CancelInvoke(nameof(EvaluateGameEnd));
            Invoke(nameof(EvaluateGameEnd), endCheckDelay);
        }

        private void EvaluateGameEnd()
        {
            if (!isGameActive) return;

            TowerController[] allTowers = FindObjectsOfType<TowerController>();

            if (allTowers.Length == 0) return;

            // Check if all towers belong to a single non-neutral team
            Team? dominantTeam = null;

            foreach (var tower in allTowers)
            {
                // If any tower is still neutral, no winner yet
                if (tower.CurrentTeam == Team.Neutral)
                    return;

                if (dominantTeam == null)
                {
                    dominantTeam = tower.CurrentTeam;
                }
                else if (tower.CurrentTeam != dominantTeam)
                {
                    // Multiple teams still alive — game continues
                    return;
                }
            }

            // All towers belong to one team
            GameResult result = dominantTeam == Team.BlueTeam
                ? GameResult.Victory
                : GameResult.Defeat;

            EndGame(result);
        }

        /// <summary>
        /// Ends the game with the given result.
        /// </summary>
        private void EndGame(GameResult result)
        {
            isGameActive = false;

            Debug.Log($"Game Over — {result}!");

            OnGameEnded?.Invoke(result);
        }

        #endregion

        #region Debug Methods

        [ContextMenu("Start Game")]
        private void DebugStartGame()
        {
            StartGame();
        }

        [ContextMenu("Pause Game")]
        private void DebugPauseGame()
        {
            PauseGame();
        }

        [ContextMenu("Restart Level")]
        private void DebugRestartLevel()
        {
            RestartLevel();
        }

        #endregion
    }
}
