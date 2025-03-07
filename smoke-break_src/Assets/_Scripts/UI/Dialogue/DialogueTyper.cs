using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace _Scripts.UI.Dialogue
{
    public class DialogueTyper : MonoBehaviour
    {
        private bool _complete;
        private string _currentText = "";
        public TextMeshProUGUI messageText;
        private readonly Dictionary<string, string[]> _dialogues = new();
        private string[] _currentLines;
        
        [Header("Audio Settings")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip dialogueSound;
        
        public float fadeDuration = 1.5f;

        private void Start()
        {
            if (messageText == null)
            {
                Debug.LogError("Message Text is not assigned in the Inspector!", this);
            }
        }

        public void WriteDialogue(DialogueKey dialogueKey)
        {
            if (_complete) return;
            messageText.gameObject.SetActive(true);
            LoadDialogueFromJson($"dialogue/dialogues");
            StartTypeWriter(dialogueKey);
        }

        private void LoadDialogueFromJson(string filePath)
        {
            var jsonFile = Resources.Load<TextAsset>(filePath);
            if (jsonFile != null)
            {
                var data = JsonUtility.FromJson<DialogueData>(jsonFile.text);
                if (data?.dialogues != null)
                {
                    _dialogues.Clear();
                    foreach (var entry in data.dialogues)
                    {
                        _dialogues[entry.key] = entry.lines;
                    }

                    Debug.Log("Dialogue JSON Loaded Successfully!");
                }
                else
                {
                    Debug.LogError("Dialogue JSON structure is incorrect or empty.");
                }
            }
            else
            {
                Debug.LogError($"JSON file not found at Resources/{filePath}.json");
            }
        }

        private void StartTypeWriter(DialogueKey dialogueKey)
        {
            var keyString = dialogueKey.ToString();
            
            if (_dialogues.TryGetValue(keyString, out var dialogue))
            {
                _currentLines = dialogue;
                StartCoroutine(WriteTextByLine());
            }
            else
            {
                Debug.LogError($"Dialogue key '{keyString}' not found in JSON.");
            }
        }

        private IEnumerator WriteTextByLine()
        {
            _complete = false;
            messageText.text = "";

            foreach (var line in _currentLines)
            {
                messageText.text = ""; // Clear text for the new line
                var words = line.Split(' ');
                
                // Play dialogue sound when a new line appears
                if (audioSource is not null && dialogueSound is not null)
                {
                    audioSource.clip = dialogueSound;
                    audioSource.loop = true; // Loop the sound while typing
                    audioSource.Play();
                }

                for (var i = 0; i < words.Length; i++)
                {
                    if (i > 0)
                        _currentText += " ";

                    _currentText += words[i];
                    messageText.text = _currentText;
                    yield return new WaitForSeconds(0.3f); // Adjust delay per word
                }
                
                // Stop sound once the line is fully displayed
                if (audioSource is not null && audioSource.isPlaying)
                    audioSource.Stop();

                _currentText = ""; // Reset for the next line
                yield return new WaitForSeconds(1f); // Pause before next line
            }

            _complete = true;
            yield return new WaitForSeconds(1f);
            StartCoroutine(FadeOutText());
        }
        
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
    }
}
