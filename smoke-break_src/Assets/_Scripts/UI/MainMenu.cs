using _Scripts.Player;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;

namespace _Scripts.UI
{
    public class MainMenu : MonoBehaviour
    {
        [Header("UI Elements")] [SerializeField]
        private GameObject mainMenuPanel, controlsPanel;

        [Header("Buttons")] [SerializeField] private Button startGameButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button controlsButton;
        [SerializeField] private Button returnButton;
        [SerializeField] private Button quitButton;

        [Header("Highlight Settings")] [SerializeField]
        private Color highlightColor = Color.green;

        [SerializeField] private Color defaultColor = Color.white;

        private InputControls _actions;

        private int _currentButtonIndex = 0;
        private Button[] _menuButtons;
        private Button[] _controlButtons;
        private Button[] _currentButtons;

        private void Awake()
        {
            _actions = new InputControls();
            _actions.UI.Navigate.performed += NavigateMenu;
            _actions.UI.Submit.performed += SelectButton;

            _menuButtons = new Button[] { startGameButton, loadButton, controlsButton, quitButton };
            _controlButtons = new Button[] { returnButton };
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

        private void StartGame()
        {
            Debug.Log("▶️ Starting New Game...");
            SaveManager.ResetGame();
            SceneManager.LoadScene("SampleScene");
        }

        private void LoadGame()
        {
            Debug.Log("▶️ Loading Game...");

            if (!PlayerPrefs.HasKey("SavedLevel"))
            {
                SceneManager.LoadScene("SampleScene");
                return;
            }

            string savedLevel = PlayerPrefs.GetString("SavedLevel");

            SaveManager.LoadGame(FindObjectOfType<PlayerProfiler>(), FindObjectOfType<PistolProfiler>());
            SceneManager.LoadScene(savedLevel);
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

        private void QuitGame()
        {
            Debug.Log("🚪 Quitting Game...");
            Application.Quit();
        }

        private void NavigateMenu(InputAction.CallbackContext context)
        {
            float direction = context.ReadValue<Vector2>().y;
            if (direction > 0) _currentButtonIndex--;
            else if (direction < 0) _currentButtonIndex++;

            _currentButtonIndex = Mathf.Clamp(_currentButtonIndex, 0, _currentButtons.Length - 1);
            UpdateButtonSelection();
        }

        private void SelectButton(InputAction.CallbackContext context)
        {
            EventSystem.current.SetSelectedGameObject(_currentButtons[_currentButtonIndex].gameObject);
            Button selectedButton = _currentButtons[_currentButtonIndex];

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
            startGameButton.onClick.AddListener(StartGame);
            loadButton.onClick.AddListener(LoadGame);
            controlsButton.onClick.AddListener(OpenControls);
            returnButton.onClick.AddListener(ReturnToMainMenu);
            quitButton.onClick.AddListener(QuitGame);
        }

        private void RemoveButtonActions()
        {
            startGameButton.onClick.RemoveAllListeners();
            loadButton.onClick.RemoveAllListeners();
            controlsButton.onClick.RemoveAllListeners();
            returnButton.onClick.RemoveAllListeners();
            quitButton.onClick.RemoveAllListeners();
        }
    }
}