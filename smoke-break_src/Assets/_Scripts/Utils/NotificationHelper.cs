using System.Collections;
using TMPro;
using UnityEngine;

namespace _Scripts.Utils
{
    // Shared notification fade coroutine used by PauseMenu and MainMenu.
    public static class NotificationHelper
    {
        public static IEnumerator ShowThenFade(TextMeshProUGUI text, float displayDuration, float fadeDuration = 1f)
        {
            if (text == null) yield break;

            text.gameObject.SetActive(true);
            text.alpha = 1f;

            yield return new WaitForSecondsRealtime(displayDuration);

            var elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                text.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
                yield return null;
            }

            text.gameObject.SetActive(false);
        }
    }
}
