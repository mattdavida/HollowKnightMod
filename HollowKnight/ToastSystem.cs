using UnityEngine;

namespace HollowKnight.Core
{
    /// <summary>
    /// Toast notification system for temporary on-screen messages.
    /// Displays feedback messages that automatically fade out after 3 seconds.
    /// </summary>
    public class ToastSystem
    {
        private string toastMessage = "";
        private float toastTimer = 0f;
        private const float TOAST_DURATION = 3.0f;

        public void ShowToast(string message)
        {
            toastMessage = message;
            toastTimer = TOAST_DURATION;
        }

        public void Update(float deltaTime)
        {
            if (toastTimer > 0)
            {
                toastTimer -= deltaTime;
            }
        }

        public void RenderToast()
        {
            if (toastTimer > 0f && !string.IsNullOrEmpty(toastMessage))
            {
                // Calculate fade alpha based on remaining time
                float alpha = toastTimer / TOAST_DURATION;
                
                // Apply fade effect to color
                Color originalColor = GUI.color;
                GUI.color = new Color(0.2f, 0.8f, 0.2f, alpha);
                
                // Render the toast message
                GUILayout.Label(toastMessage, GUI.skin.box);
                
                // Restore original color
                GUI.color = originalColor;
            }
        }
    }
}

