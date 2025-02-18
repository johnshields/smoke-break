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
        [Header("UI Panels")] [SerializeField] private GameObject pauseMenuUI;
        [SerializeField] private GameObject controlsPanel;

        [Header("Buttons")] [SerializeField] private Button resumeButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button controlsButton;
        [SerializeField] private Button returnButton;
        [SerializeField] private Button quitButton;

        [Header("Highlight Settings")] [SerializeField]
        private Color highlightColor = Color.green;

        [SerializeField] private Color defaultColor = Color.white;

        private InputControls _actions;
        private bool _isPaused = false;
        private int _currentButtonIndex = 0;
        private Button[] _pauseButtons;
        private Button[] _controlButtons;
        private Button[] _currentButtons;

        private void Awake()
        {
            _actions = new InputControls();
            _actions.UI.Pause.performed += TogglePause;
            _actions.UI.Navigate.performed += NavigateMenu;
            _actions.UI.Submit.performed += SelectButton;

            _pauseButtons = new Button[] { resumeButton, saveButton, controlsButton, quitButton };
            _controlButtons = new Button[] { returnButton };
            _currentButtons = _pauseButtons;
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

        private void TogglePause(InputAction.CallbackContext context)
        {
            if (_isPaused) ResumeGame();
            else PauseGame();
        }

        private void PauseGame()
        {
            _isPaused = true;
            pauseMenuUI.SetActive(true);
            controlsPanel.SetActive(false);
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            _currentButtons = _pauseButtons;
            _currentButtonIndex = 0;
            UpdateButtonSelection();
        }

        private void ResumeGame()
        {
            _isPaused = false;
            pauseMenuUI.SetActive(false);
            controlsPanel.SetActive(false);
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OpenControls()
        {
            pauseMenuUI.SetActive(false);
            controlsPanel.SetActive(true);

            _currentButtons = _controlButtons;
            _currentButtonIndex = 0;

            StartCoroutine(DelayedSelection());
        }

        private void ReturnToPauseMenu()
        {
            Debug.Log("↩️ Returning to Pause Menu");
            controlsPanel.SetActive(false);
            pauseMenuUI.SetActive(true);

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
            Debug.Log("💾 SaveGame() Called!");
            //Debug.Log("💾 Game Saved!");
            SaveManager.SaveGame(FindObjectOfType<PlayerProfiler>(), FindObjectOfType<PistolProfiler>());
        }

        private void QuitGame()
        {
            Debug.Log("🚪 Returning to Main Menu...");
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }

        private void NavigateMenu(InputAction.CallbackContext context)
        {
            if (!_isPaused) return;

            float direction = context.ReadValue<Vector2>().y;
            if (direction > 0) _currentButtonIndex--;
            else if (direction < 0) _currentButtonIndex++;

            _currentButtonIndex = Mathf.Clamp(_currentButtonIndex, 0, _currentButtons.Length - 1);
            UpdateButtonSelection();
        }

        private void SelectButton(InputAction.CallbackContext context)
        {
            if (!_isPaused) return;

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