using _Scripts.Player;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;
using System.IO;
using _Scripts.Managers;

namespace _Scripts.UI
{
    public class MainMenu : MonoBehaviour
    {
        private string _savePath;

        [Header("UI Elements")] [SerializeField]
        private GameObject mainMenuPanel, controlsPanel, quitConfirmPanel;

        [SerializeField] private TextMeshProUGUI saveNotificationText;
        [SerializeField] private float saveNotificationDuration = 2f;

        [Header("Buttons")] [SerializeField] private Button startGameButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button controlsButton;
        [SerializeField] private Button returnButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button confirmQuitButton;
        [SerializeField] private Button cancelQuitButton;

        [Header("Highlight Settings")] [SerializeField]
        private Color highlightColor = Color.green;

        [SerializeField] private Color defaultColor = Color.white;

        private InputControls _actions;

        private int _currentButtonIndex;
        private Button[] _menuButtons;
        private Button[] _controlButtons;
        private Button[] _quitButtons;
        private Button[] _currentButtons;

        private void Awake()
        {
            _savePath = SaveManager.GetSaveFilePath();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (saveNotificationText != null)
                saveNotificationText.gameObject.SetActive(false);

            if (quitConfirmPanel != null)
                quitConfirmPanel.SetActive(false);

            _actions = new InputControls();
            _actions.UI.Navigate.performed += NavigateMenu;
            _actions.UI.Submit.performed += SelectButton;

            _menuButtons = new[] { startGameButton, loadButton, controlsButton, quitButton };
            _controlButtons = new[] { returnButton };
            _quitButtons = new[] { confirmQuitButton, cancelQuitButton };
            _currentButtons = _menuButtons;

            _currentButtons = _menuButtons;
            _currentButtonIndex = 0;
            UpdateButtonSelection();
        }

        private void OnEnable()
        {
            _actions.UI.Enable();
            AssignButtonActions();
        }

        private void OnDisable()
        {
            _actions.UI.Disable();
            RemoveButtonActions();
        }

        private static void StartGame()
        {
            Debug.Log("▶️ Starting New Game...");
            SaveManager.ResetGame();
            SceneManager.LoadScene("Irani");
        }

        private void LoadGame()
        {
            if (!File.Exists(_savePath))
            {
                Debug.Log("no saved game");
                StartCoroutine(ShowSaveNotification());
                return;
            }

            var json = File.ReadAllText(_savePath);
            var data = JsonUtility.FromJson<SaveData>(json);

            SceneManager.LoadScene(data.savedLevel);
        }

        private IEnumerator ShowSaveNotification()
        {
            if (saveNotificationText is null) yield break;

            saveNotificationText.gameObject.SetActive(true);
            saveNotificationText.alpha = 1f;

            yield return new WaitForSecondsRealtime(saveNotificationDuration);

            const float fadeDuration = 1f;
            var elapsedTime = 0f;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                saveNotificationText.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
                yield return null;
            }

            saveNotificationText.gameObject.SetActive(false);
        }


        private void OpenControls()
        {
            mainMenuPanel.SetActive(false);
            controlsPanel.SetActive(true);

            _currentButtons = _controlButtons;
            _currentButtonIndex = 0;

            StartCoroutine(DelayedSelection());
        }

        private void ReturnToMainMenu()
        {
            Debug.Log("↩️ Returning to Main Menu...");
            controlsPanel.SetActive(false);
            mainMenuPanel.SetActive(true);

            _currentButtons = _menuButtons;
            _currentButtonIndex = 0;

            StartCoroutine(DelayedSelection());
        }

        private IEnumerator DelayedSelection()
        {
            yield return new WaitForSecondsRealtime(0.1f);
            UpdateButtonSelection();
        }

        private void HandleQuit(string action)
        {
            switch (action)
            {
                case "open":
                    quitConfirmPanel?.SetActive(true);
                    _currentButtons = _quitButtons;
                    _currentButtonIndex = 1;
                    StartCoroutine(DelayedSelection());
                    break;

                case "confirm":
                    Debug.Log("🚪 Quitting Game to desktop...");
                    Application.Quit();
                    break;

                case "cancel":
                    quitConfirmPanel?.SetActive(false);
                    _currentButtons = _menuButtons;
                    _currentButtonIndex = 0;
                    StartCoroutine(DelayedSelection());
                    break;

                default:
                    Debug.LogError("Invalid action for HandleQuit()");
                    break;
            }
        }

        private void NavigateMenu(InputAction.CallbackContext context)
        {
            var direction = context.ReadValue<Vector2>().y;

            if (direction > 0) _currentButtonIndex--;
            else if (direction < 0) _currentButtonIndex++;

            _currentButtonIndex = Mathf.Clamp(_currentButtonIndex, 0, _currentButtons.Length - 1);
            UpdateButtonSelection();
        }

        private void SelectButton(InputAction.CallbackContext context)
        {
            EventSystem.current.SetSelectedGameObject(_currentButtons[_currentButtonIndex].gameObject);
            var selectedButton = _currentButtons[_currentButtonIndex];

            Debug.Log($"🎯 Selected: {selectedButton.name}");

            if (selectedButton == startGameButton)
            {
                StartGame();
            }
            else if (selectedButton == loadButton)
            {
                LoadGame();
            }
            else if (selectedButton == controlsButton)
            {
                OpenControls();
            }
            else if (selectedButton == returnButton)
            {
                ReturnToMainMenu();
            }
            else if (selectedButton == quitButton)
            {
                HandleQuit("open");
            }
            else if (selectedButton == confirmQuitButton)
            {
                HandleQuit("confirm");
            }
            else if (selectedButton == cancelQuitButton)
            {
                HandleQuit("cancel");
            }
        }

        private void UpdateButtonSelection()
        {
            for (var i = 0; i < _currentButtons.Length; i++)
            {
                var buttonImage = _currentButtons[i].GetComponent<Image>();
                var buttonText = _currentButtons[i].GetComponentInChildren<TextMeshProUGUI>();

                if (buttonImage is not null)
                    buttonImage.color = (i == _currentButtonIndex) ? highlightColor : defaultColor;
                if (buttonText is not null)
                    buttonText.color = (i == _currentButtonIndex) ? defaultColor : highlightColor;
            }

            _currentButtons[_currentButtonIndex].Select();
        }

        private void AssignButtonActions()
        {
            startGameButton.onClick.AddListener(StartGame);
            loadButton.onClick.AddListener(LoadGame);
            controlsButton.onClick.AddListener(OpenControls);
            returnButton.onClick.AddListener(ReturnToMainMenu);
            quitButton.onClick.AddListener(() => HandleQuit("open"));
            confirmQuitButton.onClick.AddListener(() => HandleQuit("confirm"));
            cancelQuitButton.onClick.AddListener(() => HandleQuit("cancel"));
        }

        private void RemoveButtonActions()
        {
            startGameButton.onClick.RemoveAllListeners();
            loadButton.onClick.RemoveAllListeners();
            controlsButton.onClick.RemoveAllListeners();
            returnButton.onClick.RemoveAllListeners();
            quitButton.onClick.RemoveAllListeners();
            confirmQuitButton.onClick.RemoveAllListeners();
            cancelQuitButton.onClick.RemoveAllListeners();
        }
    }
}