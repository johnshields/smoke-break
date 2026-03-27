using System;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Scripts._Systems.Managers
{
    public class ScreenshotTaker : MonoBehaviour
    {
        private int _screenshotCount;
        private const int Quality = 2;

        private void Update()
        {
            if (Keyboard.current.pKey.wasPressedThisFrame)
            {
                SaveScreenshot();
            }
        }

        private void SaveScreenshot()
        {
            var picturesFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            string screenshotFolder = Path.Combine(picturesFolder, "smoke-break_pics");

            // Ensure the folder exists
            if (!Directory.Exists(screenshotFolder))
                Directory.CreateDirectory(screenshotFolder);

            string filename = $"screenshot_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png";
            string fullPath = Path.Combine(screenshotFolder, filename);

            ScreenCapture.CaptureScreenshot(fullPath, Quality);
            print("Screenshot saved: " + fullPath);
        }
    }
}