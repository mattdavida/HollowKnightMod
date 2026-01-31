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
        private object cheatManagerInstance;
        private Type cheatManagerType;
        private FieldInfo isInstaKillEnabledField;
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
        /// Gets or creates the CheatManager instance.
        /// </summary>
        private bool EnsureCheatManagerInstance()
        {
            if (cheatManagerInstance != null && cheatManagerType != null && isInstaKillEnabledField != null)
            {
                return true;
            }

            try
            {
                // Get CheatManager type
                cheatManagerType = Type.GetType("CheatManager, Assembly-CSharp");
                if (cheatManagerType == null)
                {
                    logger.Log("CheatManager type not found");
                    return false;
                }

                // Get the instance field (private static)
                FieldInfo instanceField = cheatManagerType.GetField("instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (instanceField == null)
                {
                    logger.Log("CheatManager.instance field not found");
                    return false;
                }

                cheatManagerInstance = instanceField.GetValue(null);

                // If instance is null, manually call Init() to create it (fallback)
                // This should rarely happen since we now call Init() early in mod initialization
                if (cheatManagerInstance == null)
                {
                    logger.Log("CheatManager.instance is null - attempting fallback Init() call");
                    
                    // Call CheatManager.Init() via reflection
                    MethodInfo initMethod = cheatManagerType.GetMethod("Init", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    if (initMethod != null)
                    {
                        initMethod.Invoke(null, null);
                        logger.Log("CheatManager.Init() called (fallback)");
                        
                        // Try to get the instance again
                        cheatManagerInstance = instanceField.GetValue(null);
                        
                        if (cheatManagerInstance == null)
                        {
                            logger.Log("CheatManager still not available - try toggling Insta Kill after loading into the game");
                            return false;
                        }
                        
                        logger.Log("CheatManager instance created successfully via fallback!");
                    }
                    else
                    {
                        logger.Log("CheatManager.Init() method not found");
                        return false;
                    }
                }

                // Get the isInstaKillEnabled field
                isInstaKillEnabledField = cheatManagerType.GetField("isInstaKillEnabled", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (isInstaKillEnabledField == null)
                {
                    logger.Log("isInstaKillEnabled field not found on CheatManager");
                    return false;
                }

                logger.Log("Successfully accessed CheatManager instance!");
                return true;
            }
            catch (Exception e)
            {
                logger.Log($"Error accessing CheatManager: {e.Message}");
                logger.Log($"Stack trace: {e.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Toggles the Insta Kill cheat.
        /// </summary>
        public bool ToggleInstaKill(Action<string> onSuccess = null, Action<string> onError = null)
        {
            if (!EnsureCheatManagerInstance())
            {
                onError?.Invoke("CheatManager not ready yet - try again after loading into the game");
                return false;
            }

            try
            {
                bool currentValue = (bool)isInstaKillEnabledField.GetValue(cheatManagerInstance);
                bool newValue = !currentValue;

                isInstaKillEnabledField.SetValue(cheatManagerInstance, newValue);

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
            if (!EnsureCheatManagerInstance())
            {
                return false;
            }

            try
            {
                return (bool)isInstaKillEnabledField.GetValue(cheatManagerInstance);
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
            if (!EnsureCheatManagerInstance())
            {
                onError?.Invoke("CheatManager not available");
                return false;
            }

            try
            {
                isInstaKillEnabledField.SetValue(cheatManagerInstance, value);
                
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

