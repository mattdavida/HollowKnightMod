using System;
using HollowKnight.Interfaces;
using HollowKnight.Core;

namespace HollowKnight.Services
{
    /// <summary>
    /// Service for managing infinite air jump toggle.
    /// </summary>
    public class InfiniteAirJumpService
    {
        private readonly IModLogger logger;
        private object heroController;
        private Action<string, bool> onConfigSave;

        public InfiniteAirJumpService(IModLogger logger)
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
        /// Toggles infinite air jump on/off.
        /// </summary>
        public bool ToggleInfiniteAirJump(Action<string> onSuccess = null, Action<string> onError = null)
        {
            bool currentValue = IsInfiniteAirJumpEnabled();
            bool newValue = !currentValue;
            return SetInfiniteAirJump(newValue, onSuccess, onError);
        }

        /// <summary>
        /// Sets infinite air jump to a specific state.
        /// </summary>
        public bool SetInfiniteAirJump(bool enabled, Action<string> onSuccess = null, Action<string> onError = null)
        {
            if (heroController == null)
            {
                onError?.Invoke("Hero controller not available");
                return false;
            }

            object playerData = PlayerDataHelper.GetPlayerData(heroController, (error) =>
            {
                logger.Log($"InfiniteAirJumpService.SetInfiniteAirJump: {error}");
                onError?.Invoke(error);
            });

            if (playerData == null)
                return false;

            bool success = PlayerDataHelper.SetBool(playerData, "infiniteAirJump", enabled, (error) =>
            {
                logger.Log($"InfiniteAirJumpService.infiniteAirJump: {error}");
                onError?.Invoke(error);
            });

            if (success)
            {
                string status = enabled ? "enabled" : "disabled";
                onSuccess?.Invoke($"Infinite Air Jump {status}");
                onConfigSave?.Invoke("InfiniteAirJump", enabled);
            }

            return success;
        }

        /// <summary>
        /// Checks if infinite air jump is currently enabled.
        /// </summary>
        public bool IsInfiniteAirJumpEnabled()
        {
            if (heroController == null)
                return false;

            object playerData = PlayerDataHelper.GetPlayerData(heroController, (error) => logger.Log($"InfiniteAirJumpService.IsInfiniteAirJumpEnabled: {error}"));
            if (playerData == null)
                return false;

            return PlayerDataHelper.GetBool(playerData, "infiniteAirJump", false, (error) => logger.Log($"InfiniteAirJumpService.infiniteAirJump: {error}"));
        }
    }
}

