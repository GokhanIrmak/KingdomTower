using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using TMPro;

namespace KingdomTower
{
    /// <summary>
    /// Handles the in-game UI overlay. Currently shows a Victory/Defeat panel
    /// when the game ends. Creates its own Canvas at runtime for quick prototyping.
    /// </summary>
    public class GameUIController : MonoBehaviour
    {
        [Header("Colors")]
        [SerializeField] private Color victoryColor = new Color(0.2f, 0.6f, 1f);  // Blue
        [SerializeField] private Color defeatColor = new Color(1f, 0.3f, 0.3f);   // Red
        [SerializeField] private Color overlayColor = new Color(0f, 0f, 0f, 0.6f);
        [SerializeField] private Color buttonColor = new Color(1f, 1f, 1f, 0.9f);
        [SerializeField] private Color buttonTextColor = new Color(0.1f, 0.1f, 0.1f);

        // Runtime-created UI references
        private Canvas canvas;
        private GameObject endGamePanel;
        private TextMeshProUGUI resultText;
        private TextMeshProUGUI subtitleText;
        private TextMeshProUGUI speedButtonLabel;
        private GameObject speedButton;

        #region Unity Lifecycle

        private void Start()
        {
            CreateUI();

            // Subscribe to events
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameEnded += OnGameEnded;
                GameManager.Instance.OnSpeedChanged += OnSpeedChanged;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameEnded -= OnGameEnded;
                GameManager.Instance.OnSpeedChanged -= OnSpeedChanged;
            }
        }

        #endregion

        #region Event Handlers

        private void OnGameEnded(GameResult result)
        {
            bool isVictory = result == GameResult.Victory;

            resultText.text = isVictory ? "VICTORY" : "DEFEAT";
            resultText.color = isVictory ? victoryColor : defeatColor;

            subtitleText.text = isVictory
                ? "All towers captured!"
                : "Your towers have fallen...";

            endGamePanel.SetActive(true);
            speedButton.SetActive(false); // Hide speed button when game ends
        }

        private void OnSpeedChanged(float speed)
        {
            speedButtonLabel.text = $"{speed:0}x";
        }

        #endregion

        #region UI Construction

        private void CreateUI()
        {
            // --- Canvas ---
            GameObject canvasObj = new GameObject("GameUI_Canvas");
            canvasObj.transform.SetParent(transform);

            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100; // On top of everything

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // --- EventSystem (required for button clicks) ---
            if (FindObjectOfType<EventSystem>() == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.transform.SetParent(transform);
                eventSystemObj.AddComponent<EventSystem>();
                eventSystemObj.AddComponent<InputSystemUIInputModule>();
            }

            // --- Speed button (top-right corner, always visible during gameplay) ---
            CreateSpeedButton(canvasObj.transform);

            // --- End Game Panel (full-screen overlay, initially hidden) ---
            endGamePanel = CreatePanel(canvasObj.transform, "EndGamePanel", overlayColor);
            endGamePanel.SetActive(false);

            // --- Result text (VICTORY / DEFEAT) ---
            resultText = CreateText(endGamePanel.transform, "ResultText", "VICTORY",
                fontSize: 96, yPosition: 80f);
            resultText.fontStyle = FontStyles.Bold;

            // --- Subtitle text ---
            subtitleText = CreateText(endGamePanel.transform, "SubtitleText", "",
                fontSize: 36, yPosition: -20f);
            subtitleText.color = Color.white;

            // --- Restart button ---
            CreateRestartButton(endGamePanel.transform, yPosition: -120f);
        }

        private GameObject CreatePanel(Transform parent, string name, Color color)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);

            var image = panel.AddComponent<Image>();
            image.color = color;

            // Stretch to fill parent
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            return panel;
        }

        private void CreateSpeedButton(Transform parent)
        {
            speedButton = new GameObject("SpeedButton");
            speedButton.transform.SetParent(parent, false);

            var image = speedButton.AddComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.5f);

            var button = speedButton.AddComponent<Button>();
            button.onClick.AddListener(OnSpeedClicked);

            // Top-right corner
            var rect = speedButton.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
            rect.anchoredPosition = new Vector2(-20, -20);
            rect.sizeDelta = new Vector2(100, 60);

            // Label
            speedButtonLabel = CreateText(speedButton.transform, "SpeedLabel", "1x",
                fontSize: 32, yPosition: 0f);

            // Stretch label to fill button
            var labelRect = speedButtonLabel.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            labelRect.anchoredPosition = Vector2.zero;
        }

        private void OnSpeedClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CycleSpeed();
            }
        }

        private void CreateRestartButton(Transform parent, float yPosition)
        {
            // Button container
            GameObject buttonObj = new GameObject("RestartButton");
            buttonObj.transform.SetParent(parent, false);

            var image = buttonObj.AddComponent<Image>();
            image.color = buttonColor;

            var button = buttonObj.AddComponent<Button>();
            button.onClick.AddListener(OnRestartClicked);

            // Size and position
            var rect = buttonObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0, yPosition);
            rect.sizeDelta = new Vector2(300, 80);

            // Button label
            var label = CreateText(buttonObj.transform, "ButtonLabel", "RESTART",
                fontSize: 40, yPosition: 0f);
            label.color = buttonTextColor;

            // Stretch label to fill button
            var labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            labelRect.anchoredPosition = Vector2.zero;
        }

        private void OnRestartClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartLevel();
            }
        }

        private TextMeshProUGUI CreateText(Transform parent, string name, string defaultText,
            float fontSize, float yPosition)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);

            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = defaultText;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            var rect = textObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0.5f);
            rect.anchorMax = new Vector2(1, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0, yPosition);
            rect.sizeDelta = new Vector2(0, fontSize + 20);

            return tmp;
        }

        #endregion
    }
}
