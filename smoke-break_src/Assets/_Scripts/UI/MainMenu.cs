using System;
using _Scripts.Player;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using _Scripts.Managers;

namespace _Scripts.UI
{
    public class MainMenu : MonoBehaviour
    {
        #region Variables & Dependencies

        private string _savePath;
        private PlayerProfiler _playerProfiler;
        private PlayerHealth _playerHealth;
        private PistolProfiler _pistolProfiler;
        private InputControls _actions;

        private int _currentButtonIndex;
        private Button[] _currentButtons;

        private readonly Dictionary<GameObject, ConfirmType> _confirmTypeMap = new();

        #endregion

        #region UI Elements

        [Header("Panels")] [SerializeField] private GameObject menuPanel;
        [SerializeField] private GameObject controlsPanel;
        [SerializeField] private GameObject quitPanel;
        [SerializeField] private GameObject newGamePanel;

        [Header("Notification Settings")] [SerializeField]
        private TextMeshProUGUI saveNotificationText;

        [SerializeField] private float saveNotificationDuration = 2f;

        [Header("Highlight Settings")] [SerializeField]
        private Color highlightColor = Color.green;

        [SerializeField] private Color defaultColor = Color.white;

        #endregion

        #region Buttons

        [Header("Main Menu Buttons")] [SerializeField]
        private Button startGameButton;

        [SerializeField] private Button loadButton;
        [SerializeField] private Button controlsButton;
        [SerializeField] private Button quitButton;

        [Header("Control Panel Buttons")] [SerializeField]
        private Button returnButton;

        [Header("Quit Confirmation Buttons")] [SerializeField]
        private Button confirmQuitButton;

        [SerializeField] private Button cancelQuitButton;

        [Header("New Game Confirmation Buttons")] [SerializeField]
        private Button confirmNewGameButton;

        [SerializeField] private Button cancelNewGameButton;

        private Button[] _menuButtons;
        private Button[] _controlButtons;
        private Button[] _quitButtons;
        private Button[] _newGameButtons;

        #endregion

        #region Initialization

        private void Awake()
        {
            InitializeDependencies();
            InitializeUI();
            InitializeButtonArrays();
            InitializeConfirmTypeMap();
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

        private void InitializeDependencies()
        {
            _savePath = SaveManager.GetSaveFilePath();
            _playerProfiler = FindObjectOfType<PlayerProfiler>();
            _playerHealth = FindObjectOfType<PlayerHealth>();
            _pistolProfiler = FindObjectOfType<PistolProfiler>();

            _actions = new InputControls();
            _actions.UI.Navigate.performed += NavigateMenu;
            _actions.UI.Submit.performed += SelectButton;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void InitializeUI()
        {
            if (saveNotificationText != null) saveNotificationText.gameObject.SetActive(false);
            if (quitPanel != null) quitPanel.SetActive(false);
            if (newGamePanel != null) newGamePanel.SetActive(false);
        }

        private void InitializeButtonArrays()
        {
            _menuButtons = new[] { startGameButton, loadButton, controlsButton, quitButton };
            _controlButtons = new[] { returnButton };
            _quitButtons = new[] { confirmQuitButton, cancelQuitButton };
            _newGameButtons = new[] { confirmNewGameButton, cancelNewGameButton };

            _currentButtons = _menuButtons;
            _currentButtonIndex = 0;
            UpdateButtonSelection();
        }

        private void InitializeConfirmTypeMap()
        {
            _confirmTypeMap[newGamePanel] = ConfirmType.NewGame;
            _confirmTypeMap[quitPanel] = ConfirmType.Quit;
        }

        private void AssignButtonActions()
        {
            startGameButton.onClick.AddListener(StartGame);
            confirmNewGameButton.onClick.AddListener(() =>
                HandleConfirm(ConfirmAction.Confirm, newGamePanel, _newGameButtons));
            cancelNewGameButton.onClick.AddListener(() =>
                HandleConfirm(ConfirmAction.Cancel, newGamePanel, _newGameButtons));
            loadButton.onClick.AddListener(LoadGame);
            controlsButton.onClick.AddListener(OpenControls);
            returnButton.onClick.AddListener(ReturnToMainMenu);
            quitButton.onClick.AddListener(() => HandleConfirm(ConfirmAction.Open, quitPanel, _quitButtons));
            confirmQuitButton.onClick.AddListener(() => HandleConfirm(ConfirmAction.Confirm, quitPanel, _quitButtons));
            cancelQuitButton.onClick.AddListener(() => HandleConfirm(ConfirmAction.Cancel, quitPanel, _quitButtons));
        }

        private void RemoveButtonActions()
        {
            startGameButton.onClick.RemoveAllListeners();
            confirmNewGameButton.onClick.RemoveAllListeners();
            cancelNewGameButton.onClick.RemoveAllListeners();
            loadButton.onClick.RemoveAllListeners();
            controlsButton.onClick.RemoveAllListeners();
            returnButton.onClick.RemoveAllListeners();
            quitButton.onClick.RemoveAllListeners();
            confirmQuitButton.onClick.RemoveAllListeners();
            cancelQuitButton.onClick.RemoveAllListeners();
        }

        #endregion

        #region Menu Navigation & Selection

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

            // Dictionary mapping buttons to their respective actions
            var buttonActions = new Dictionary<Button, Action>
            {
                { startGameButton, StartGame },
                {
                    confirmNewGameButton,
                    () => HandleConfirm(ConfirmAction.Confirm, newGamePanel, _newGameButtons)
                },
                {
                    cancelNewGameButton,
                    () => HandleConfirm(ConfirmAction.Cancel, newGamePanel, _newGameButtons)
                },
                { loadButton, LoadGame },
                { controlsButton, OpenControls },
                { returnButton, ReturnToMainMenu },
                { quitButton, () => HandleConfirm(ConfirmAction.Open, quitPanel, _quitButtons) },
                { confirmQuitButton, () => HandleConfirm(ConfirmAction.Confirm, quitPanel, _quitButtons) },
                { cancelQuitButton, () => HandleConfirm(ConfirmAction.Cancel, quitPanel, _quitButtons) }
            };

            // Execute action if button is in dictionary
            if (buttonActions.TryGetValue(selectedButton, out var action))
            {
                action.Invoke();
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

        #endregion

        #region Panel Management

        private void OpenPanel(GameObject panel, Button[] buttons)
        {
            panel?.SetActive(true);
            _currentButtons = buttons;
            _currentButtonIndex = 1;
            StartCoroutine(DelayedSelection());
        }

        private void ClosePanel(GameObject panel)
        {
            panel?.SetActive(false);
            _currentButtons = _menuButtons;
            _currentButtonIndex = 0;
            StartCoroutine(DelayedSelection());
        }

        private void OpenControls()
        {
            menuPanel.SetActive(false);
            controlsPanel.SetActive(true);

            _currentButtons = _controlButtons;
            _currentButtonIndex = 0;

            StartCoroutine(DelayedSelection());
        }

        private void ReturnToMainMenu()
        {
            Debug.Log("↩️ Returning to Main Menu...");
            controlsPanel.SetActive(false);
            menuPanel.SetActive(true);

            _currentButtons = _menuButtons;
            _currentButtonIndex = 0;

            StartCoroutine(DelayedSelection());
        }

        private IEnumerator DelayedSelection()
        {
            yield return new WaitForSecondsRealtime(0.1f);
            UpdateButtonSelection();
        }

        #endregion

        #region Game Actions

        private void StartGame()
        {
            Debug.Log("▶️ Starting New Game...");

            if (!File.Exists(_savePath))
            {
                SaveManager.ResetGame();
                SceneManager.LoadScene("Irani");
            }
            else
            {
                HandleConfirm(ConfirmAction.Open, newGamePanel, _newGameButtons);
            }
        }

        private void LoadGame()
        {
            if (!File.Exists(_savePath))
            {
                Debug.Log("no saved game");
                StartCoroutine(ShowSaveNotification());
            }
            else
            {
                SaveManager.LoadGame(_playerProfiler, _playerHealth, _pistolProfiler);
            }
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

        #endregion

        #region Confirm Actions

        private void HandleConfirm(ConfirmAction action, GameObject panel, Button[] buttons)
        {
            var confirmType = _confirmTypeMap.GetValueOrDefault(panel, ConfirmType.None);

            switch (action)
            {
                case ConfirmAction.Open:
                    OpenPanel(panel, buttons);
                    break;

                case ConfirmAction.Confirm:
                    ConfirmActionHandler(confirmType);
                    break;

                case ConfirmAction.Cancel:
                    ClosePanel(panel);
                    break;

                default:
                    Debug.LogError($"Invalid action for {nameof(HandleConfirm)}");
                    break;
            }
        }

        private void ConfirmActionHandler(ConfirmType confirmType)
        {
            switch (confirmType)
            {
                case ConfirmType.Quit:
                    Debug.Log("🚪 Quitting Game to desktop...");
                    Application.Quit();
                    break;

                case ConfirmType.NewGame:
                    SaveManager.ResetGame();
                    SceneManager.LoadScene("Irani");
                    break;

                case ConfirmType.None:
                default:
                    Debug.LogError("Unhandled or unknown confirmation type.");
                    break;
            }
        }

        #endregion
    }
}