using System;
using System.Reflection;
using HollowKnight.Interfaces;

namespace HollowKnight.Services
{
    /// <summary>
    /// Service for managing the Insta Kill cheat (from CheatManager).
    /// </summary>
    public class InstaKillService
    {
        private readonly IModLogger logger;
        private Type cheatManagerType;
        private PropertyInfo isInstaKillEnabledProperty;
        private Action<string, bool> onConfigSave;

        public InstaKillService(IModLogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Sets callback for saving config state when toggles change.
        /// </summary>
        public void SetConfigSaveCallback(Action<string, bool> callback)
        {
            onConfigSave = callback;
        }

        /// <summary>
        /// Resolves the CheatManager type and the IsInstaKillEnabled static property.
        /// No instance is required — the property is static.
        /// </summary>
        private bool EnsureCheatManagerType()
        {
            if (cheatManagerType != null && isInstaKillEnabledProperty != null)
            {
                return true;
            }

            try
            {
                cheatManagerType = Type.GetType("CheatManager, Assembly-CSharp");
                if (cheatManagerType == null)
                {
                    logger.Log("CheatManager type not found");
                    return false;
                }

                // IsInstaKillEnabled is a public static property
                isInstaKillEnabledProperty = cheatManagerType.GetProperty(
                    "IsInstaKillEnabled",
                    BindingFlags.Public | BindingFlags.Static);

                if (isInstaKillEnabledProperty == null)
                {
                    logger.Log("CheatManager.IsInstaKillEnabled property not found");
                    return false;
                }

                logger.Log("Successfully resolved CheatManager.IsInstaKillEnabled!");
                return true;
            }
            catch (Exception e)
            {
                logger.Log($"Error resolving CheatManager: {e.Message}");
                logger.Log($"Stack trace: {e.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Toggles the Insta Kill cheat.
        /// </summary>
        public bool ToggleInstaKill(Action<string> onSuccess = null, Action<string> onError = null)
        {
            if (!EnsureCheatManagerType())
            {
                onError?.Invoke("CheatManager not available");
                return false;
            }

            try
            {
                // null target because IsInstaKillEnabled is static
                bool currentValue = (bool)isInstaKillEnabledProperty.GetValue(null);
                bool newValue = !currentValue;

                isInstaKillEnabledProperty.SetValue(null, newValue);

                string status = newValue ? "enabled" : "disabled";
                string success = $"Insta Kill {status}";
                logger.Log(success);
                onSuccess?.Invoke(success);
                onConfigSave?.Invoke("InstaKill", newValue);
                return true;
            }
            catch (Exception e)
            {
                string error = $"Error toggling Insta Kill: {e.Message}";
                logger.Log(error);
                onError?.Invoke(error);
                return false;
            }
        }

        /// <summary>
        /// Checks if Insta Kill is currently enabled.
        /// </summary>
        public bool IsInstaKillEnabled()
        {
            if (!EnsureCheatManagerType())
            {
                return false;
            }

            try
            {
                return (bool)isInstaKillEnabledProperty.GetValue(null);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Sets the Insta Kill state directly.
        /// </summary>
        public bool SetInstaKill(bool value, Action<string> onSuccess = null, Action<string> onError = null)
        {
            if (!EnsureCheatManagerType())
            {
                onError?.Invoke("CheatManager not available");
                return false;
            }

            try
            {
                isInstaKillEnabledProperty.SetValue(null, value);

                string status = value ? "enabled" : "disabled";
                onSuccess?.Invoke($"Insta Kill set to {status}");
                return true;
            }
            catch (Exception e)
            {
                onError?.Invoke($"Error setting Insta Kill: {e.Message}");
                return false;
            }
        }
    }
}
