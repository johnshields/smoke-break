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
        [Header("UI Elements")] [SerializeField]
        private GameObject mainMenuPanel, controlsPanel;

        [SerializeField] private TextMeshProUGUI saveNotificationText;
        [SerializeField] private float saveNotificationDuration = 2f;

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
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (saveNotificationText != null)
                saveNotificationText.gameObject.SetActive(false);

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

        private static void StartGame()
        {
            Debug.Log("▶️ Starting New Game...");
            SaveManager.ResetGame();
            SceneManager.LoadScene("Irani");
        }

        private void LoadGame()
        {
            var playerId = SaveManager.GetOrCreatePlayerId();
            var savePath = Path.Combine(SaveManager.SaveDirectory, $"savegame_{playerId}.json");

            if (!File.Exists(savePath))
            {
                Debug.Log("no saved game");
                StartCoroutine(ShowSaveNotification());
                return;
            }

            var json = File.ReadAllText(savePath);
            var data = JsonUtility.FromJson<SaveData>(json);

            SceneManager.LoadScene(data.savedLevel);
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