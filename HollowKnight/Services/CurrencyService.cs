using System;
using System.Reflection;
using UnityEngine;
using HollowKnight.Interfaces;
using HollowKnight.Core;

namespace HollowKnight.Services
{
    /// <summary>
    /// Service for managing player currency (Geo).
    /// Handles adding/removing geo for Hollow Knight.
    /// </summary>
    public class CurrencyService
    {
        private readonly IModLogger logger;
        private Component heroController;

        public CurrencyService(IModLogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Sets the hero controller reference. Must be called when hero is available.
        /// </summary>
        /// <param name="heroController">The hero controller component</param>
        public void SetHeroController(Component heroController)
        {
            this.heroController = heroController;
        }

        /// <summary>
        /// Adds the specified amount of Geo to the player.
        /// </summary>
        /// <param name="amount">Amount of Geo to add (can be negative to remove)</param>
        /// <param name="onSuccess">Callback for success message</param>
        /// <param name="onError">Callback for error message</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool AddGeo(int amount, Action<string> onSuccess = null, Action<string> onError = null)
        {
            if (heroController == null)
            {
                string error = "Cannot add Geo: Hero controller not available";
                logger.Log(error);
                onError?.Invoke(error);
                return false;
            }

            try
            {
                Type type = heroController.GetType();
                MethodInfo addGeoMethod = type.GetMethod("AddGeo");

                if (addGeoMethod != null)
                {
                    addGeoMethod.Invoke(heroController, new object[] { amount });
                    string success = $"Added {amount} Geo";
                    logger.Log(success);
                    onSuccess?.Invoke(success);
                    return true;
                }
                else
                {
                    string error = "AddGeo method not found on HeroController";
                    logger.Log(error);
                    onError?.Invoke(error);
                    return false;
                }
            }
            catch (Exception e)
            {
                string error = $"Error adding Geo: {e.Message}";
                logger.Log(error);
                onError?.Invoke(error);
                return false;
            }
        }

        /// <summary>
        /// Refills soul (MP) to maximum.
        /// Uses HeroController.SetMPCharge to set soul to maxMP from PlayerData.
        /// </summary>
        /// <param name="onSuccess">Callback for success message</param>
        /// <param name="onError">Callback for error message</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool RefillSoul(Action<string> onSuccess = null, Action<string> onError = null)
        {
            if (heroController == null)
            {
                string error = "Cannot refill soul: Hero controller not available";
                logger.Log(error);
                onError?.Invoke(error);
                return false;
            }

            try
            {
                // Get playerData to read maxMP
                object playerData = PlayerDataHelper.GetPlayerData(heroController, (error) =>
                {
                    logger.Log($"CurrencyService.RefillSoul: {error}");
                    onError?.Invoke(error);
                });

                if (playerData == null)
                    return false;

                // Get maxMP value
                int maxMP = PlayerDataHelper.GetInt(playerData, "maxMP", 99, (error) =>
                {
                    logger.Log($"CurrencyService.maxMP: {error}");
                    onError?.Invoke(error);
                });

                // Call HeroController.SetMPCharge(maxMP) to refill soul
                Type type = heroController.GetType();
                MethodInfo setMPChargeMethod = type.GetMethod("SetMPCharge");

                if (setMPChargeMethod != null)
                {
                    setMPChargeMethod.Invoke(heroController, new object[] { maxMP });
                    string success = "Soul refilled!";
                    logger.Log(success);
                    onSuccess?.Invoke(success);
                    return true;
                }
                else
                {
                    string error = "SetMPCharge method not found on HeroController";
                    logger.Log(error);
                    onError?.Invoke(error);
                    return false;
                }
            }
            catch (Exception e)
            {
                string error = $"Error refilling soul: {e.Message}";
                logger.Log(error);
                onError?.Invoke(error);
                return false;
            }
        }
    }
}

