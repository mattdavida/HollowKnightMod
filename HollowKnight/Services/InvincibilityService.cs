using System;
using HollowKnight.Interfaces;
using HollowKnight.Core;

namespace HollowKnight.Services
{
    /// <summary>
    /// Service for managing player invincibility.
    /// </summary>
    public class InvincibilityService
    {
        private readonly IModLogger logger;
        private object heroController;
        private Action<string, bool> onConfigSave;

        public InvincibilityService(IModLogger logger)
        {
            this.logger = logger;
        }

        public void SetHeroController(object controller)
        {
            heroController = controller;
        }

        public void SetConfigSaveCallback(Action<string, bool> callback)
        {
            onConfigSave = callback;
        }

        /// <summary>
        /// Toggles invincibility on/off.
        /// </summary>
        public bool ToggleInvincibility(Action<string> onSuccess = null, Action<string> onError = null)
        {
            bool currentValue = IsInvincible();
            bool newValue = !currentValue;
            return SetInvincibility(newValue, onSuccess, onError);
        }

        /// <summary>
        /// Sets invincibility to a specific state.
        /// </summary>
        public bool SetInvincibility(bool enabled, Action<string> onSuccess = null, Action<string> onError = null)
        {
            if (heroController == null)
            {
                onError?.Invoke("Hero controller not available");
                return false;
            }

            object playerData = PlayerDataHelper.GetPlayerData(heroController, (error) =>
            {
                logger.Log($"InvincibilityService.SetInvincibility: {error}");
                onError?.Invoke(error);
            });

            if (playerData == null)
                return false;

            bool success = PlayerDataHelper.SetBool(playerData, "isInvincible", enabled, (error) =>
            {
                logger.Log($"InvincibilityService.isInvincible: {error}");
                onError?.Invoke(error);
            });

            if (success)
            {
                string status = enabled ? "enabled" : "disabled";
                onSuccess?.Invoke($"Invincibility {status}");
                onConfigSave?.Invoke("Invincibility", enabled);
            }

            return success;
        }

        /// <summary>
        /// Checks if invincibility is currently enabled.
        /// </summary>
        public bool IsInvincible()
        {
            if (heroController == null)
                return false;

            object playerData = PlayerDataHelper.GetPlayerData(heroController, (error) => logger.Log($"InvincibilityService.IsInvincible: {error}"));
            if (playerData == null)
                return false;

            return PlayerDataHelper.GetBool(playerData, "isInvincible", false, (error) => logger.Log($"InvincibilityService.isInvincible: {error}"));
        }
    }
}
