using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using _Scripts.enums;
using _Scripts.Managers;
using _Scripts.Player;
using UnityEngine.EventSystems;

namespace _Scripts.UI
{
    public class PauseMenu : MonoBehaviour
    {
        #region Variables & Dependencies

        public bool isPaused;
        private InputControls _actions;
        private int _currentButtonIndex;
        private Button[] _pauseButtons;
        private Button[] _controlButtons;
        private Button[] _quitButtons;
        private Button[] _currentButtons;

        private PlayerProfiler _playerProfiler;
        private PlayerHealth _playerHealth;
        private PistolProfiler _pistolProfiler;

        #endregion

        #region UI Elements

        [Header("Panels")] [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject controlsPanel;
        [SerializeField] private GameObject quitConfirmPanel;
        [SerializeField] private TextMeshProUGUI saveNotificationText;
        [SerializeField] private float saveNotificationDuration = 2f;

        [Header("Highlight Settings")] [SerializeField]
        private Color highlightColor = Color.green;

        [SerializeField] private Color defaultColor = Color.white;

        #endregion

        #region Buttons

        [Header("Pause Menu Buttons")] [SerializeField]
        private Button resumeButton;

        [SerializeField] private Button saveButton;
        [SerializeField] private Button controlsButton;
        [SerializeField] private Button quitButton;

        [Header("Control Panel Buttons")] [SerializeField]
        private Button returnButton;

        [Header("Quit Confirmation Buttons")] [SerializeField]
        private Button confirmQuitButton;

        [SerializeField] private Button cancelQuitButton;

        #endregion

        #region Initialization

        private void Awake()
        {
            InitializeDependencies();
            InitializeUI();
            InitializeButtonArrays();
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
            _playerProfiler = FindFirstObjectByType<PlayerProfiler>();
            _playerHealth = FindFirstObjectByType<PlayerHealth>();
            _pistolProfiler = FindFirstObjectByType<PistolProfiler>();

            _actions = new InputControls();
            _actions.UI.Pause.performed += TogglePause;
            _actions.UI.Navigate.performed += NavigateMenu;
            _actions.UI.Submit.performed += SelectButton;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void InitializeUI()
        {
            if (saveNotificationText != null) saveNotificationText.gameObject.SetActive(false);
            if (quitConfirmPanel != null) quitConfirmPanel.SetActive(false);
        }

        private void InitializeButtonArrays()
        {
            _pauseButtons = new[] { resumeButton, saveButton, controlsButton, quitButton };
            _controlButtons = new[] { returnButton };
            _quitButtons = new[] { confirmQuitButton, cancelQuitButton };
            _currentButtons = _pauseButtons;
            _currentButtonIndex = 0;
            UpdateButtonSelection();
        }

        #endregion

        # region Enablors

        private void AssignButtonActions()
        {
            resumeButton.onClick.AddListener(ResumeGame);
            saveButton.onClick.AddListener(SaveGame);
            controlsButton.onClick.AddListener(OpenControls);
            returnButton.onClick.AddListener(ReturnToPauseMenu);
            quitButton.onClick.AddListener(() => HandleQuit(ConfirmAction.Open));
            confirmQuitButton.onClick.AddListener(() => HandleQuit(ConfirmAction.Confirm));
            cancelQuitButton.onClick.AddListener(() => HandleQuit(ConfirmAction.Cancel));
        }

        private void RemoveButtonActions()
        {
            resumeButton.onClick.RemoveAllListeners();
            saveButton.onClick.RemoveAllListeners();
            controlsButton.onClick.RemoveAllListeners();
            returnButton.onClick.RemoveAllListeners();
            quitButton.onClick.RemoveAllListeners();
            confirmQuitButton.onClick.RemoveAllListeners();
            cancelQuitButton.onClick.RemoveAllListeners();
        }

        private void DisableInputs()
        {
            _actions.Profiler.Disable();
            InputSystem.ResetHaptics();
            InputSystem.DisableDevice(Keyboard.current);
            InputSystem.DisableDevice(Gamepad.current);
            InputSystem.DisableDevice(Mouse.current);

            StartCoroutine(EnableInputsAfterDelay());
        }

        private IEnumerator EnableInputsAfterDelay()
        {
            yield return new WaitForSecondsRealtime(0.5f);

            _actions.Profiler.Enable();
            InputSystem.EnableDevice(Keyboard.current);
            InputSystem.EnableDevice(Gamepad.current);
            InputSystem.EnableDevice(Mouse.current);
        }

        # endregion

        #region PauseMenu Functionality

        private void TogglePause(InputAction.CallbackContext context)
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }

        private void PauseGame()
        {
            isPaused = true;
            pausePanel.SetActive(true);
            controlsPanel.SetActive(false);
            AudioListener.pause = true;
            Time.timeScale = 0f;
            _currentButtons = _pauseButtons;
            _currentButtonIndex = 0;
            UpdateButtonSelection();
        }

        private void ResumeGame()
        {
            isPaused = false;
            pausePanel.SetActive(false);
            controlsPanel.SetActive(false);
            AudioListener.pause = false;
            Time.timeScale = 1f;
            DisableInputs();
        }

        private void SaveGame()
        {
            SaveManager.SaveGame(_playerProfiler, _playerHealth, _pistolProfiler);
            StartCoroutine(ShowSaveNotification());
        }

        private void OpenControls()
        {
            pausePanel.SetActive(false);
            controlsPanel.SetActive(true);

            _currentButtons = _controlButtons;
            _currentButtonIndex = 0;

            StartCoroutine(DelayedSelection());
        }

        private void ReturnToPauseMenu()
        {
            Debug.Log("↩️ Returning to Pause Menu");
            controlsPanel.SetActive(false);
            pausePanel.SetActive(true);

            _currentButtons = _pauseButtons;
            _currentButtonIndex = 0;

            StartCoroutine(DelayedSelection());
        }

        private void QuitGame()
        {
            Debug.Log("🚪 Returning to Main Menu...");
            AudioListener.pause = false;
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");

            StartCoroutine(EnableInputsAfterDelay());
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

        #region UI Navigation & Selection

        private void NavigateMenu(InputAction.CallbackContext context)
        {
            if (!isPaused) return;

            var direction = context.ReadValue<Vector2>().y;

            if (direction > 0) _currentButtonIndex--;
            else if (direction < 0) _currentButtonIndex++;

            _currentButtonIndex = Mathf.Clamp(_currentButtonIndex, 0, _currentButtons.Length - 1);
            UpdateButtonSelection();
        }

        private void SelectButton(InputAction.CallbackContext context)
        {
            if (!isPaused) return;

            EventSystem.current.SetSelectedGameObject(_currentButtons[_currentButtonIndex].gameObject);
            var selectedButton = _currentButtons[_currentButtonIndex];

            Debug.Log($"🎯 Selected: {selectedButton.name}");

            // Dictionary mapping buttons to their respective actions
            var buttonActions = new Dictionary<Button, Action>
            {
                { resumeButton, ResumeGame },
                { saveButton, SaveGame },
                { controlsButton, OpenControls },
                { returnButton, ReturnToPauseMenu },
                { quitButton, () => HandleQuit(ConfirmAction.Open) },
                { confirmQuitButton, () => HandleQuit(ConfirmAction.Confirm) },
                { cancelQuitButton, () => HandleQuit(ConfirmAction.Cancel) }
            };

            // Execute action if button is in dictionary
            if (buttonActions.TryGetValue(selectedButton, out var action))
            {
                action.Invoke();
            }
        }

        private IEnumerator DelayedSelection()
        {
            yield return new WaitForSecondsRealtime(0.1f);
            UpdateButtonSelection();
        }

        private void UpdateButtonSelection()
        {
            if (SceneManager.GetActiveScene().buildIndex == 0) return; 
            
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

        #region Quit Actions

        private void HandleQuit(ConfirmAction action)
        {
            switch (action)
            {
                case ConfirmAction.Open:
                    QuitHelper(true, _quitButtons, 1);
                    break;
                case ConfirmAction.Confirm:
                    QuitGame();
                    break;
                case ConfirmAction.Cancel:
                    QuitHelper(false, _pauseButtons, 0);
                    break;
                default:
                    Debug.LogError("Invalid action for HandleQuit()");
                    break;
            }
        }

        private void QuitHelper(bool active, Button[] buttons, int index)
        {
            quitConfirmPanel?.SetActive(active);
            _currentButtons = buttons;
            _currentButtonIndex = index;
            StartCoroutine(DelayedSelection());
        }

        #endregion
    }
}