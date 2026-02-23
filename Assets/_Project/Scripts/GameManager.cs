using UnityEngine;

namespace KingdomTower
{
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
        /// Ends the current game session.
        /// </summary>
        public void EndGame()
        {
            isGameActive = false;
            Debug.Log("Game ended");

            // Future: Show end game screen, save score, etc.
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

        #region Victory/Defeat Conditions (Future Expansion)

        /// <summary>
        /// Checks if the player has won the level.
        /// This is a placeholder for future implementation.
        /// </summary>
        public void CheckVictoryCondition()
        {
            // Future: Check if all enemy towers are captured
            // For now, this is just a stub
        }

        /// <summary>
        /// Checks if the player has lost the level.
        /// This is a placeholder for future implementation.
        /// </summary>
        public void CheckDefeatCondition()
        {
            // Future: Check if all player towers are lost
            // For now, this is just a stub
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
