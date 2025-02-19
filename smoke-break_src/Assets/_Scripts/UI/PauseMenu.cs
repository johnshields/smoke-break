using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using _Scripts.Player;
using UnityEngine.EventSystems;

namespace _Scripts.UI
{
    public class PauseMenu : MonoBehaviour
    {
        [Header("UI Panels")] [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject controlsPanel;
        [SerializeField] private TextMeshProUGUI saveNotificationText;
        [SerializeField] private float saveNotificationDuration = 2f;

        [Header("Buttons")] [SerializeField] private Button resumeButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button controlsButton;
        [SerializeField] private Button returnButton;
        [SerializeField] private Button quitButton;

        [Header("Highlight Settings")] [SerializeField]
        private Color highlightColor = Color.green;

        [SerializeField] private Color defaultColor = Color.white;

        private InputControls _actions;
        public bool isPaused = false;
        private int _currentButtonIndex = 0;
        private Button[] _pauseButtons;
        private Button[] _controlButtons;
        private Button[] _currentButtons;

        private void Awake()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            _actions = new InputControls();
            _actions.UI.Pause.performed += TogglePause;
            _actions.UI.Navigate.performed += NavigateMenu;
            _actions.UI.Submit.performed += SelectButton;

            _pauseButtons = new[] { resumeButton, saveButton, controlsButton, quitButton };
            _controlButtons = new[] { returnButton };
            _currentButtons = _pauseButtons;

            if (saveNotificationText != null)
                saveNotificationText.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            _actions.UI.Enable();
            if (resumeButton != null)
                AssignButtonActions();
        }

        private void OnDisable()
        {
            _actions.UI.Disable();
            if (resumeButton != null)
                RemoveButtonActions();
        }

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

        private IEnumerator DelayedSelection()
        {
            yield return new WaitForSecondsRealtime(0.1f);
            UpdateButtonSelection();
        }

        private void SaveGame()
        {
            SaveManager.SaveGame(FindObjectOfType<PlayerProfiler>(), FindObjectOfType<PistolProfiler>());
            StartCoroutine(ShowSaveNotification());
        }

        private IEnumerator ShowSaveNotification()
        {
            if (saveNotificationText is null) yield break;

            saveNotificationText.gameObject.SetActive(true);
            saveNotificationText.alpha = 1f;

            yield return new WaitForSecondsRealtime(saveNotificationDuration);

            float fadeDuration = 1f;
            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                saveNotificationText.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
                yield return null;
            }

            saveNotificationText.gameObject.SetActive(false);
        }

        private void QuitGame()
        {
            Debug.Log("🚪 Returning to Main Menu...");
            AudioListener.pause = false;
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");

            StartCoroutine(EnableInputsAfterDelay());
        }

        private void NavigateMenu(InputAction.CallbackContext context)
        {
            if (!isPaused) return;

            float direction = context.ReadValue<Vector2>().y;
            if (direction > 0) _currentButtonIndex--;
            else if (direction < 0) _currentButtonIndex++;

            _currentButtonIndex = Mathf.Clamp(_currentButtonIndex, 0, _currentButtons.Length - 1);
            UpdateButtonSelection();
        }

        private void SelectButton(InputAction.CallbackContext context)
        {
            if (!isPaused) return;

            EventSystem.current.SetSelectedGameObject(_currentButtons[_currentButtonIndex].gameObject);
            Button selectedButton = _currentButtons[_currentButtonIndex];

            Debug.Log($"🎯 Selected: {selectedButton.name}");

            if (selectedButton == saveButton)
            {
                SaveGame();
            }
            else if (selectedButton == resumeButton)
            {
                ResumeGame();
            }
            else if (selectedButton == controlsButton)
            {
                OpenControls();
            }
            else if (selectedButton == returnButton)
            {
                ReturnToPauseMenu();
            }
            else if (selectedButton == quitButton)
            {
                QuitGame();
            }
        }


        private void UpdateButtonSelection()
        {
            for (int i = 0; i < _currentButtons.Length; i++)
            {
                Image buttonImage = _currentButtons[i].GetComponent<Image>();
                TextMeshProUGUI buttonText = _currentButtons[i].GetComponentInChildren<TextMeshProUGUI>();

                if (buttonImage is not null)
                    buttonImage.color = (i == _currentButtonIndex) ? highlightColor : defaultColor;
                if (buttonText is not null)
                    buttonText.color = (i == _currentButtonIndex) ? defaultColor : highlightColor;
            }

            _currentButtons[_currentButtonIndex].Select();
        }

        private void AssignButtonActions()
        {
            resumeButton.onClick.AddListener(ResumeGame);
            saveButton.onClick.AddListener(SaveGame);
            controlsButton.onClick.AddListener(OpenControls);
            returnButton.onClick.AddListener(ReturnToPauseMenu);
            quitButton.onClick.AddListener(QuitGame);
        }

        private void RemoveButtonActions()
        {
            resumeButton.onClick.RemoveAllListeners();
            saveButton.onClick.RemoveAllListeners();
            controlsButton.onClick.RemoveAllListeners();
            returnButton.onClick.RemoveAllListeners();
            quitButton.onClick.RemoveAllListeners();
        }
    }
}