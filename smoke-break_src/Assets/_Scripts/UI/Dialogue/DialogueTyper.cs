using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Scripts.UI.Dialogue
{
    public class DialogueTyper : MonoBehaviour
    {
        #region Variables

        [Header("UI References")] [SerializeField]
        private TextMeshProUGUI messageText;

        [SerializeField] private AudioSource audioSource;

        [Header("Dialogue Settings")] [SerializeField]
        private float wordDelay = 0.05f;

        [SerializeField] private float lineDelay = 1.5f;
        [SerializeField] private float fadeDuration = 1.5f;

        [Header("Debug Settings")] [SerializeField]
        private bool enableDebugLogs;

        private readonly Dictionary<string, List<(string text, string sound)>> _dialogues = new();
        private List<(string text, string sound)> _currentLines;
        public InputAction skipAction;

        #endregion

        #region Initialization

        private void OnEnable()
        {
            skipAction.Enable();
        }

        private void OnDisable()
        {
            skipAction.Disable();
        }

        public void InitDialogue(DialogueKey dialogueKey)
        {
            if (!messageText || !audioSource)
            {
                Debug.LogError("❌ UI components (messageText or audioSource) are not assigned.");
                return;
            }

            messageText.gameObject.SetActive(true);
            if (_dialogues.Count == 0)
            {
                LoadDialogueFromJson("dialogues");
            }

            StartTypeWriter(dialogueKey);
        }

        #endregion

        #region JSON Loading

        private void LoadDialogueFromJson(string filePath)
        {
            var jsonFile = Resources.Load<TextAsset>(filePath);
            if (jsonFile == null)
            {
                LogError($"❌ JSON file not found at Resources/{filePath}.json");
                return;
            }

            var data = JsonUtility.FromJson<DialogueData>(jsonFile.text);
            if (data?.dialogues == null)
            {
                LogError("❌ Dialogue JSON structure is incorrect or empty.");
                return;
            }

            _dialogues.Clear();
            foreach (var entry in data.dialogues)
            {
                var lines = new List<(string text, string sound)>();
                foreach (var line in entry.lines)
                {
                    lines.Add((line.text, line.sound));
                }

                _dialogues[entry.key] = lines;
            }

            LogDebug($"✅ Dialogue JSON Loaded Successfully! Keys: {string.Join(", ", _dialogues.Keys)}");
        }

        #endregion

        #region Dialogue Processing

        private void StartTypeWriter(DialogueKey dialogueKey)
        {
            var keyString = dialogueKey.ToString();
            if (!_dialogues.TryGetValue(keyString, out _currentLines) || _currentLines.Count == 0)
            {
                LogError($"❌ Dialogue key '{dialogueKey}' not found or has no lines.");
                return;
            }

            StartCoroutine(WriteTextByLine());
        }

        private IEnumerator WriteTextByLine()
        {
            messageText.text = "";

            foreach (var (text, sound) in _currentLines)
            {
                messageText.text = "";
                PlayDialogueSound(sound);

                var currentText = "";
                foreach (var word in text.Split(' '))
                {
                    if (skipAction.WasPressedThisFrame())
                    {
                        audioSource.Stop();
                        StartCoroutine(FadeOutText());
                        yield break; // Immediately exit the coroutine
                    }

                    currentText += (string.IsNullOrEmpty(currentText) ? "" : " ") + word;
                    messageText.text = currentText;
                    yield return new WaitForSeconds(wordDelay);
                }

                yield return new WaitForSeconds(lineDelay);
            }

            StartCoroutine(FadeOutText());
        }

        #endregion

        #region Audio

        private void PlayDialogueSound(string soundName)
        {
            if (string.IsNullOrEmpty(soundName)) return;

            var clip = Resources.Load<AudioClip>($"dialogue/{soundName}");
            if (clip)
            {
                audioSource.clip = clip;
                audioSource.Play();
            }
            else
            {
                LogWarning($"⚠ Sound file '{soundName}' not found in Resources/Sounds/");
            }
        }

        #endregion

        #region UI Effects

        private IEnumerator FadeOutText()
        {
            var elapsedTime = 0f;
            var textColor = messageText.color;

            while (elapsedTime < fadeDuration)
            {
                textColor.a = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
                messageText.color = textColor;
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            messageText.color = new Color(textColor.r, textColor.g, textColor.b, 0f);
            messageText.text = "";
        }

        #endregion

        #region Debug Logging

        private void LogDebug(string message)
        {
            if (enableDebugLogs) Debug.Log(message);
        }

        private void LogError(string message)
        {
            Debug.LogError(message);
        }

        private void LogWarning(string message)
        {
            if (enableDebugLogs) Debug.LogWarning(message);
        }

        #endregion
    }
}